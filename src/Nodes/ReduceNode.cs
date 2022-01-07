using Orel.Expressions;
using Orel.Schema;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Orel.Nodes
{
    internal class ReduceNode : Node, IRuntimeMemberGenerator
    {
        public IMemberDescriptor RuntimeMembers { get; set; }
        public ReduceNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var leftExp = LeftChild.GernerateExpression(context);
            if (!leftExp.Type.IsList())
            {
                throw ThrowHelper.InvalidOperator(Token);
            }
            var memberDescriptor = (LeftChild as IRuntimeMemberGenerator)?.RuntimeMembers;
            var listContext = context.GenerateChildContext(leftExp.Type, LeftChild.GetObjectMemberPath(), memberDescriptor);
            var rightExp = RightChild.GernerateExpression(listContext);
            var reducer = ExpressionHelper.ReduceTo(leftExp, rightExp, listContext.CurrentObject).Wrap(Token);
            if (RightChild is IRuntimeMemberGenerator outputterNode)
            {
                RuntimeMembers = outputterNode.RuntimeMembers;
            }
            return reducer;
        }

    }
}
