using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class ParameterNode : Node
    {
        public string ParameterName { get; private set; }
        public ParameterNode(Token token) : base(token)
        {
            ParameterName = token.Value.ToString();
        }

        internal void SetParameter(ParameterExpression expression)
        {
            NodeExpression = expression.Wrap(Token);
        }

        internal void SetUndecided()
        {
            NodeExpression = new UndecidedParameterExpression().Wrap(Token);
        }

        internal bool Undecided
        {
            get
            {
                return NodeExpression == null || NodeExpression.Expression is UndecidedParameterExpression;
            }
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            if (context.ParameterManager.Parameters.TryGetValue(this.ParameterName, out var parameterDefinition))
            {
                if (parameterDefinition.Nodes == null)
                {
                    parameterDefinition.Nodes = new List<ParameterNode>();
                }
                parameterDefinition.Nodes.Add(this);
                return parameterDefinition.Expression.Wrap(Token);
            }
            else
            {
                return context.ParameterManager.AddParameter(this, typeof(object)).Wrap(Token);
            }
            //throw ThrowHelper.InvalidParameterOperation(Parent.Token, this.Token);
            //var paramName = Token.Value.ToString();
            //var type = GetExpectType();
            //if (context.Parameters.TryGetValue(paramName, out var existence))
            //{
            //    //相同参数存在的情况下，object类型为低优先级类型，会被其他的类型定义所覆盖
            //    if (existence.Type == type || type == typeof(object))
            //    {
            //        return existence.Expression.Wrap(Token);
            //    }
            //    else if (existence.Type == typeof(object))
            //    {
            //        context.Parameters[paramName] = Expression.Parameter(type, paramName).Wrap(Token);
            //    }
            //    throw ThrowHelper.ConflictParameterType(Token, existence.Token);
            //}
            //var parameter = Expression.Parameter(type, paramName).Wrap(Token);
            //context.Parameters.Add(paramName, parameter);
            //return parameter;
            //return null;
        }

        private void GetExpectType(BuildContext context)
        {
            var siblings = GetSiblings(this);
            switch (Parent)
            {
                case AddNode node:
                    //get another parameter
                    var sibling = siblings.First();
                    if (sibling.TryCast<ParameterNode>(out var pNode))
                    {
                        if (pNode.Undecided)
                        {
                            context.ParameterManager.SetUndecided(this);
                            return;
                        }
                    }
                    switch (sibling.NodeExpression.ExpectedType)
                    {
                        case DataType.Text:
                            context.ParameterManager.AddParameter(this, typeof(string));
                            //return typeof(string);
                            break;
                        case DataType.Number:
                            context.ParameterManager.AddParameter(this, typeof(decimal?));
                            //return typeof(decimal?);
                            break;
                        case DataType.DateTime:
                            context.ParameterManager.AddParameter(this, typeof(DateTimeOffset?));
                            //return typeof(DateTimeOffset?);
                            break;
                        default:
                            throw ThrowHelper.InvalidParameterOperation(node.Token, this.Token);
                    }
                    break;
                case SubstractNode node:
                    sibling = siblings.First();
                    if (sibling.TryCast<ParameterNode>(out pNode))
                    {
                        if (pNode.Undecided)
                        {
                            context.ParameterManager.SetUndecided(this);
                            return;
                        }
                    }
                    if (sibling.NodeExpression.ExpectedType == DataType.Number)
                    {
                        //return typeof(decimal?);
                        context.ParameterManager.AddParameter(this, typeof(decimal?));
                    }
                    if (sibling.NodeExpression.ExpectedType == DataType.DateTime && sibling == Parent.LeftChild)
                    {
                        context.ParameterManager.AddParameter(this, typeof(string));
                        //return typeof(string);
                    }
                    throw ThrowHelper.InvalidParameterOperation(node.Token, this.Token);
                case MultDivNode node:
                    //只支持数值类型
                    context.ParameterManager.AddParameter(this, typeof(decimal?));
                    break;
                //return typeof(decimal?);
                case LikeNode node:
                    context.ParameterManager.AddParameter(this, typeof(string));
                    break;
                //return typeof(string);
                case BetweenNode node:
                    //0,2,3的位置是有效参数节点
                    var otherParams = siblings.Where(s => !(s is ParameterNode) && !s.NodeExpression.Type.IsBoolean());
                    var value = node.Children[0];
                    var lowBound = node.Children[2];
                    var highBound = node.Children[3];
                    var currentParams = new[] { value, lowBound, highBound };
                    if (currentParams.All(v => v is ParameterNode)) //三者都是参数节点的情况无效
                    {
                        throw ThrowHelper.InvalidParameterOperation(node.Token, value.Token, lowBound.Token, highBound.Token);
                    }
                    var another = currentParams.First(v => !(v is ParameterNode));
                    //switch (another.NodeExpression.ExpectedType)
                    //{
                    //    case DataType.Text:
                    //        return typeof(string);
                    //    case DataType.Number:
                    //        return typeof(decimal?);
                    //    case DataType.DateTime:
                    //        return typeof(DateTimeOffset?);
                    //    default:
                    //        throw ThrowHelper.InvalidParameterOperation(node.Token, this.Token);
                    //}
                    break;
                case MethodNode node:
                    var arguments = node.Children.Skip(1).Where(p => p != this).ToArray();
                    var methods = ExpressionHelper.GetMethods(typeof(IntrinsicFunctions), node.MethodName);
                    var candidates = new List<(MethodInfo, List<(ParameterNode, Type)>)>();
                    foreach (var method in methods)
                    {
                        var @params = method.GetParameters();
                        if (@params.Length != arguments.Length)
                            continue;
                        var match = true;
                        for (int i = 0; i < @params.Length; i++)
                        {
                            var argNode = arguments[i];
                            var param = @params[i];
                            if (!(argNode is ParameterNode))
                            {
                                if (!ExpressionHelper.CompatibleTypes(param.ParameterType, argNode.NodeExpression.Type))
                                {
                                    match = false;
                                    break;
                                }
                            }
                        }
                        if (match)
                        {
                            // candidates.Add(method);
                        }
                    }
                    //在候选方法数多于1的情况下，parameter的优先级:
                    //1.如果上下文中已定义，则用已定义的类型（如果已定义类型为Object，则存在覆盖）
                    //2.如果未定义，则倾向于更具体的类型
                    if (candidates.Count > 1)
                    {

                    }
                    if (!candidates.Any())
                    {
                        throw ThrowHelper.InvalidMethodCall(node.MethodName, arguments.Select(arg => arg.Token.Text));
                    }
                    break;
                case ColonNode node:
                    context.ParameterManager.AddParameter(this, typeof(object));
                    break;
                //return typeof(object);
                case BuildObjectNode node:
                    //return typeof(object);
                    context.ParameterManager.AddParameter(this, typeof(object));
                    break;
                default:
                    context.ParameterManager.AddParameter(this, typeof(object));
                    break;
            }
            //return typeof(object);
        }
    }
}
