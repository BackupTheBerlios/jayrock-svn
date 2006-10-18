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

namespace Jayrock.Json.Conversion.Import
{
    #region Imports

    using System;
    using Jayrock.Json.Conversion.Export.Exporters;
    using Jayrock.Json.Conversion.Import.Importers;

    #endregion

    public sealed class JsonImport
    {
        public static object Import(JsonReader reader)
        {
            return Import(reader, null);
        }

        public static object Import(JsonReader reader, Type type)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (type == null)
                type = typeof(object);

            reader.MoveToContent();
            
            if (reader.TokenClass == JsonTokenClass.Null)
            {
                reader.Read();
                return null;
            }
            
            IJsonImporter importer = TryGetImporter(type);
                
            if (importer == null)
                throw new JsonException(string.Format("Don't know how to read the type {0} from JSON.", type.FullName)); // TODO: Review the choice of exception type here.

            return importer.Import(reader);
        }
        
        public static IJsonImporter TryGetImporter(Type type)
        {
            return JsonImporters.Find(type);
        }
        
        public static IJsonImporter GetImporter(Type type)
        {
            IJsonImporter importer = TryGetImporter(type);
                
            if (importer == null)
                throw new ArgumentException(string.Format("No JSON importer exists for {0}.", type.FullName), "type");
            
            return importer;
        }

        private JsonImport()
        {
            throw new NotSupportedException();
        }
    }
}