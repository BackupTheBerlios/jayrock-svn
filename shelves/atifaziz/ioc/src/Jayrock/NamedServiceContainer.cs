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

namespace Jayrock
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    public class NamedServiceContainer : NameObjectCollectionBase
    {
        public delegate object ActivationCallback(NamedServiceContainer container, string name);

        private Hashtable _activatorByName = new Hashtable();
        private static readonly object _nullService = new object();

        public NamedServiceContainer() : base(null, null) { }

        public object this[string name] { get { return Get(name); } }

        public void Add(string name, Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            name = Mask.NullString(name);
            _activatorByName.Add(name, serviceType);
            BaseAdd(name, _nullService);
        }

        public void Add(string name, object service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            BaseAdd(Mask.NullString(name), service);
        }

        public void Add(string name, ActivationCallback callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            name = Mask.NullString(name);
            _activatorByName.Add(name, callback);
            BaseAdd(name, _nullService);
        }

        public void Remove(string name)
        {
            name = Mask.NullString(name);
            BaseRemove(name);            
            _activatorByName.Remove(name);
        }
        
        public void Clear()
        {
            BaseClear();
            _activatorByName.Clear();
        }

        public void CopyTo(Array target, int index)
        {
            BaseGetAllKeys().CopyTo(target, index);
        }

        public object Get(string name)
        {
            if (!BaseHasKeys())
                return null;

            name = Mask.NullString(name);
            object service = BaseGet(name);

            //
            // Is the service inactive?
            //

            if (_nullService.Equals(service))
            {
                //
                // Get the activation method and invoke it.
                //

                object activator = _activatorByName[name];
                
                Type type = activator as Type;

                if (type != null)
                {
                    service = Activator.CreateInstance(type);
                }
                else
                {
                    ActivationCallback callback = (ActivationCallback) _activatorByName[name];
                    service = callback(this, name);
                }

                //
                // Install the new service instance in the main table so
                // that it will be found on next request. Also remove 
                // its activator as it won't be needed anymore.
                //

                BaseSet(name, service);
                _activatorByName.Remove(name);
            }

            return service;
        }

        public bool Has(string name)
        {
            return Get(name) != null;
        }
    }
}