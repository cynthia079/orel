using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orel.Schema.Dynamic
{
    public class KeyValuePairSchemaReader : IDynamicSchemaReader
    {
        private readonly IDynamicSchemaReader _baseReader;

        public KeyValuePairSchemaReader(IDynamicSchemaReader baseReader)
        {
            if (baseReader == null)
            {
                throw new ArgumentNullException(nameof(baseReader));
            }
            _baseReader = baseReader;
        }

        public void Read(ISchemaProvider provider, IEnumerable<KeyValuePair<string, object>> dynamicMetaObject, string scope)
        {
            foreach (var pair in dynamicMetaObject)
            {
                if (pair.Value == null)
                {
                    continue;
                }
                Type runtimeType = null;
                DataType? elType;
                if (pair.Value is JValue value)
                {
                    switch (value.Type)
                    {
                        case JTokenType.Float:
                            elType = DataType.Number;
                            runtimeType = typeof(decimal?);
                            break;
                        case JTokenType.Integer:
                            elType = DataType.Number;
                            runtimeType = typeof(long?);
                            break;
                        case JTokenType.Array:
                            elType = DataType.List;
                            runtimeType = typeof(IList);
                            break;
                        default:
                            elType = value.Type.GetORELDataType();
                            break;
                    }
                }
                else if (pair.Value is JArray)
                {
                    elType = DataType.List;
                    runtimeType = typeof(IList);
                }
                else
                {
                    elType = pair.Value.GetType().GetORELDataType();
                    if (pair.Value.GetType().IsInterger())
                    {
                        runtimeType = typeof(long?);
                    }
                    else if (pair.Value.GetType().IsFloat())
                    {
                        runtimeType = typeof(decimal?);
                    }
                }
                if (elType == null) continue;
                if (runtimeType == null)
                {
                    runtimeType = elType.Value.GetRuntimeType();
                }
                var member = new MemberDefinition(pair.Key, elType.Value, runtimeType, typeof(object), scope);
                provider.Add(member);

                if (elType == DataType.Object || elType == DataType.List)
                {
                    _baseReader.Read(provider, pair.Value, member.UniqueName);
                }
            }
        }

        public void Read(ISchemaProvider provider, object dynamicMetaObject, string scope)
        {
            Read(provider, dynamicMetaObject as IEnumerable<KeyValuePair<string, object>>, scope);
        }
    }
}
