#region License, Terms and Conditions
//
// JayRock - A JSON-RPC implementation for the Microsoft .NET Framework
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

namespace JayRock.Json.Rpc
{
    #region Imports

    using System;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestRpcServiceDescriptor
    {
        [ SetUp ]
        public void Init()
        {
        }

        [ TearDown ]
        public void Dispose()
        {
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void NullType()
        {
            RpcServiceDescriptor.GetDescriptor(null);
        }

        [ Test ]
        public void ServiceNameIsTypeName()
        {
            RpcServiceDescriptor descriptor = RpcServiceDescriptor.GetDescriptor(typeof(EmptyService));
            Assert.AreEqual("EmptyService", descriptor.Name);
        }

        [ Test ]
        public void UntaggedMethodsNotExported()
        {
            RpcServiceDescriptor descriptor = RpcServiceDescriptor.GetDescriptor(typeof(EmptyService));
            Assert.AreEqual(0, descriptor.GetMethods().Length);
        }

        [ Test ]
        public void TaggedMethodsExported()
        {
            RpcServiceDescriptor descriptor = RpcServiceDescriptor.GetDescriptor(typeof(TestService));
            Assert.AreEqual(2, descriptor.GetMethods().Length);
        }

        private sealed class EmptyService
        {
        }

        private sealed class TestService
        {
            [ JsonRpcMethod ]
            public void Foo() {}

            [ JsonRpcMethod ]
            public void Bar() {}
        }
    }
}
