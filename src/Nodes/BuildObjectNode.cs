using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Orel.Expressions;
using Orel.Schema;

namespace Orel.Nodes
{
    internal class BuildObjectNode : Node, IRuntimeMemberGenerator
    {
        public IMemberDescriptor RuntimeMembers { get ; set ; }        

        public BuildObjectNode(Token token) : base(token)
        {
            RuntimeMembers = new InternalMemberDescriptor();
        }

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            var childrenExps = GetChildrenExpressions(context);
            var members = new List<(string, Expression)>();
            //parameter.TempMemberDescriptor.Clear();//重置TempMemberDescriptor
            //context.TempMemberDescriptor.Clear();
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                string memberName = GetMemberName(child, i);
                members.Add((memberName, childrenExps[i]));                
                //创建这些Member的Description
                if (context != null)
                {
                    var memberDef = new MemberDefinition(memberName, childrenExps[i].ExpectedType, context.TempPrefix);
                    //context.TempMemberDescriptor.Add(memberDef);
                    RuntimeMembers.Add(memberDef);
                }
            }
            var objectExp = ExpressionHelper.MakeORELObjectExpression(members);
            return objectExp.Wrap(typeof(ORELObject), Token);
        }

        private string GetMemberName(Node child, int index)
        {
            switch (child.Token.Type)
            {
                case TokenType.Colon:
                    return child.LeftChild.Token.Text;
                case TokenType.MemberAccess:
                    return child.Token.Text;
                case TokenType.Dot:
                    return child.RightChild.Token.Text;
                case TokenType.ArrayIndexStart:
                    var arrayNode = child.LeftChild;
                    if (arrayNode.Token.Type == TokenType.Dot)
                    {
                        return arrayNode.RightChild.Token.Text;
                    }
                    return arrayNode.Token.Text;
                case TokenType.Parameter:
                    return child.Token.Value.ToString();
                default:
                    return $"_{index + 1}";
            }
        }

        public override string ToString()
        {
            return $"{{{String.Join(',', Children)}}}";
        }
    }
}
