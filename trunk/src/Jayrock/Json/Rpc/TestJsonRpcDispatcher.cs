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
    using System.IO;
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

        [ Test, ExpectedException(typeof(NotSupportedException)) ]
        public void NotificationNotSupported()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            server.Process("{ id : null, method : 'Dummy' }");
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
            object[] result = ((JsonArray) response["result"]).ToArray();
            Assert.AreEqual(new string[] { "Hello", "Hello", "Hello" }, result);
        }

        [ Test ]
        public void ProcWithArrayArg()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 42, method : 'rev', params : [ [ 1, 'two', 3 ] ] }");
            IDictionary response = (IDictionary) Parse(responseString);
            object[] result = ((JsonArray) response["result"]).ToArray();
            Assert.AreEqual(new object[] { 3, "two", 1 }, result);
        }

        [ Test ]
        public void CallWithNamedArgs()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 42, method : 'replicate', params : { count : 3, text : 'Hello' } }");
            IDictionary response = (IDictionary) Parse(responseString);
            object[] result = ((JsonArray) JsonRpcServices.GetResult(response)).ToArray();
            Assert.AreEqual(new string[] { "Hello", "Hello", "Hello" }, result);
        }
        
        [ Test ]
        public void CallWithIntArray()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 42, method : 'sum', params : [ [ 1, 2, 3, 4, 5 ] ] }");
            IDictionary response = (IDictionary) Parse(responseString);
            Assert.AreEqual(15, JsonRpcServices.GetResult(response));
        }
        
        [ Test ]
        public void Bug8320()
        {
            //
            // Bug #8320: Parameter at Dispatcher without method are not handeld
            // http://developer.berlios.de/bugs/?func=detailbug&bug_id=8320&group_id=4638
            //

            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 42, params : [ [ 1, 2, 3, 4, 5 ] ], method : 'sum' }");
            IDictionary response = (IDictionary) Parse(responseString);
            Assert.AreEqual(15, JsonRpcServices.GetResult(response));
        }
        
        [ Test ]
        public void CallWithTooManyArgsHarmless()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 1, method : 'Say', params : [ 'Hello', 'World' ] }");
            IDictionary response = (IDictionary) Parse(responseString);
            Assert.AreEqual("Hello", response["result"]);
        }

        [ Test ]
        public void CallWithUnknownArgsHarmless()
        {
            JsonRpcDispatcher server = new JsonRpcDispatcher(new TestService());
            string responseString = server.Process("{ id : 1, method : 'Say', params : { message : 'Hello', bad : 'World' } }");
            IDictionary response = (IDictionary) Parse(responseString);
            Assert.AreEqual("Hello", response["result"]);
        }

        private object Parse(string source)
        {
            return (new JsonTextReader(new StringReader(source))).DeserializeNext();
        }

        private sealed class TestService : JsonRpcService
        {
            [ JsonRpcMethod ]
            public void Dummy()
            {                
            }
            
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
            public object[] Reverse(object[] values)
            {
                Array.Reverse(values);
                return values;
            }
            
            [ JsonRpcMethod("sum") ]
            public int Sum(int[] ints)
            {
                int sum = 0;
                foreach (int i in ints)
                    sum += i;
                return sum;
            }
        }
    }
}
