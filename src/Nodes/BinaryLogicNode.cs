using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class BinaryLogicNode : Node
    {
        public BinaryLogicNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            (ExpressionWrapper left, ExpressionWrapper right) = GetBinaryChildrenExpressions(context);
            left.AlignExpressionType(DataType.Boolean);
            right.AlignExpressionType(DataType.Boolean);
            switch (Token.Type)
            {
                case TokenType.And:
                    return Expression.And(left, right).Wrap(Token);
                case TokenType.Or:
                    return Expression.Or(left, right).Wrap(Token);
                default:
                    throw ThrowHelper.UnSupportedSyntax(Token);
            }
        }
    }
}
