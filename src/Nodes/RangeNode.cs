using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class RangeNode : Node
    {
        public RangeNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            ExpressionWrapper[] fromto = GetChildrenExpressions(context);
            if (fromto.Any(e => !e.Type.IsNumeric()))
            {
                throw ThrowHelper.InvalidOperand(Token, LeftChild.Token, RightChild.Token);
            }
            Expression from = fromto[0].Expression;
            Expression to = fromto.Length > 1 ? fromto[1].Expression : Expression.Constant(null, typeof(decimal?));
            return new RangeExpression(from, to).Wrap(Token);
        }
    }
}
