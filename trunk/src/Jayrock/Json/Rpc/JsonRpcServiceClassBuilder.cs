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

    #endregion

    [ Serializable ]
    public sealed class JsonRpcServiceClassBuilder
    {
        private string _name;
        private ArrayList _methodList;
        private string _description;

        public string Name
        {
            get { return Mask.NullString(_name); }
            set { _name = value; }
        }

        public string Description
        {
            get { return Mask.NullString(_description); }
            set { _description = value; }
        }

        public JsonRpcServiceClass CreateClass()
        {
            return new JsonRpcServiceClass(this);
        }

        public JsonRpcMethodBuilder DefineMethod()
        {
            JsonRpcMethodBuilder builder = new JsonRpcMethodBuilder(this);
            MethodList.Add(builder);
            return builder;
        }

        public ICollection Methods
        {
            get { return MethodList; }
        }

        public bool HasMethods
        {
            get { return _methodList != null && _methodList.Count > 0; }
        }

        private ArrayList MethodList
        {
            get
            {
                if (_methodList == null)
                    _methodList = new ArrayList();
                
                return _methodList;
            }
        }
    }
}