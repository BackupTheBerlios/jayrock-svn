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
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.InteropServices;

    #endregion

    public class JsonRpcService : Component, IRpcService
    {
        private RpcServiceDescriptor _descriptor;

        public virtual IRpcServiceDescriptor GetDescriptor()
        {
            if (_descriptor == null)
                _descriptor = RpcServiceDescriptor.GetDescriptor(this.GetType());

            return _descriptor;
        }

        public virtual object Invoke(string methodName, object[] args)
        {
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            IRpcMethodDescriptor method = GetDescriptor().GetMethodByName(methodName);

            if (method == null)
                throw new MethodNotFoundException();

            return Invoke(method, args);
        }

        protected object Invoke(IRpcMethodDescriptor method, object[] args)
        {    
            return ((RpcMethodDescriptor) method).Invoke(this, args);
        }

        /// <remarks>
        /// The default implementation calls Invoke synchronously and returns an
        /// IAsyncResult that also indicates that the operation completed
        /// synchronously. If a callback was supplied, it will be called before
        /// BeginInvoke returns. Also, if Invoke throws an exception, it delayed
        /// until EndInvoke is called to retrieve the results.
        /// </remarks>

        public IAsyncResult BeginInvoke(string methodName, object[] args, AsyncCallback callback, object asyncState)
        {
            if (methodName == null)
                throw new ArgumentNullException("methodName");

            SynchronousAsyncResult asyncResult;

            try
            {
                object result = Invoke(methodName, args);
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
                throw new ArgumentOutOfRangeException("asyncResult", "IAsyncResult object did not come from the corresponding async method on this type.");

            //
            // IMPORTANT! The End method on SynchronousAsyncResult will throw an
            // exception is that's what Invoke when BeginInvoke called it. There
            // is an unforunate side effect here that the stack trace
            // information for the exception is lost and reset to this point.
            // There seems to be a basic failure in the framework for
            // accommodate for this case more generally. We could handle this
            // through a custom exception that wraps the original exception, but
            // this assumes that an invocation will only throw an exception of
            // that custom type. We need to think more about this.
            //

            return ar.End("Invoke");
        }

        /// <remarks>
        /// Provides introspection capabilities. 
        /// See http://scripts.incutio.com/xmlrpc/introspection.html.
        /// </remarks>

        [ JsonRpcMethod("system.listMethods") ]
        [ JsonRpcHelp("Returns an array of method names implemented by this service") ]
        public virtual string[] SystemListMethods()
        {
            IRpcMethodDescriptor[] methods = GetDescriptor().GetMethods();
            string[] names = new string[methods.Length];
            
            for (int i = 0; i < methods.Length; i++)
                names[i] = methods[i].Name;

            return names;
        }
    }
}