using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orel.Schema
{
    /// <summary>
    /// 数据成员验证器
    /// </summary>
    public interface IMemberDescriptor : IEnumerable<IMemberDefinition>
    {
        /// <summary>
        /// 通过语义名称，获取数据成员的定义
        /// </summary>
        /// <param name="semanticPath"></param>
        /// <returns></returns>
        IMemberDefinition Get(string uniquePath, string prefix);
        /// <summary>
        /// 添加一个数据成员定义，并生成名称和实际名称
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        IMemberDefinition Add(IMemberDefinition def);
        /// <summary>
        /// 默认的Scope名称，当数据类型使用默认Scope名称时，不必在语义名称前添加该Scope名称
        /// </summary>
        string DefaultScope { get; }
        /// <summary>
        /// 通过实际名称，获得数据成员定义
        /// </summary>
        /// <param name="actualName"></param>
        /// <returns></returns>
        IMemberDefinition GetByActualName(string actualName);
        RootType RootType { get; set; }
    }

    public interface IMemberDefinition
    {
        /// <summary>
        /// 数据成员的标识名称
        /// </summary>
        string MemberName { get; }
        /// <summary>
        /// 数据成员的实际名称
        /// </summary>
        string ActualName { get; }
        /// <summary>
        /// 数据成员的唯一名称，包含访问的Path
        /// </summary>
        string UniqueName { get; }
        /// <summary>
        /// Scope，数据所属的对象的名称，可以是Object或者List
        /// </summary>
        string Scope { get; }
        /// <summary>
        /// Scope所属的对象
        /// </summary>
        IMemberDefinition Parent { get; }
        /// <summary>
        /// 数据的类型
        /// </summary>
        DataType DataType { get; }
        /// <summary>
        /// 数据的CLR类型
        /// </summary>
        Type Type { get; }
        /// <summary>
        /// 数据的所属对象的类型
        /// </summary>
        Type ContextType { get; }
    }

    public interface ISchemaReader
    {
        void Read(ISchemaProvider provider, Type type, string scope);
    }

    public interface ISchemaProvider : IMemberDescriptor
    {
        //new RootType RootType { get; set; }
    }

    public enum RootType
    {
        Object,
        List
    }
}
