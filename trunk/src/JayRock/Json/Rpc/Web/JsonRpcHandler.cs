#region License, Terms and Conditions
//
// JayRock - A JSON-RPC implementation for the Microsoft .NET Framework
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

namespace JayRock.Json.Rpc.Web
{
    #region Imports

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Security.Principal;
    using System.Web;
    using System.Web.SessionState;

    #endregion

    public class JsonRpcHandler : JsonRpcService, IHttpHandler
    {
        private HttpContext _context;
        private IDictionary _features;
        private bool _featuresInitialized;

        public virtual void ProcessRequest(HttpContext context)
        {
            _context = context;

            try
            {
                ProcessRequest();
            }
            finally
            {
                _context = null;
            }
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

            feature.Initialize(this);
            handler.ProcessRequest(Context);
        }

        protected virtual IRpcServiceFeature GetDefaultFeature()
        {
            if (CaselessEquals(Request.RequestType, "GET") ||
                CaselessEquals(Request.RequestType, "HEAD"))
            {
                return GetFeatureByName("help");
            }
            else if (CaselessEquals(Request.RequestType, "POST") &&
                     Mask.NullString(Request.Headers["X-JSON-RPC"]).Length > 0) 
            {
                return GetFeatureByName("rpc");
            }

            return null;
        }

        protected virtual IDictionary GetFeatures()
        {
            if (!_featuresInitialized)
            {
                _featuresInitialized = true;
                _features = (IDictionary) ConfigurationSettings.GetConfig("jayrock/json.rpc/features");
            }

            return _features;
        }

        protected virtual IRpcServiceFeature GetFeatureByName(string name)
        {
            IDictionary features = GetFeatures();
            
            if (features == null || !features.Contains(name))
                throw new JsonRpcException(string.Format("There is no feature registered for '{0}' type of requests.", name));

            string featureTypeSpec = Mask.NullString((string) features[name]);
            
            if (featureTypeSpec.Length == 0)
                throw new JsonRpcException("Missing feature type specification.");

            Type featureType = Type.GetType(featureTypeSpec, true);
            object featureObject = Activator.CreateInstance(featureType);

            IRpcServiceFeature feature = featureObject as IRpcServiceFeature;

            if (feature == null)
                throw new JsonRpcException(string.Format("{0} is not a valid type for JSON-RPC.", featureObject.GetType().FullName));

            return feature;
        }

        protected virtual string FeatureNameFromContext()
        {
            return Mask.NullString(Request.QueryString[null]);
        }

        public HttpContext Context
        {
            get { return _context; }
        }

        public HttpApplication ApplicationInstance
        {
            get { return Context.ApplicationInstance; }
        }

        public HttpApplicationState Application
        {
            get { return Context.Application; }
        }

        public HttpServerUtility Server
        {
            get { return Context.Server; }
        }

        public HttpSessionState Session
        {
            get { return Context.Session; }
        }

        public HttpRequest Request
        {
            get { return Context.Request; }
        }

        public HttpResponse Response
        {
            get { return Context.Response; }
        }
        
        public IPrincipal User
        {
            get { return Context.User; }
        }

        public virtual bool IsReusable
        {
            get { return false; }
        }

        private bool CaselessEquals(string a, string b)
        {
            return string.Compare(a, b, true, CultureInfo.InvariantCulture) == 0;
        }
    }
}
