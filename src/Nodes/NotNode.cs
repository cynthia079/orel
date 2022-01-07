using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class NotNode : Node
    {
        public NotNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            ExpressionWrapper leftExp = Children[1].GernerateExpression(context);
            return Expression.Not(leftExp).Wrap(Token);
        }        
    }
}
