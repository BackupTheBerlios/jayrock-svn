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

namespace Jayrock.Json.Conversion.Export.Exporters
{
    #region Imports

    using System;
    using System.Data;
    using System.Diagnostics;

    #endregion
    
    public sealed class DataTableExporterFamily : IJsonExporterFamily
    {
        public IJsonExporter Page(Type type)
        {
            return typeof(DataTable).IsAssignableFrom(type) ? 
                new DataTableExporter(type) : null;
        }
    }

    public class DataTableExporter : JsonExporterBase
    {
        public DataTableExporter() :
            this(typeof(DataTable)) {}

        public DataTableExporter(Type inputType) : 
            base(inputType) {}

        protected override void ExportValue(object value, JsonWriter writer)
        {
            Debug.Assert(value != null);
            Debug.Assert(writer != null);

            ExportTable((DataTable) value, writer);
        }

        internal static void ExportTable(DataTable table, JsonWriter writer)
        {
            Debug.Assert(table != null);
            Debug.Assert(writer != null);

            DataViewExporter.ExportView(table.DefaultView, writer);
       }
    }
}
