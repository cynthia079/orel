using System;
using System.Linq.Expressions;

namespace Orel.Expressions
{
    internal class RangeExpression : Expression
    {
        internal Expression From { get; private set; }
        internal Expression To { get; private set; }

        internal RangeExpression(Expression from, Expression to)
        {
            From = from;
            To = to;
        }

        public override Type Type => typeof(void);
    }

    internal class UndecidedParameterExpression: Expression
    {
        internal UndecidedParameterExpression() : base()
        {            
        }
    }
}
