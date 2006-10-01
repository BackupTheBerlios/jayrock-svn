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
    using System.Collections;
    using System.Diagnostics;
    using Jayrock.Json.Importers;

    #endregion

    public sealed class JsonImporterStock
    {
        // TODO: Review if these are still needed.
        // JsonImporterStock.Find provides the same functionality.
        
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
        public static readonly IJsonImporter IList;
        public static readonly IJsonImporter IDictionary;
        public static readonly IJsonImporter Auto;
        public static readonly ArrayImporterSet Array;
        public static readonly EnumImporterSet Enum;
        
        private static readonly JsonImporterRegistry _stockRegistry;
        [ ThreadStatic ] private static IJsonImporterRegistry _userRegistry;

        static JsonImporterStock()
        {
            Byte = NumberImporter.Byte;         
            Int16 = NumberImporter.Int16;       
            Int32 = NumberImporter.Int32;       
            Int64 = NumberImporter.Int64;       
            Single = NumberImporter.Single;     
            Double = NumberImporter.Double;     
            Decimal = NumberImporter.Decimal;   
            String = new StringImporter();      
            Boolean = new BooleanImporter();    
            DateTime = new DateTimeImporter();
            IDictionary = new ImportableImporter(typeof(IDictionary), new ObjectCreationHandler(CreateJsonObject));
            IList = new ImportableImporter(typeof(IList), new ObjectCreationHandler(CreateJsonArray));
            Auto = new AutoImporter(); 
            Array = new ArrayImporterSet(); 
            Enum = new EnumImporterSet();
                        
            JsonImporterRegistry registry = new JsonImporterRegistry();
            Register(registry);
            _stockRegistry = registry;
        }

        public static event ValueChangingEventHandler RegistryChanging;

        public static IJsonImporterRegistry Registry
        {
            get
            {
                // FIXME: Detect re-entrancy.
                // Re-entrancy can occur is someone tries to access this
                // property from within the DefaultInitialization event
                // handlers, causing the same chain of event to occur. This
                // would happen because the new registry is not installed 
                // until *after* the event has finished firing.
                
                if (_userRegistry == null)
                {
                    //
                    // NOTE: We register the stock importers by default
                    // so that the system does not appear completely void
                    // of any support for most basic and commonly used types.
                    //
                    
                    JsonImporterRegistry registry = new JsonImporterRegistry();
                    Register(registry);
                    SetRegistry(registry);
                }
                
                return _userRegistry;
            }
        }

        /// <remarks>
        /// This method is exception-safe.
        /// </remarks>

        public static void SetRegistry(IJsonImporterRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");
                    
            ValueChangingEventHandler handler = RegistryChanging;
                
            if (handler != null)
                handler(typeof(JsonImporterStock), new ValueChangingEventArgs(registry));
            
            _userRegistry = registry;
        }

        public static void Register(IJsonImporterRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");
            
            registry.Register(Byte);
            registry.Register(Int16);
            registry.Register(Int32);
            registry.Register(Int64);
            registry.Register(Single);
            registry.Register(Double);
            registry.Register(Decimal);
            registry.Register(String);
            registry.Register(Boolean);
            registry.Register(DateTime);
            registry.Register(Auto);
            registry.Register(IDictionary);
            registry.Register(IList);
            
            registry.Register(new ImportableImporterSet());
            registry.Register(Array);
            registry.Register(Enum);
        }

        public static IJsonImporter Get(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            IJsonImporter importer = Lookup(type);

            if (importer == null)
                throw new JsonException(string.Format("There is no stock importer that can get {0} from JSON data.", type.FullName));
            
            return importer;
        }

        public static IJsonImporter Lookup(Type type)
        {
            return _stockRegistry.Find(type);
        }
        
        private static object CreateJsonObject(object[] args)
        {
            Debug.Assert(args == null || args.Length == 0);
            
            return new JsonObject();
        }

        private static object CreateJsonArray(object[] args)
        {
            Debug.Assert(args == null || args.Length == 0);

            return new JsonArray();
        }

        private JsonImporterStock()
        {
            throw new NotSupportedException();
        }
    }
}
