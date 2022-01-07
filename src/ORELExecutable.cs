using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orel.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Orel
{
    public class ORELExecutable<T>
    {
        private ORELExecutable _internalExecutable;
        public IDictionary<string, ParameterDefinition> Parameters => _internalExecutable.Parameters;
        internal ORELExecutable(ORELExecutable intern)
        {
            var match = intern.ReturnType == typeof(T)
                || typeof(T).IsAssignableFrom(intern.ReturnType)
                || Nullable.GetUnderlyingType(intern.ReturnType) == typeof(T)
                || Nullable.GetUnderlyingType(typeof(T)) == intern.ReturnType;
            if (!match)
            {
                throw new ArgumentException($"Return type of statement is not compliance with the generic paramerter: {typeof(T).FullName}");
            }
            _internalExecutable = intern;
        }

        public T Execute(object arg = null, object param = null)
        {
            var result = _internalExecutable.Execute(arg, param);
            return (T)result;
        }
    }

    public class ORELExecutable
    {
        internal ORELExecutable() { }
        public LambdaExpression Lambda { get; internal set; }
        public Delegate Delegate { get; internal set; }
        public IMemberDescriptor MemberDescriptor { get; internal set; }
        public IDictionary<string, ParameterDefinition> Parameters { get; internal set; }
        public Type ReturnType => Lambda.ReturnType;
        /// <summary>
        /// 执行表达式
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public object Execute(object arg = null, object param = null)
        {
            if (param == null && (Parameters != null && Parameters.Any()))
            {
                throw new ArgumentNullException(nameof(param));
            }
            var args = Enumerable.Empty<object>();
            if (param != null)
            {
                args = GetParameterValues(param, Parameters);
            }
            if (arg != null)
            {
                args = arg.HeadOf(args);
            }
            return Delegate.DynamicInvoke(args.ToArray());
        }

        internal static IEnumerable<object> GetParameterValues(object param, IDictionary<string, ParameterDefinition> parameters)
        {
            if (param is JObject jObject)
            {
                return GetValuesFromJObject(jObject, parameters);
            }
            else if (param is IEnumerable<KeyValuePair<string, object>> kvp)
            {
                return GetValuesFromDynamic(kvp, parameters);
            }
            else if (!param.GetType().IsDynamic())
            {
                return GetValuesFromObject(param, parameters);
            }
            else
            {
                throw new ArgumentException($"can not read values from type : {param.GetType()}");
            }
        }

        private static IEnumerable<object> GetValuesFromObject(object param, IDictionary<string, ParameterDefinition> parameters)
        {
            var type = param.GetType();
            foreach (var pair in parameters)
            {
                var prop = type.GetProperty(pair.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (prop == null)
                {
                    //throw new ArgumentException($"缺少参数：{pair.Key}");
                    yield return null;
                }
                else
                {
                    var value = prop.GetValue(param);
                    yield return ConvertValue(value, pair.Value.DataType, prop.Name);
                }
            }
        }

        private static IEnumerable<object> GetValuesFromJObject(JObject jObj, IDictionary<string, ParameterDefinition> parameters)
        {
            var props = jObj.Properties();
            foreach (var pair in parameters)
            {
                var prop = props.FirstOrDefault(p => p.Name.Equals(pair.Key, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                {
                    //throw new ArgumentException($"缺少参数：{pair.Key}");
                    yield return null;
                }
                else
                {
                    var value = prop.Value.ToObject(pair.Value.DataType.GetRuntimeType());
                    yield return ConvertValue(value, pair.Value.DataType, prop.Name);
                }
            }
        }

        private static IEnumerable<object> GetValuesFromDynamic(IEnumerable<KeyValuePair<string, object>> props, IDictionary<string, ParameterDefinition> parameters)
        {
            foreach (var pair in parameters)
            {
                var prop = props.FirstOrDefault(p => p.Key.Equals(pair.Key, StringComparison.OrdinalIgnoreCase));
                if (prop.Key == null)
                {
                    //throw new ArgumentException($"缺少参数：{pair.Key}");
                    yield return null;
                }
                else
                {
                    yield return ConvertValue(prop.Value, pair.Value.DataType, prop.Key);
                }
            }
        }

        private static object ConvertValue(object value, DataType dataType, string propName)
        {
            switch (dataType)
            {
                case DataType.Number:
                    if (value.GetType().IsNumeric())
                    {
                        return Convert.ToDecimal(value);
                    }
                    else
                    {
                        throw new ArgumentException($"参数{propName}是无效的数值类型, value:{value}");
                    }
                case DataType.DateTime:
                    if (value.GetType().IsDateTime())
                    {
                        if (value is DateTimeOffset || value is DateTimeOffset?)
                            return value;
                        else
                            return new DateTimeOffset((DateTime)value);
                    }
                    else
                    {
                        throw new ArgumentException($"参数{propName}是无效的时间类型, value:{value}");
                    }
                case DataType.Text:
                    if (value.GetType() != typeof(string))
                        return value.ToString();
                    else
                        return value;
                default:
                    return value;
            }
        }

        /// <summary>
        /// Json数据作为参数执行表达式
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        public object ExecuteJson(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                throw new ArgumentNullException("jsonText");
            }
            if (MemberDescriptor == null)
            {
                throw new InvalidOperationException("Member definitions can not be null");
            }
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter>() { new ORELJsonConverter(MemberDescriptor) }
            };
            dynamic obj = JsonConvert.DeserializeObject<ORELObject>(jsonText, settings);
            return Delegate.DynamicInvoke(obj);
        }
    }
}
