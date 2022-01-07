using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class BetweenNode : Node
    {
        public BetweenNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            if (Children.Count != 5)
            {
                throw ThrowHelper.InvalidOperand(Token, Children.Select(c => c.Token).ToArray());
            }
            var childrenExps = GetChildrenExpressions(context);
            ExpressionWrapper valueExp = childrenExps[0];
            ExpressionWrapper lowBound = childrenExps[2];
            ExpressionWrapper highBound = childrenExps[3];
            ExpressionWrapper includeLowBound = childrenExps[1];
            ExpressionWrapper includeHighBound = childrenExps[4];

            valueExp.AlignExpressionType();
            DataType dataType = valueExp.ExpectedType;
            lowBound.AlignExpressionType(dataType);
            highBound.AlignExpressionType(dataType);

            return ExpressionHelper.GetMethodCallExpression(Token, valueExp, lowBound, highBound, includeLowBound, includeHighBound).Wrap(Token);
        }

        public override string ToString()
        {
            return $"{LeftChild} {Token} {Children[1]}{Children[2]},{Children[3]}{Children[4]}";
        }
    }
}
