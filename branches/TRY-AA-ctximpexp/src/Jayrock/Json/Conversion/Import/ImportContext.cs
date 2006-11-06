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
    using System.Collections;
    using System.Threading;
    using Jayrock.Json.Conversion.Import.Importers;

    #endregion

    [ Serializable ]
    public class ImportContext
    {
        private ITypeImporterBinder _importerBinder;

        private static object _defaultImporterBinder;
        
        public ITypeImporterBinder ImporterBinder
        {
            get
            {
                if (_importerBinder == null)
                    _importerBinder = DefaultImporterBinder;
                
                return _importerBinder;
            }
            
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                
                _importerBinder = value;
            }
        }

        public virtual object ImportAny(JsonReader reader)
        {
            return Import(AnyType.Value, reader);
        }

        public virtual object Import(Type type, JsonReader reader)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            ITypeImporter importer = ImporterBinder.Bind(this, type);

            if (importer == null)
                throw new JsonException(string.Format("Don't know how to import {0}.", type.FullName));

            reader.MoveToContent();
            return importer.Import(this, reader);
        }

        private static ITypeImporterBinder DefaultImporterBinder
        {
            get
            {
                if (_defaultImporterBinder == null)
                {
                    TypeImporterCollection bindings = new TypeImporterCollection();

                    bindings.Add(new ByteImporter());
                    bindings.Add(new Int16Importer());
                    bindings.Add(new Int32Importer());
                    bindings.Add(new Int64Importer());
                    bindings.Add(new SingleImporter());
                    bindings.Add(new DoubleImporter());
                    bindings.Add(new DecimalImporter());
                    bindings.Add(new StringImporter());
                    bindings.Add(new BooleanImporter());
                    bindings.Add(new DateTimeImporter());
                    bindings.Add(new AutoImporter());
                    bindings.Add(new DictionaryImporter());
                    bindings.Add(new ListImporter());

                    TypeImporterBinderCollection binders = new TypeImporterBinderCollection();

                    binders.Add(bindings);
                    binders.Add(new ImportAwareImporterFamily());
                    binders.Add(new ArrayImporterFamily());
                    binders.Add(new EnumImporterFamily());
                    binders.Add(new ComponentImporterFamily());

                    Interlocked.CompareExchange(ref _defaultImporterBinder, binders, null);
                }
                
                return (ITypeImporterBinder) _defaultImporterBinder;
            }
        }
    }
}