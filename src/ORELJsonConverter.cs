using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Orel.Expressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orel.Schema;

namespace Orel
{
    /// <summary>
    /// 帮助将OREL类型对象正确转换为Json格式
    /// </summary>
    public class ORELJsonWriter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///     <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ORELObject) || objectType == typeof(decimal) || objectType == typeof(decimal?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return null;
        }

        /// <summary>
        /// 覆盖WriteJson对于ORELObject的处理
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ORELObject)
            {
                var obj = value as ORELObject;
                writer.WriteStartObject();
                foreach (var kv in obj)
                {
                    WriteInternal(writer, kv.Value, serializer, kv.Key);
                }
                writer.WriteEndObject();
            }
            else if (value is decimal)
            {
                WriteDecimalValue(writer, ((decimal)value).ToString("G", CultureInfo.InvariantCulture));
            }
            else if (value is decimal?)
            {
                if (value == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    WriteDecimalValue(writer, ((decimal)value).ToString("G", CultureInfo.InvariantCulture));
                }
            }
        }

        private void WriteDecimalValue(JsonWriter writer, string decimalValue)
        {
            if (decimalValue.IndexOf(".") == -1)
            {
                writer.WriteToken(JsonToken.Integer, decimalValue);
            }
            else
            {
                writer.WriteToken(JsonToken.Float, decimalValue);
            }
        }

        private void WriteInternal(JsonWriter writer, object value, JsonSerializer serializer, string propertyName)
        {
            writer.WritePropertyName(propertyName);
            serializer.Serialize(writer, value);
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite
        {
            get { return true; }
        }
    }


    /// <summary>
    /// 自定义JsonLoader，参考了https://stackoverflow.com/questions/9247478/pascal-case-dynamic-properties-with-json-net
    /// https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Converters/ExpandoObjectConverter.cs
    /// </summary>
    public class ORELJsonConverter : ORELJsonWriter
    {
        readonly IMemberDescriptor _memberDescriptor;

        public ORELJsonConverter(IMemberDescriptor memberDescriptor)
        {
            _memberDescriptor = memberDescriptor;
        }

        //CHANGED
        //the ExpandoObjectConverter needs this internal method so we have to copy it
        //from JsonReader.cs
        internal static bool IsPrimitiveToken(JsonToken token)
        {
            switch (token)
            {
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Null:
                case JsonToken.Undefined:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                    throw new Exception("Unexpected end.");
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadList(reader);
                default:
                    //CHANGED
                    //call to static method declared inside this class
                    if (IsPrimitiveToken(reader.TokenType))
                        return reader.Value;

                    //CHANGED
                    //Use string.format instead of some util function declared inside JSON.NET
                    throw new Exception(string.Format(CultureInfo.InvariantCulture, "Unexpected token when converting ExpandoObject: {0}", reader.TokenType));
            }
        }

        private object ReadList(JsonReader reader)
        {
            IList<object> list = new List<object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    default:
                        object v = ReadValue(reader);

                        list.Add(v);
                        break;
                    case JsonToken.EndArray:
                        return list;
                }
            }

            throw new Exception("Unexpected end.");
        }

        private object ReadObject(JsonReader reader)
        {
            ORELObject container = new ORELObject();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        //CHANGED
                        //generate propertyName according to member definition
                        string propertyName = reader.Value.ToString();
                        var member = _memberDescriptor.GetByActualName(propertyName);
                        if (member != null)
                        {
                            propertyName = member.ActualName;
                        }
                        if (!reader.Read())
                            throw new Exception("Unexpected end.");

                        object v = ReadValue(reader);

                        container.SetMember(propertyName, v);
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return container;
                }
            }

            throw new Exception("Unexpected end.");
        }
    }
}
