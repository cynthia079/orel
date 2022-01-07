using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class ConstantBooleanNode : Node
    {
        public ConstantBooleanNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            return Expression.Constant(Token.Value, typeof(bool)).Wrap(Token);
        }
    }
}
