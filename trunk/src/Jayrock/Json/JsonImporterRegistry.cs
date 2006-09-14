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
    using System.Diagnostics;
    using Jayrock.Json.Importers;

    #endregion

    public sealed class JsonImporterRegistry : IJsonImporterRegistry
    {
        private readonly Hashtable _importerByType = new Hashtable();
        private ArrayList _locators;
        [ NonSerialized ] private IJsonImporterRegistryTargetable[] _cachedItems;

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
            
            OnRegistering();
            
            _importerByType[type] = importer;
        }

        public void RegisterLocator(IJsonImporterLocator locator)
        {
            if (locator == null)
                throw new ArgumentNullException("locator");
            
            if (locator == this)
                throw new ArgumentException("Locator to register cannot be the registry itself.");
            
            if (Locators.Contains(locator))
                throw new ArgumentException("Locator is already registered.");
            
            OnRegistering();
            Locators.Insert(0, locator);
        }

        public void RegisterSelf(IJsonImporterRegistry registry)
        {
            registry.RegisterLocator(this);
        }
        
        //
        // This property is weakly typed so that the caller cannot assume the
        // actual type here. You see, we're returning a direct reference to 
        // an internal array that could be modified by the caller if the 
        // property was also typed as an array. With a weaker type, if the
        // caller wants to modify the collection then a copy must first be
        // made into a private array via ICollection.CopyTo.
        //
        
        internal ICollection Items
        {
            get
            {
                if (_cachedItems == null)
                    _cachedItems = GetItems();

                return _cachedItems;
            }
        }
        
        private ArrayList Locators
        {
            get
            {
                if (_locators == null)
                    _locators = new ArrayList(4);
                
                return _locators;
            }
        }

        private void OnRegistering()
        {
            _cachedItems = null;
        }

        private IJsonImporterRegistryTargetable[] GetItems()
        {
            //
            // Count total items and allocate an appropriately sized array.
            //
            
            int count = 0;

            if (_importerByType != null)
                count += _importerByType.Count;

            if (_locators != null)
                count += _locators.Count;

            IJsonImporterRegistryTargetable[] items = new IJsonImporterRegistryTargetable[count];
            
            //
            // Now copy items into the array.
            //

            int index = 0;

            if (_importerByType != null)
            {
                _importerByType.Values.CopyTo(items, index);
                index += _importerByType.Count;
            }

            if (_locators != null)
            {
                _locators.CopyTo(items, index);
                index += _locators.Count;
            }

            Debug.Assert(index == count);
                
            return items;
        }
    }
}