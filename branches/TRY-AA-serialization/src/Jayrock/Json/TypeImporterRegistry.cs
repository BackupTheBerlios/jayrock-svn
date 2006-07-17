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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Collections;

    #endregion

    public sealed class TypeImporterRegistry : ITypeImporterRegistry
    {
        private readonly Hashtable _importerByType = new Hashtable();
        private readonly Hashtable _factoryByType = new Hashtable();

        public ITypeImporter Find(Type type)
        {
            ITypeImporter importer = (ITypeImporter) _importerByType[type];
            
            if (importer == null)
            {
                foreach (DictionaryEntry entry in _factoryByType)
                {
                    Type wideType = (Type) entry.Key;
                
                    if (wideType.IsAssignableFrom(type))
                    {
                        ITypeImporterFactory factory = (ITypeImporterFactory) entry.Value;
                        importer = factory.Create(type);
                        importer.Register(this);
                        break;
                    }
                }
            }
            
            return importer;
        }

        public void Register(Type type, ITypeImporter importer)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (importer == null)
                throw new ArgumentNullException("importer");
            
            _importerByType.Add(type, importer);
        }

        public void RegisterFactory(Type type, ITypeImporterFactory factory)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (factory == null)
                throw new ArgumentNullException("factory");
            
            _factoryByType.Add(type, factory);
        }
    }
}