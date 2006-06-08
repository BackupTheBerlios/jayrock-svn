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

namespace Jayrock
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestNamedServiceContainer
    {
        private NamedServiceContainer _container;

        [ SetUp ]
        public void Init()
        {
            _container = new NamedServiceContainer();
        }

        [ Test ]
        public void Add()
        {
            _container.Add("object", new object());
            Assert.AreEqual(1, _container.Count);
        }

        [ Test ]
        public void Clear()
        {
            _container.Add("object", new object());
            Assert.AreEqual(1, _container.Count);
            _container.Clear();
            Assert.AreEqual(0, _container.Count);
        }
 
        [ Test ]
        public void CopyTo()
        {
            _container.Add("object", new object());
            _container.Add("int", 42);
            string[] keys = new string[_container.Count];
            _container.CopyTo(keys, 0);
            Assert.AreEqual(2, keys.Length);
            Assert.IsTrue(Array.IndexOf(keys, "object") >= 0);
            Assert.IsTrue(Array.IndexOf(keys, "int") >= 0);
        }
 
        [ Test ]
        public void FindUnavailabelService()
        {
            Assert.IsNull(_container.Get("Unknown"));
        }
    
        [ Test ]
        public void JitActivation()
        {
            const string serviceName = "test";

            TestServiceCreator creator = new TestServiceCreator();
            _container.Add(serviceName, new NamedServiceContainer.ActivationCallback(creator.Create));

            object first = _container.Get(serviceName);
            Assert.IsTrue(creator.Called, "Created on first request.");
            Assert.AreEqual(_container, creator.CallContainer);
            Assert.AreEqual(serviceName, creator.CallName);

            creator.Called = false;
            object second = _container.Get(serviceName);
            Assert.IsFalse(creator.Called, "No creation on subsequent request.");

            Assert.AreSame(first, second);
        }

        [ Test ]
        public void NamedRegistration()
        {
            NameValueCollection appSettings = new NameValueCollection();
            _container.Add("appSettings", appSettings);
            Assert.AreSame(appSettings, _container.Get("appSettings"));
        }

        [ Test ]
        public void AutoFactoryForType()
        {
            _container.Add("List", typeof(ArrayList));
            Assert.IsTrue(_container.Get("List") is ArrayList);
        }

        [ Test ]
        public void NameCaseSensitivity()
        {
            const string name = "object";
            _container.Add(name, new object());
            Assert.IsNotNull(_container.Get(name));
            Assert.IsNull(_container.Get(name.ToUpper()));
        }

        private sealed class TestServiceCreator
        {
            public bool Called;
            public NamedServiceContainer CallContainer;
            public string CallName;

            public object Create(NamedServiceContainer container, string name)
            {
                Called = true;
                CallContainer = container;
                CallName = name;
                return new object();
            }
        }
    }
}