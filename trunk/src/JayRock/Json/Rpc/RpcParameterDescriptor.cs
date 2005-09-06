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

namespace JayRock.Json.Rpc
{
    #region Imports

    using System;
    using System.Reflection;

    #endregion

    [ Serializable ]
    internal sealed class RpcParameterDescriptor : IRpcParameterDescriptor
    {
        private readonly IRpcMethodDescriptor _method;
        private readonly ParameterInfo _parameter;

        public RpcParameterDescriptor(IRpcMethodDescriptor method, ParameterInfo parameter)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            if (parameter == null)
                throw new ArgumentNullException("parameter");

            _method = method;
            _parameter = parameter;

            // TODO: Parameter validation, e.g. cannot be by-reference.
        }

        public string Name
        {
            get { return _parameter.Name; }
        }

        public Type ParameterType
        {
            get { return _parameter.ParameterType; }
        }

        public int Position
        {
            get { return _parameter.Position; }
        }

        public IRpcMethodDescriptor MethodDescriptor
        {
            get { return _method; }
        }

        public ICustomAttributeProvider AttributeProvider
        {
            get { return _parameter; }
        }
    }
}