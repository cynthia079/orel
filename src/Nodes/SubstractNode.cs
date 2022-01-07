using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class SubstractNode : Node
    {
        public SubstractNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var (left, right) = GetBinaryChildrenExpressions(context);
            return CreateSubstractExpression(left, right).Wrap(Token);
        }

        /// <summary>
        /// 构建操作符-表达式
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Expression CreateSubstractExpression(ExpressionWrapper left, ExpressionWrapper right)
        {
            left.AlignExpressionType();
            right.AlignExpressionType();
            if (left.ExpectedType == DataType.Number && right.ExpectedType == DataType.Number)
            {
                return Expression.Subtract(left, right);
            }
            else if (left.Type == typeof(void) && right.ExpectedType == DataType.Number)
            {
                left.Expression = Expression.Constant(0m, typeof(decimal?));
                return Expression.Subtract(left, right);
            }
            else if (left.ExpectedType == DataType.DateTime && right.ExpectedType == DataType.Text)
            {
                return ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "DateAdd", left, right, Expression.Constant(true));
            }
            throw ThrowHelper.InvalidOperand(Token, left.Token, right.Token);
        }
    }
}
