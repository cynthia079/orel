using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;
using Orel.Schema;

namespace Orel.Nodes
{
    internal class YieldNode : Node, IRuntimeMemberGenerator
    {
        public IMemberDescriptor RuntimeMembers { get; set; }
        public YieldNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var leftExp = LeftChild.GernerateExpression(context);
            if (!leftExp.Type.IsList())
            {
                throw ThrowHelper.InvalidOperator(Token);
            }

            var listMember = context.FindMemberDefinition(TokenDefinitions.DefaultArgument, context.MemberPrefix);
            var targetType = leftExp.ItemType ?? listMember?.DataType.GetRuntimeType();
            //左子树也是yeild节点时, 传递该节点的OutputSchema，形成yield调用链
            var memberDescriptor = (LeftChild as IRuntimeMemberGenerator)?.RuntimeMembers;

            var itemContext = context.GenerateChildContext(targetType, LeftChild.GetObjectMemberPath(), memberDescriptor);
            var rightExp = RightChild.GernerateExpression(itemContext);
            ExpressionWrapper transformer = ExpressionHelper.TransformTo(leftExp, rightExp, itemContext.CurrentObject, itemContext.IndexExpression).Wrap(Token);
            transformer.ItemType = rightExp.Type;
            if (RightChild is IRuntimeMemberGenerator outputterNode)
            {
                RuntimeMembers = outputterNode.RuntimeMembers;
            }
            return transformer;
        }
    }
}
