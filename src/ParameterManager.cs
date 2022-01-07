using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Orel.Nodes;

namespace Orel
{
    internal class ParameterManager
    {
        private readonly HashSet<HashSet<ParameterNode>> _undecidedGroups = new HashSet<HashSet<ParameterNode>>();
        private readonly Dictionary<string, ParameterDefinition> _decidedParameters = new Dictionary<string, ParameterDefinition>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 参数的定义忽略大小写
        /// </summary>
        public IDictionary<string, ParameterDefinition> Parameters
        {
            get
            {
                return _decidedParameters;
            }
        }

        /// <summary>
        /// 合并关联的未确定类型参数节点
        /// </summary>
        /// <param name="nodes"></param>
        private void AddUndecidedNodes(params ParameterNode[] nodes)
        {
            var groupsForMerge = new HashSet<HashSet<ParameterNode>>();
            foreach (var group in _undecidedGroups)
            {
                foreach (var node in nodes)
                {
                    if (group.Contains(node))
                    {
                        groupsForMerge.Add(group);
                    }
                }
            }
            if (groupsForMerge.Count == 0)
            {
                var group = new HashSet<ParameterNode>(nodes);
                _undecidedGroups.Add(group);
            }
            else
            {
                var group = groupsForMerge.First();
                foreach (var g in groupsForMerge.Skip(1))
                {
                    group.AddRange(g);
                    _undecidedGroups.Remove(g);
                }
                group.AddRange(nodes);
            }
        }

        public void SetUndecided(ParameterNode parameter)
        {
            parameter.SetUndecided();
            AddUndecidedNodes(parameter);
        }

        public void AddPreDefined(ParameterDefinition definition)
        {
            _decidedParameters.Add(definition.Name, definition);
        }

        public ParameterExpression AddParameter(ParameterNode node, Type expectType)
        {
            //lookup in decided
            if (_decidedParameters.TryGetValue(node.ParameterName, out var info))
            {
                info.Nodes.Add(node);
                var curType = info.Expression.Type;
                if (curType == expectType || expectType == typeof(object))
                {
                    node.SetParameter(info.Expression);
                    return info.Expression;
                }
                else if (curType == typeof(object))
                {
                    var paramExp = Expression.Parameter(expectType);
                    foreach (var n in info.Nodes)
                    {
                        n.SetParameter(paramExp);
                    }
                    return paramExp;
                }
                else
                {
                    throw ThrowHelper.ConflictParameterType(node.Token, expectType, curType);
                }
            }
            else
            {
                var paramExp = Expression.Parameter(expectType);
                ParameterDefinition parameterInfo = null;
                foreach (var group in _undecidedGroups)
                {
                    if (group.Any(g => g.ParameterName.Equals(node.ParameterName, StringComparison.OrdinalIgnoreCase)))
                    {
                        //将未决定类型的参数从未决定分组中移出，放入已决定节点中去
                        var groupByNames = group.GroupBy(g => g.ParameterName, StringComparer.OrdinalIgnoreCase);
                        foreach (var gn in groupByNames)
                        {
                            var nodes = gn.ToList();
                            parameterInfo = new ParameterDefinition(gn.Key, nodes, paramExp);
                            nodes.ForEach(n => n.SetParameter(paramExp));
                            _decidedParameters.TryAdd(parameterInfo.Name, parameterInfo);
                        }
                        _undecidedGroups.Remove(group);
                        return paramExp;
                    }
                }
                parameterInfo = new ParameterDefinition(node.ParameterName, new List<ParameterNode> { node }, paramExp);
                _decidedParameters.TryAdd(parameterInfo.Name, parameterInfo);
                return paramExp;
            }
        }

    }


    public class ParameterDefinition
    {
        internal List<ParameterNode> Nodes { get; set; }
        public ParameterExpression Expression { get; internal set; }
        public DataType DataType { get; set; }
        public string Name { get; set; }

        internal ParameterDefinition(string name, List<ParameterNode> nodes, ParameterExpression exp)
        {
            CheckParameterName(name);
            this.Name = name;
            this.Nodes = nodes;
            this.Expression = exp;
            this.DataType = exp.Type.GetORELDataType();
        }

        public ParameterDefinition(string name, DataType dataType)
        {
            CheckParameterName(name);
            this.Name = name;
            this.DataType = dataType;
            this.Expression = System.Linq.Expressions.Expression.Parameter(dataType.GetRuntimeType(), name);
        }

        /// <summary>
        /// 该参数是否被表达式引用
        /// </summary>
        /// <returns></returns>
        public bool IsRefered => Nodes != null && Nodes.Any();

        public static string ConvertToLegalParamName(string parameterName)
        {
            return parameterName?.Replace('.', '_').Replace('-', '_')
                .Replace("\"", String.Empty).Replace("'", String.Empty);
        }

        private void CheckParameterName(string name)
        {
            if (name.Contains(".") || name.Contains("-") || name.Contains("\"") || name.Contains("'"))
            {
                throw new ArgumentException("Parameter name contains illegal character");
            }
        }
    }
}
