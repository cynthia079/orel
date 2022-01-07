using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class EmptyNode : Node
    {
        private EmptyNode(Token token) : base(token)
        {
        }

        public EmptyNode() : this(Token.Empty) { }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            return Expression.Empty().Wrap(Token); ;
        }
    }
}
