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

namespace Jayrock.Json.Rpc
{
    #region Imports

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    [ Serializable ]
    internal sealed class RpcServiceDescriptor : IRpcServiceDescriptor
    {
        private readonly Type _serviceType;
        private readonly string _serviceName;
        private readonly Hashtable _nameMethodTable;
        private readonly RpcMethodDescriptor[] _methods;
        private static readonly Hashtable _typeDescriptorCache = Hashtable.Synchronized(new Hashtable());
        
        public static RpcServiceDescriptor GetDescriptor(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            RpcServiceDescriptor descriptor = (RpcServiceDescriptor) _typeDescriptorCache[type];

            if (descriptor == null)
            {
                descriptor = new RpcServiceDescriptor(type);
                _typeDescriptorCache[type] = descriptor;
            }

            return descriptor;
        }

        private RpcServiceDescriptor(Type type)
        {
            Debug.Assert(type != null);

            _serviceType = type;

            //
            // Determine the service name, allowing customization via the
            // JsonRpcService attribute.
            //

            JsonRpcServiceAttribute serviceAttribute = (JsonRpcServiceAttribute) Attribute.GetCustomAttribute(type, typeof(JsonRpcServiceAttribute), true);

            if (serviceAttribute != null)
                _serviceName = serviceAttribute.Name.Trim();

            _serviceName = Mask.EmptyString(_serviceName, type.Name);

            //
            // Get all the public instance methods on the type and create a
            // filtered table of those to expose from the service.
            //

            MethodInfo[] publicMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            _nameMethodTable = new Hashtable(publicMethods.Length);

            foreach (MethodInfo method in publicMethods)
            {
                if (method.IsAbstract)
                    continue;

                JsonRpcMethodAttribute methodAttribute = (JsonRpcMethodAttribute) Attribute.GetCustomAttribute(method, typeof(JsonRpcMethodAttribute));

                //
                // Only proceed with methods that are decorated with the
                // JsonRpcMethod attribute.
                //
                
                if (methodAttribute == null)
                    continue;

                //
                // Determine the external/public name of the method. This
                // usually comes from the attribute, but if missing then from
                // the actual base method name.
                //

                string externalName = methodAttribute.Name;

                if (externalName.Length == 0)
                    externalName = method.Name;

                //
                // Check for duplicates.
                //

                if (_nameMethodTable.ContainsKey(externalName))
                    throw new DuplicateMethodException(string.Format("The method '{0}' cannot be exported as '{1}' because this name has already been used by another method on the '{2}' service.", method.Name, externalName, _serviceName));

                //
                // Finally, add the name to the table along with a method
                // descriptor.
                //

                _nameMethodTable.Add(externalName, new RpcMethodDescriptor(this, method));
            }

            _methods = new RpcMethodDescriptor[_nameMethodTable.Count];
            _nameMethodTable.Values.CopyTo(_methods, 0);
        }

        public string Name
        {
            get { return _serviceName; }
        }

        public RpcMethodDescriptor[] GetMethods()
        {
            //
            // IMPORTANT! Never return the private array instance since the
            // caller could modify its state and compromise the integrity as
            // well as the assumptions made in this implementation.
            //

            return (RpcMethodDescriptor[]) _methods.Clone();
        }

        public RpcMethodDescriptor FindMethodByName(string name)
        {
            return (RpcMethodDescriptor) _nameMethodTable[name];
        }

        public RpcMethodDescriptor GetMethodByName(string name)
        {
            RpcMethodDescriptor method = FindMethodByName(name);

            if (method == null)
                throw new MethodNotFoundException();

            return method;
        }

        IRpcMethodDescriptor[] IRpcServiceDescriptor.GetMethods()
        {
            return GetMethods();
        }

        IRpcMethodDescriptor IRpcServiceDescriptor.FindMethodByName(string name)
        {
            return FindMethodByName(name);
        }

        IRpcMethodDescriptor IRpcServiceDescriptor.GetMethodByName(string name)
        {
            return GetMethodByName(name);
        }

        public ICustomAttributeProvider AttributeProvider
        {
            get { return _serviceType; }
        }
    }
}
