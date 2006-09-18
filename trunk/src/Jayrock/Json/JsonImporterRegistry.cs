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
    
    public sealed class JsonImporterRegistry : IJsonImporterRegistry, IJsonImporterRegistrar, IJsonImporterSet
    {        
        private readonly Hashtable _importerByType = new Hashtable();
        private ArrayList _importerSetList;
        [ NonSerialized ] private IJsonImporterRegistryItem[] _cachedItems;
        
        public IJsonImporter Lookup(Type type)
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
        
            foreach (IJsonImporterSet importerSet in ImporterSets)
            {
                importer = importerSet.Lookup(type, this);
            
                if (importer != null)
                    break;
            }
            
            //
            // If an import was found, then register it so that it we don't
            // have to go through the same trouble of locating it again.
            //
            
            if (importer != null)
                Register(importer);
            
            return importer;
        }

        public void Register(IJsonImporterRegistryItem item)
        {
            OnRegistering();
            item.Register(this);
        }

        void IJsonImporterRegistrar.Register(Type type, IJsonImporter importer)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (importer == null)
                throw new ArgumentNullException("importer");
            
            _importerByType[type] = importer;
        }

        void IJsonImporterRegistrar.Register(IJsonImporterSet set)
        {
            if (set == null)
                throw new ArgumentNullException("set");
            
            ImporterSets.Insert(0, set);
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
        
        private ArrayList ImporterSets
        {
            get
            {
                if (_importerSetList == null)
                    _importerSetList = new ArrayList(4);
                
                return _importerSetList;
            }
        }

        private void OnRegistering()
        {
            _cachedItems = null;
        }

        private IJsonImporterRegistryItem[] GetItems()
        {
            //
            // Count total items and allocate an appropriately sized array.
            //
            
            int count = 0;

            if (_importerByType != null)
                count += _importerByType.Count;

            if (_importerSetList != null)
                count += _importerSetList.Count;

            IJsonImporterRegistryItem[] items = new IJsonImporterRegistryItem[count];
            
            //
            // Now copy items into the array.
            //

            int index = 0;

            if (_importerByType != null)
            {
                _importerByType.Values.CopyTo(items, index);
                index += _importerByType.Count;
            }

            if (_importerSetList != null)
            {
                _importerSetList.CopyTo(items, index);
                index += _importerSetList.Count;
            }

            Debug.Assert(index == count);
                
            return items;
        }

        IJsonImporter IJsonImporterSet.Lookup(Type type, IJsonImporterLookup site)
        {
            return Lookup(type);
        }

        void IJsonImporterRegistryItem.Register(IJsonImporterRegistrar registrar)
        {
            registrar.Register(this);
        }
    }
}