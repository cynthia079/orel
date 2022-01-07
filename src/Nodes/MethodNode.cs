using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Orel.Expressions;
using Orel.Schema;
using Orel.Schema.Dynamic;
using Newtonsoft.Json.Linq;

namespace Orel.Nodes
{
    internal class MethodNode : Node, IRuntimeMemberGenerator
    {
        internal string MethodName { get; private set; }
        public IMemberDescriptor RuntimeMembers { get; set; }

        public MethodNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var childrenExps = GetChildrenExpressions(context, 1);
            if (MethodName.Equals("if", StringComparison.OrdinalIgnoreCase))
            {
                return ExpressionHelper.MakeIfElseExpression(childrenExps[0], childrenExps[1], childrenExps[2]).Wrap(Token);
            }
            var callExp = ExpressionHelper.CreateMethodCallExpression(typeof(IntrinsicFunctions), MethodName, childrenExps);
            if (callExp != null)
            {
                SetOutputSchema(callExp.Type);
                return callExp.Wrap(Token);
            }
            else if (context.ExternalMethods.TryGetValue(MethodName, out var delegates))
            {
                var @delegate = ExpressionHelper.GetAppropriateDelegate(delegates, childrenExps);
                if (@delegate != null)
                {
                    callExp = Expression.Call(Expression.Constant(@delegate.Target), @delegate.Method, childrenExps.Select(c => c.Expression));
                    if (callExp.Type.IsGenericType && callExp.Type.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        var underlyingType = callExp.Type.GenericTypeArguments[0];
                        var propExp = Expression.Property(callExp, "Result");
                        SetOutputSchema(propExp.Type);
                        return propExp.Wrap(Token);
                    }
                    else
                    {
                        SetOutputSchema(callExp.Type);
                        return callExp.Wrap(Token);
                    }
                }
            }
            throw ThrowHelper.InvalidMethodCall(MethodName, childrenExps.Select(a => a.Expression.Type.Name));
        }

        private void SetOutputSchema(Type returnType)
        {
            if (returnType.IsList() && returnType.IsGenericType)
            {
                returnType = returnType.GetGenericArguments()[0];
            }
            var underlyingType = Nullable.GetUnderlyingType(returnType);
            if (underlyingType != null) returnType = underlyingType;
            if (!returnType.IsClass || returnType == typeof(string))
                return;
            //returnType is jtoken
            if (typeof(JToken).IsAssignableFrom(returnType))
            {
                RuntimeMembers = new JTokenMemberDescriptor(returnType);
                return;
            }
            //returnType is poco
            var provider = new SchemaProvider();
            var schemaReader = new SchemaReader();
            schemaReader.Read(provider, returnType, null);
            RuntimeMembers = provider;
        }

        public override Node SetBinaryLeft(Node node)
        {
            if (node.Token.Type != TokenType.MethodCall)
            {
                throw ThrowHelper.UnSupportedSyntax(node.Token);
            }
            //如果是预编译方法，去要去除前面的"$"符号
            MethodName = node.Token.Text.StartsWith("$") ? node.Token.Text.Substring(1) : node.Token.Text;
            return base.SetBinaryLeft(node);
        }

        public override string ToString()
        {
            return $"{LeftChild}({String.Join(',', Children.Skip(1))})";
        }

        protected override void TryPrecompile()
        {
            bool ret = TryPrecompileMethodToday();
            if (!ret)
            {
                TryPrecompileMethodNow();
            }
        }

        private object TryToExecuteExpression(Expression expression)
        {
            LambdaExpression lambda = Expression.Lambda(expression);
            return lambda.Compile().DynamicInvoke();
        }

        private bool TryPrecompileMethodToday()
        {
            if (LeftChild.Token.Text.Equals("$today", StringComparison.OrdinalIgnoreCase))
            {
                DateTimeOffset? result;
                decimal? timezone = null;
                if (Children.Count == 2)
                {
                    Node param = Children[1];
                    try
                    {
                        Expression exp = param.GernerateExpression(null);
                        timezone = (decimal?)TryToExecuteExpression(exp);
                    }
                    catch
                    {
                        throw ThrowHelper.InvalidPrecompileMethodArguments(LeftChild.Token);
                    }
                    result = IntrinsicFunctions.Today(timezone);
                }
                else
                {
                    result = IntrinsicFunctions.Today();
                }
                //固化：使用date方法代替                
                Children.Clear();
                Children.Add(new Node(new Token(TokenType.MethodCall, "date", -1)));
                Children.Add(new ConstantNode(new Token(TokenType.ConstantString, result.Value.ToString("yyyy-MM-dd HH:mm:ss"), -1)));
                if (timezone.HasValue)
                {
                    Children.Add(new ConstantNode(new Token(TokenType.ConstantNumber, timezone.ToString(), -1)));
                }
                return true;
            }
            return false;
        }

        private bool TryPrecompileMethodNow()
        {
            if (LeftChild.Token.Text.Equals("$now", StringComparison.OrdinalIgnoreCase))
            {
                DateTimeOffset? result;
                decimal? timezone = null;
                if (Children.Count == 2)
                {
                    Node param = Children[1];
                    try
                    {
                        Expression exp = param.GernerateExpression(null);
                        timezone = (decimal?)TryToExecuteExpression(exp);
                    }
                    catch
                    {
                        throw ThrowHelper.InvalidPrecompileMethodArguments(LeftChild.Token);
                    }
                    result = IntrinsicFunctions.Now(timezone);
                }
                else
                {
                    result = IntrinsicFunctions.Now();
                }
                //固化：使用date方法代替                
                Children.Clear();
                Children.Add(new Node(new Token(TokenType.MethodStart, "date", -1)));
                Children.Add(new ConstantNode(new Token(TokenType.ConstantString, result.Value.ToString("yyyy-MM-dd HH:mm:ss"), -1)));
                if (timezone.HasValue)
                {
                    Children.Add(new ConstantNode(new Token(TokenType.ConstantNumber, timezone.ToString(), -1)));
                }
                return true;
            }
            return false;
        }
    }
}
