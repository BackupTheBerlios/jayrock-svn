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
    using System.Reflection;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonRpcDispatcher
    {
        [ SetUp ]
        public void Init()
        {
        }

        [ TearDown ]
        public void Dispose()
        {
        }

        [ Test ]
        public void EchoCall()
        {
            EchoService service = new EchoService();
            JsonRpcDispatcher server = new JsonRpcDispatcher(service);
            service.NextResult = "Hello";
            string responseString = server.Process("{ id : 1, method : 'Say', params : [ 'Hello' ] }");
            IDictionary response = (IDictionary) Parse(responseString);
            Assert.AreEqual(1, response["id"]);
            Assert.IsFalse(response.Contains("error"));
            Assert.AreEqual("Say", service.LastMethodName);
            Assert.AreEqual(new object[] { "Hello" }, service.LastArguments);
            Assert.AreEqual("Hello", response["result"]);
        }

        [ Test, ExpectedException(typeof(NotSupportedException)) ]
        public void NotificationNotSupported()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new EchoService());
            server.Process("{ id : null, method : 'Test' }");
        }

        [ Test ]
        public void SimpleCall()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 1, method : 'Say', params : [ 'Hello' ] }");
            IDictionary response = (IDictionary) Parse(responseString);
            Assert.AreEqual(1, response["id"]);
            Assert.AreEqual("Hello", response["result"]);
        }

        [ Test ]
        public void CallWithArrayResult()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 'F9A2CC85-79A2-489f-AE61-84348654008C', method : 'replicate', params : [ 'Hello', 3 ] }");
            IDictionary response = (IDictionary) Parse(responseString);
            Assert.AreEqual("F9A2CC85-79A2-489f-AE61-84348654008C", response["id"]);
            object[] result = ((JArray) response["result"]).ToArray();
            Assert.AreEqual(new string[] { "Hello", "Hello", "Hello" }, result);
        }

        [ Test ]
        public void ProcWithArrayArg()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 42, method : 'rev', params : [ [ 1, 2, 3 ] ] }");
            IDictionary response = (IDictionary) Parse(responseString);
            object[] result = ((JArray) response["result"]).ToArray();
            Assert.AreEqual(new int[] { 3, 2, 1 }, result);
        }

        private object Parse(string source)
        {
            return (new JsonParser()).Parse(source);
        }

        private sealed class EchoService : IRpcService, IRpcServiceDescriptor, IRpcMethodDescriptor
        {
            public string LastMethodName;
            public object[] LastArguments;
            public object NextResult;

            IRpcServiceDescriptor IRpcService.GetDescriptor()
            {
                return this;
            }

            string IRpcServiceDescriptor.Name
            {
                get { throw new NotImplementedException(); }
            }

            IRpcMethodDescriptor[] IRpcServiceDescriptor.GetMethods()
            {
                throw new NotImplementedException();
            }

            IRpcMethodDescriptor IRpcServiceDescriptor.FindMethodByName(string name)
            {
                LastMethodName = name;
                return this;
            }

            IRpcMethodDescriptor IRpcServiceDescriptor.GetMethodByName(string name)
            {
                LastMethodName = name;
                return this;
            }
            ICustomAttributeProvider IRpcAnnotated.AttributeProvider
            {
                get { throw new NotImplementedException(); }
            }

            string IRpcMethodDescriptor.Name
            {
                get { throw new NotImplementedException(); }
            }

            IRpcParameterDescriptor[] IRpcMethodDescriptor.GetParameters()
            {
                throw new NotImplementedException();
            }

            Type IRpcMethodDescriptor.ResultType
            {
                get { throw new NotImplementedException(); }
            }

            ICustomAttributeProvider IRpcMethodDescriptor.ReturnTypeAttributeProvider
            {
                get { throw new NotImplementedException(); }
            }

            IRpcServiceDescriptor IRpcMethodDescriptor.ServiceDescriptor
            {
                get { throw new NotImplementedException(); }
            }

            object IRpcMethodDescriptor.Invoke(IRpcService service, object[] args)
            {
                LastArguments = args;
                return NextResult;
            }

            IAsyncResult IRpcMethodDescriptor.BeginInvoke(IRpcService service, object[] args, AsyncCallback callback, object asyncState)
            {
                throw new NotImplementedException();
            }

            object IRpcMethodDescriptor.EndInvoke(IAsyncResult asyncResult)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class TestService : JsonRpcService
        {
            [ JsonRpcMethod ]
            public string Say(string message)
            {
                return message;
            }

            [ JsonRpcMethod("replicate") ]
            public string[] Replicate(string text, int count)
            {
                return (string[]) ArrayList.Repeat(text, count).ToArray(typeof(string));
            }

            [ JsonRpcMethod("rev") ]
            public JArray Reverse(JArray values)
            {
                JArray reversedValues = new JArray(values);
                reversedValues.Reverse();
                return reversedValues;
            }
        }
    }
}
