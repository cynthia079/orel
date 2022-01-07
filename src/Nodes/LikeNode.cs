using System;
using System.Collections.Generic;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class LikeNode : Node
    {
        public LikeNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var (left, right) = GetBinaryChildrenExpressions(context);
            return ExpressionHelper.CreateMethodCallExpression(typeof(IntrinsicFunctions), "Like", left, right).Wrap(Token);
        }        
    }
}
