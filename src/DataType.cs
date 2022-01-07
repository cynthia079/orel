using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orel
{
    public enum DataType
    {
        Number,
        DateTime,
        Text,
        List,
        Object,
        Boolean
    }

    public static class DataTypeExtensions
    {
        public static DataType? GetORELDataType(this JTokenType jTokenType)
        {
            switch (jTokenType)
            {
                case JTokenType.Boolean:
                    return DataType.Boolean;
                case JTokenType.Date:
                    return DataType.DateTime;
                case JTokenType.Integer:
                case JTokenType.Float:
                    return DataType.Number;
                case JTokenType.Array:
                    return DataType.List;
                case JTokenType.Object:
                    return DataType.Object;
                case JTokenType.String:
                case JTokenType.Uri:
                case JTokenType.Guid:
                    return DataType.Text;
                case JTokenType.Null:
                    return null;
                default:
                    throw new NotSupportedException($"不支持的Json类型：{jTokenType}");
            }
        }

        public static DataType GetORELDataType(this Type type)
        {
            if (type.IsNumeric() || type.IsEnum)
            {
                return DataType.Number;
            }
            else if (type.IsString())
            {
                return DataType.Text;
            }
            else if (type.IsDateTime())
            {
                return DataType.DateTime;
            }
            else if (type.IsBoolean())
            {
                return DataType.Boolean;
            }
            else if (type.IsDynamic())
            {
                return DataType.Object;
            }
            else if (type.IsList())
            {
                return DataType.List;
            }
            else
            {
                return DataType.Object;
            }
        }

        public static Type GetRuntimeType(this DataType dataType)
        {
            switch (dataType)
            {
                case DataType.DateTime:
                    return typeof(DateTimeOffset?);
                case DataType.List:
                    return typeof(IList);
                case DataType.Number:
                    return typeof(decimal?);
                case DataType.Object:
                    return typeof(object);
                case DataType.Boolean:
                    return typeof(bool);
                case DataType.Text:
                default:
                    return typeof(string);
            }
        }
    }
}
