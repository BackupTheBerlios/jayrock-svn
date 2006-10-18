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

namespace Jayrock.Json.Serialization.Export
{
    #region Imports

    using System;
    using Jayrock.Json.Serialization.Export.Exporters;

    #endregion

    public sealed class JsonExport
    {
        public static void Export(object value, JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            IJsonExporter exporter = TryGetExporter(value.GetType());
                
            if (exporter != null)
                exporter.Export(value, writer);
            else
                writer.WriteString(value.ToString());
        }
        
        public static IJsonExporter TryGetExporter(Type type)
        {
            return JsonExporters.Find(type);
        }
        
        public static IJsonExporter GetExporter(Type type)
        {
            IJsonExporter exporter = TryGetExporter(type);
                
            if (exporter == null)
                throw new ArgumentException(string.Format("No JSON exporter exists for {0}.", type.FullName), "type");
            
            return exporter;
        }

        private JsonExport()
        {
            throw new NotSupportedException();
        }
    }
}