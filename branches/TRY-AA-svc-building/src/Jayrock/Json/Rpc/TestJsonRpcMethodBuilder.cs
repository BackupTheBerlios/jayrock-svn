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
    public class TestJsonRpcMethodBuilder
    {
        private JsonRpcMethod.Builder _builder;
        private JsonRpcServiceClass.Builder _classBuilder;

        [ SetUp ]
        public void Init()
        {
            _classBuilder = new JsonRpcServiceClass.Builder();
            _builder = _classBuilder.DefineMethod();
        }

        [ Test ] 
        public void Defaults()
        {
            Assert.IsNotNull(_builder.Name);
            Assert.IsNotNull(_builder.InternalName);
            Assert.AreSame(typeof(void), _builder.ResultType);
            Assert.IsNull(_builder.Dispatcher);
            Assert.IsFalse(_builder.IsObsolete);
            Assert.IsNotNull(_builder.ObsoletionMessage);
            Assert.AreEqual(0, _builder.ObsoletionMessage.Length);
            Assert.IsNotNull(_builder.Description);
            Assert.AreEqual(0, _builder.Description.Length);
            Assert.IsNotNull(_builder.ServiceClass);
        }
        
        [ Test ]
        public void GetSetName()
        {
            const string name = "MyMethod";
            _builder.Name = name;
            Assert.AreEqual(name, _builder.Name);
        }
        
        [ Test ]
        public void GetSetInternalName()
        {
            const string name = "MyMethod";
            _builder.InternalName = name;
            Assert.AreEqual(name, _builder.InternalName);
        }
        
        [ Test ]
        public void GetSetReturnType()
        {
            Type type = typeof(int);
            _builder.ResultType = type;
            Assert.AreSame(type, _builder.ResultType);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void ReturnTypeCannotBeNull()
        {
            _builder.ResultType = null;
        }

        [ Test ]
        public void DefineTwoParameters()
        {
            Assert.IsNotNull(_builder.DefineParameter());
            Assert.IsNotNull(_builder.DefineParameter());
            JsonRpcParameter.Builder[] parameters = _builder.GetParameterBuilders();
            Assert.IsNotNull(parameters);
            Assert.AreEqual(2, parameters.Length);
        }
        
        [ Test ]
        public void GetSetDispatcher()
        {
            IDispatcher dispatcher = new StubDispatcher();
            _builder.Dispatcher = dispatcher;
            Assert.AreSame(dispatcher, _builder.Dispatcher);
        }

        [ Test ]
        public void ParametersAreAutoPositioned()
        {
            Assert.AreEqual(0, _builder.DefineParameter().Position);
            Assert.AreEqual(1, _builder.DefineParameter().Position);
            Assert.AreEqual(2, _builder.DefineParameter().Position);
        }

        [ Test ]
        public void GetSetIsObsolete()
        {
            _builder.IsObsolete = true;
            Assert.IsTrue(_builder.IsObsolete);
        }

        [ Test ]
        public void GetSetObsoletionMessage()
        {
            const string message = "This is a test message.";
            _builder.ObsoletionMessage = message;
            Assert.AreEqual(message, _builder.ObsoletionMessage);
        }
        
        [ Test ]
        public void SettingObsoletionMessageFlagsIsObsolete()
        {
            Assert.IsFalse(_builder.IsObsolete);
            _builder.ObsoletionMessage = "This is a test message.";
            Assert.IsTrue(_builder.IsObsolete);
        }

        [ Test ]
        public void GetSetDescription()
        {
            const string name = "Test description of a method.";
            _builder.Description = name;
            Assert.AreEqual(name, _builder.Description);
        }

        [ Test ]
        public void ServiceClassReference()
        {
            Assert.AreSame(_classBuilder, _builder.ServiceClass);
        }
        
        private class StubDispatcher : IDispatcher
        {
            public object Invoke(IRpcService service, object[] args)
            {
                throw new NotImplementedException();
            }

            public IAsyncResult BeginInvoke(IRpcService service, object[] args, AsyncCallback callback,
                                            object asyncState)
            {
                throw new NotImplementedException();
            }

            public object EndInvoke(IAsyncResult asyncResult)
            {
                throw new NotImplementedException();
            }
        }
    }
}
