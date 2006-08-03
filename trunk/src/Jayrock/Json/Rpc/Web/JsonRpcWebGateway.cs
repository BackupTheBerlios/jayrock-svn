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
        private readonly IRpcService _service;
        private IDictionary _features;
        private bool _featuresInitialized;

        public JsonRpcWebGateway(HttpContext context, IRpcService service)
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

        public IRpcService Service
        {
            get { return _service; }
        }

        public virtual void ProcessRequest()
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

            feature.Initialize(Service);
            handler.ProcessRequest(Context);
        }

        protected virtual IRpcServiceFeature GetDefaultFeature()
        {
            string verb = Context.Request.RequestType;

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

        protected IDictionary Features
        {
            get
            {
                if (!_featuresInitialized)
                {
                    _featuresInitialized = true;
                    _features = GetFeatures();
                }

                return _features;
            }
        }

        protected virtual IDictionary GetFeatures()
        {
            return (IDictionary) ConfigurationSettings.GetConfig("jayrock/json.rpc/features");
        }

        protected virtual IRpcServiceFeature GetFeatureByName(string name)
        {
            if (Features == null || !Features.Contains(name))
                throw new JsonRpcException(string.Format("There is no feature registered for '{0}' type of requests.", name));

            string featureTypeSpec = Mask.NullString((string) Features[name]);
            
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
            return Mask.NullString(Context.Request.QueryString[null]);
        }
    }
}