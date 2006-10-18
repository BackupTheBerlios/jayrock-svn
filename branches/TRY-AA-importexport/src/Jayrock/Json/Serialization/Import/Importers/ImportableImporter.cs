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

namespace Jayrock.Json.Serialization.Import.Importers
{
    #region Importer

    using System;
    using System.Collections;
    using Jayrock.Json.Serialization.Import;

    #endregion

    public sealed class ImportAwareImporterFamily : IJsonImporterFamily
    {
        public IJsonImporter Page(Type type)
        {
            return typeof(IJsonImportable).IsAssignableFrom(type) ? 
                new ImportableImporter(type) : null;
        }
    }

    public class ImportableImporter : JsonImporterBase
    {
        public ImportableImporter(Type type) : 
            base(type) {}

        public override object Import(JsonReader reader)
        {
            if (reader == null) 
                throw new ArgumentNullException("reader");

            reader.MoveToContent();
            
            if (reader.TokenClass == JsonTokenClass.Null)
                return null;
            
            IJsonImportable o = (IJsonImportable) CreateObject();
            o.Import(reader);
            return o;
        }

        protected virtual object CreateObject()
        {
            return Activator.CreateInstance(OutputType);
        }
    }
    
    public class DictionaryImporter : ImportableImporter
    {
        public DictionaryImporter() : 
            base(typeof(IDictionary)) {}

        protected override object CreateObject()
        {
            return new Hashtable();
        }
    }

    public class ListImporter : ImportableImporter
    {
        public ListImporter() : 
            base(typeof(IList)) {}

        protected override object CreateObject()
        {
            return new ArrayList(8);
        }
    }
}