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
    using System.Globalization;
    using System.Reflection;

    #endregion

    public sealed class JsonRpcServices
    {
        public static string GetServiceName(IRpcService service)
        {
            return GetServiceName(service, "(anonymous)");
        }

        /// <summary>
        /// Returns the name of the service or an anonymous default if it does
        /// not have a name.
        /// </summary>

        public static string GetServiceName(IRpcService service, string anonymousName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            string name = null;
    
            JsonRpcServiceClass clazz = service.GetClass();
    
            if (clazz != null)
                name = clazz.Name;
    
            return Mask.EmptyString(name, anonymousName);
        }

        public static object GetResult(IDictionary response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            object errorObject = response["error"];

            if (errorObject != null)
            {
                IDictionary error = errorObject as IDictionary;
                
                string message = null;

                if (!JsonNull.LogicallyEquals(error))
                {
                    object messageObject = error["message"];
                    
                    if (!JsonNull.LogicallyEquals(messageObject))
                        message = messageObject.ToString();
                }
                else
                    message = error.ToString();
   
                throw new JsonRpcException(message);
            }

            if (!response.Contains("result"))
                throw new ArgumentException("Response object is not valid because it does not contain the expected 'result' member.");

            return response["result"];
        }

        private JsonRpcServices()
        {
            throw new NotSupportedException();
        }
    }
}
