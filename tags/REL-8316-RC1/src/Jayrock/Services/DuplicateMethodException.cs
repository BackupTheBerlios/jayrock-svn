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

namespace Jayrock.Services
{
    #region Imports

    using System;
    using System.Runtime.Serialization;

    #endregion

    [ Serializable ]
    public class DuplicateMethodException : System.ApplicationException
    {
        private const string _defaultMessage = "A method with the same name has been defined elsewhere on the service.";
        
        public DuplicateMethodException() : this(null) {}

        public DuplicateMethodException(string message) : 
            base(Mask.NullString(message, _defaultMessage)) {}

        public DuplicateMethodException(string message, Exception innerException) :
            base(Mask.NullString(message, _defaultMessage), innerException) {}

        protected DuplicateMethodException(SerializationInfo info, StreamingContext context) :
            base(info, context) {}
    }
}