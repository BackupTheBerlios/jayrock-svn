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
    
    [ Serializable ]
    public sealed class JsonImporterRegistry : IJsonImporterRegistry
    {        
        private readonly Hashtable _importerByType = new Hashtable();
        private ArrayList _importerFamilyList;
        [ NonSerialized ] private readonly Hashtable _importerByTypeCache = new Hashtable();
        [ NonSerialized ] private object[] _cachedRegistrations;
        [ NonSerialized ] private object _syncRoot;

        public IJsonImporter Find(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            //
            // First look up a previously served requests.
            //
            
            IJsonImporter importer = (IJsonImporter) _importerByTypeCache[type];
            
            if (importer != null)
                return importer;

            //
            // Next, look up explicit type registrations.
            //
            
            importer = (IJsonImporter) _importerByType[type];
            
            if (importer == null)
            {
                //
                // No importer found using an exact matching type so we ask
                // the set of "chained" families to page one. The first one 
                // to respond concludes the search.
                //
            
                foreach (IJsonImporterFamily importerFamily in ImporterFamilies)
                {
                    importer = importerFamily.Page(type);
                
                    if (importer != null)
                        break;
                }
            }
            
            //
            // If an import was found, then cache the mapping so that we 
            // don't have to go through the same trouble of locating it 
            // again.
            //
            
            if (importer != null)
                _importerByTypeCache.Add(type, importer);
            
            return importer;
        }

        public void Register(IJsonImporter importer)
        {
            if (importer == null)
                throw new ArgumentNullException("importer");
            
            InvalidateCaches();
            _importerByType[importer.OutputType] = importer;
        }

        public void Register(IJsonImporterFamily importerFamily)
        {
            if (importerFamily == null)
                throw new ArgumentNullException("importerFamily");
            
            InvalidateCaches();
            ImporterFamilies.Add(importerFamily);
        }

        private void InvalidateCaches()
        {
            _importerByTypeCache.Clear();
            _cachedRegistrations = null;
        }

        private bool HasImporterSets
        {
            get { return _importerFamilyList != null && _importerFamilyList.Count > 0; }
        }

        private ArrayList ImporterFamilies
        {
            get
            {
                if (_importerFamilyList == null)
                    _importerFamilyList = new ArrayList(4);
                
                return _importerFamilyList;
            }
        }

        public int Count
        {
            get { return _importerByType.Count + (HasImporterSets ? ImporterFamilies.Count : 0); }
        }
        
        public IEnumerator GetEnumerator()
        {
            return GetRegistrations().GetEnumerator();
        }

        public void CopyTo(Array array, int index)
        {
            GetRegistrations().CopyTo(array, index);
        }
        
        private object[] GetRegistrations()
        {
            //
            // If registrations have been previously requested then a cache
            // is already available and used. Otherwise it is built on
            // first request for all registrations.
            //
            
            if (_cachedRegistrations == null)
            {
                object[] registrations = new object[Count];

                ICollection importers = _importerByType.Values;
                importers.CopyTo(registrations, 0);

                if (HasImporterSets)
                    ImporterFamilies.CopyTo(registrations, importers.Count);
                
                _cachedRegistrations = registrations;
            }
            
            return _cachedRegistrations;
        }

        object ICollection.SyncRoot
        {
            get 
            {
                if (_syncRoot == null)
                    System.Threading.Interlocked.CompareExchange(ref _syncRoot, new object(), null);

                return _syncRoot; 
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }
    }
}