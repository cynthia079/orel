using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Orel.Schema
{
    /// <summary>
    /// Create a default member descriptor that case insensitive for member name
    /// </summary>  
    public class DefaultMemberDescriptor : ISchemaProvider
    {
        public string DefaultScope { get; private set; }
        public RootType RootType { get; set; } = RootType.Object;
        private Dictionary<string, MemberDefinition> _memberDicts = new Dictionary<string, MemberDefinition>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, List<MemberDefinition>> _actualNameLookup = new Dictionary<string, List<MemberDefinition>>(StringComparer.OrdinalIgnoreCase);
        private Func<MemberDefinition, MemberDefinition, string> _actualNameGetter;
        /// <summary>
        /// MemberDescriptor构造方法
        /// </summary>
        /// <param name="defaultScope">表示该Scope下面的所有节点可以作为顶级节点看待
        /// 访问该Scope下的所有子节点时可以省略[Scope.]的对象访问符前缀，
        /// 如果该Scope下有和根节点重名的节点，则仍需要使用对象访问符前缀</param>
        public DefaultMemberDescriptor(string defaultScope = null, Func<MemberDefinition, MemberDefinition, string> actualNameGetter = null)
        {
            DefaultScope = defaultScope;
            if (!string.IsNullOrEmpty(defaultScope))
            {
                Add(new MemberDefinition(defaultScope, DataType.Object));
            }
            _actualNameGetter = actualNameGetter;
        }

        /// <summary>
        /// MemberDescriptor构造方法
        /// </summary>
        /// <param name="memberDefinitions">数据成员定义</param>
        /// <param name="defaultScope">表示该Scope下面的所有节点可以作为顶级节点看待
        /// 访问该Scope下的所有子节点时可以省略[Scope.]的对象访问符前缀，
        /// 如果该Scope下有和根节点重名的节点，则仍需要使用对象访问符前缀</param>
        public DefaultMemberDescriptor(IEnumerable<MemberDefinition> memberDefinitions, string defaultScope = null, Func<MemberDefinition, MemberDefinition, string> actualNameSetter = null) : this(defaultScope, actualNameSetter)
        {
            SetMembers(memberDefinitions);
        }

        /// <summary>
        /// 设定memberDefinitions的各项值，并返回
        /// </summary>
        /// <param name="memberDefinitions"></param>
        internal IEnumerable<MemberDefinition> SetMembers(IEnumerable<MemberDefinition> memberDefinitions)
        {
            foreach (MemberDefinition def in memberDefinitions)
            {
                Add(def);
            }
            return memberDefinitions;
        }

        /// <summary>
        /// 对外只允许在构造时添加成员，即_memberDicts中的成员是不可变的
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public MemberDefinition Add(MemberDefinition def)
        {
            if (!string.IsNullOrEmpty(def.Scope))
            {
                MemberDefinition scopeMember = GetScopeMember(def.Scope);
                def.Parent = scopeMember;
                def.UniqueName = $"{scopeMember.UniqueName}.{def.MemberName}";
                def.ActualName = _actualNameGetter != null ? _actualNameGetter.Invoke(scopeMember, def) : def.MemberName;
            }
            else
            {
                def.UniqueName = def.MemberName;
                def.ActualName = def.MemberName;
            }

            if (_memberDicts.ContainsKey(def.UniqueName))
            {
                if (def.UniqueName == DefaultScope)  //重复添加默认Scope可以忽略
                {
                    return def;
                }
                throw new ArgumentException($"已存在相同的对象成员路径:{def.UniqueName}");
            }

            _memberDicts.Add(def.UniqueName, def);
            //构建通过ActualName字典
            if (_actualNameLookup.TryGetValue(def.ActualName, out var memberDefinitions))
            {
                memberDefinitions.Add(def);
            }
            else
            {
                _actualNameLookup.Add(def.ActualName, new List<MemberDefinition>() { def });
            }
            return def;
        }

        private MemberDefinition GetScopeMember(string scope)
        {
            if (_memberDicts.TryGetValue(scope, out var parent))
            {
                if (parent.DataType == DataType.List || parent.DataType == DataType.Object)
                {
                    return parent;
                }
            }
            throw new ArgumentException($"Scope：{scope}不存在，或数据类型为List或Object以外的类型");
        }

        public MemberDefinition Get(string uniquePath, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                uniquePath = $"{prefix}.{uniquePath}";
            }
            if (_memberDicts.TryGetValue(uniquePath, out MemberDefinition memberDefinition))
            {
                return memberDefinition;
            }
            if (_memberDicts.TryGetValue($"{DefaultScope}.{uniquePath}", out memberDefinition))
            {
                return memberDefinition;
            }
            return null;
            //return new MemberDefinition() { MemberName = memberName, ActualPath = memberName, DataType = DataType.Text };
        }

        public MemberDefinition GetByActualName(string actualName)
        {
            MemberDefinition value = null;
            if (!_actualNameLookup.TryGetValue(actualName, out var match))
            {
                return null;
            }
            if (match.Count() > 1)
            {
                value = match.FirstOrDefault(m => m.ActualName.Equals(actualName, StringComparison.Ordinal));
                if (value == null)
                {
                    throw new System.Reflection.AmbiguousMatchException($"存在多个可能匹配的字段名：{actualName}");
                }
                return value;
            }
            return match.First();
        }

        IMemberDefinition IMemberDescriptor.Get(string uniquePath, string prefix)
        {
            return Get(uniquePath, prefix);
        }

        public IMemberDefinition Add(IMemberDefinition def)
        {
            return Add(def as MemberDefinition);
        }

        IMemberDefinition IMemberDescriptor.GetByActualName(string actualName)
        {
            return GetByActualName(actualName);
        }

        public IEnumerator<IMemberDefinition> GetEnumerator()
        {
            return _memberDicts.Values.Cast<IMemberDefinition>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<IMemberDefinition> IEnumerable<IMemberDefinition>.GetEnumerator()
        {
            return _memberDicts.Values.GetEnumerator();
        }
    }
}
