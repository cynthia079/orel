using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Orel.Expressions;

namespace Orel.Nodes
{
    /// <summary>
    /// 转换节点
    /// </summary>
    internal class Node
    {
        public Token Token { get; private set; }
        public List<Node> Children { get; private set; }
        public Node Parent { get; set; }
        public Node LeftChild
        {
            get
            {
                if (Children.Count == 0)
                {
                    return null;
                }

                return Children[0];
            }
        }
        public Node RightChild
        {
            get
            {
                if (Children.Count < 2)
                {
                    return null;
                }

                return Children[1];
            }
        }

        public bool IsBinary => Token.IsComparer || Token.IsLogic || Token.IsOperator
            || Token.Type == TokenType.Comma || Token.Type == TokenType.Range
            || Token.Type == TokenType.Colon || Token.Type == TokenType.Yield
            || Token.Type == TokenType.Reduce || Token.Type == TokenType.Export;

        /// <summary>
        /// 表示该节点的结果是否是不可变的
        /// </summary>
        internal bool Immutable { get; set; } = false;

        public Node(Token token)
        {
            Token = token;
            Children = new List<Node>();
        }

        public Node GetRoot()
        {
            Node node = this;
            while (node.Parent != null)
            {
                node = node.Parent;
            }
            return node;
        }

        public IEnumerable<Node> GetSiblings(Node node)
        {
            return Children.Where(c => c != node);
        }

        public override string ToString()
        {
            if (Children.Count == 0)
            {
                return Token.ToString();
            }
            else if (Immutable)
            {
                return $"({LeftChild} {Token} {RightChild})";
            }
            else
            {
                return $"{LeftChild} {Token} {RightChild}";
            }
        }

        public static EmptyNode Empty { get; private set; } = new EmptyNode();

        public bool CanbeArrayIndexed
        {
            get
            {
                return (this is MemberAccessNode)
                    || (this is ArrayNode)
                    || (this is DotNode)
                    || (this is MethodNode)
                    || (this is ComposeNode)
                    || (this is YieldNode)
                    || (this is DefaultArgumentNode);
            }
        }

        public Node SetBinaryLeftEmpty()
        {
            Node node = Empty;
            return SetBinaryLeft(node);
        }

        /// <summary>
        /// 设置左子节点，返回被替换的子节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual Node SetBinaryLeft(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            Node curLeft = LeftChild;
            if (curLeft == null)
            {
                Children.Add(node);
            }
            else
            {
                Children[0] = node;
            }
            node.Parent = this;
            return curLeft;
        }

        /// <summary>
        /// 设置右子节点，返回被替换的子节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual Node SetBinaryRight(Node node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            Node curRight = RightChild;
            if (curRight == null)
            {
                Children.Add(node);
            }
            else
            {
                Children[1] = node;
            }
            node.Parent = this;
            return curRight;
        }

        public void AddChild(Node node)
        {
            Children.Add(node);
            node.Parent = this;
        }

        public void Precompile()
        {
            foreach (var c in Children)
            {
                c.Precompile();
            }
            this.TryPrecompile();
        }

        protected virtual void TryPrecompile()
        {
            return;
        }

        public ExpressionWrapper NodeExpression { get; protected set; }

        public virtual ExpressionWrapper GernerateExpression(BuildContext context)
        {
            //throw new NotImplementedException();
            return null;
        }

        #region temp code
        public void Generate(BuildContext context)
        {
            GenerateChildNonParameterExpressions(context);
            GenerateChildParameterExpressions(context);
            NodeExpression = GernerateExpression(context);
        }

        public void GenerateChildNonParameterExpressions(BuildContext context)
        {
            foreach (var child in Children)
            {
                if (child.Token.Type != TokenType.Parameter)
                {
                    child.Generate(context);
                }
            }
        }

        public void GenerateChildParameterExpressions(BuildContext context)
        {
            foreach (var child in Children)
            {
                if (child.Token.Type == TokenType.Parameter)
                {
                    child.GernerateExpression(context);
                }
            }
        }
        #endregion

        protected ExpressionWrapper[] GetChildrenExpressions(BuildContext parameter, int start = 0)
        {
            ExpressionWrapper[] childrenExps = Children.Skip(start).Select(c =>
            {
                ExpressionWrapper exp = c.GernerateExpression(parameter);
                return exp;
            }).ToArray();
            return childrenExps;
        }

        protected (ExpressionWrapper left, ExpressionWrapper right) GetBinaryChildrenExpressions(BuildContext parameter)
        {
            ExpressionWrapper[] childrenExps = GetChildrenExpressions(parameter);
            ValidateBinary(childrenExps, out ExpressionWrapper left, out ExpressionWrapper right);
            return (left, right);
        }

        internal virtual string GetObjectMemberPath()
        {
            return String.Empty;
        }

        private void ValidateBinary(IList<ExpressionWrapper> parameters, out ExpressionWrapper left, out ExpressionWrapper right)
        {
            if (parameters.Count < 2)
            {
                throw ThrowHelper.InvalidOperator(Token);
            }
            left = parameters[0];
            right = parameters[1];
        }

        /// <summary>
        /// 构建通用二值表达式
        /// </summary>
        /// <param name="expressions"></param>
        /// <param name="creator"></param>
        /// <returns></returns>
        protected Expression CreateGeneralBinaryExpression(BuildContext parameter, Func<Expression, Expression, BinaryExpression> creator)
        {
            (ExpressionWrapper left, ExpressionWrapper right) = GetBinaryChildrenExpressions(parameter);
            AlignBinaryExpressions(ref left, ref right);
            return creator(left, right);
        }

        /// <summary>
        /// 双参数类型对齐
        /// 1、基础参数类型为字符串的情形，当参照类型为日期时，对齐为日期类型，当参照类型为数据成员时，对齐为字符串
        /// 2、基础参数类型为其它情况时，保留原来类型，数据成员按照数据成员本身类型处理
        /// 3、基础类型一个是bool，一个是nullable<bool>时，对齐到nullable<bool>处理
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private static void AlignBinaryExpressions(ref ExpressionWrapper left, ref ExpressionWrapper right)
        {
            if (!AlignTextExpression(ref right, ref left))
            {
                if (!AlignTextExpression(ref left, ref right))
                {
                    left.AlignExpressionType();
                    right.AlignExpressionType();
                    if (left.Type == typeof(bool) && right.Type == typeof(bool?))
                    {
                        left.AlignToType<bool?>();
                    }
                    else if (left.Type == typeof(bool?) && right.Type == typeof(bool))
                    {
                        right.AlignToType<bool?>();
                    }
                }
            }
        }

        /// <summary>
        /// 文本参数对齐      
        /// </summary>
        /// <param name="expBase"></param>
        /// <param name="expRef"></param>
        /// <returns></returns>
        private static bool AlignTextExpression(ref ExpressionWrapper expBase, ref ExpressionWrapper expRef)
        {
            if ((expBase.Type.IsString() || expBase.IsParameter) && !expRef.Type.IsString())
            {
                if (expRef.MemberDefinition?.DataType == DataType.DateTime || expRef.Type.IsDateTime())
                {
                    expBase.AlignExpressionType(DataType.DateTime);
                    expRef.AlignExpressionType(DataType.DateTime);
                    return true;
                }
                else if (!expBase.IsParameter)
                {
                    expRef.AlignExpressionType(DataType.Text);
                    return true;
                }
            }
            return false;
        }
    }
}
