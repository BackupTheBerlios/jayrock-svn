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
    using System.Globalization;
    using System.Reflection;

    #endregion

    [ Serializable ]
    internal sealed class RpcMethodDescriptor : IRpcMethodDescriptor
    {
        private readonly IRpcServiceDescriptor _serviceDescriptor;
        private readonly MethodInfo _method;
        private readonly string _name;
        private readonly IRpcParameterDescriptor[] _rpcParameters;

        public RpcMethodDescriptor(IRpcServiceDescriptor serviceDescriptor, MethodInfo method) :
            this(serviceDescriptor, method, null) {}

        public RpcMethodDescriptor(IRpcServiceDescriptor serviceDescriptor, MethodInfo method, JsonRpcMethodAttribute attribute)
        {
            if (serviceDescriptor == null)
                throw new ArgumentNullException("serviceDescriptor");

            if (method == null)
                throw new ArgumentNullException("method");

            _serviceDescriptor = serviceDescriptor;
            _method = method;
            _name = method.Name;

            //
            // If an attribute was not supplied then grab one from the method.
            //

            if (attribute == null)
                attribute = (JsonRpcMethodAttribute) Attribute.GetCustomAttribute(method, typeof(JsonRpcMethodAttribute), true);

            //
            // If an attribute was supplied then use it to apply customizations,
            // such as the external method name.
            //

            if (attribute != null)
            {
                if (attribute.Name.Length > 0)
                    _name = attribute.Name;
            }

            //
            // Enumerate the parameters.
            //

            ParameterInfo[] parameters = _method.GetParameters();
            _rpcParameters = new IRpcParameterDescriptor[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
                _rpcParameters[i] = new RpcParameterDescriptor(this, parameters[i]);
        }

        public string Name
        {
            get { return _name; }
        }

        public IRpcParameterDescriptor[] GetParameters()
        {
            //
            // IMPORTANT! Never return the private array instance since the
            // caller could modify its state and compromise the integrity as
            // well as the assumptions made in this implementation.
            //

            return (IRpcParameterDescriptor[]) _rpcParameters.Clone();
        }

        public Type ResultType
        {
            get { return _method.ReturnType; }
        }

        public ICustomAttributeProvider ReturnTypeAttributeProvider
        {
            get { return _method.ReturnTypeCustomAttributes; }
        }

        public IRpcServiceDescriptor ServiceDescriptor
        {
            get { return _serviceDescriptor; }
        }

        public object Invoke(IRpcService service, object[] args)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            args = JsonRpcServices.TransposeVariableArguments(this, args);

            try
            {
                return _method.Invoke(service, args);
            }
            catch (ArgumentException e)
            {
                //
                // The type of the parameters parameter does not match the
                // signature of the method or constructor reflected by this
                // instance.                
                //

                throw new InvocationException(e);
            }
            catch (TargetParameterCountException e)
            {
                //
                // The parameters array does not have the correct number of
                // arguments.
                //

                throw new InvocationException(e.Message, e);
            }
            catch (TargetInvocationException e)
            {
                throw new TargetMethodException(e.InnerException);
            }
        }

        public bool IsAsync
        {
            get { return false; }
        }

        public IAsyncResult BeginInvoke(IRpcService service, object[] args, AsyncCallback callback, object asyncState)
        {
            throw new NotImplementedException();
        }
        
        public object EndInvoke(IRpcService service, IAsyncResult asyncResult)
        {
            throw new NotImplementedException();
        }

        public ICustomAttributeProvider AttributeProvider
        {
            get { return _method; }
        }
    }
}