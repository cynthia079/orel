using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Orel.Schema.Dynamic
{
    public interface IDynamicSchemaReader
    {
        void Read(ISchemaProvider provider, object dynamicMetaObject, string scope);
    }

    public interface IDynamicSchemaReader<T> : IDynamicSchemaReader where T : IDynamicMetaObjectProvider
    {
        void Read(ISchemaProvider provider, T dynamicMetaObject, string scope);
    }
}
