using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orel
{
    public class ORELObject : DynamicObject, IEnumerable<KeyValuePair<string, object>>
    {
        readonly IDictionary<string, object> _container;
        readonly DynamicMetaObject _refererMeta;
        readonly Type _refereType;
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return base.GetDynamicMemberNames();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_container.TryGetValue(binder.Name, out result))
            {
                if (_refereType != null)
                {
                    if (_refereType == typeof(JObject))
                    {
                        var jObj = (JObject)Referrer;
                        result = jObj.GetValue(binder.Name);
                        return true;
                    }
                    else if (_refererMeta != null)
                    {
                        //一般动态类型Fallback到Referer字段，比较慢，目前还没有特别好的办法处理，暂且使用CompiledLamda解决
                        var metaObj = _refererMeta.BindGetMember(binder);
                        var lamda = Expression.Lambda(metaObj.Expression);
                        result = lamda.Compile().DynamicInvoke();
                        _container.TryAdd(binder.Name, result);
                        return true;
                    }
                    else
                    {
                        var prop = _refereType.GetProperty(binder.Name, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance);
                        if (prop != null)
                        {
                            result = prop.GetValue(Referrer);
                            _container.TryAdd(binder.Name, result);
                            return true;
                        }
                        else
                        {
                            throw ThrowHelper.InvalidMemberName(binder.Name);
                        }
                    }
                }
                result = null;
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            SetMember(binder.Name, value);
            return true;
        }

        public void SetMember(string name, object value)
        {
            _container[name] = value;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (Referrer == null)
                return _container.GetEnumerator();
            if (Referrer is ORELObject elRef)
            {
                return new ORELObjectEnumerator<object>(elRef.GetEnumerator(), _container.GetEnumerator());
            }
            else if (Referrer is JObject jRef)
            {
                return new ORELObjectEnumerator<JToken>(jRef.GetEnumerator(), _container.GetEnumerator());
            }
            return _container.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ORELObject Compose(ORELObject other)
        {
            if (other != null)
            {
                foreach (var item in other)
                {
                    SetMember(item.Key, item.Value);
                }
            }
            return this;
        }

        public bool HasMember(string memberName)
        {
            return _container.ContainsKey(memberName);
        }

        public object Referrer { get; private set; }  //执行Compose操作时，如果Item类型不为ORELObject,则需要存放原始数据        

        public ORELObject(object referer, ORELObject extra)
        {
            if (referer is ORELObject)
            {
                var refererObject = referer as ORELObject;
                _container = new Dictionary<string, object>(refererObject._container);
            }
            else
            {
                Referrer = referer;
                _container = new Dictionary<string, object>();
                if (Referrer != null)
                {
                    if (Referrer is IDynamicMetaObjectProvider)
                    {
                        var dynm = Referrer as IDynamicMetaObjectProvider;
                        _refererMeta = dynm.GetMetaObject(Expression.Constant(dynm));
                    }
                    _refereType = Referrer.GetType();
                }
            }
            Compose(extra);
        }

        public ORELObject()
        {
            _container = new Dictionary<string, object>();
        }

        public static MethodInfo ComposeMethodInfo => (typeof(ORELObject)).GetMethod(nameof(Compose));

        public static MethodInfo CheckMemberMethodInfo => (typeof(ORELObject)).GetMethod(nameof(HasMember));

        public static PropertyInfo RefererInfo => (typeof(ORELObject)).GetProperty(nameof(Referrer));

        public static ConstructorInfo ConstructorInfo => (typeof(ORELObject)).GetConstructor(new[] { typeof(object), typeof(ORELObject) });
    }
}
