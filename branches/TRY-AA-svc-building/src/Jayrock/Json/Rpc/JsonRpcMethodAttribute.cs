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

    #endregion

    [ Serializable ]
    [ AttributeUsage(AttributeTargets.Method) ]
    public sealed class JsonRpcMethodAttribute : Attribute, IBuilderAttribute
    {
        private string _name;

        public JsonRpcMethodAttribute() {}

        public JsonRpcMethodAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return Mask.NullString(_name); }
            set { _name = value; }
        }

        void IBuilderAttribute.BuildServiceClass(JsonRpcServiceClass.Builder builder, object attachment)
        {
            throw new NotSupportedException();
        }

        void IBuilderAttribute.BuildMethod(JsonRpcMethod.Builder builder, object attachment)
        {
            builder.Name = Name;
        }

        void IBuilderAttribute.BuildParameter(JsonRpcParameter.Builder builder, object attachment)
        {
            throw new NotSupportedException();
        }
    }
}