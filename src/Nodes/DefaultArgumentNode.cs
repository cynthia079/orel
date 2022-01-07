using System;
using System.Collections.Generic;
using System.Text;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class DefaultArgumentNode : Node
    {
        public DefaultArgumentNode(Token token) : base(token)
        {
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            if (!string.IsNullOrEmpty(context.MemberPrefix))
            {
                var memberDefinition = context.FindMemberDefinition(TokenDefinitions.DefaultArgument);
                if (memberDefinition == null)
                {
                    memberDefinition = context.FindMemberDefinition(context.MemberPrefix, string.Empty);
                    if (memberDefinition == null)
                    {
                        throw ThrowHelper.InvalidMemberName(context.MemberPrefix);
                    }
                }
                return context.CurrentObject.Wrap(memberDefinition, Token);
            }
            return context.CurrentObject.Wrap(Token);
        }
    }
}
