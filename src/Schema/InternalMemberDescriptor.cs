using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Orel.Schema
{
    internal class InternalMemberDescriptor : ISchemaProvider
    {
        private Dictionary<string, MemberDefinition> _memberDicts = new Dictionary<string, MemberDefinition>(StringComparer.OrdinalIgnoreCase);
        public string DefaultScope { get; set; }
        public RootType RootType { get; set; } = RootType.Object;
        public MemberDefinition Add(MemberDefinition def)
        {
            def.UniqueName = string.IsNullOrEmpty(def.Scope) ? def.MemberName : $"{ def.Scope}.{ def.MemberName}";
            def.ActualName = def.MemberName;

            _memberDicts[def.UniqueName] = def;
            return def;
        }

        public IMemberDefinition Add(IMemberDefinition def)
        {
            return Add(def as MemberDefinition);
        }

        public void Clear()
        {
            _memberDicts.Clear();
        }

        public MemberDefinition Get(string uniquePath, string prefix)
        {
            if (!string.IsNullOrEmpty(prefix))
            {
                uniquePath = $"{prefix}.{uniquePath}";
            }
            if (_memberDicts.TryGetValue(uniquePath, out var memberDefinition))
            {
                return memberDefinition;
            }
            if (_memberDicts.TryGetValue($"{DefaultScope}.{uniquePath}", out memberDefinition))
            {
                return memberDefinition;
            }
            return null;
        }

        public MemberDefinition GetByActualName(string actualName)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<MemberDefinition> GetEnumerator()
        {
            return _memberDicts.Values.GetEnumerator();
        }

        IEnumerator<IMemberDefinition> IEnumerable<IMemberDefinition>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IMemberDefinition IMemberDescriptor.Get(string uniquePath, string prefix)
        {
            return Get(uniquePath, prefix);
        }

        IMemberDefinition IMemberDescriptor.GetByActualName(string actualName)
        {
            return GetByActualName(actualName);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        //简单复制
        public InternalMemberDescriptor Clone()
        {
            var newOne = new InternalMemberDescriptor();
            foreach (var pair in _memberDicts)
            {
                newOne.Add(pair.Value);
            }
            return newOne;
        }

        public InternalMemberDescriptor() { }

        public InternalMemberDescriptor(IMemberDescriptor template)
        {
            foreach (var item in template)
            {
                Add(item);
            }
        }
    }
}
