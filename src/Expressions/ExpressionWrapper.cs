using Orel.Schema;
using System;
using System.Linq.Expressions;

namespace Orel.Expressions
{
    /// <summary>
    /// 表达式包装器
    /// </summary>
    public class ExpressionWrapper
    {
        internal Expression Expression { get; set; }
        internal Expression OriginExpression { get; private set; }
        internal IMemberDefinition MemberDefinition { get; private set; }
        internal Token Token { get; private set; }
        internal DataType ExpectedType { get; private set; }
        internal Type ItemType { get; set; }
        internal Type ExplicitType { get; set; }
        internal bool IsParameter { get { return Token.Type == TokenType.Parameter; } }

        internal ExpressionWrapper(Expression expression, IMemberDefinition memberDefinition, Token token)
        {
            Expression = expression;
            OriginExpression = expression;
            Token = token;
            if (memberDefinition != null)
            {
                MemberDefinition = memberDefinition;
                ExpectedType = memberDefinition.DataType;
            }
            else
            {
                ExpectedType = expression.Type.GetORELDataType();
            }
        }

        internal ExpressionWrapper(Expression expression, DataType expectedType, Token token)
        {
            Expression = expression;
            OriginExpression = expression;
            Token = token;
            ExpectedType = expectedType;
        }

        public static implicit operator Expression(ExpressionWrapper wrapper)
        {
            return wrapper.Expression;
        }

        public Type Type => ExplicitType ?? Expression.Type;

        internal void AlignExpressionType()
        {
            AlignExpressionType(ExpectedType);
        }

        /// <summary>
        /// 强制转换为指定类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal void AlignToType<T>()
        {
            Expression = Expression.AsType<T>();
        }

        /// <summary>
        /// 强制转换为指定类型
        /// </summary>
        /// <param name="type"></param>
        internal void AlignToType(Type type)
        {
            if (Expression.Type == type)
                return;
            if (type.IsString())
            {
                Expression = ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "ToText", Expression);
            }
            else if (type.IsList())
            {
                Expression = Expression.AsType(type);
            }
            else
            {
                Expression = ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.ConvertClassType), new[] { type }, Expression);
            }
        }

        internal void AlignExpressionType(DataType targetType)
        {
            if (Type == typeof(void))
            {
                return;
            }
            switch (targetType)
            {
                case DataType.Text:
                    if (Type.IsString()) return;
                    Expression = ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "ToText", Expression);
                    break;
                case DataType.Number:
                    if (Type == typeof(decimal?)) return;
                    if (Type.IsNumeric())
                    {
                        Expression = Expression.CastType<decimal?>();
                        return;
                    }
                    Expression = ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "ToNumExt", Expression);
                    break;
                case DataType.DateTime:
                    if (Type == typeof(DateTimeOffset?)) return;
                    if (Type.IsDateTime())
                    {
                        Expression = Expression.CastType<DateTimeOffset?>();
                        return;
                    }
                    Expression = ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "ToDateExt", Expression);
                    break;
                case DataType.List:
                    if (Type.IsList()) return;
                    Expression = ExpressionHelper.AsList(Expression);
                    break;
                case DataType.Object:
                    if (Type == typeof(object)) return;
                    Expression = ExpressionHelper.AsObject(Expression);
                    break;
                case DataType.Boolean:
                    if (Type == typeof(bool?)) return;
                    if (Type.IsBoolean())
                    {
                        Expression = Expression.CastType<bool?>();
                        return;
                    }
                    Expression = ExpressionHelper.GetMethodCallExpression(typeof(IntrinsicFunctions), "ToBool", Expression);
                    break;
                default:
                    throw new ArgumentException("targetType");
            }
        }

        internal void SetExpectType(DataType dataType)
        {
            if (Expression is ParameterExpression)
            {
                this.ExpectedType = dataType;
            }
        }
    }

}
