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

namespace Jayrock
{
    #region Imports

    using System;
    using System.Diagnostics;
    using SerializationInfo = System.Runtime.Serialization.SerializationInfo;
    using StreamingContext = System.Runtime.Serialization.StreamingContext;

    #endregion

    [ Serializable ]
    public class ServiceNotAvailableException : System.ApplicationException
    {
        private const string _defaultMessage = "The requested service is not available.";

        public ServiceNotAvailableException() : 
            this(null) {}

        public ServiceNotAvailableException(string message) : 
            base(Mask.NullString(message, _defaultMessage), null) {}

        public ServiceNotAvailableException(string message, Exception innerException) :
            base(Mask.NullString(message, _defaultMessage), innerException) {}

        protected ServiceNotAvailableException(SerializationInfo info, StreamingContext context) :
            base(info, context) {}

        internal static void ThrowFormatted(string serviceName)
        {
            throw new ServiceNotAvailableException(string.Format("The request service '{0}' is not available.", serviceName));
        }
        
        internal static void ThrowFormatted(Type serviceType)
        {
            Debug.Assert(serviceType != null);
            ThrowFormatted(serviceType.FullName);
        }
    }
}