using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Orel.Schema.Dynamic;
using Newtonsoft.Json.Linq;

namespace Orel.Schema
{
    public class SchemaReader : ISchemaReader, IDynamicSchemaReader
    {
        private readonly Dictionary<Type, Stack> _typeStacks = new Dictionary<Type, Stack>();
        private readonly HashSet<Object> _objectCache = new HashSet<object>();
        private readonly Func<object, IDynamicSchemaReader> _dynamicReaderSelector;
        private readonly int _maxPathRecursiveCount = 3;
        private readonly object _dummy = new object();
        private readonly Lazy<JObjectSchemaReader> _jObjectReader;
        private readonly Lazy<KeyValuePairSchemaReader> _keyValueReader;

        public SchemaReader(SchemaReaderOptions options = null)
        {
            if (options == null)
            {
                _dynamicReaderSelector = new Func<object, IDynamicSchemaReader>(GetDynamic);
            }
            else
            {
                _dynamicReaderSelector = options.DynamicReaderSelector ?? new Func<object, IDynamicSchemaReader>(GetDynamic);
                _maxPathRecursiveCount = options.MaxPathRecursiveCount;
            }
            _jObjectReader = new Lazy<JObjectSchemaReader>(true);
            _keyValueReader = new Lazy<KeyValuePairSchemaReader>(() => new KeyValuePairSchemaReader(this), true);
        }

        private IDynamicSchemaReader GetDynamic(object dynamicMetaObject)
        {
            if (dynamicMetaObject is JToken)
            {
                return _jObjectReader.Value;
            }
            else if (dynamicMetaObject is IEnumerable<KeyValuePair<string, object>>)
            {
                return _keyValueReader.Value;
            }
            throw new NotSupportedException("Type of dynamicMetaObject is not supported.");
        }

        private bool PushTypeHierarchy(Type type)
        {
            if (_typeStacks.TryGetValue(type, out var stack))
            {
                if (stack.Count >= _maxPathRecursiveCount)
                {
                    return false;
                }
            }
            else
            {
                stack = new Stack();
                _typeStacks.Add(type, stack);
            }
            stack.Push(_dummy);
            return true;
        }

        private void PopTypeHierarchy(Type type)
        {
            _typeStacks[type].Pop();
        }

        /// <summary>
        /// 从类型定义读取Schema信息
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <param name="scope"></param>
        public void Read(ISchemaProvider provider, Type type, string scope = null)
        {
            if (!type.IsClass || type.IsList() || type.IsDynamic())
            {
                //throw new ArgumentException($"Cannot read schema info from type of { type.Name } directly.");
                //ignore
                return;
            }
            var props = type.GetProperties();
            foreach (var p in props)
            {
                var dataType = p.PropertyType.GetORELDataType();
                var member = new MemberDefinition(p.Name, dataType, p.PropertyType, type, scope);
                provider.Add(member);
                if (p.PropertyType.IsDynamic())
                {
                    //该对象为动态类型，需要从实例读取Schema信息    
                    //throw new ArgumentException($"Cannot read schema info from dynamic object:{p.Name}.");
                    //ignore
                    return;
                }
                if (dataType == DataType.Object)
                {
                    if (PushTypeHierarchy(p.PropertyType))
                    {
                        Read(provider, p.PropertyType, member.UniqueName);
                        PopTypeHierarchy(p.PropertyType);
                    }
                }
                else if (dataType == DataType.List)
                {
                    Type itemType;
                    if (p.PropertyType.IsArray)
                    {
                        itemType = p.PropertyType.GetElementType();
                    }
                    else
                    {
                        itemType = p.PropertyType.GenericTypeArguments.FirstOrDefault();
                    }

                    if (itemType != null)
                    {
                        if (itemType.IsDynamic())
                        {
                            //子类需要更新实际字段
                            //该对象为动态类型，需要从实例读取Schema信息    
                            //throw new ArgumentException("Cannot read schema info from dynamic object.");
                            //ignore
                            return;
                        }
                        var itemDataType = itemType.GetORELDataType();
                        if (itemDataType == DataType.Object || itemDataType == DataType.List)
                        {
                            if (PushTypeHierarchy(p.PropertyType))
                            {
                                Read(provider, itemType, member.UniqueName);
                                PopTypeHierarchy(p.PropertyType);
                            }
                        }
                        else
                        {
                            member = new MemberDefinition(TokenDefinitions.DefaultArgument, itemDataType, itemType, p.PropertyType, member.UniqueName);
                            provider.Add(member);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从Object实例读取Schema信息，适合动态类型的情形
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="dynamicMetaObject"></param>
        /// <param name="scope"></param>
        public void Read(ISchemaProvider provider, object dynamicMetaObject, string scope = null)
        {
            if (dynamicMetaObject == null || _objectCache.Contains(dynamicMetaObject))
            {
                return;
            }
            //已处理的对象放入缓存，防止自引用造成无限循环
            _objectCache.Add(dynamicMetaObject);
            var type = dynamicMetaObject.GetType();
            if (type.IsDynamic())
            {
                if (type == typeof(JValue))
                {
                    return;
                }
                else
                {
                    var dynamicReader = _dynamicReaderSelector(dynamicMetaObject);
                    dynamicReader.Read(provider, dynamicMetaObject, scope);
                }
            }
            else if (!CanBreakdown(type))
            {
                return;
            }
            else if (type.IsList())
            {
                var itemType = type.HasElementType ? type.GetElementType()
                    : type.GenericTypeArguments[0];
                if (itemType == typeof(object))
                {
                    var list = ((IList)dynamicMetaObject);
                    if (list.Count > 0)
                        itemType = list[0].GetType();
                }
                if (itemType.IsDynamic())
                {
                    ReadFromAllListItems(provider, dynamicMetaObject, scope);
                }
                else
                {
                    ReadFromList(provider, dynamicMetaObject, type, scope);
                }
                provider.RootType = scope == null ? RootType.List : RootType.Object;
            }
            else
            {
                var props = type.GetProperties();
                foreach (var p in props)
                {
                    var dataType = p.PropertyType.GetORELDataType();
                    //当属性类型是JObject时，需要使用object类型，防止动态运行时访问出错
                    var propType = p.PropertyType == typeof(JObject) ? typeof(object) : p.PropertyType;
                    var member = new MemberDefinition(p.Name, dataType, propType, type, scope);
                    provider.Add(member);
                    if (dataType == DataType.Object || dataType == DataType.List)
                    {
                        var value = p.GetValue(dynamicMetaObject);
                        if (value is JArray && member.DataType == DataType.Object)
                        {
                            member.ChangeType(DataType.List);
                        }
                        Read(provider, value, member.UniqueName);
                    }
                }
            }
        }

        internal void ReadFromAllListItems(ISchemaProvider provider, object dynamicMetaObject, string scope)
        {
            var itor = (dynamicMetaObject as IEnumerable).GetEnumerator();
            if (!itor.MoveNext())
            {
                //ignore empty collection
                return;
            }
            var item = itor.Current;
            ReadCurrent(provider, item, scope);
            var readCount = 1; //读取前十个元素
            while (itor.MoveNext() && readCount++ < 10)
            {
                item = itor.Current;
                ReadCurrent(provider, item, scope);
                //dynamicReader.Read(provider, item, scope);
            }
        }

        private void ReadCurrent(ISchemaProvider provider, object item, string scope)
        {
            if (item.GetType().IsDynamic())
            {
                var dynamicReader = _dynamicReaderSelector(item);
                dynamicReader.Read(provider, item, scope);
            }
            else
            {
                Read(provider, item, scope);
            }
        }

        internal void ReadFromList(ISchemaProvider provider, object dynamicMetaObject, Type contextType, string scope)
        {
            var itor = (dynamicMetaObject as IEnumerable).GetEnumerator();
            if (!itor.MoveNext())
            {
                //ignore empty collection
                return;
            }
            while (itor.Current == null) itor.MoveNext();
            var type = itor.Current.GetType();

            if (type.IsList())
            {
                var elType = type.GetORELDataType();
                var member = new MemberDefinition(TokenDefinitions.DefaultArgument, elType, type, contextType, scope);
                provider.Add(member);
                Read(provider, itor.Current, TokenDefinitions.DefaultArgument);
            }
            else if (CanBreakdown(type))
            {
                Read(provider, itor.Current, scope);
            }
            else
            {
                var elType = type.GetORELDataType();
                var member = new MemberDefinition(TokenDefinitions.DefaultArgument, elType, type, contextType, scope);
                provider.Add(member);
            }
        }

        private bool CanBreakdown(Type type)
        {
            if (type.IsPrimitive || Nullable.GetUnderlyingType(type)?.IsPrimitive == true)
            {
                return false;
            }
            //其它情况
            var primative = type.IsNumeric() || type.IsString() || type.IsDateTime()
                || type.IsBoolean();
            return !primative;
        }
    }
}
