using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Orel.Schema.Dynamic;
using Newtonsoft.Json.Linq;

namespace Orel.Schema
{
    public class SchemaReaderOptions
    {
        public int MaxPathRecursiveCount { get; set; } = 3;
        public Func<object, IDynamicSchemaReader> DynamicReaderSelector { get; set; }        
    }
}
