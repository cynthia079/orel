using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Orel.Expressions;
using Orel.Schema;

namespace Orel.Nodes
{
    internal class DotNode : Node
    {
        public DotNode(Token token) : base(token) { }
        private static Regex indexAccessPattern = new Regex(@"\$(\d+)", RegexOptions.Compiled);
        public override ExpressionWrapper GernerateExpression(BuildContext context)
        {
            Expression objExp = LeftChild.GernerateExpression(context);
            string objPath = null;
            IMemberDefinition rightMember = null;
            if (LeftChild is DefaultArgumentNode)
            {
                objPath = $"{RightChild.Token.Text}";
            }
            else if (LeftChild is IRuntimeMemberGenerator schemaOutputter && schemaOutputter.RuntimeMembers != null)
            {
                var memberDescriptor = schemaOutputter.RuntimeMembers;
                objPath = $"{RightChild.Token.Text}";
                rightMember = memberDescriptor.Get(objPath, string.Empty);
            }
            else
            {
                var leftPath = LeftChild.GetObjectMemberPath();
                objPath = $"{leftPath}.{RightChild.Token.Text}";
                if (indexAccessPattern.IsMatch(leftPath))
                {
                    var listItemMember = context.FindListItemDefinition(null);
                    rightMember = context.FindMemberDefinition(RightChild.Token.Text, listItemMember?.UniqueName);
                }
                else
                {
                    var lookupResult = context.LookupMemberDefinition(leftPath);
                    var leftMember = lookupResult.member;
                    if (leftMember == null)
                    {
                        throw ThrowHelper.InvalidMemberName(objPath);
                    }
                    if (context != lookupResult.context)
                    {
                        context = lookupResult.context;
                        objExp = LeftChild.GernerateExpression(context);
                    }
                }
            }

            if (rightMember == null)
            {
                rightMember = context.FindMemberDefinition(objPath);
            }
            if (rightMember == null)
            {
                throw ThrowHelper.InvalidMemberName(objPath);
            }

            if (objExp.Type.IsList()) //数组类型，需要将参数转换成数组类型
            {
                var wrapper = objExp.Wrap(LeftChild.Token); //new ExpressionWrapper(objExp, LeftChild.Token);
                wrapper.AlignExpressionType(DataType.List);
                //将字段投射为一个新的数组，如果右侧类型也为数组类型，则执行SelectMany操作
                Expression selected = null;
                Type itemType = null;
                if (rightMember.DataType == DataType.List)
                {
                    var defaultMember = context.FindMemberDefinition("_", rightMember.UniqueName);
                    //itemType = defaultMember?.Type != null ? typeof(IList<>).MakeGenericType(defaultMember.Type) : typeof(object);
                    //itemType = typeof(object);
                    itemType = defaultMember?.Type ?? typeof(object);
                    selected = ExpressionHelper.SelectMany(wrapper.Expression, rightMember.ActualName, rightMember.ContextType, itemType);
                }
                else
                {
                    itemType = rightMember.Type;
                    selected = ExpressionHelper.Select(wrapper.Expression, rightMember.ActualName, rightMember.ContextType, itemType);
                }
                wrapper = selected.Wrap(RightChild.Token);
                wrapper.ItemType = itemType;
                return wrapper;
            }
            else
            {
                Expression tempExp = ExpressionHelper.MakeMemberAccessExpression(objExp, rightMember.ActualName, rightMember.ContextType, rightMember.Type);
                if (Parent == null || rightMember.DataType == DataType.List)
                {
                    var wrapper = new ExpressionWrapper(tempExp, rightMember, RightChild.Token);
                    wrapper.AlignExpressionType();
                    return wrapper;
                }
                return tempExp.Wrap(rightMember, RightChild.Token);
            }
        }

        internal override string GetObjectMemberPath()
        {
            return $"{LeftChild.GetObjectMemberPath()}.{RightChild.GetObjectMemberPath()}";
        }

        public override string ToString()
        {
            return $"{LeftChild}.{RightChild}";
        }
    }
}
