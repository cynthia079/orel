using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class ConstantNode : Node
    {
        public Expression Expression { get; private set; }

        public ConstantNode(Token token) : base(token)
        {
            SetExpression();
        }

        private void SetExpression()
        {
            switch (Token.Type)
            {
                case TokenType.ConstantNumber:
                    Expression = Expression.Constant(Token.Value, typeof(decimal?));
                    break;
                case TokenType.ConstantString:
                    Expression = Expression.Constant(Token.Text, typeof(string));                                    
                    break;
                case TokenType.ConstantNull:
                    Expression = Expression.Constant(null).Wrap(Token);
                    break;
                default:
                    throw ThrowHelper.UnSupportedSyntax(Token);
            }
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            return Expression.Wrap(Token);
        }

        public override string ToString()
        {
            if (Token.Type == TokenType.ConstantString)
            {
                return $"'{Token.ToString().Replace("'", "\\'")}'";  //处理含有'的数据时，需要做转义
            }
            else
            {
                return base.ToString();
            }
        }
    }
}
