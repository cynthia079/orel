using System;
using System.Collections.Generic;
using System.Linq;
using Orel.Nodes;

namespace Orel
{
    internal enum BranchType
    {
        Default,
        Method,
        Array,
        Object,
        Assignment
    }

    internal class Branch
    {
        internal Node Root { get; set; }
        internal Node Attachable { get; set; }
        internal BranchType Type { get; private set; }

        internal Branch(BranchType branchType = BranchType.Default)
        {
            Type = branchType;
        }
    }

    internal class BranchManager
    {
        internal Stack<Branch> _branches = new Stack<Branch>();
        internal Branch Main { get; private set; }
        internal Branch Current { get; private set; }
        internal Node Root => Current.Root;

        internal BranchManager()
        {
            Main = new Branch();
            _branches.Push(Main);
            Current = Main;
        }

        /// <summary>
        /// 创建一个新的分支，添加到堆栈，重置当前分支指针到新创建的分支，返回新分支
        /// </summary>
        /// <param name="branchType"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        internal Branch New(BranchType branchType = BranchType.Default, Node root = null)
        {
            Branch branch = new Branch(branchType) { Root = root };
            _branches.Push(branch);
            Current = branch;
            return branch;
        }

        /// <summary>
        /// 关闭并删除当前分支，并重置当前分支指针到上一级分支，返回被消除的分支
        /// </summary>
        /// <returns>被消除的Branch</returns>
        internal Branch Eliminate()
        {
            Branch eliminated = _branches.Pop();
            if (!_branches.TryPeek(out Branch branch))
            {
                throw new InvalidOperationException("主分支不能被消除");
            }
            Current = branch;
            return eliminated;
        }

        //internal void EnsureCurrentType(BranchType branchType, Token token)
        //{
        //    if (Current.Type != branchType)
        //    {
        //        throw ThrowHelper.UnSupportedSyntax(token);
        //    }
        //}       

        internal void Merge(Node node)
        {
            if (node == null)
            {
                return;
            }

            Node root = node.GetRoot();
            //只更新根节点的Branch属性
            if (Current.Root == null)
            {
                Current.Root = root;
                Current.Attachable = node;
                return;
            }
            Node dockingNode = Current.Attachable != null ? FindAttachable(Current.Attachable, node.Token.Priority) : Current.Root;
            if (dockingNode != null)
            {
                if (dockingNode.RightChild != null)
                {
                    Node temp = dockingNode.SetBinaryRight(root);
                    node.SetBinaryLeft(temp);
                }
                else
                {
                    dockingNode.SetBinaryRight(root);
                }
            }
            else
            {
                root.SetBinaryLeft(Current.Root);
                Current.Root = root;
            }
            Current.Attachable = node;
        }

        private Node FindAttachable(Node node, int priority)
        {
            if (node.IsBinary && node.RightChild == null)
            {
                return node;
            }
            node = node.Parent;
            while (node != null)
            {
                if (node.IsBinary && (node.RightChild == null || (priority < node.Token.Priority && !node.Immutable)))
                {
                    return node;
                }
                node = node.Parent;
            }
            return null;
        }
    }
}
