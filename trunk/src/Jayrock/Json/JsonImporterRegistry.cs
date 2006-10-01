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
        private readonly Hashtable _importerByTypeCache = new Hashtable();
        private ArrayList _importerSetList;
        private object[] _cachedRegistrations;
        
        public IJsonImporter Find(Type type)
        {
            //
            // First look up the cache.
            //
            
            IJsonImporter importer = (IJsonImporter) _importerByTypeCache[type];
            
            if (importer != null)
                return importer;

            //
            // Now look up explicit type registrations.
            //
            
            importer = (IJsonImporter) _importerByType[type];
            
            if (importer == null)
            {
                //
                // No importer found using an exact matching type, so we ask
                // the set of "chained" sets to page one.
                //
            
                foreach (IJsonImporterSet importerSet in ImporterSets)
                {
                    importer = importerSet.Page(type);
                
                    if (importer != null)
                        break;
                }
            }
            
            //
            // If an import was found, then register it so that it we don't
            // have to go through the same trouble of locating it again.
            //
            
            if (importer != null)
                _importerByTypeCache.Add(type, importer);
            
            return importer;
        }

        public void Register(IJsonImporter importer)
        {
            if (importer == null)
                throw new ArgumentNullException("importer");
            
            InvalidateCache();
            _importerByType[importer.OutputType] = importer;
        }

        public void Register(IJsonImporterSet set)
        {
            if (set == null)
                throw new ArgumentNullException("set");
            
            InvalidateCache();
            ImporterSets.Add(set);
        }

        private void InvalidateCache()
        {
            _importerByTypeCache.Clear();
            _cachedRegistrations = null;
        }

        private bool HasImporterSets
        {
            get { return _importerSetList != null && _importerSetList.Count > 0; }
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

        public int Count
        {
            get { return _importerByType.Count + (HasImporterSets ? ImporterSets.Count : 0); }
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
            if (_cachedRegistrations == null)
            {
                object[] registrations = new object[Count];

                ICollection importers = _importerByType.Values;
                importers.CopyTo(registrations, 0);

                if (HasImporterSets)
                    ImporterSets.CopyTo(registrations, importers.Count);
                
                _cachedRegistrations = registrations;
            }
            
            return _cachedRegistrations;
        }

        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }
    }
}