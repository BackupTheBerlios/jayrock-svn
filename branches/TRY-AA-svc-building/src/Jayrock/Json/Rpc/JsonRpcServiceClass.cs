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

    #endregion

    [ Serializable ]
    public sealed class JsonRpcServiceClass
    {
        private readonly string _serviceName;
        private readonly string _description;
        private readonly Hashtable _methodByName;
        private readonly JsonRpcMethod[] _methods;
        private static readonly Hashtable _classByTypeCache = Hashtable.Synchronized(new Hashtable());
        
        public static JsonRpcServiceClass FromType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            JsonRpcServiceClass clazz = (JsonRpcServiceClass) _classByTypeCache[type];

            if (clazz == null)
            {
                clazz = JsonRpcServiceReflector.FromType(type);
                _classByTypeCache[type] = clazz;
            }

            return clazz;
        }

        internal JsonRpcServiceClass(JsonRpcServiceClassBuilder classBuilder)
        {
            Debug.Assert(classBuilder != null);
            
            _serviceName = classBuilder.Name;
            _description = classBuilder.Description;

            JsonRpcMethodBuilder[] methodBuilders = classBuilder.GetMethods();
            
            _methods = new JsonRpcMethod[methodBuilders.Length];
            _methodByName = new Hashtable(methodBuilders.Length);
            int methodIndex = 0;

            foreach (JsonRpcMethodBuilder methodBuilder in methodBuilders)
            {
                JsonRpcMethod method = new JsonRpcMethod(methodBuilder, this);

                //
                // Check for duplicates.
                //

                if (_methodByName.ContainsKey(method.Name))
                    throw new DuplicateMethodException(string.Format("The method '{0}' cannot be exported as '{1}' because this name has already been used by another method on the '{2}' service.", method.Name, method.InternalName, _serviceName));

                //
                // Add the method to the class and index it by its name.
                //

                _methods[methodIndex++] = method;
                _methodByName.Add(method.Name, method);
            }
        }

        public string Name
        {
            get { return _serviceName; }
        }

        public string Description
        {
            get { return _description; }
        }

        public JsonRpcMethod[] GetMethods()
        {
            //
            // IMPORTANT! Never return the private array instance since the
            // caller could modify its state and compromise the integrity as
            // well as the assumptions made in this implementation.
            //

            return (JsonRpcMethod[]) _methods.Clone();
        }

        public JsonRpcMethod FindMethodByName(string name)
        {
            return (JsonRpcMethod) _methodByName[name];
        }

        public JsonRpcMethod GetMethodByName(string name)
        {
            JsonRpcMethod method = FindMethodByName(name);

            if (method == null)
                throw new MethodNotFoundException();

            return method;
        }
    }
}
