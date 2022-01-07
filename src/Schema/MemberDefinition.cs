using System;
using System.Collections.Generic;
using System.Text;

namespace Orel.Schema
{
    public class MemberDefinition : IMemberDefinition
    {
        /// <summary>
        /// 用户标记的成员名称，与Property名称对应
        /// </summary>
        public string MemberName { get; internal set; }
        /// <summary>
        /// 数据成员的实际名称
        /// </summary>
        public string ActualName { get; internal set; }
        /// <summary>
        /// 数据成员的唯一名称
        /// </summary>
        public string UniqueName { get; internal set; }
        /// <summary>
        /// Scope，数据所属的对象的名称，可以是Object或者List
        /// </summary>
        public string Scope { get; internal set; }
        /// <summary>
        /// Scope所属的对象
        /// </summary>
        public MemberDefinition Parent { get; internal set; }
        /// <summary>
        /// 数据的类型
        /// </summary>
        public DataType DataType { get; internal set; }
        /// <summary>
        /// 数据的CLR类型
        /// </summary>
        public Type Type { get; internal set; }
        /// <summary>
        /// 数据的所属对象的类型
        /// </summary>
        public Type ContextType { get; internal set; }

        IMemberDefinition IMemberDefinition.Parent => Parent;

        public MemberDefinition(string memberName, DataType dataType, string scope = null, Type contextType = null)
        {
            MemberName = memberName;
            DataType = dataType;
            Type = dataType.GetRuntimeType();
            Scope = scope;
            ContextType = contextType ?? typeof(object);
        }

        public MemberDefinition(string memberName, DataType dataType, Type type, Type contextType, string scope)
        {
            MemberName = memberName;
            DataType = dataType;
            Type = type;
            ContextType = contextType;
            Scope = scope;
        }

        public void ChangeType(DataType targetType)
        {
            DataType = targetType;
            Type = DataType.GetRuntimeType();
        }
    }
}
