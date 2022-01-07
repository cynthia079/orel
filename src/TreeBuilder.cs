using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Orel.Nodes;

[assembly: InternalsVisibleTo("Orel.Test")]

namespace Orel
{
    internal class TreeBuilder
    {
        private Node _current = null;
        private BranchManager Branches = new BranchManager();

        public Expression GernerateTree(BuildContext accessor = null)
        {
            if (Branches.Main != Branches.Current)
            {
                throw new InvalidOperationException("Incompletion Expression");
            }

            Branches.Merge(_current);

            if (Branches.Root == null)
            {
                throw new InvalidOperationException("Empty Expression");
            }
            return Branches.Root.GernerateExpression(accessor);
        }

        public Node Root => Branches.Root;

        public string Precompile()
        {
            if (Branches.Main != Branches.Current)
            {
                throw new InvalidOperationException("Incompletion Expression");
            }
            Branches.Merge(_current);
            if (Branches.Root == null)
            {
                throw new InvalidOperationException("Empty Expression");
            }
            Branches.Root.Precompile();
            return Branches.Root.ToString();
        }

        public void AppendRange(IEnumerable<Token> tokens)
        {
            foreach (Token token in tokens)
            {
                Append(token);
            }
        }

        public void Append(Token token)
        {
            //Node node = new Node(token);
            var node = NodeFactory.CreateFrom(token);
            switch (token.Type)
            {
                #region Operand 
                case TokenType.Parameter:
                case TokenType.MemberAccess:
                case TokenType.DefaultArgument:
                    if (_current == null)
                    {
                        _current = node;
                    }
                    else if (_current.Token.Type == TokenType.Dot)
                    {
                        if (token.Type == TokenType.Parameter)
                        {
                            throw ThrowHelper.InvalidOperator(_current.Token);
                        }
                        _current.SetBinaryRight(node);
                    }
                    else if (_current.Token.IsOperator)
                    {
                        if (_current.RightChild?.Token.Type == TokenType.Dot)
                        {
                            _current.RightChild.SetBinaryRight(node);
                        }
                        else
                        {
                            _current.SetBinaryRight(node);
                            _current = _current.GetRoot();
                        }
                    }
                    else
                    {
                        throw ThrowHelper.UnSupportedSyntax(token);
                    }
                    break;
                case TokenType.Dot:
                    EnsureCurrentNotNull(token);
                    if (_current.Token.IsOperator)
                    {
                        EnsureNotNull(_current.RightChild, token);
                        if (_current.RightChild.Token.Type == TokenType.MemberAccess || _current.RightChild.Token.Type == TokenType.Dot)
                        {
                            node.SetBinaryLeft(_current.RightChild);
                            _current.SetBinaryRight(node);
                        }
                    }
                    else if (_current.Token.Type == TokenType.Parameter)
                    {
                        throw ThrowHelper.InvalidOperator(token);
                    }
                    else
                    {
                        node.SetBinaryLeft(_current);
                        _current = node;
                    }
                    break;
                case TokenType.ConstantNumber:
                case TokenType.ConstantString:
                case TokenType.ConstantNull:
                case TokenType.ConstantBoolean:
                    if (_current == null)
                    {
                        _current = node;
                    }
                    else if (_current.Token.IsOperator)
                    {
                        _current.SetBinaryRight(node);
                        _current = _current.GetRoot();  //TODO: shrink
                    }
                    else
                    {
                        _current.SetBinaryRight(node);
                    }
                    break;
                #endregion
                #region Compare
                case TokenType.Equal:
                case TokenType.NotEqual:
                case TokenType.GreaterThan:
                case TokenType.GreaterThanOrEqual:
                case TokenType.LessThan:
                case TokenType.LessThanOrEqual:
                case TokenType.Like:
                #endregion
                #region Logic
                case TokenType.And:
                case TokenType.Or:
                case TokenType.Yield:
                case TokenType.Reduce:
                case TokenType.Export:
                    Branches.Merge(_current);
                    Branches.Merge(node);
                    _current = null;
                    break;
                case TokenType.Not:
                    node.SetBinaryLeftEmpty();
                    Branches.Merge(_current);
                    Branches.Merge(node);
                    _current = null;
                    break;
                #endregion
                #region Arithmetic
                case TokenType.Add:
                case TokenType.Substract:
                    if ((_current == null || (_current.Token.IsOperator && _current.RightChild == null) || _current.Token.Type == TokenType.Range) && token.Type == TokenType.Substract)
                    {
                        if (_current != null) //negative symbol
                        {
                            _current.SetBinaryRight(node);
                        }
                        node.SetBinaryLeftEmpty();
                    }
                    else
                    {
                        EnsureCurrentNotNull(token);
                        node.SetBinaryLeft(_current);
                    }
                    _current = node;
                    break;
                case TokenType.Multiply:
                case TokenType.Divide:
                    EnsureCurrentNotNull(token);
                    if (_current.Token.IsAddOrSub && !IsNegativeNode(_current) && !_current.Immutable)
                    {
                        Node temp = _current.SetBinaryRight(node);  //调整运算符优先级，先做乘除再加减
                        node.SetBinaryLeft(temp);
                    }
                    else
                    {
                        node.SetBinaryLeft(_current);
                    }
                    _current = node;
                    break;
                #endregion
                #region Block
                case TokenType.BlockStart:
                    Branches.Merge(_current);
                    Branches.New();
                    _current = null;
                    break;
                case TokenType.BlockEnd:
                    Branches.Merge(_current);
                    _current = Branches.Root;
                    if (_current != null)
                    {
                        _current.Immutable = true;
                    }
                    Branches.Eliminate();
                    break;
                #endregion
                #region Method
                case TokenType.MethodCall:
                    Branches.Merge(_current);
                    _current = node;
                    break;
                case TokenType.MethodStart:
                    EnsureCurrentNotNull(token);
                    node.SetBinaryLeft(_current);
                    Branches.New(BranchType.Method, node);
                    _current = null;
                    Branches.New();
                    break;
                case TokenType.MethodEnd:
                    Branches.Merge(_current);
                    Node paramNode = Branches.Root;
                    Branches.Eliminate();
                    EnsureCurrentBranchType(Branches, token, BranchType.Method);
                    _current = Branches.Root;
                    if (paramNode != null)
                    {
                        _current.AddChild(paramNode);
                    }
                    _current.Immutable = true;
                    Branches.Eliminate();
                    break;
                #endregion
                case TokenType.Comma:
                    Branches.Merge(_current);
                    paramNode = Branches.Root;
                    if (Branches.Current.Type == BranchType.Assignment)
                    {
                        Branches.Eliminate();
                    }
                    Branches.Eliminate();
                    EnsureNotNull(paramNode, token);
                    EnsureCurrentBranchType(Branches, token, BranchType.Method, BranchType.Object, BranchType.Array);
                    Branches.Root.AddChild(paramNode);
                    Branches.New();
                    _current = null;
                    break;
                #region Array
                case TokenType.ArrayIndexStart:
                    //EnsureCurrentNotNull(token); //暂且不支持自定义空数组， 空数组的情况
                    if (_current == null)
                    {
                        node.SetBinaryLeftEmpty();  //初始化数组的情况，左子节点设为空
                    }
                    else if (_current.CanbeArrayIndexed)
                    {
                        node.SetBinaryLeft(_current); //当前节点作为第一个参数                    
                    }
                    else if (_current is AddNode)
                    {
                        node.SetBinaryLeftEmpty();
                        _current.SetBinaryRight(node);
                    }
                    else
                    {
                        throw ThrowHelper.UnSupportedSyntax(token);
                    }
                    Branches.New(BranchType.Array, node);
                    Branches.New();
                    _current = null;
                    break;
                case TokenType.ArrayIndexEnd:
                    Branches.Merge(_current);
                    paramNode = Branches.Root;
                    //EnsureNotNull(paramNode, token); //数组索引器不支持空参数   
                    Branches.Eliminate();
                    EnsureCurrentBranchType(Branches, token, BranchType.Array);
                    if (paramNode != null)
                    {
                        Branches.Root.AddChild(paramNode);
                    }
                    _current = Branches.Root;
                    _current.Immutable = true;
                    Branches.Eliminate();
                    if (_current.GetRoot() is AddNode)
                    {
                        //Branches.Merge(_current);
                        _current = _current.GetRoot();
                    }
                    break;
                case TokenType.Range:
                    EnsureCurrentNotNull(token);
                    node.SetBinaryLeft(_current);
                    _current = node;
                    break;
                #endregion
                #region Between
                case TokenType.Between:
                    EnsureCurrentNotNull(token);
                    Branches.New(BranchType.Method, node);
                    node.SetBinaryLeft(_current); //当前节点作为第一个参数                    
                    _current = node;
                    break;
                case TokenType.BetweenStart:
                    EnsureCurrentNotNull(token);
                    //node.Token.Type = TokenType.ConstantBoolean; //转换成布尔型节点,表示开闭区间
                    _current.AddChild(node);
                    Branches.New();
                    _current = null;
                    break;
                case TokenType.BetweenEnd:
                    Branches.Merge(_current);
                    paramNode = Branches.Root;
                    EnsureNotNull(paramNode, token);
                    Branches.Eliminate();
                    EnsureCurrentBranchType(Branches, token, BranchType.Method);
                    _current = Branches.Root;
                    _current.AddChild(paramNode);
                    _current.Immutable = true;
                    //node.Token.Type = TokenType.ConstantBoolean; //转换成布尔型节点,表示开闭区间  
                    _current.AddChild(node);
                    if (_current.Children.Count != 5) //Between需要满足5个参数
                    {
                        throw ThrowHelper.InvalidOperand(_current.Token, _current.Children.Select(n => n.Token).ToArray());
                    }
                    Branches.Eliminate();
                    break;
                #endregion
                #region BuildObject
                case TokenType.OpenBrace:
                    Branches.Merge(_current);
                    _current = null;
                    //token.Type = TokenType.BuildObject;
                    Branches.New(BranchType.Object, node);
                    Branches.New();
                    break;
                case TokenType.CloseBrace:
                    Branches.Merge(_current);
                    paramNode = Branches.Root;
                    if (Branches.Current.Type == BranchType.Assignment)
                    {
                        Branches.Eliminate();
                    }
                    Branches.Eliminate();
                    EnsureCurrentBranchType(Branches, token, BranchType.Object);
                    _current = Branches.Root;
                    if (paramNode != null)
                    {
                        _current.AddChild(paramNode);
                    }
                    _current.Immutable = true;
                    Branches.Eliminate();
                    break;
                case TokenType.Colon:
                    EnsureCurrentNotNull(token);
                    EnsureNodeType(_current, TokenType.MemberAccess);
                    node.SetBinaryLeft(_current);
                    Branches.New(BranchType.Assignment, node);
                    _current = null;
                    break;
                #endregion
                case TokenType.Compose:
                    EnsureCurrentNotNull(token);
                    EnsureNodeType(_current, TokenType.MemberAccess, TokenType.Yield, TokenType.Dot);
                    node.SetBinaryLeft(_current.GetRoot());
                    _current = node;
                    break;
                //case TokenType.Reduce:
                //    Branches.Merge(_current);
                //    Branches.Merge(node);
                //    _current = null;
                //    break;
                default:
                    throw ThrowHelper.UnSupportedSyntax(token);
            }
        }

        private void EnsureCurrentNotNull(Token token)
        {
            EnsureNotNull(_current, token);
        }

        private void EnsureNotNull(Node node, Token token)
        {
            if (node == null)
            {
                throw ThrowHelper.InvalidOperator(token);
            }
        }

        /// <summary>
        /// 检查token所在的Branch是否符合期待的Branch
        /// </summary>
        /// <param name="branches"></param>
        /// <param name="token"></param>
        /// <param name="branchTypes">需要符合的BranchType</param>
        private void EnsureCurrentBranchType(BranchManager branches, Token token, params BranchType[] branchTypes)
        {
            if (!branchTypes.Any(bt => bt == branches.Current.Type))
            {
                throw ThrowHelper.UnSupportedSyntax(token);
            }
        }


        /// <summary>
        /// 检查Node的TokenType是否符合预期
        /// </summary>
        /// <param name="node"></param>
        /// <param name="tokenType"></param>
        private void EnsureNodeType(Node node, TokenType tokenType)
        {
            if (node.Token.Type != tokenType)
            {
                throw ThrowHelper.UnSupportedSyntax(node.Token);
            }
        }

        private void EnsureNodeType(Node node, params TokenType[] tokenTypes)
        {
            if (tokenTypes.All(t => node.Token.Type != t))
            {
                throw ThrowHelper.UnSupportedSyntax(node.Token);
            }
        }

        private bool IsNegativeNode(Node node)
        {
            return node.Token.Type == TokenType.Substract && node.LeftChild.Token.Type == TokenType.Empty;
        }
    }
}
