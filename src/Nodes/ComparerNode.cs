using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class ComparerNode : Node
    {
        public Func<Expression, Expression, BinaryExpression> Comparer { get; private set; }
        public ComparerNode(Token token) : base(token)
        {
            switch (Token.Type)
            {
                case TokenType.Equal:
                    Comparer = Expression.Equal;
                    break;
                case TokenType.NotEqual:
                    Comparer = Expression.NotEqual;
                    break;
                case TokenType.GreaterThan:
                    Comparer = Expression.GreaterThan;
                    break;
                case TokenType.GreaterThanOrEqual:
                    Comparer = Expression.GreaterThanOrEqual;
                    break;
                case TokenType.LessThan:
                    Comparer = Expression.LessThan;
                    break;
                case TokenType.LessThanOrEqual:
                    Comparer = Expression.LessThanOrEqual;
                    break;
                default:
                    throw ThrowHelper.UnSupportedSyntax(Token);
            }
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            if(Token.Type == TokenType.Equal)
            {
                if (LeftChild.Token.Type == TokenType.ConstantNull)
                {
                    return ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.IsNull), RightChild.GernerateExpression(context)).Wrap(Token);
                }
                else if (RightChild.Token.Type == TokenType.ConstantNull)
                {
                    return ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.IsNull), LeftChild.GernerateExpression(context)).Wrap(Token);
                }
            }
            return CreateGeneralBinaryExpression(context, Comparer).Wrap(Token);
        }
    }
}
