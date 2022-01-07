using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orel.Schema.Dynamic
{
    /// <summary>
    /// 用于Json转换Object时，Member的处理
    /// </summary>
    public class JTokenMemberDescriptor : IMemberDescriptor
    {
        public string DefaultScope => throw new NotImplementedException();

        public IMemberDefinition Add(IMemberDefinition def)
        {
            throw new NotImplementedException();
        }

        public IMemberDefinition Get(string uniquePath, string prefix)
        {
            var name = string.IsNullOrEmpty(prefix) ? uniquePath : $"{uniquePath}.{prefix}";
            return JTokenMember.Create(name);
        }

        public IMemberDefinition GetByActualName(string actualName)
        {
            return JTokenMember.Create(actualName);
        }

        public IEnumerator<IMemberDefinition> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public JTokenMemberDescriptor(Type jTokenType)
        {
            if (typeof(JObject).IsAssignableFrom(jTokenType))
            {
                RootType = RootType.Object;
            }
            else if (typeof(JArray).IsAssignableFrom(jTokenType))
            {
                RootType = RootType.List;
            }
            else
            {
                throw new ArgumentException(jTokenType.ToString());
            }
        }

        public RootType RootType { get; set; }
    }

    internal class JTokenMember : IMemberDefinition
    {
        public string MemberName { get; set; }

        public string ActualName { get; set; }

        public string UniqueName { get; set; }

        public string Scope => null;

        public IMemberDefinition Parent => null;

        public DataType DataType => DataType.Object;

        public Type Type => typeof(JToken);

        public Type ContextType => typeof(object);

        internal static JTokenMember Create(string name)
        {
            var member = new JTokenMember() { UniqueName = name };
            var hierarchies = name.Split('.');
            var actualName = hierarchies.Last();
            member.ActualName = actualName;
            member.MemberName = actualName;
            return member;
        }
    }
}
