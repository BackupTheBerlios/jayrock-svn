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
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Diagnostics;

    #endregion

    public sealed class RpcServiceFeatureRegistry : NamedServiceContainer
    {
        public static NamedServiceContainer FromConfig()
        {
            IDictionary config = (IDictionary) ConfigurationSettings.GetConfig("jayrock/json.rpc/features");

            NamedServiceContainer registry = new RpcServiceFeatureRegistry();
            
            foreach (DictionaryEntry entry in config)
            {
                string typeSpec = Mask.NullString((string) entry.Value);
            
                if (typeSpec.Length == 0)
                    throw new JsonRpcException("Missing feature type specification.");

                registry.Add(entry.Key.ToString(), FeatureActivator.Create(Type.GetType(typeSpec, true)));
            }

            return registry;
        }

        private sealed class FeatureActivator
        {
            private Type _type;

            private FeatureActivator(Type type)
            {
                Debug.Assert(type != null);
                _type = type;
            }

            public static NamedServiceContainer.ActivationCallback Create(Type type)
            {
                FeatureActivator activator = new FeatureActivator(type);
                return new NamedServiceContainer.ActivationCallback(activator.Activate);
            }

            private object Activate(NamedServiceContainer container, string name)
            {
                object featureObject = Activator.CreateInstance(_type);

                IRpcServiceFeature feature = featureObject as IRpcServiceFeature;

                if (feature == null)
                    throw new JsonRpcException(string.Format("{0} is not a valid type for JSON-RPC.", featureObject.GetType().FullName));

                return feature;
            }
        }
    }
}
