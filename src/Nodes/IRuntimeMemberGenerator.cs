using Orel.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orel.Nodes
{
    interface IRuntimeMemberGenerator
    {
        /// <summary>
        /// 运行时生成的Member定义
        /// </summary>
        IMemberDescriptor RuntimeMembers { get; set; }
    }
}
