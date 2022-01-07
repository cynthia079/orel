using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Orel.Expressions;
using Orel.Schema;

namespace Orel.Nodes
{
    internal class ArrayNode : Node, IRuntimeMemberGenerator
    {
        public IMemberDescriptor RuntimeMembers { get; set; }

        public ArrayNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            if (LeftChild != Empty)
            {
                RuntimeMembers = null;
                return MakeArrayAccessExpression(context);
            }
            else
            {
                var childrenNodes = Children.Skip(1);
                var children = GetChildrenExpressions(context, 1);
                var expression = ExpressionHelper.MakeListInitiation(children).Wrap(Token);
                RuntimeMembers = new InternalMemberDescriptor();
                if (!childrenNodes.Any())
                {
                    return expression;
                }
                if (childrenNodes.All(c => c is ConstantNode))
                {
                    //暂时已第一个元素的类型为准，不考虑数组元素中有类型不同的情形
                    var itemType = children.First().Type;
                    var curMember = new MemberDefinition(TokenDefinitions.DefaultArgument, itemType.GetORELDataType(), itemType, typeof(IList), null);
                    RuntimeMembers.Add(curMember);
                }
                else
                {
                    if (childrenNodes.All(c => c is ArrayNode))
                    {
                        RuntimeMembers.RootType = RootType.List;
                    }
                    foreach (var child in childrenNodes)
                    {
                        if (child is IRuntimeMemberGenerator memberGenerator)
                        {
                            foreach (var m in memberGenerator.RuntimeMembers)
                            {
                                RuntimeMembers.Add(m);
                            }
                        }
                    }
                }
                return expression;
            }
        }

        private ExpressionWrapper MakeArrayAccessExpression(BuildContext context)
        {
            var leftExp = LeftChild.GernerateExpression(context);
            if (!leftExp.Type.IsList())
            {
                throw ThrowHelper.InvalidMemberUsage(Children[0].Token.Text);
            }
            Type itemType = leftExp.ItemType;
            IMemberDefinition itemMember = null;
            if (itemType == null)
            {
                itemMember = context.FindListItemDefinition(leftExp.MemberDefinition);
                itemType = itemMember?.DataType.GetRuntimeType();
            }

            if (LeftChild is IRuntimeMemberGenerator outputter)
            {
                RuntimeMembers = outputter.RuntimeMembers;
            }
            var itemParameter = context.GenerateChildContext(itemType, LeftChild.GetObjectMemberPath(), RuntimeMembers);
            ExpressionWrapper rightExp = RightChild.GernerateExpression(itemParameter);
            if (rightExp.Expression is RangeExpression)
            {
                RangeExpression range = rightExp.Expression as RangeExpression;
                var exp = ExpressionHelper.CreateMethodCallExpression(typeof(IntrinsicFunctions), "ListRange",
                    leftExp, range.From.Wrap(), range.To.Wrap()).Wrap(Token);
                exp.ItemType = itemType;
                return exp;
            }
            else if (rightExp.Type.IsNumeric())
            {
                var exp = ExpressionHelper.CreateMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.ListIndex), leftExp, rightExp);
                if (itemMember == null && leftExp.MemberDefinition != null)
                {
                    itemMember = context.FindListItemDefinition(leftExp.MemberDefinition);
                }
                var wrapped = exp.Wrap(itemMember, Token);
                if (itemType != null)
                {
                    wrapped.AlignToType(itemType);
                }
                return wrapped;
            }
            else if (rightExp.Type.IsBoolean())
            {
                var condtion = rightExp.Type == typeof(bool?) ? rightExp.Expression.CastType<bool>() : rightExp;
                return ExpressionHelper.Where(leftExp, condtion, itemParameter.CurrentObject).Wrap(Token);
            }
            else
            {
                throw ThrowHelper.InvalidOperand(Token, RightChild.Token);
            }
        }

        private ExpressionWrapper MakeArrayInitiationExpression(BuildContext context)
        {
            var children = GetChildrenExpressions(context, 1);
            return ExpressionHelper.MakeListInitiation(children).Wrap(Token);
        }

        internal override string GetObjectMemberPath()
        {
            return LeftChild.GetObjectMemberPath();
        }

        public override string ToString()
        {
            return $"{LeftChild}[{ string.Join(',', Children.Skip(1)) }]";
        }
    }
}
