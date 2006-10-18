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

namespace Jayrock.Json.Serialization.Import
{
    #region Imports

    using System;
    using System.Collections;
    using System.Diagnostics;
    using Jayrock.Json.Serialization.Import.Importers;

    #endregion

    [ Serializable ]
    public sealed class JsonImporterCollection : JsonTraderCollection
    {
        public IJsonImporter Find(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            return (IJsonImporter) BaseFind(type);
        }

        public void Register(IJsonImporter importer)
        {
            if (importer == null)
                throw new ArgumentNullException("importer");
            
            Register(importer.OutputType, importer);
        }

        public void Register(IJsonImporterFamily importerFamily)
        {
            if (importerFamily == null)
                throw new ArgumentNullException("importerFamily");
            
            RegisterFamily(importerFamily);
        }

        protected override object Page(object family, Type type)
        {
            return ((IJsonImporterFamily) family).Page(type);
        }

        protected override ICollection GetDefaultConfiguration()
        {
            return new object[]
            {
                new ByteImporter(),
                new Int16Importer(),
                new Int32Importer(),
                new Int64Importer(),
                new SingleImporter(),
                new DoubleImporter(),
                new DecimalImporter(),
                new StringImporter(),
                new BooleanImporter(),
                new DateTimeImporter(),
                new AutoImporter(),
                new DictionaryImporter(),
                new ListImporter(),

                new ImportAwareImporterFamily(),
                new ArrayImporterFamily(),
                new EnumImporterFamily(),
                new ComponentImporterFamily()
            };
        }

        protected override void Register(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            
            IJsonImporter importer = item as IJsonImporter;
                    
            if (importer != null)
            {
                Register(importer);
            }
            else
            {
                IJsonImporterFamily family = item as IJsonImporterFamily;
                        
                if (family == null)
                    throw new ArgumentException(string.Format("The type {0} is not a valid JSON importer. Expected {1} or {2}.", item.GetType().FullName, typeof(IJsonImporter).FullName, typeof(IJsonImporterFamily).FullName));

                Register(family);
            }
        }
    }
}