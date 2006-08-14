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
    using Jayrock.Json.Importers;

    #endregion

    public sealed class JsonImporterRegistry : IJsonImporterRegistry
    {
        private readonly Hashtable _importerByType = new Hashtable();
        private ArrayList _locators;

        public IJsonImporter Find(Type type)
        {
            //
            // First look up in the table of specific type registrations.
            //
            
            IJsonImporter importer = (IJsonImporter) _importerByType[type];
            
            if (importer != null)
                return importer;
            
            //
            // No importer found using an exact matching type, so we ask
            // the set of "chained" locators to find one.
            //
            
            foreach (IJsonImporterLocator locator in Locators)
            {
                importer = locator.Find(type);
                
                if (importer != null)
                    break;
            }
            
            if (importer == null)
            {
                //
                // If still no importer found, then check for importer
                // specification via a custom attriubte.
                //

                object[] attributes = type.GetCustomAttributes(typeof(IJsonImporterLocator), true);
                
                if (attributes.Length > 0)
                    importer = ((IJsonImporterLocator) attributes[0]).Find(type);
            }
            
            //
            // If an import was found, then register it so that it we don't
            // have to go through the same trouble of locating it again.
            //
            
            if (importer != null)
                importer.RegisterSelf(this);
            
            return importer;
        }

        public void Register(Type type, IJsonImporter importer)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (importer == null)
                throw new ArgumentNullException("importer");
            
            _importerByType.Add(type, importer);
        }

        public void RegisterLocator(IJsonImporterLocator locator)
        {
            if (locator == null)
                throw new ArgumentNullException("locator");
            
            if (locator == this)
                throw new ArgumentException("Locator to register cannot be the registry itself.");
            
            if (Locators.Contains(locator))
                throw new ArgumentException("Locator is already registered.");
            
            Locators.Insert(0, locator);
        }

        public void RegisterSelf(IJsonImporterRegistry registry)
        {
            registry.RegisterLocator(this);
        }

        private ArrayList Locators
        {
            get
            {
                if (_locators == null)
                {
                    _locators = new ArrayList(4);

                    //
                    // Kindly register the stock locator so that known and 
                    // common type importers are conveniently served. 
                    // Otherwise, the system just appears too dumb out of
                    // the box.
                    //
                
                    JsonImporterStock.Locator.RegisterSelf(this);
                }
                
                return _locators;
            }
        }
    }
}