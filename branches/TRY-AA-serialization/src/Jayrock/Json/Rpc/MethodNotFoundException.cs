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

    using SerializationInfo = System.Runtime.Serialization.SerializationInfo;
    using StreamingContext = System.Runtime.Serialization.StreamingContext;

    #endregion

    [ Serializable ]
    public class MethodNotFoundException : System.ApplicationException
    {
        private const string _defaultMessage = "Method not found.";

        public MethodNotFoundException() : this(null) {}

        public MethodNotFoundException(string message) : 
            base(Mask.NullString(message, _defaultMessage)) {}

        public MethodNotFoundException(string message, Exception innerException) :
            base(Mask.NullString(message, _defaultMessage), innerException) {}

        protected MethodNotFoundException(SerializationInfo info, StreamingContext context) :
            base(info, context) {}
    }
}