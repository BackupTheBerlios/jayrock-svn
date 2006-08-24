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

namespace Jayrock.Json.Importers
{
    #region Imports

    using System;
    using System.Diagnostics;
    using Jayrock.Json.Importers;

    #endregion

    public sealed class JsonImporterStock
    {
        public readonly static IJsonImporterLocator StockLocator;
        
        public static readonly IJsonImporter Byte;
        public static readonly IJsonImporter Int16;
        public static readonly IJsonImporter Int32;
        public static readonly IJsonImporter Int64;
        public static readonly IJsonImporter Single;
        public static readonly IJsonImporter Double;
        public static readonly IJsonImporter Decimal;
        public static readonly IJsonImporter String;
        public static readonly IJsonImporter Boolean;
        public static readonly IJsonImporter DateTime;
        public static readonly IJsonImporter Auto;
        public static readonly IJsonImporterLocator Array;
        public static readonly IJsonImporterLocator Enum;
        
        static JsonImporterStock()
        {
            JsonImporterRegistry registry = new JsonImporterRegistry(new NullImporterLocator());

            Byte = NumberImporter.Byte;         Byte.RegisterSelf(registry);
            Int16 = NumberImporter.Int16;       Int16.RegisterSelf(registry);
            Int32 = NumberImporter.Int32;       Int32.RegisterSelf(registry);
            Int64 = NumberImporter.Int64;       Int64.RegisterSelf(registry);
            Single = NumberImporter.Single;     Single.RegisterSelf(registry);
            Double = NumberImporter.Double;     Double.RegisterSelf(registry);
            Decimal = NumberImporter.Decimal;   Decimal.RegisterSelf(registry);
            String = new StringImporter();      String.RegisterSelf(registry);
            Boolean = new BooleanImporter();    Boolean.RegisterSelf(registry);
            DateTime = new DateTimeImporter();  DateTime.RegisterSelf(registry);
            Auto = new AutoImporter();          Auto.RegisterSelf(registry);
            Array = new ArrayImporter();        Array.RegisterSelf(registry);
            Enum = new EnumImporter();          Enum.RegisterSelf(registry);

            StockLocator = registry;
        }
        
        public static IJsonImporter Get(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            IJsonImporter importer = Find(type);

            if (importer == null)
                throw new JsonException(string.Format("There is no stock importer that can get {0} from JSON data.", type.FullName));
            
            return importer;
        }

        public static IJsonImporter Find(Type type)
        {
            return StockLocator.Find(type);
        }
        
        private JsonImporterStock()
        {
            throw new NotSupportedException();
        }
        
        /// <summary>
        /// An importer locator that never finds anything!
        /// </summary>
 
        private sealed class NullImporterLocator : IJsonImporterLocator
        {
            public IJsonImporter Find(Type type)
            {
                return null;
            }

            public void RegisterSelf(IJsonImporterRegistry registry)
            {
                registry.RegisterLocator(this);
            }
        }
    }
}