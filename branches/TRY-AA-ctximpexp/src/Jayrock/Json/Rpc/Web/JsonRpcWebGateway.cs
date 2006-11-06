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

namespace Jayrock.Json.Rpc.Web
{
    #region Imports

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Web;

    #endregion

    public class JsonRpcWebGateway
    {
        private readonly HttpContext _context;
        private readonly IService _service;
        private IDictionary _bindingByName;
        private bool _bindingsInitialized;

        public JsonRpcWebGateway(HttpContext context, IService service)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (service == null)
                throw new ArgumentNullException("service");
            
            _context = context;
            _service = service;
        }

        public HttpContext Context
        {
            get { return _context; }
        }

        public IService Service
        {
            get { return _service; }
        }

        public virtual void ProcessRequest()
        {
            IServiceBinding binding = InferBinding();

            if (binding == null)
                throw new JsonRpcException("There is no service binding available for this type of request.");

            IHttpHandler handler = binding as IHttpHandler;
        
            if (handler == null)
                throw new JsonRpcException(string.Format("The {0} binding does not support HTTP.", binding.GetType().FullName));

            handler.ProcessRequest(Context);
        }

        protected virtual IServiceBinding InferBinding()
        {
            HttpRequest request = Context.Request;
            string verb = request.RequestType;

            if (CaselessString.Equals(verb, "GET") ||
                CaselessString.Equals(verb, "HEAD"))
            {
                //
                // If there is path tail then it indicates a GET-safe RPC.
                //
                
                if (request.PathInfo.Length > 1)
                    return GetBindingByName("getrpc");
                
                //
                // Otherwise, get the binding name from anonymous query 
                // string parameter.
                //

                return GetBindingByName(Mask.EmptyString(request.QueryString[null], "help"));
            }
            else if (CaselessString.Equals(verb, "POST")) 
            {
                //
                // POST means RPC.
                //
                
                return GetBindingByName("rpc");
            }

            return null;
        }

        protected IDictionary Bindings
        {
            get
            {
                if (!_bindingsInitialized)
                {
                    _bindingsInitialized = true;
                    _bindingByName = GetBindings();
                }

                return _bindingByName;
            }
        }

        protected virtual IDictionary GetBindings()
        {
            object config = ConfigurationSettings.GetConfig("jayrock/json.rpc/bindings");
            
            if (config == null)
            {
                //
                // Check an alternate path for backward compatibility.
                //
                
                config = ConfigurationSettings.GetConfig("jayrock/json.rpc/features");
            }
            
            return (IDictionary) config;
        }

        protected virtual IServiceBinding GetBindingByName(string name)
        {
            if (Bindings == null || !Bindings.Contains(name))
                throw new JsonRpcException(string.Format("There is no binding registered for '{0}' type of requests.", name));

            string bindingTypeSpec = Mask.NullString((string) Bindings[name]);
            
            if (bindingTypeSpec.Length == 0)
                throw new JsonRpcException("Missing binding type specification.");

            Type bindingType = Type.GetType(bindingTypeSpec, true);
            object bindingObject = Activator.CreateInstance(bindingType, new object[] { Service });

            IServiceBinding binding = bindingObject as IServiceBinding;

            if (binding == null)
                throw new JsonRpcException(string.Format("{0} is not a valid binding type.", bindingObject.GetType().FullName));

            return binding;
        }
    }
}