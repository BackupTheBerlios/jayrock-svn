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

    using System.Collections.Specialized;
    using System.IO;
    using System.Web;
    using Jayrock.Json;

    #endregion

    public sealed class JsonRpcGetProtocol : JsonRpcServiceBindingBase
        // TODO: Add IHttpAsyncHandler as soon as JsonRpcWorker supports 
        //       async processing.
    {
        public JsonRpcGetProtocol(IService service) : 
            base(service) {}

        protected override void ProcessRequest()
        {
            string httpMethod = Request.RequestType;

            if (!CaselessString.Equals(httpMethod, "GET") &&
                !CaselessString.Equals(httpMethod, "HEAD"))
            {
                throw new JsonRpcException(string.Format("HTTP {0} is not supported for RPC execution. Use HTTP GET or HEAD only.", httpMethod));
            }

            //
            // Response will be plain text, though it would have been nice to 
            // be more specific, like text/json.
            //

            Response.ContentType = "text/plain";
            
            //
            // Convert the query string into a call object.
            //

            JsonWriter writer = new JsonTextWriter();
            
            writer.WriteStartObject();
            
            writer.WriteMember("id");
            writer.WriteNumber(0);
            
            writer.WriteMember("method");
            string methodName = Mask.NullString(Request.PathInfo);
            if (methodName.Length == 0)
                writer.WriteNull();
            else
                writer.WriteString(methodName.Substring(1));
            
            writer.WriteMember("params");
            writer.WriteStartObject();

            NameValueCollection query = Request.QueryString;
            
            if (query.HasKeys())
            {
                foreach (string name in query)
                {
                    if (Mask.NullString(name).Length == 0)
                        continue;
                
                    writer.WriteMember(name);

                    string[] values = query.GetValues(name);                    
                    
                    if (values.Length == 0)
                        writer.WriteNull();
                    else if (values.Length == 1)
                        writer.WriteString(values[0]);
                    else
                        writer.WriteStringArray(values);
                }
            }
            
            writer.WriteEndObject();
            
            writer.WriteEndObject();
            
            //
            // Delegate rest of the work to JsonRpcDispatcher.
            //

            JsonRpcDispatcher dispatcher = new JsonRpcDispatcher(Service);
            
            dispatcher.RequireIdempotency = true;
            
            if (HttpRequestSecurity.IsLocal(Request))
                dispatcher.SetLocalExecution();
            
            dispatcher.Process(new StringReader(writer.ToString()), Response.Output);
        }
    }
}