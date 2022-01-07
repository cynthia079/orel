using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Orel.Expressions;

namespace Orel.Nodes
{
    internal class MemberAccessNode : Node
    {
        public MemberAccessNode(Token token) : base(token) { }

        private static Regex indexAccessPattern = new Regex(@"\$(\d+)", RegexOptions.Compiled);

        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            if (Token.Text.Equals("$i"))   //内置参数$i获取循环变量index
            {
                var indexParam = Expression.Parameter(typeof(decimal?), "index");
                context.IndexExpression = indexParam;
                return indexParam.Wrap(Token);
            }
            var match = indexAccessPattern.Match(Token.Text);
            if (match.Success)
            {
                var index = decimal.Parse(match.Groups[1].Value);
                var exp = ExpressionHelper.CreateMethodCallExpression(typeof(IntrinsicFunctions), nameof(IntrinsicFunctions.ListIndex), context.CurrentObject.Wrap(), Expression.Constant(index).Wrap());
                return exp.Wrap(Token);
            }
            var lookupResult = context.LookupMemberDefinition(Token.Text);
            var memberDef = lookupResult.member;
            if (memberDef == null)
                throw ThrowHelper.InvalidMemberName(Token.Text);
            Expression objExp = null;
            context = lookupResult.context;
            if (!string.IsNullOrEmpty(memberDef.Scope) && string.IsNullOrEmpty(context.MemberPrefix))
            {
                objExp = ExpressionHelper.MakeMemberAccessExpression(context.CurrentObject, memberDef.Parent.MemberName, memberDef.Parent.ContextType, memberDef.Parent.Type);
            }
            else
            {
                objExp = context.CurrentObject;
            }
            Expression memberExp = ExpressionHelper.MakeMemberAccessExpression(objExp, memberDef.ActualName, memberDef.ContextType, memberDef.Type);
            if (Parent == null) //当前动态节点为根节点，或数据类型为数组时，自动做类型转换
            {
                ExpressionWrapper wrapper = new ExpressionWrapper(memberExp, memberDef, Token);
                wrapper.AlignExpressionType();
                return wrapper;
            }
            else if (memberDef.DataType == DataType.List)
            {
                ExpressionWrapper wrapper = new ExpressionWrapper(memberExp, memberDef, Token);
                var itemMember = context.FindListItemDefinition(memberDef);
                if (itemMember != null)
                {
                    wrapper.ItemType = itemMember.DataType.GetRuntimeType();
                }
                //wrapper.AlignExpressionType();
                return wrapper;
            }
            return memberExp.Wrap(memberDef, Token);
        }

        internal override string GetObjectMemberPath()
        {
            return Token.Text;
        }

        public override string ToString()
        {
            return Token.Text.CapsulateAsMemberName();
        }
    }
}
