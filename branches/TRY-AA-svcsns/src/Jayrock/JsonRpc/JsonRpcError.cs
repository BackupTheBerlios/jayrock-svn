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

namespace Jayrock.JsonRpc
{
    #region Imports

    using System;
    using Jayrock.Json;

    #endregion

    public sealed class JsonRpcError
    {
        public static JsonObject FromException(Exception e)
        {
            return FromException(e, false);
        }

        public static JsonObject FromException(Exception e, bool includeStackTrace)
        {
            if (e == null)
                throw new ArgumentNullException("e");

            JsonObject error = new JsonObject();
            
            error.Put("name", "JSONRPCError");
            error.Put("message", e.GetBaseException().Message);

            if (includeStackTrace)
                error.Put("stackTrace", e.StackTrace);

            return error;
        }

        private JsonRpcError()
        {
            throw new NotSupportedException();
        }
    }
}
