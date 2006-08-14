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

    public sealed class JsonImporterStock : IJsonImporterLocator
    {
        public readonly static IJsonImporterLocator Locator = new JsonImporterStock();
        
        public static readonly IJsonImporter Byte = NumberImporter.Byte;
        public static readonly IJsonImporter Int16 = NumberImporter.Int16;
        public static readonly IJsonImporter Int32 = NumberImporter.Int32;
        public static readonly IJsonImporter Int64 = NumberImporter.Int64;
        public static readonly IJsonImporter Single = NumberImporter.Single;
        public static readonly IJsonImporter Double = NumberImporter.Double;
        public static readonly IJsonImporter Decimal = NumberImporter.Decimal;
        public static readonly IJsonImporter String = new StringImporter();
        public static readonly IJsonImporter Boolean = new BooleanImporter();
        public static readonly IJsonImporter DateTime = new DateTimeImporter();
        public static readonly IJsonImporterLocator Array = new ArrayImporter();
        public static readonly IJsonImporter Auto = new AutoImporter();
        
        private static readonly IJsonImporter[] _importers = 
        {
            null,    // Empty = 0     - Null reference
            null,    // Object = 1    - Instance that isn't a value
            null,    // DBNull = 2    - Database null value
            Boolean, // Boolean = 3   - Boolean
            null,    // Char = 4      - Unicode character
            null,    // SByte = 5     - Signed 8-bit integer
            Byte,    // Byte = 6      - Unsigned 8-bit integer
            Int16,   // Int16 = 7     - Signed 16-bit integer
            null,    // UInt16 = 8    - Unsigned 16-bit integer
            Int32,   // Int32 = 9     - Signed 32-bit integer
            null,    // UInt32 = 10   - Unsigned 32-bit integer
            Int64,   // Int64 = 11    - Signed 64-bit integer
            null,    // UInt64 = 12   - Unsigned 64-bit integer
            Single,  // Single = 13   - IEEE 32-bit float
            Double,  // Double = 14   - IEEE 64-bit double
            Decimal, // Decimal = 15  - Decimal
            DateTime,// DateTime = 16 - DateTime
            null,
            String   // String = 18   - Unicode character string
        };  
        
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
            if (type == null)
                throw new ArgumentNullException("type");

            if (type.IsArray)
                return Array.Find(type);
            
            if (type == typeof(object))
                return JsonImporterStock.Auto;
            
            return Find(Type.GetTypeCode(type));
        }

        private static IJsonImporter Find(TypeCode typeCode)
        {
            int index = (int) typeCode;
            
            Debug.Assert(index >= 0 && index < _importers.Length);
            
            return _importers[index];            
        }

        IJsonImporter IJsonImporterLocator.Find(Type type)
        {
            return JsonImporterStock.Find(type);
        }

        void IJsonImporterRegistryTargetable.RegisterSelf(IJsonImporterRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");
            
            registry.RegisterLocator(this);
        }
 
        private JsonImporterStock() {}
    }
}