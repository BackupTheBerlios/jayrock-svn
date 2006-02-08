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
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonRpcService
    {
        [ Test ]
        public void FailedMethodYieldsInvocationException()
        {
            try
            {
                TestService service = new TestService();
                service.Invoke("BadMethod", null);
                Assert.Fail("Expecting an exception.");
            }
            catch (TargetMethodException e)
            {
                Assert.IsTrue(e.InnerException.GetType() == typeof(ApplicationException), "Unexpected inner exception ({0}).", e.InnerException.GetType().FullName);
            }
        }

        [ Test ]
        public void VariableArguments()
        {
            TestService service = new TestService();
            IRpcMethodDescriptor method = service.GetDescriptor().GetMethodByName("VarMethod");
            object[] args = new object[] { 1, 2, 3, 4 };
            object[] invokeArgs = JsonRpcServices.TransposeVariableArguments(method, args);
            Assert.AreEqual(1, invokeArgs.Length);
            Assert.AreEqual(args, invokeArgs[0]);
        }

        [ Test ]
        public void FixedAndVariableArguments()
        {
            TestService service = new TestService();
            IRpcMethodDescriptor method = service.GetDescriptor().GetMethodByName("FixedVarMethod");
            object[] args = new object[] { 1, 2, 3, 4 };
            args = JsonRpcServices.TransposeVariableArguments(method, args);
            Assert.AreEqual(3, args.Length);
            Assert.AreEqual(1, args[0]);
            Assert.AreEqual(2, args[1]);
            Assert.AreEqual(new object[] { 3, 4 }, args[2]);
        }

        [ Test ]
        public void RetransposingYieldsTheSame()
        {
            TestService service = new TestService();
            IRpcMethodDescriptor method = service.GetDescriptor().GetMethodByName("FixedVarMethod");
            object[] args = new object[] { 1, 2, 3, 4 };
            args = JsonRpcServices.TransposeVariableArguments(method, args);
            args = JsonRpcServices.TransposeVariableArguments(method, args);
            Assert.AreEqual(3, args.Length);
            Assert.AreEqual(1, args[0]);
            Assert.AreEqual(2, args[1]);
            Assert.AreEqual(new object[] { 3, 4 }, args[2]);
        }

        [ Test ]
        public void Test()
        {
            RpcServiceDescriptor serviceDescriptor = RpcServiceDescriptor.GetDescriptor(typeof(TestService));
            Assert.IsFalse(JsonRpcServices.HasMethodVariableArguments(serviceDescriptor.GetMethodByName("BadMethod")));
            Assert.IsTrue(JsonRpcServices.HasMethodVariableArguments(serviceDescriptor.GetMethodByName("VarMethod")));
            Assert.IsTrue(JsonRpcServices.HasMethodVariableArguments(serviceDescriptor.GetMethodByName("FixedVarMethod")));
        }


        private sealed class TestService : JsonRpcService
        {
            [ JsonRpcMethod ]
            public void BadMethod()
            {
                throw new ApplicationException();
            }

            [ JsonRpcMethod ]
            public void VarMethod([ JsonRpcParams ] object[] args)
            {
            }

            [ JsonRpcMethod ]
            public void FixedVarMethod(int a, int b, [ JsonRpcParams ] object[] args)
            {
            }
        }
    }
}
