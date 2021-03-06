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
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    [ Serializable ]
    public sealed class JsonRpcParameter
    {
        private readonly string _name;
        private readonly Type _parameterType;
        private readonly int _position;
        private readonly bool _isParamArray;
        private readonly JsonRpcMethod _method;

        internal JsonRpcParameter(Builder builder, JsonRpcMethod method)
        {
            Debug.Assert(builder != null);
            Debug.Assert(builder.Position >= 0);
            Debug.Assert(method != null);
            
            _name = builder.Name;
            _parameterType = builder.ParameterType;
            _position = builder.Position;
            _isParamArray = builder.IsParamArray;
            _method = method;
        }
        
        public string Name
        {
            get { return _name; }
        }

        public Type ParameterType
        {
            get { return _parameterType; }
        }

        public int Position
        {
            get { return _position; }
        }

        public JsonRpcMethod Method
        {
            get { return _method; }
        }

        public bool IsParamArray
        {
            get { return _isParamArray; }
        }

        [ Serializable ]
        public sealed class Builder
        {
            private string _name;
            private int _position;
            private Type _parameterType = typeof(object);
            private bool _isParamArray;
            private JsonRpcMethod.Builder _method;

            internal Builder(JsonRpcMethod.Builder method)
            {
                Debug.Assert(method != null);
                
                _method = method;
            }

            public JsonRpcMethod.Builder Method
            {
                get { return _method; }
            }

            public string Name
            {
                get { return Mask.NullString(_name); }
                set { _name = value; }
            }

            public int Position
            {
                get { return _position; }
                
                set
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("value");

                    _position = value;
                }
            }
            
            public Type ParameterType
            {
                get { return _parameterType; }
                
                set
                {
                    if (value == null)
                        throw new ArgumentNullException("value");
                    
                    _parameterType = value;
                }
            }

            public bool IsParamArray
            {
                get { return _isParamArray; }
                set { _isParamArray = value; }
            }
        }
    }
}
