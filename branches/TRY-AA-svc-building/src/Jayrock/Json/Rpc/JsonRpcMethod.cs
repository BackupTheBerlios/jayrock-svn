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
    using System.Globalization;
    using System.Reflection;

    #endregion

    [ Serializable ]
    public sealed class JsonRpcMethod
    {
        private readonly JsonRpcServiceClass _class;
        private readonly string _name;
        private readonly string _internalName;
        private readonly Type _resultType;
        private JsonRpcParameter[] _parameters;
        private readonly IDispatcher _dispatcher;
        private readonly bool _isObsolete;
        private readonly string _obsoletionMessage;
        private readonly string _description;

        internal JsonRpcMethod(Builder methodBuilder, JsonRpcServiceClass clazz)
        {
            Debug.Assert(methodBuilder != null);
            Debug.Assert(clazz != null);
            
            _name = methodBuilder.Name;
            _internalName = Mask.EmptyString(methodBuilder.InternalName, methodBuilder.Name);
            _resultType = methodBuilder.ResultType;
            _isObsolete = methodBuilder.IsObsolete;
            _obsoletionMessage = methodBuilder.IsObsolete ? methodBuilder.ObsoletionMessage : string.Empty;
            _description = methodBuilder.Description;
            _dispatcher = methodBuilder.Dispatcher;
            _class = clazz;
            
            JsonRpcParameter.Builder[] parameterBuilders = methodBuilder.GetParameterBuilders();
            _parameters = new JsonRpcParameter[parameterBuilders.Length];
            int paramIndex = 0;

            foreach (JsonRpcParameter.Builder parameterBuilder in parameterBuilders)
                _parameters[paramIndex++] = new JsonRpcParameter(parameterBuilder, this);
        }

        public string Name
        {
            get { return _name; }
        }

        public string InternalName
        {
            get { return _internalName; }
        }
        
        public JsonRpcParameter[] GetParameters()
        {
            //
            // IMPORTANT! Never return the private array instance since the
            // caller could modify its state and compromise the integrity as
            // well as the assumptions made in this implementation.
            //

            return (JsonRpcParameter[]) _parameters.Clone();
        }

        public Type ResultType
        {
            get { return _resultType; }
        }

        public bool IsObsolete
        {
            get { return _isObsolete; }
        }

        public string ObsoletionMessage
        {
            get { return _obsoletionMessage; }
        }

        public string Description
        {
            get { return _description; }
        }

        public JsonRpcServiceClass ServiceClass
        {
            get { return _class; }
        }

        public object Invoke(IRpcService service, object[] args)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            try
            {
                return _dispatcher.Invoke(service, args);
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

        /// <remarks>
        /// The default implementation calls Invoke synchronously and returns
        /// an IAsyncResult that also indicates that the operation completed
        /// synchronously. If a callback was supplied, it will be called 
        /// before BeginInvoke returns. Also, if Invoke throws an exception, 
        /// it is delayed until EndInvoke is called to retrieve the results.
        /// </remarks>

        public IAsyncResult BeginInvoke(IRpcService service, object[] args, AsyncCallback callback, object asyncState)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            SynchronousAsyncResult asyncResult;

            try
            {
                object result = Invoke(service, args);
                asyncResult = SynchronousAsyncResult.Success(asyncState, result);
            }
            catch (Exception e)
            {
                asyncResult = SynchronousAsyncResult.Failure(asyncState, e);
            }

            if (callback != null)
                callback(asyncResult);
                
            return asyncResult;
        }

        public object EndInvoke(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
                throw new ArgumentException("asyncResult");

            SynchronousAsyncResult ar = asyncResult as SynchronousAsyncResult;

            if (ar == null)
                throw new ArgumentException("asyncResult", "IAsyncResult object did not come from the corresponding async method on this type.");

            //
            // IMPORTANT! The End method on SynchronousAsyncResult will 
            // throw an exception if that's what Invoke did when 
            // BeginInvoke called it. The unforunate side effect of this is
            // the stack trace information for the exception is lost and 
            // reset to this point. There seems to be a basic failure in the 
            // framework to accommodate for this case more generally. One 
            // could handle this through a custom exception that wraps the 
            // original exception, but this assumes that an invocation will 
            // only throw an exception of that custom type. We need to 
            // think more about this.
            //

            return ar.End("Invoke");
        }

        /// <summary>
        /// Determines if the method accepts variable number of arguments or
        /// not. A method is designated as accepting variable arguments by
        /// annotating the last parameter of the method with the JsonRpcParams
        /// attribute.
        /// </summary>

        public bool HasParamArray
        {
            get
            {
                return _parameters.Length > 0 && 
                    _parameters[_parameters.Length - 1].IsParamArray;
            }
        }

        [ Serializable ]
        public sealed class Builder
        {
            private string _name;
            private string _internalName;
            private Type _resultType = typeof(void);
            private ArrayList _paramBuilderList;
            private IDispatcher _dispatcher;
            private bool _isObsolete;
            private string _obsoletionMessage;
            private string _description;
            private readonly JsonRpcServiceClass.Builder _serviceClass;

            internal Builder(JsonRpcServiceClass.Builder serviceClass)
            {
                Debug.Assert(serviceClass != null);
                _serviceClass = serviceClass;
            }

            public JsonRpcServiceClass.Builder ServiceClass
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

            public bool IsObsolete
            {
                get { return _isObsolete; }
                set { _isObsolete = value; }
            }

            private ArrayList ParameterBuilders
            {
                get
                {
                    if (_paramBuilderList == null)
                        _paramBuilderList = new ArrayList();
                
                    return _paramBuilderList;
                }
            }

            public string ObsoletionMessage
            {
                get { return Mask.NullString(_obsoletionMessage); }
                
                set
                {
                    _obsoletionMessage = value;
                    IsObsolete = _obsoletionMessage.Length > 0;
                }
            }

            public string Description
            {
                get { return Mask.NullString(_description); }
                set { _description = value; }
            }

            public JsonRpcParameter.Builder DefineParameter()
            {
                JsonRpcParameter.Builder builder = new JsonRpcParameter.Builder(this);
                builder.Position = ParameterBuilders.Count;
                ParameterBuilders.Add(builder);
                return builder;
            }

            internal JsonRpcParameter.Builder[] GetParameterBuilders()
            {
                if (_paramBuilderList == null)
                    return new JsonRpcParameter.Builder[0];
            
                return (JsonRpcParameter.Builder[]) _paramBuilderList.ToArray(typeof(JsonRpcParameter.Builder));
            }
        }
    }
}
