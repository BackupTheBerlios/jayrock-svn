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

namespace Jayrock.JsonRpc.Web
{
    #region Imports

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Globalization;
    using System.Runtime.Remoting.Contexts;
    using System.Security.Principal;
    using System.Web;
    using System.Web.SessionState;

    #endregion

    public class JsonRpcHandler : JsonRpcService, IHttpHandler
    {
        private HttpContext _context;

        public virtual void ProcessRequest(HttpContext context)
        {
            _context = context;
            JsonRpcWebGateway gateway = new JsonRpcWebGateway(context, this);
            gateway.ProcessRequest();
        }

        bool IHttpHandler.IsReusable
        {
            get { return false; }
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
    }
}
