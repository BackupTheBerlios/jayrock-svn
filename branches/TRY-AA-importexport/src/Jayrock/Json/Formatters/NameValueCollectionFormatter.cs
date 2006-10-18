#region License, Terms and Conditions
//
// Jayrock - A JSON-RPC implementation for the Microsoft .NET Framework
// Written by Atif Aziz (atif.aziz@skybow.com)
// Copyright (c) Atif Aziz. All rights reserved.
//
// This library is free software; you can redistribute it and/or modify it under
// the terms of the GNU Lesser General Public License as published by the Free
// Software Foundation; either version 2.1 of the License, or (at your option)
// any later version.
//
// This library is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more
// details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this library; if not, write to the Free Software Foundation, Inc.,
// 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA 
//
#endregion

namespace Jayrock.Json.Formatters
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;

    #endregion

    public sealed class NameValueCollectionFormatter : JsonFormatter
    {
        protected override void FormatEnumerable(IEnumerable enumerable, JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (JsonNull.LogicallyEquals(enumerable))
            {
                writer.WriteNull();   
                return;
            }

            NameValueCollection collection = enumerable as NameValueCollection;

            if (collection != null)
                FormatNameValueCollection(collection, writer);
            else
                base.FormatEnumerable(enumerable, writer);
        }

        private static void FormatNameValueCollection(NameValueCollection collection, JsonWriter writer)
        {
            Debug.Assert(collection != null);
            Debug.Assert(writer != null);

            writer.WriteStartObject();

            for (int i = 0; i < collection.Count; i++)
            {
                writer.WriteMember(collection.GetKey(i));

                string[] values = collection.GetValues(i);

                if (values == null)
                    writer.WriteNull();
                else if (values.Length > 1)
                    writer.WriteValue(values);
                else
                    writer.WriteValue(values[0]);
            }

            writer.WriteEndObject();
        }
    }
}

namespace Jayrock.Json.Serialization.Export.Exporters
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using Jayrock.Json.Formatters;

    #endregion

    public sealed class NameValueCollectionExporterFamily : IJsonExporterFamily
    {
        public IJsonExporter Page(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return typeof(NameValueCollection).IsAssignableFrom(type) ? new NameValueCollectionExporter(type) : null;
        }
    }

    public sealed class NameValueCollectionExporter : JsonExporterBase // FIXME: Merge in NameValueCollectionFormatter
    {
        public NameValueCollectionExporter(Type inputType) : 
            base(inputType) {}

        protected override void SubExport(object value, JsonWriter writer)
        {
            NameValueCollectionFormatter formatter = new NameValueCollectionFormatter();
            formatter.Format(value, writer);
        }
    }
}
