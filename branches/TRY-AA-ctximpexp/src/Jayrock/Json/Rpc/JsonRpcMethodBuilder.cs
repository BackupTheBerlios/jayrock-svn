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
    using System.Diagnostics;

    #endregion

    [ Serializable ]
    public sealed class JsonRpcMethodBuilder
    {
        private string _name;
        private string _internalName;
        private Type _resultType = typeof(void);
        private ArrayList _parameterList;
        private IDispatcher _dispatcher;
        private string _description;
        private readonly JsonRpcServiceClassBuilder _serviceClass;
        private ArrayList _attributes;

        internal JsonRpcMethodBuilder(JsonRpcServiceClassBuilder serviceClass)
        {
            Debug.Assert(serviceClass != null);
            _serviceClass = serviceClass;
        }

        public JsonRpcServiceClassBuilder ServiceClass
        {
            get { return _serviceClass; }
        }

        public string Name
        {
            get { return Mask.NullString(_name); }
            set { _name = value; }
        }

        public string InternalName
        {
            get { return Mask.NullString(_internalName); }
            set { _internalName = value; }
        }

        public Type ResultType
        {
            get { return _resultType; }
                
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                    
                _resultType = value;
            }
        }

        public IDispatcher Dispatcher
        {
            get { return _dispatcher; }
            set { _dispatcher = value; }
        }


        public void AddCustomAttribute(Attribute attribute)
        {
            if (attribute == null)
                throw new ArgumentNullException("attribute");
                
            CustomAttributes.Add(attribute);
        }
            
        public Attribute[] GetCustomAttributes()
        {
            if (!HasCustomAttributes)
                return new Attribute[0];
                
            return (Attribute[]) CustomAttributes.ToArray(typeof(Attribute));
        }

        public string Description
        {
            get { return Mask.NullString(_description); }
            set { _description = value; }
        }

        public JsonRpcParameterBuilder DefineParameter()
        {
            JsonRpcParameterBuilder builder = new JsonRpcParameterBuilder(this);
            builder.Position = ParameterList.Count;
            ParameterList.Add(builder);
            return builder;
        }

        public ICollection Parameters
        {
            get { return ParameterList; }
        }

        private bool HasCustomAttributes
        {
            get { return _attributes != null && _attributes.Count > 0; }
        }

        private ArrayList CustomAttributes
        {
            get
            {
                if (_attributes == null)
                    _attributes = new ArrayList();
                
                return _attributes;
            }
        }
            
        private bool HasParameters
        {
            get { return _parameterList != null && _parameterList.Count > 0; }
        }

        private ArrayList ParameterList
        {
            get
            {
                if (_parameterList == null)
                    _parameterList = new ArrayList();
                
                return _parameterList;
            }
        }
    }
}