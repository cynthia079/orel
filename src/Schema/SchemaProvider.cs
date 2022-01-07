using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Orel.Schema.Dynamic;
using Newtonsoft.Json.Linq;

namespace Orel.Schema
{
    public class SchemaProvider : ISchemaProvider
    {
        private readonly Dictionary<string, MemberDefinition> _memberDicts = new Dictionary<string, MemberDefinition>(StringComparer.OrdinalIgnoreCase);

        public string DefaultScope { get; private set; }

        public RootType RootType { get; set; }

        public MemberDefinition Add(MemberDefinition def)
        {
            if (!string.IsNullOrEmpty(def.Scope))
            {
                MemberDefinition scopeMember = GetScopeMember(def.Scope);
                def.Parent = scopeMember;
                def.UniqueName = $"{scopeMember.UniqueName}.{def.MemberName}";
                def.ActualName = def.MemberName;
            }
            else
            {
                def.UniqueName = def.MemberName;
                def.ActualName = def.MemberName;
            }
            _memberDicts.TryAdd(def.UniqueName, def);
            return def;
        }

        public MemberDefinition Get(string uniquePath, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                uniquePath = $"{prefix}.{uniquePath}";
            }
            if (_memberDicts.TryGetValue(uniquePath, out var memberDefinition))
            {
                return memberDefinition;
            }
            if (_memberDicts.TryGetValue($"{DefaultScope}.{uniquePath}", out memberDefinition))
            {
                return memberDefinition;
            }
            return null;
        }

        public MemberDefinition GetByActualName(string actualName)
        {
            //throw new NotImplementedException();
            return null;
        }

        private MemberDefinition GetScopeMember(string scope)
        {
            if (_memberDicts.TryGetValue(scope, out var parent))
            {
                if (parent.DataType == DataType.List || parent.DataType == DataType.Object)
                {
                    return parent;
                }
                else
                {
                    parent.ChangeType(DataType.Object);   //简单类型->对象类型
                    return parent;
                }
            }
            throw new ArgumentException($"Scope：{scope}不存在，或数据类型为List或Object以外的类型");
        }

        IMemberDefinition IMemberDescriptor.Get(string uniquePath, string prefix)
        {
            return this.Get(uniquePath, prefix);
        }

        public IMemberDefinition Add(IMemberDefinition def)
        {
            return Add(def as MemberDefinition);
        }

        IMemberDefinition IMemberDescriptor.GetByActualName(string actualName)
        {
            return GetByActualName(actualName);
        }

        public SchemaProvider(string defaultScope = null)
        {
            DefaultScope = defaultScope;
        }

        public static SchemaProvider Empty()
        {
            return new SchemaProvider();
        }

        #region Factory Methods
        /// <summary>
        /// 通过类型创建SchemaProvider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultScope">缺省成员访问路径</param>
        /// <returns></returns>
        public static SchemaProvider FromType<T>(string defaultScope = null) where T : class
        {
            var schemaReader = new SchemaReader();
            return FromType<T>(schemaReader, defaultScope);
        }

        /// <summary>
        /// 通过类型创建SchemaProvider
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="schemaReader">自定义SchemaReader</param>
        /// <param name="defaultScope">缺省成员访问路径</param>
        /// <returns></returns>
        public static SchemaProvider FromType<T>(ISchemaReader schemaReader, string defaultScope = null) where T : class
        {
            var type = typeof(T);
            var provider = new SchemaProvider(defaultScope);
            schemaReader.Read(provider, type, null);
            return provider;
        }

        /// <summary>
        /// 通过对象实例创建SchemaProvider
        /// </summary>
        /// <param name="metaObject">对象实例</param>
        /// <param name="defaultScope">缺省成员访问路径</param>
        /// <returns></returns>
        public static SchemaProvider FromObject(object metaObject, string defaultScope = null)
        {
            var schemaReader = new SchemaReader();
            return FromObject(metaObject, schemaReader, defaultScope);
        }

        /// <summary>
        /// 通过对象实例创建SchemaProvider
        /// </summary>
        /// <param name="metaObject">对象实例</param>
        /// <param name="schemaReader">自定义SchemaReader</param>
        /// <param name="defaultScope">缺省成员访问路径</param>
        /// <returns></returns>
        public static SchemaProvider FromObject(object metaObject, IDynamicSchemaReader schemaReader, string defaultScope = null)
        {
            var provider = new SchemaProvider(defaultScope);
            schemaReader.Read(provider, metaObject, null);
            return provider;
        }

        /// <summary>
        /// create SchemaProvider by member definitions
        /// </summary>
        /// <param name="members"></param>
        /// <param name="defaultScope"></param>
        /// <returns></returns>
        public static SchemaProvider FromMemberDefinitions(IEnumerable<MemberDefinition> members, string defaultScope = null)
        {
            var provider = new SchemaProvider(defaultScope);
            foreach (var member in members)
            {
                provider.Add(member);
            }
            return provider;
        }
        #endregion

        #region Implemention of IEnumerable
        public IEnumerator<IMemberDefinition> GetEnumerator()
        {
            return _memberDicts.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _memberDicts.Values.GetEnumerator();
        }
        #endregion

        public void Merge(ISchemaProvider another, string scope)
        {
            if (_memberDicts.TryGetValue(scope, out var parent))
            {
                if ((another.RootType == RootType.Object && parent.DataType != DataType.Object)
                    || (another.RootType == RootType.List && parent.DataType != DataType.List))
                    throw new ArgumentException($"Scope：{scope}已存在，但是类型不符");
            }
            else
            {
                parent = new MemberDefinition(scope, another.RootType == RootType.Object ? DataType.Object : DataType.List)
                {
                    UniqueName = scope,
                    ActualName = scope
                };
                _memberDicts.Add(scope, parent);
            }
            MergeByScope(another.Where(a => a.Parent == null), parent, another);
        }

        private void MergeByScope(IEnumerable<IMemberDefinition> members, MemberDefinition parent, ISchemaProvider another)
        {
            foreach (var member in members)
            {
                var memberDef = new MemberDefinition(member.MemberName, member.DataType, member.Type, parent.Type, parent.UniqueName)
                {
                    Parent = parent,
                    UniqueName = $"{parent.UniqueName}.{member.MemberName}",
                    ActualName = member.MemberName
                };
                _memberDicts.TryAdd(memberDef.UniqueName, memberDef);
                var children = another.Where(a => a.Parent == member);
                if (children.Any())
                {
                    MergeByScope(children, memberDef, another);
                }
            }
        }
    }
}
