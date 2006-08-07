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
    using System.Reflection;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonRpcMethod
    {
        private JsonRpcServiceClass _service;

        [ SetUp ]
        public void Init()
        {
            _service = (JsonRpcServiceClass) JsonRpcServiceClass.FromType(typeof(StubService));
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void NullService()
        {
            new JsonRpcMethod(null, null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void NullMethod()
        {
            new JsonRpcMethod(null, null);
        }

        [ Test ]
        public void DefaultNameIsMethodName()
        {
            JsonRpcMethod method = new JsonRpcMethod(_service, StubService.FooInfo);
            Assert.AreEqual("Foo", method.Name);
        }

        [ Test ]
        public void AffliatedWithService()
        {
            JsonRpcMethod method = new JsonRpcMethod(_service, StubService.FooInfo);
            Assert.AreSame(_service, method.ServiceClass);
        }

        [ Test ]
        public void CustomNameViaAttribute()
        {
            JsonRpcMethod method = new JsonRpcMethod(_service, StubService.FooInfo, new JsonRpcMethodAttribute("foo"));
            Assert.AreEqual("foo", method.Name);
        }

        [ Test ]
        public void AttributeFromMethod()
        {
            JsonRpcMethod method = new JsonRpcMethod(_service, StubService.BarInfo);
            Assert.AreEqual("bar", method.Name);
        }

        [ Test ]
        public void ResultTypeIsMethodReturnType()
        {
            JsonRpcMethod method = new JsonRpcMethod(_service, StubService.SumInfo);
            Assert.AreEqual(typeof(int), method.ResultType);
        }

        [ Test ]
        public void Parameters()
        {
            JsonRpcMethod method = new JsonRpcMethod(_service, StubService.SumInfo);
            Assert.AreEqual(2, method.GetParameters().Length);
            Assert.AreEqual("a", method.GetParameters()[0].Name);
            Assert.AreEqual("b", method.GetParameters()[1].Name);
        }

        [ Test ]
        public void Invocation()
        {
            JsonRpcMethod method = new JsonRpcMethod(_service, StubService.SumInfo);
            StubService serviceInstance = new StubService();
            object result = method.Invoke(serviceInstance, new object[] { 2, 3 });
            Assert.AreEqual(5, result);
        }

        private sealed class StubService : IRpcService
        {
            public static MethodInfo FooInfo { get { return GetMethodInfo("Foo"); } }
            public static MethodInfo BarInfo { get { return GetMethodInfo("Bar"); } }
            public static MethodInfo SumInfo { get { return GetMethodInfo("Sum"); } }
        
            private static MethodInfo GetMethodInfo(string name)
            {
                return typeof(StubService).GetMethod(name);
            }

            public void Foo()
            {
            }

            [ JsonRpcMethod("bar") ]
            public void Bar()
            {
            }

            public int Sum(int a, int b)
            {
                return a + b;
            }

            public IRpcServiceClass GetClass()
            {
                throw new NotImplementedException();
            }
        }
    }
}
