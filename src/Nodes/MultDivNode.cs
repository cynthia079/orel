using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class MultDivNode : Node
    {
        public Func<Expression, Expression, BinaryExpression> Operator { get; private set; }
        public MultDivNode(Token token) : base(token)
        {
            switch (Token.Type)
            {
                case TokenType.Multiply:
                    Operator = Expression.Multiply;
                    break;
                case TokenType.Divide:
                    Operator = Expression.Divide;
                    break;
                default:
                    throw ThrowHelper.UnSupportedSyntax(Token);
            }
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            return CreateGeneralBinaryExpression(context, Operator).Wrap(Token);
        }
    }
}
