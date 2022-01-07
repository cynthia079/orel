using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;
using Orel.Schema;

namespace Orel.Nodes
{
    internal class ComposeNode : Node, IRuntimeMemberGenerator
    {
        public ComposeNode(Token token) : base(token) { }

        public IMemberDescriptor RuntimeMembers { get; set; }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var leftExp = LeftChild.GernerateExpression(context);
            if (!leftExp.Type.IsList())
            {
                throw ThrowHelper.InvalidOperand(Token, new[] { LeftChild.ToString() }, "需要List类型");
            }
            var itemContext = context.GenerateChildContext(leftExp.ItemType, LeftChild.GetObjectMemberPath());
            var rightExp = RightChild.GernerateExpression(context);
            //缓存rightExp获取的值
            var lamda = Expression.Lambda(rightExp, context.CurrentObject);
            var param = Expression.Parameter(typeof(ORELObject), "composing");
            var assign = Expression.Assign(param, Expression.Invoke(lamda, context.CurrentObject).AsType<ORELObject>());
            Expression compose;
            if (leftExp.ItemType == typeof(ORELObject)) //已经是ORELObject的情况下，直接织入
            {
                compose = Expression.Call(itemContext.CurrentObject, ORELObject.ComposeMethodInfo, param);
            }
            else
            {
                compose = Expression.New(ORELObject.ConstructorInfo, itemContext.CurrentObject, param);
            }
            var yields = ExpressionHelper.TransformTo(leftExp, compose, itemContext.CurrentObject, itemContext.IndexExpression);
            var composed = Expression.Block(new[] { param }, assign, yields).Wrap(Token);
            composed.ItemType = typeof(ORELObject);

            RuntimeMembers = new InternalMemberDescriptor();
            if (RightChild is IRuntimeMemberGenerator outputter && outputter.RuntimeMembers != null)
            {
                foreach (var m in outputter.RuntimeMembers)
                    RuntimeMembers.Add(m);
            }
            return composed;
        }

        internal override string GetObjectMemberPath()
        {
            return LeftChild.GetObjectMemberPath();
        }
    }
}
