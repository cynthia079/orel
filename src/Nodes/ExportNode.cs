using Orel.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orel.Nodes
{
    internal class ExportNode : Node
    {
        public ExportNode(Token token) : base(token) { }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            return RightChild.GernerateExpression(context);
        }
    }
}
