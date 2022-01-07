using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class AddNode : Node
    {
        public AddNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var (left, right) = GetBinaryChildrenExpressions(context);
            return CreateAddExpression(left, right).Wrap(Token);
        }

        /// <summary>
        /// 构建操作符+表达式
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        private Expression CreateAddExpression(ExpressionWrapper left, ExpressionWrapper right)
        {
            if (left.Type.IsString() && right.Type.IsString())
            {
                return ExpressionHelper.GetMethodCallExpression(typeof(string), "Concat", left, right);
            }
            if (left.Type.IsNumeric() && right.Type.IsNumeric())
            {
                return Expression.Add(left, right);
            }
            if (left.ExpectedType == DataType.Number && right.ExpectedType == DataType.Number)
            {
                left.AlignExpressionType(DataType.Number);
                right.AlignExpressionType(DataType.Number);
                return Expression.Add(left, right);
            }
            if (left.ExpectedType == DataType.DateTime && right.ExpectedType == DataType.Text)
            {
                left.AlignExpressionType(DataType.DateTime);
                right.AlignExpressionType(DataType.Text);
                return ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "DateAdd", left, right, Expression.Constant(false));
            }
            if (left.ExpectedType == DataType.Text && right.ExpectedType == DataType.DateTime)
            {
                left.AlignExpressionType(DataType.Text);
                right.AlignExpressionType(DataType.DateTime);
                return ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "DateAdd", right, left, Expression.Constant(false));
            }
            if (left.ExpectedType == DataType.Text || right.ExpectedType == DataType.Text)
            {
                left.AlignExpressionType(DataType.Text);
                right.AlignExpressionType(DataType.Text);
                return ExpressionHelper.GetMethodCallExpression(typeof(string), "Concat", left, right);
            }
            if (left.Type.IsList() && right.Type.IsList())
            {
                return ExpressionHelper.MakeListConcat(left, right);
            }
            throw ThrowHelper.InvalidOperand(Token, left.Token, right.Token);
        }
    }
}
