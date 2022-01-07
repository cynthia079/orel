using System;
using System.Collections.Generic;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class ColonNode : Node
    {
        public ColonNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var rightExp = RightChild.GernerateExpression(context);
            return rightExp;
        }        
    }
}
