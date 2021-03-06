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

    #endregion

    [ Serializable ]
    public sealed class JsonRpcParameterBuilder
    {
        private string _name;
        private int _position;
        private Type _parameterType = typeof(object);
        private bool _isParamArray;
        private JsonRpcMethodBuilder _method;

        internal JsonRpcParameterBuilder(JsonRpcMethodBuilder method)
        {
            Debug.Assert(method != null);
                
            _method = method;
        }

        public JsonRpcMethodBuilder Method
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