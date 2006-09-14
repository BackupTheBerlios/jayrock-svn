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
        // JsonImporterStock.Locator.Find provides the same functionality.
        
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
        
        private static readonly JsonImporterRegistry _stockRegistry;
        [ ThreadStatic ] private static IJsonImporterRegistry _userStockRegistry;

        static JsonImporterStock()
        {
            _stockRegistry = new JsonImporterRegistry();
            
            //
            // Register importers for primitive types.
            //

             Byte = NumberImporter.Byte;         _stockRegistry.Register(Byte);
             Int16 = NumberImporter.Int16;       _stockRegistry.Register(Int16);
             Int32 = NumberImporter.Int32;       _stockRegistry.Register(Int32);
             Int64 = NumberImporter.Int64;       _stockRegistry.Register(Int64);
             Single = NumberImporter.Single;     _stockRegistry.Register(Single);
             Double = NumberImporter.Double;     _stockRegistry.Register(Double);
             Decimal = NumberImporter.Decimal;   _stockRegistry.Register(Decimal);
             String = new StringImporter();      _stockRegistry.Register(String);
             Boolean = new BooleanImporter();    _stockRegistry.Register(Boolean);
             DateTime = new DateTimeImporter();  _stockRegistry.Register(DateTime);
 
             //
             // Register the auto importer that automatically imports the
             // type based on what's coming in the JSON data.
             //
 
             Auto = new AutoImporter(); _stockRegistry.Register(Auto);
             
             //
             // Register for IDictionary and IList such that these yield
             // to JsonObject and JsonArray, respectively.
             //
             
             _stockRegistry.Register(new ImportableImporter(typeof(IDictionary), new ObjectCreationHandler(CreateJsonObject)));
             _stockRegistry.Register(new ImportableImporter(typeof(IList), new ObjectCreationHandler(CreateJsonArray)));
             
             //
             // Register importer that can handle types that implement
             // IJsonImportable.
             //
             
            _stockRegistry.Register(new ImportableBaseImporter());
             
             //
             // Register importers that dynamically handle arrays and enums.
             //
             
             Array = new ArrayBaseImporter(); _stockRegistry.Register(Array);
             Enum = new EnumBaseImporter(); _stockRegistry.Register(Enum);
        }

        public static IJsonImporterLocator Locator
        {
            get { return _stockRegistry; }
        }

        public static event ObjectInitializationEventHandler RegistryInitializing;

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
                
                if (_userStockRegistry == null)
                {
                    //
                    // NOTE: We register the stock importers by default
                    // so that the system does not appear completely void
                    // of any support for most basic and commonly used types.
                    //
                    
                    JsonImporterRegistry registry = new JsonImporterRegistry();
                    Locator.RegisterSelf(registry);
                    SetRegistry(registry, true);
                }
                
                return _userStockRegistry;
            }
        }

        /// <remarks>
        /// This method is exception-safe.
        /// </remarks>

        public static void SetRegistry(IJsonImporterRegistry registry)
        {
            SetRegistry(registry, false);
        }

        /// <remarks>
        /// This method is exception-safe.
        /// </remarks>

        public static void SetRegistry(IJsonImporterRegistry registry, bool initialize)
        {
            if (registry == null)
                throw new ArgumentNullException("registry");
                    
            if (initialize)
            {
                ObjectInitializationEventHandler handler = RegistryInitializing;
                    
                if (handler != null)
                    handler(typeof(JsonImporterRegistry), new ObjectInitializationEventArgs(registry));
            }
            
            _userStockRegistry = registry;
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
            return Locator.Find(type);
        }
        
        private JsonImporterStock()
        {
            throw new NotSupportedException();
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
    }
}
