using Orel.Schema;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Orel
{
    public class ORELScriptRunner
    {
        private readonly SchemaProvider _defaultSchema;
        private readonly List<ParameterDefinition> _parameters;
        private Dictionary<string, SortedList<string, Delegate>> externaMethodBags;
        public ORELScriptRunner()
        {
            //_defaultSchema = defaultSchema;
            _defaultSchema = SchemaProvider.Empty();
            _parameters = new List<ParameterDefinition>();
            externaMethodBags = new Dictionary<string, SortedList<string, Delegate>>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddExternalMethod(string methodName, Delegate @delegate)
        {
            var sign = @delegate.Method.GetMethodSign();
            if (externaMethodBags.TryGetValue(methodName, out var methodList))
            {
                if (methodList.ContainsKey(sign))
                    throw new ArgumentException("Method with same signature has already exsited");
                methodList.Add(sign, @delegate);
            }
            else
            {
                methodList = new SortedList<string, Delegate>();
                methodList.Add(sign, @delegate);
                externaMethodBags.Add(methodName, methodList);
            }
        }

        public void AddParameter(ParameterDefinition parameterDefinition)
        {
            _parameters.Add(parameterDefinition);
        }

        public object Invoke(string source)
        {
            return Invoke(source, null, null, null);
        }

        public object Invoke(string source, object param, Action<string, object> onStepSuccess, Action<string, Exception> onStepFailed)
        {
            if (param == null && (_parameters.Any()))
            {
                throw new ArgumentNullException(nameof(param));
            }
            var statements = Tokenizer.Scan(source);

            ParameterExpression paramExp = Expression.Parameter(typeof(object), "x");

            var context = BuildContext.Create(paramExp, _defaultSchema, _parameters);
            foreach (var item in externaMethodBags)
            {
                context.ExternalMethods.Add(item.Key, item.Value.Values);
            }
            var lamdaParameters = paramExp.HeadOf(context.ParameterManager.Parameters.Select(p => p.Value.Expression));
            object result = null;
            ExpandoObject exports = new ExpandoObject();

            var args = param == null ? new object[] { exports } : exports.HeadOf(ORELExecutable.GetParameterValues(param, context.ParameterManager.Parameters));
            for (int i = 0; i < statements.Count; i++)
            {
                var tb = new TreeBuilder();
                tb.AppendRange(statements[i]);
                try
                {
                    var exp = tb.GernerateTree(context);
                    LambdaExpression lambda = Expression.Lambda(exp, lamdaParameters);
                    Delegate del = lambda.Compile();
                    result = del.DynamicInvoke(new object[] { exports });
                    //判断是否绑定变量
                    if (tb.Root is Nodes.ExportNode && i < statements.Count - 1)
                    {
                        var variableName = tb.Root.LeftChild.Token.Text;
                        exports.TryAdd(variableName, result);
                        if (result != null)
                        {
                            var schema = SchemaProvider.FromObject(result);
                            if (schema.Any())
                            {
                                _defaultSchema.Merge(schema, variableName);
                            }
                            else
                            {
                                _defaultSchema.Add(new MemberDefinition(variableName, result.GetType().GetORELDataType()));
                            }
                        }
                    }
                    if (onStepSuccess != null)
                    {
                        onStepSuccess(tb.Root.ToString(), result);
                    }
                }
                catch (Exception e)
                {
                    if (onStepFailed != null)
                    {
                        onStepFailed(tb.Root.ToString(), e);
                        break;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            //获取最后语句的输出值
            return result;
        }

        public IList<DebugInfo> Debug(string source)
        {
            var debugInfos = new List<DebugInfo>();
            var onSuccess = new Action<string, object>((s, o) =>
            {
                debugInfos.Add(new DebugInfo()
                {
                    Statement = s,
                    Result = o,
                    Success = true
                });
            });
            var onFailed = new Action<string, Exception>((s, e) =>
            {
                debugInfos.Add(new DebugInfo()
                {
                    Statement = s,
                    Success = false,
                    Error = e
                });
            });
            Invoke(source, null, onSuccess, onFailed);
            return debugInfos;
        }
    }

    public class DebugInfo
    {
        public string Statement { get; set; }
        public object Result { get; set; }
        public bool Success { get; set; }
        public Exception Error { get; set; }
    }
}
