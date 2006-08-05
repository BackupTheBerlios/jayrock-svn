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
    public class TestJsonRpcParameter
    {
        private static readonly MethodInfo _sumInfo = typeof(TestJsonRpcParameter).GetMethod("Sum", BindingFlags.Static | BindingFlags.NonPublic);

        [ SetUp ]
        public void Init()
        {
        }

        [ TearDown ]
        public void Dispose()
        {
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void NullMethod()
        {
            new JsonRpcParameter(null, null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void NullParameter()
        {
            new JsonRpcParameter(new StubMethodDescriptor(), null);
        }

        [ Test ]
        public void ReflectsInfo()
        {
            IRpcMethod method = new StubMethodDescriptor();
            
            JsonRpcParameter parameter = new JsonRpcParameter(method, _sumInfo.GetParameters()[0]);
            Assert.AreEqual("a", parameter.Name);
            Assert.AreEqual(typeof(int), parameter.ParameterType);
            Assert.AreSame(method, parameter.Method);
        }

        private static int Sum(int a, int b)
        {
            return a + b;
        }

        private sealed class StubMethodDescriptor : IRpcMethod
        {
            public string Name
            {
                get { throw new NotImplementedException(); }
            }

            public IRpcParameter[] GetParameters()
            {
                throw new NotImplementedException();
            }

            public Type ResultType
            {
                get { throw new NotImplementedException(); }
            }

            public ICustomAttributeProvider ReturnTypeAttributeProvider
            {
                get { throw new NotImplementedException(); }
            }

            public IRpcServiceClass ServiceClass
            {
                get { throw new NotImplementedException(); }
            }

            public object Invoke(IRpcService service, object[] args)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginInvoke(IRpcService service, object[] args, AsyncCallback callback, object asyncState)
            {
                throw new NotImplementedException();
            }

            public object EndInvoke(IAsyncResult asyncResult)
            {
                throw new NotImplementedException();
            }

            public ICustomAttributeProvider AttributeProvider
            {
                get { throw new NotImplementedException(); }
            }
        }
    }
}
