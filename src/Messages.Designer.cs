﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Orel {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OREL.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 计算离当前日期（时间）一定时间间隔的新日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_DateAdd_Name {
            get {
                return ResourceManager.GetString("Func_DateAdd_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 当前日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_DateAdd_Param1 {
            get {
                return ResourceManager.GetString("Func_DateAdd_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 时间间隔，格式为数字+单位，单位是：Y年M月D日H时m分S秒，如1d2h30m 的本地化字符串。
        /// </summary>
        internal static string Func_DateAdd_Param2 {
            get {
                return ResourceManager.GetString("Func_DateAdd_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 是否向前偏移，即在当前日期（时间）之前 的本地化字符串。
        /// </summary>
        internal static string Func_DateAdd_Param3 {
            get {
                return ResourceManager.GetString("Func_DateAdd_Param3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 新的日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_DateAdd_Ret {
            get {
                return ResourceManager.GetString("Func_DateAdd_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将日期（时间）格式化为指定格式的文本 的本地化字符串。
        /// </summary>
        internal static string Func_DateFormat_Name {
            get {
                return ResourceManager.GetString("Func_DateFormat_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_DateFormat_Param1 {
            get {
                return ResourceManager.GetString("Func_DateFormat_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 格式，Y年M月D日H时m分S秒，如&quot;yyyyMMdd&quot; 的本地化字符串。
        /// </summary>
        internal static string Func_DateFormat_Param2 {
            get {
                return ResourceManager.GetString("Func_DateFormat_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 格式化后的文本 的本地化字符串。
        /// </summary>
        internal static string Func_DateFormat_Ret {
            get {
                return ResourceManager.GetString("Func_DateFormat_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 获取日期（时间）的一部分 的本地化字符串。
        /// </summary>
        internal static string Func_DatePart_Name {
            get {
                return ResourceManager.GetString("Func_DatePart_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_DatePart_Param1 {
            get {
                return ResourceManager.GetString("Func_DatePart_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 表示部位的文字，Y年M月D日H时m分S秒，只能指定一个，如&quot;Y&quot; 的本地化字符串。
        /// </summary>
        internal static string Func_DatePart_Param2 {
            get {
                return ResourceManager.GetString("Func_DatePart_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 日期部分对应的数值 的本地化字符串。
        /// </summary>
        internal static string Func_DatePart_Ret {
            get {
                return ResourceManager.GetString("Func_DatePart_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 使用正则匹配文本，并获取指定匹配的文本段 的本地化字符串。
        /// </summary>
        internal static string Func_Extract_Name {
            get {
                return ResourceManager.GetString("Func_Extract_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_Extract_Param1 {
            get {
                return ResourceManager.GetString("Func_Extract_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 匹配的正则 的本地化字符串。
        /// </summary>
        internal static string Func_Extract_Param2 {
            get {
                return ResourceManager.GetString("Func_Extract_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 指定返回第几个匹配 的本地化字符串。
        /// </summary>
        internal static string Func_Extract_Param3 {
            get {
                return ResourceManager.GetString("Func_Extract_Param3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 指定的匹配字段 的本地化字符串。
        /// </summary>
        internal static string Func_Extract_Ret {
            get {
                return ResourceManager.GetString("Func_Extract_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 使用正则匹配文本，并获取所有匹配的文本段 的本地化字符串。
        /// </summary>
        internal static string Func_ExtractAll_Name {
            get {
                return ResourceManager.GetString("Func_ExtractAll_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_ExtractAll_Param1 {
            get {
                return ResourceManager.GetString("Func_ExtractAll_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 匹配的正则 的本地化字符串。
        /// </summary>
        internal static string Func_ExtractAll_Param2 {
            get {
                return ResourceManager.GetString("Func_ExtractAll_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 所有的匹配字段 的本地化字符串。
        /// </summary>
        internal static string Func_ExtractAll_Ret {
            get {
                return ResourceManager.GetString("Func_ExtractAll_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 依据条件表达式的真否，执行相应的结果表达式，并返回结果 的本地化字符串。
        /// </summary>
        internal static string Func_IfElse_Name {
            get {
                return ResourceManager.GetString("Func_IfElse_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 条件表达式为真时，执行的结果表达式 的本地化字符串。
        /// </summary>
        internal static string Func_IfElse_Param1 {
            get {
                return ResourceManager.GetString("Func_IfElse_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 条件表达式为假时，执行的结果表达式 的本地化字符串。
        /// </summary>
        internal static string Func_IfElse_Param2 {
            get {
                return ResourceManager.GetString("Func_IfElse_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 结果表达式的返回值 的本地化字符串。
        /// </summary>
        internal static string Func_IfElse_Ret {
            get {
                return ResourceManager.GetString("Func_IfElse_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 判断数据是否未设值（NULL） 的本地化字符串。
        /// </summary>
        internal static string Func_IsNull_Name {
            get {
                return ResourceManager.GetString("Func_IsNull_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标数据 的本地化字符串。
        /// </summary>
        internal static string Func_IsNull_Param1 {
            get {
                return ResourceManager.GetString("Func_IsNull_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 未设值返回True，否则返回False 的本地化字符串。
        /// </summary>
        internal static string Func_IsNull_Ret {
            get {
                return ResourceManager.GetString("Func_IsNull_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 判断数据是否为未设值或空值，空值包含空字符串，空数组及空对象 的本地化字符串。
        /// </summary>
        internal static string Func_IsNullOrEmpty_Name {
            get {
                return ResourceManager.GetString("Func_IsNullOrEmpty_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标数据 的本地化字符串。
        /// </summary>
        internal static string Func_IsNullOrEmpty_Param1 {
            get {
                return ResourceManager.GetString("Func_IsNullOrEmpty_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 未设值或空值返回True，否则返回False 的本地化字符串。
        /// </summary>
        internal static string Func_IsNullOrEmpty_Ret {
            get {
                return ResourceManager.GetString("Func_IsNullOrEmpty_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 拼接数组内的多个文本 的本地化字符串。
        /// </summary>
        internal static string Func_Join_Name {
            get {
                return ResourceManager.GetString("Func_Join_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标数组 的本地化字符串。
        /// </summary>
        internal static string Func_Join_Param1 {
            get {
                return ResourceManager.GetString("Func_Join_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 分隔字符，若不输入，使用“,”作为默认字符 的本地化字符串。
        /// </summary>
        internal static string Func_Join_Param2 {
            get {
                return ResourceManager.GetString("Func_Join_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 拼接后的文本 的本地化字符串。
        /// </summary>
        internal static string Func_Join_Ret {
            get {
                return ResourceManager.GetString("Func_Join_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将表示Json数组的文本转换为实际的列表 的本地化字符串。
        /// </summary>
        internal static string Func_JsonToArray_Name {
            get {
                return ResourceManager.GetString("Func_JsonToArray_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_JsonToArray_Param1 {
            get {
                return ResourceManager.GetString("Func_JsonToArray_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的列表 的本地化字符串。
        /// </summary>
        internal static string Func_JsonToArray_Ret {
            get {
                return ResourceManager.GetString("Func_JsonToArray_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将表示Json对象的文本转换为实际的对象 的本地化字符串。
        /// </summary>
        internal static string Func_JsonToObject_Name {
            get {
                return ResourceManager.GetString("Func_JsonToObject_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_JsonToObject_Param1 {
            get {
                return ResourceManager.GetString("Func_JsonToObject_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的对象 的本地化字符串。
        /// </summary>
        internal static string Func_JsonToObject_Ret {
            get {
                return ResourceManager.GetString("Func_JsonToObject_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 获取集合的元素个数 的本地化字符串。
        /// </summary>
        internal static string Func_ListLength_Name {
            get {
                return ResourceManager.GetString("Func_ListLength_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标集合 的本地化字符串。
        /// </summary>
        internal static string Func_ListLength_Param1 {
            get {
                return ResourceManager.GetString("Func_ListLength_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 元素个数 的本地化字符串。
        /// </summary>
        internal static string Func_ListLength_Ret {
            get {
                return ResourceManager.GetString("Func_ListLength_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 文本是否匹配指定的正则 的本地化字符串。
        /// </summary>
        internal static string Func_Match_Name {
            get {
                return ResourceManager.GetString("Func_Match_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_Match_Param1 {
            get {
                return ResourceManager.GetString("Func_Match_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 匹配的正则 的本地化字符串。
        /// </summary>
        internal static string Func_Match_Param2 {
            get {
                return ResourceManager.GetString("Func_Match_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 匹配则返回True，否则返回False 的本地化字符串。
        /// </summary>
        internal static string Func_Match_Ret {
            get {
                return ResourceManager.GetString("Func_Match_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 获取当前时间 的本地化字符串。
        /// </summary>
        internal static string Func_Now_Name {
            get {
                return ResourceManager.GetString("Func_Now_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 时区，默认为+8 的本地化字符串。
        /// </summary>
        internal static string Func_Now_Param1 {
            get {
                return ResourceManager.GetString("Func_Now_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 当前时间 的本地化字符串。
        /// </summary>
        internal static string Func_Now_Ret {
            get {
                return ResourceManager.GetString("Func_Now_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 通过正则匹配替换文本中的值 的本地化字符串。
        /// </summary>
        internal static string Func_Replace_Name {
            get {
                return ResourceManager.GetString("Func_Replace_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_Replace_Param1 {
            get {
                return ResourceManager.GetString("Func_Replace_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 匹配的正则 的本地化字符串。
        /// </summary>
        internal static string Func_Replace_Param2 {
            get {
                return ResourceManager.GetString("Func_Replace_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 要替换的文本 的本地化字符串。
        /// </summary>
        internal static string Func_Replace_Param3 {
            get {
                return ResourceManager.GetString("Func_Replace_Param3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 替换后的文本 的本地化字符串。
        /// </summary>
        internal static string Func_Replace_Ret {
            get {
                return ResourceManager.GetString("Func_Replace_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 分割文本 的本地化字符串。
        /// </summary>
        internal static string Func_Split_Name {
            get {
                return ResourceManager.GetString("Func_Split_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_Split_Param1 {
            get {
                return ResourceManager.GetString("Func_Split_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 分隔符 的本地化字符串。
        /// </summary>
        internal static string Func_Split_Param2 {
            get {
                return ResourceManager.GetString("Func_Split_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 分割后的文本集合 的本地化字符串。
        /// </summary>
        internal static string Func_Split_Ret {
            get {
                return ResourceManager.GetString("Func_Split_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 获取文本的字符数 的本地化字符串。
        /// </summary>
        internal static string Func_TextLength_Name {
            get {
                return ResourceManager.GetString("Func_TextLength_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_TextLength_Param1 {
            get {
                return ResourceManager.GetString("Func_TextLength_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 字符数 的本地化字符串。
        /// </summary>
        internal static string Func_TextLength_Ret {
            get {
                return ResourceManager.GetString("Func_TextLength_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将文本转换为日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_ToDate_Name {
            get {
                return ResourceManager.GetString("Func_ToDate_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_ToDate_Param1 {
            get {
                return ResourceManager.GetString("Func_ToDate_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 时区，默认为+8 的本地化字符串。
        /// </summary>
        internal static string Func_ToDate_Param2 {
            get {
                return ResourceManager.GetString("Func_ToDate_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_ToDate_Ret {
            get {
                return ResourceManager.GetString("Func_ToDate_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将文本转换为日期（时间），当文本为非标准日期格式，如：“N天前”，“昨天”等时，也会尝试转换为标准时间，以当前时间作为基准 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateExt_Name {
            get {
                return ResourceManager.GetString("Func_ToDateExt_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateExt_Param1 {
            get {
                return ResourceManager.GetString("Func_ToDateExt_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 时区，默认为+8 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateExt_Param2 {
            get {
                return ResourceManager.GetString("Func_ToDateExt_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将文本转换为日期（时间），当文本为非标准日期格式，如：“N天前”，“昨天”等时，也会尝试转换为标准时间，以指定的时间作为基准 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateExt2_Name {
            get {
                return ResourceManager.GetString("Func_ToDateExt2_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateExt2_Param1 {
            get {
                return ResourceManager.GetString("Func_ToDateExt2_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 基准时间 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateExt2_Param2 {
            get {
                return ResourceManager.GetString("Func_ToDateExt2_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 时区，默认为+8 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateExt2_Param3 {
            get {
                return ResourceManager.GetString("Func_ToDateExt2_Param3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将Unix时间戳转换为时间类型数据 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateFromUnixTimestamp_Name {
            get {
                return ResourceManager.GetString("Func_ToDateFromUnixTimestamp_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Unix时间戳 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateFromUnixTimestamp_Param1 {
            get {
                return ResourceManager.GetString("Func_ToDateFromUnixTimestamp_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 要转换的时间的时区 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateFromUnixTimestamp_Param2 {
            get {
                return ResourceManager.GetString("Func_ToDateFromUnixTimestamp_Param2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的时间 的本地化字符串。
        /// </summary>
        internal static string Func_ToDateFromUnixTimestamp_Ret {
            get {
                return ResourceManager.GetString("Func_ToDateFromUnixTimestamp_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 获取今天的日期（时间） 的本地化字符串。
        /// </summary>
        internal static string Func_Today_Name {
            get {
                return ResourceManager.GetString("Func_Today_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 时区，默认为+8 的本地化字符串。
        /// </summary>
        internal static string Func_Today_Param1 {
            get {
                return ResourceManager.GetString("Func_Today_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 今天的日期，时间为当天零点 的本地化字符串。
        /// </summary>
        internal static string Func_Today_Ret {
            get {
                return ResourceManager.GetString("Func_Today_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将文本转换为数字 的本地化字符串。
        /// </summary>
        internal static string Func_ToNum_Name {
            get {
                return ResourceManager.GetString("Func_ToNum_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_ToNum_Param1 {
            get {
                return ResourceManager.GetString("Func_ToNum_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的数字 的本地化字符串。
        /// </summary>
        internal static string Func_ToNum_Ret {
            get {
                return ResourceManager.GetString("Func_ToNum_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 数字转换为文本，文本中含有“千“、”万“、”亿”、“k”的字样时，会转换相应的倍数 的本地化字符串。
        /// </summary>
        internal static string Func_ToNumExt_Name {
            get {
                return ResourceManager.GetString("Func_ToNumExt_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_ToNumExt_Param1 {
            get {
                return ResourceManager.GetString("Func_ToNumExt_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的数字 的本地化字符串。
        /// </summary>
        internal static string Func_ToNumExt_Ret {
            get {
                return ResourceManager.GetString("Func_ToNumExt_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将目标数据转换为文本 的本地化字符串。
        /// </summary>
        internal static string Func_ToText_Name {
            get {
                return ResourceManager.GetString("Func_ToText_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标数据 的本地化字符串。
        /// </summary>
        internal static string Func_ToText_Param1 {
            get {
                return ResourceManager.GetString("Func_ToText_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的文本 的本地化字符串。
        /// </summary>
        internal static string Func_ToText_Ret {
            get {
                return ResourceManager.GetString("Func_ToText_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将时间类型数据转换为Unix时间戳(10位） 的本地化字符串。
        /// </summary>
        internal static string Func_ToUnixTimestamp_Name {
            get {
                return ResourceManager.GetString("Func_ToUnixTimestamp_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标时间 的本地化字符串。
        /// </summary>
        internal static string Func_ToUnixTimestamp_Param1 {
            get {
                return ResourceManager.GetString("Func_ToUnixTimestamp_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的Unix时间戳 的本地化字符串。
        /// </summary>
        internal static string Func_ToUnixTimestamp_Ret {
            get {
                return ResourceManager.GetString("Func_ToUnixTimestamp_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 将时间类型数据转换为Unix时间戳(13位带毫秒数） 的本地化字符串。
        /// </summary>
        internal static string Func_ToUnixTimestampWithMillisecond_Name {
            get {
                return ResourceManager.GetString("Func_ToUnixTimestampWithMillisecond_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标时间 的本地化字符串。
        /// </summary>
        internal static string Func_ToUnixTimestampWithMillisecond_Param1 {
            get {
                return ResourceManager.GetString("Func_ToUnixTimestampWithMillisecond_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 转换后的Unix时间戳 的本地化字符串。
        /// </summary>
        internal static string Func_ToUnixTimestampWithMillisecond_Ret {
            get {
                return ResourceManager.GetString("Func_ToUnixTimestampWithMillisecond_Ret", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 移除文本两边的空白字符 的本地化字符串。
        /// </summary>
        internal static string Func_Trim_Name {
            get {
                return ResourceManager.GetString("Func_Trim_Name", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 目标文本 的本地化字符串。
        /// </summary>
        internal static string Func_Trim_Param1 {
            get {
                return ResourceManager.GetString("Func_Trim_Param1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 移除空白字符后的新文本 的本地化字符串。
        /// </summary>
        internal static string Func_Trim_Ret {
            get {
                return ResourceManager.GetString("Func_Trim_Ret", resourceCulture);
            }
        }
    }
}