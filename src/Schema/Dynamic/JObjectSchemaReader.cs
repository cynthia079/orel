using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Orel.Schema.Dynamic
{
    public class JObjectSchemaReader : IDynamicSchemaReader<JToken>
    {
        public void Read(ISchemaProvider provider, JToken dynamicMetaObject, string scope)
        {
            if (dynamicMetaObject is JObject jObject)
            {
                ReadFromObject(provider, jObject, scope);
            }
            else if (dynamicMetaObject is JArray jArray)
            {
                ReadFromList(provider, jArray, scope);
                if (string.IsNullOrEmpty(scope))
                {
                    provider.RootType = RootType.List;
                }
            }
        }

        private bool GetORELTypeByJTokenType(JTokenType tokenType, out DataType? dataType, out Type runtimeType)
        {
            dataType = tokenType.GetORELDataType();
            if (dataType == null)
            {
                runtimeType = null;
                return false;
            }
            switch (tokenType)
            {
                case JTokenType.Integer:
                    runtimeType = typeof(long?);
                    break;
                case JTokenType.Float:
                    runtimeType = typeof(decimal?);
                    break;
                default:
                    runtimeType = dataType.Value.GetRuntimeType();
                    break;
            }
            return true;
        }


        public void Read(ISchemaProvider provider, object dynamicMetaObject, string scope)
        {
            Read(provider, dynamicMetaObject as JToken, scope);
        }

        private void ReadFromList(ISchemaProvider provider, JArray jArray, string scope)
        {
            if (!jArray.Any())
            {
                //ignore empty collection
                return;
            }
            else
            {
                foreach (var item in jArray)
                {
                    if (GetORELTypeByJTokenType(item.Type, out var itemDataType, out var itemRuntimeType))
                    {
                        var member = new MemberDefinition(TokenDefinitions.DefaultArgument, itemDataType.Value, itemRuntimeType, typeof(IList), scope);

                        if (scope == null)
                        {
                            provider.Add(member); //add default member while the list is a root element
                        }
                        if (itemDataType == DataType.Object)
                        {
                            Read(provider, item, scope);
                        }
                        else if (itemDataType == DataType.List)
                        {
                            provider.Add(member);  //add default member while the item type of list is also a list
                            Read(provider, item, member.UniqueName);
                        }
                        else
                        {
                            provider.Add(member); //add default member while the item type cannot breakdown
                            break;
                        }
                    }
                }
            }
        }

        private void ReadFromObject(ISchemaProvider provider, JObject jObject, string scope)
        {
            var props = jObject.Properties();
            foreach (var prop in props)
            {
                if (GetORELTypeByJTokenType(prop.Value.Type, out var elType, out var runtimeType))
                {
                    var member = new MemberDefinition(prop.Name, elType.Value, runtimeType, typeof(object), scope);
                    provider.Add(member);
                    if (elType == DataType.Object || elType == DataType.List)
                    {
                        Read(provider, prop.Value, member.UniqueName);
                    }
                }
            }
        }
    }
}
