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
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.Web;

    #endregion

    public sealed class JsonRpcWebAdapter
    {
        public static void ProcessRequest(IRpcService service, HttpContext httpContext)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            if (httpContext == null)
                throw new ArgumentNullException("httpContext");

            //
            // Create a context to service the request and configure it.
            //

            RpcServiceContext context = new RpcServiceContext(service);

            //
            // Cross-connect the service and HTTP context so that one
            // can be found given the other.
            //
            
            context.ContextServices.AddService(typeof(HttpContext), httpContext);
            httpContext.Items.Add(typeof(RpcServiceContext), context);

            context.ContextServices.AddService(typeof(RpcServiceFeatureRegistry), RpcServiceFeatureRegistry.FromConfig());

            JsonRpcServiceWebBinder binder = new JsonRpcServiceWebBinder(context);
            IRpcServiceFeature feature = binder.Bind();

            IHttpHandler handler = feature as IHttpHandler;
        
            if (handler == null)
                throw new JsonRpcException(string.Format("The {0} feature does not support the HTTP protocol.", feature.GetType().FullName));

            handler.ProcessRequest(httpContext);
        }

        /*
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            _context = context;
            Services.AddService(typeof(HttpContext), context);
            Services.AddService(typeof(RpcServiceFeatureRegistry), RpcServiceFeatureRegistry.FromConfig());

            try
            {
                string featureName = Mask.NullString(FeatureNameFromContext());
                featureName = featureName.ToLower(CultureInfo.InvariantCulture);

                IRpcServiceFeature feature = featureName.Length == 0 ? 
                    GetDefaultFeature() : GetFeatureByName(featureName);

                if (feature == null)
                    throw new JsonRpcException(string.Format("Don't know how to handle {0} type of JSON-RPC requests.", Mask.EmptyString(featureName, "(default)")));

                IHttpHandler handler = feature as IHttpHandler;
        
                if (handler == null)
                    throw new JsonRpcException(string.Format("The {0} feature does not support the HTTP protocol.", feature.GetType().FullName));

                feature.Initialize(_service);
                return handler;
            }
            finally
            {
                _context = null;        
            }

            throw new NotImplementedException();
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
        }

        protected virtual void ProcessRequest()
        {
            string featureName = Mask.NullString(FeatureNameFromContext());
            featureName = featureName.ToLower(CultureInfo.InvariantCulture);

            IRpcServiceFeature feature = featureName.Length == 0 ? 
                GetDefaultFeature() : GetFeatureByName(featureName);

            if (feature == null)
                throw new JsonRpcException(string.Format("Don't know how to handle {0} type of JSON-RPC requests.", Mask.EmptyString(featureName, "(default)")));

            IHttpHandler handler = feature as IHttpHandler;
        
            if (handler == null)
                throw new JsonRpcException(string.Format("The {0} feature does not support the HTTP protocol.", feature.GetType().FullName));

            feature.Initialize(_service);
            handler.ProcessRequest(Context);
        }
        */

        private JsonRpcWebAdapter()
        {
            throw new NotSupportedException();
        }
    }
}
