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

namespace Jayrock.Json.Conversion
{
    #region Imports

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;

    #endregion
    
    /// <summary>
    /// This type supports the library infrastructure and is not intended to 
    /// be used directly from your code.
    /// </summary>

    [ Serializable ]
    public abstract class JsonConverterCollection : ICollection
    {
        private readonly Hashtable _traderByType = new Hashtable();
        private ArrayList _familyList;
        [ NonSerialized ] private readonly Hashtable _traderByTypeCache = Hashtable.Synchronized(new Hashtable());
        [ NonSerialized ] private object[] _cachedRegistrations;
        [ NonSerialized ] private object _syncRoot;

        protected object BaseFind(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            //
            // First look up cache for previously served requests.
            //
            
            object trader = _traderByTypeCache[type];
            
            if (trader != null)
                return trader;

            //
            // Next, look up explicit type registrations.
            //
            
            trader = _traderByType[type];
            
            if (trader == null)
            {
                //
                // No luck using an exact matching type so far so we ask
                // the set of "chained" families to page one. The first one 
                // to respond concludes the search.
                //
            
                foreach (object family in Families)
                {
                    trader = Page(family, type);
                
                    if (trader != null)
                        break;
                }
            }
            
            //
            // If found one, then cache the mapping so that we 
            // don't have to go through the same trouble of locating it 
            // again.
            //
            
            if (trader != null)
                _traderByTypeCache.Add(type, trader);
            
            return trader;
        }

        protected abstract object Page(object family, Type type);

        protected void Register(Type type, object trader)
        {
            if (trader == null)
                throw new ArgumentNullException("trader");
            
            InvalidateCaches();
            _traderByType[type] = trader;
        }

        protected void RegisterFamily(object family)
        {
            if (family == null)
                throw new ArgumentNullException("family");
            
            InvalidateCaches();
            Families.Add(family);
        }

        protected void InvalidateCaches()
        {
            _traderByTypeCache.Clear();
            _cachedRegistrations = null;
        }

        private bool HasFamilies
        {
            get { return _familyList != null && _familyList.Count > 0; }
        }

        protected ArrayList Families
        {
            get
            {
                if (_familyList == null)
                    _familyList = new ArrayList(4);
                
                return _familyList;
            }
        }

        public int Count
        {
            get { return _traderByType.Count + (HasFamilies ? Families.Count : 0); }
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

                ICollection importers = _traderByType.Values;
                importers.CopyTo(registrations, 0);

                if (HasFamilies)
                    Families.CopyTo(registrations, importers.Count);
                
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

        public void Configure(ICollection items)
        {
            if (items != null && items.Count > 0)
                items = EnsureObjects(items);
            else
                items = GetDefaultConfiguration();

            foreach (object item in items)
            {
                if (item == null)
                    continue;
                
                try
                {
                    Register(item);
                }
                catch (ArgumentException e)
                {
                    throw new ConfigurationException(e.Message, e);
                }
            }
        }

        protected abstract ICollection GetDefaultConfiguration();

        protected abstract void Register(object item);

        private static IList EnsureObjects(ICollection typeSpecs)
        {
            Debug.Assert(typeSpecs != null);
            
            ArrayList objectList = new ArrayList(typeSpecs.Count);
            
            foreach (string typeSpec in typeSpecs)
            {
                if (Mask.NullString(typeSpec).Length == 0)
                    continue;

                objectList.Add(Activator.CreateInstance(Compat.GetType(typeSpec)));
            }
            
            return objectList;
        }
    }
}