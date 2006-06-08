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
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;

    #endregion

    public class RpcServiceContext : IServiceProvider // TODO: Add IRpcServiceBound
    {
        private readonly ServiceContainer _contextServices;
        private readonly Container _container;
        private readonly IRpcService _service;

        public RpcServiceContext(IRpcService service)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            _service = service;
            _contextServices = new ServiceContainer();
            _contextServices.AddService(typeof(IRpcService), service);
            _container = new Container(this);

            IComponent component = service as IComponent;

            if (component != null)
                _container.Add(component);
        }

        public static RpcServiceContext Find(IDictionary map)
        {
            if (map == null)
                return null;

            return (RpcServiceContext) map[typeof(RpcServiceContext)];
        }

        public static RpcServiceContext Get(IDictionary map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            RpcServiceContext context = Find(map);

            if (context == null)
                ServiceNotAvailableException.ThrowFormatted(typeof(RpcServiceContext));

            return context;
        }

        public IServiceContainer ContextServices
        {
            get { return _contextServices; }
        }

        public IRpcService Service
        {
            get { return _service; }
        }

        public virtual object GetService(Type serviceType)
        {
            return ServiceQuery.FindByType(ContextServices, serviceType);
        }

        public override string ToString()
        {
            IRpcService service = Service;
            
            if (service == null)
                return base.ToString();

            return "Host for " + JsonRpcServices.GetServiceName(service);
        }

        private sealed class Container : System.ComponentModel.Container
        {
            private RpcServiceContext _host;

            public Container(RpcServiceContext context)
            {
                _host = context;
            }

            protected override object GetService(Type service)
            {
                object serviceObject = ServiceQuery.FindByType(_host.ContextServices, service);
                return serviceObject != null ? serviceObject : base.GetService(service);
            }
        }
    }
}
