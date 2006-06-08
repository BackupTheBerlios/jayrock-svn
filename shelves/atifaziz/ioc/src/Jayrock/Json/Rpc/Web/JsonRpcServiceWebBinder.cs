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
    using System.Diagnostics;
    using System.Globalization;
    using System.Web;

    #endregion

    public class JsonRpcServiceWebBinder : IRpcServiceBinder
    {
        private readonly RpcServiceContext _context;

        public JsonRpcServiceWebBinder(RpcServiceContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            _context = context;
        }

        public RpcServiceContext Context
        {
            get { return _context; }
        }

        public virtual IRpcServiceFeature Bind()
        {
            string featureName = Mask.NullString(FeatureNameFromContext());
            featureName = featureName.ToLower(CultureInfo.InvariantCulture);

            IRpcServiceFeature feature = featureName.Length == 0 ? 
                GetDefaultFeature() : GetFeatureByName(featureName);

            if (feature == null)
                throw new JsonRpcException(string.Format("Don't know how to handle '{0}' type of JSON-RPC requests.", Mask.EmptyString(featureName, "(default)")));

            feature.Initialize(Context.Service);
            return feature;
        }

        protected virtual IRpcServiceFeature GetDefaultFeature()
        {
            string verb = HttpContextFromServiceContext(Context).Request.RequestType;

            if (CaselessString.Equals(verb, "GET") ||
                CaselessString.Equals(verb, "HEAD"))
            {
                return GetFeatureByName("help");
            }
            else if (CaselessString.Equals(verb, "POST")) 
            {
                return GetFeatureByName("rpc");
            }

            return null;
        }

        protected virtual IRpcServiceFeature GetFeatureByName(string name)
        {
            NamedServiceContainer registry = (NamedServiceContainer) ServiceQuery.FindByType(Context, typeof(RpcServiceFeatureRegistry));
            
            if (registry == null || !registry.Has(name))
                throw new JsonRpcException(string.Format("There is no feature registered for '{0}' type of requests.", name));

            return (IRpcServiceFeature) registry[name];
        }

        protected virtual string FeatureNameFromContext()
        {
            return Mask.NullString(HttpContextFromServiceContext(Context).Request.QueryString[null]);
        }
 
        private static HttpContext HttpContextFromServiceContext(RpcServiceContext context)
        {
            Debug.Assert(context != null);

            return (HttpContext) ServiceQuery.GetByType(context, typeof(HttpContext));
        }
    }
}