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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using Jayrock.Json.Importers;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonImporterRegistry
    {
        private JsonImporterRegistry _registry;
        
        [ SetUp ]
        public void Init()
        {
            _registry = new JsonImporterRegistry();
        }

        [ Test ]
        public void RegistrationViaAttribute()
        {
            IJsonImporter importer = _registry.Find(typeof(Thing));
            Assert.IsNotNull(importer);
            Assert.IsInstanceOfType(typeof(TestImporter), importer);
            Assert.AreSame(typeof(Thing), ((TestImporter) importer).Type);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotRegisterSelfAsLocator()
        {
            _registry.RegisterSelf(_registry);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotRegisterLocatorMoreThanOnce()
        {
            IJsonImporterLocator locator = new TestLocator();
            locator.RegisterSelf(_registry);
            locator.RegisterSelf(_registry);
        }
        
        [ Test ]
        public void LastLocatorComesFirst()
        {
            Type thingType = typeof(Thing);
            RegisterTestLocator(thingType, new TestImporter(thingType));

            TestImporter expected = new TestImporter(thingType);
            RegisterTestLocator(thingType, expected);
            
            Assert.AreSame(expected, _registry.Find(thingType));
        }
        
        [ Test ]
        public void LocatorNotCalledOnceFound()
        {
            Type thingType = typeof(Thing);
            TestLocator locator = RegisterTestLocator(thingType, new TestImporter(thingType));
            Assert.IsFalse(locator.FindCalled);
            _registry.Find(thingType);
            Assert.IsTrue(locator.FindCalled);
            locator.FindCalled = false;
            _registry.Find(thingType);
            Assert.IsFalse(locator.FindCalled);
        }
        
        [ Test ]
        public void LastRegistrationWins()
        {
            Type type = typeof(Thing);
            
            TestImporter importer1 = new TestImporter(type);
            _registry.Register(type, importer1);
            Assert.AreSame(importer1, _registry.Find(type));

            TestImporter importer2 = new TestImporter(type);
            _registry.Register(type, importer2);
            Assert.AreSame(importer2, _registry.Find(type));
        }

        private TestLocator RegisterTestLocator(Type type, TestImporter importer)
        {
            TestLocator locator = new TestLocator();
            locator.NextFindType = type;
            locator.NextImporter = importer;
            locator.RegisterSelf(_registry);
            return locator;
        }
        
        private class TestLocator : IJsonImporterLocator
        {
            public Type NextFindType;
            public IJsonImporter NextImporter;
            public bool FindCalled;
            
            public IJsonImporter Find(Type type)
            {
                FindCalled = true;
                return type == NextFindType ? NextImporter : null;
            }

            public void RegisterSelf(IJsonImporterRegistry registry)
            {
                registry.Register(this);
            }
        }

        [ TestImporter ]
        private sealed class Thing
        {
        }
        
        private sealed class TestImporter : JsonImporterBase
        {
            public readonly Type Type;

            public TestImporter(Type type)
            {
                Type = type;
            }

            public override void RegisterSelf(IJsonImporterRegistry registry)
            {
                registry.Register(Type, this);
            }

            protected override object SubImport(JsonReader reader)
            {
                throw new NotImplementedException();
            }
        }

        [ AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct) ]
        private sealed class TestImporterAttribute : Attribute, IJsonImporterLocator
        {
            IJsonImporter IJsonImporterLocator.Find(Type type)
            {
                return new TestImporter(type);
            }

            void IJsonImporterRegistryItem.RegisterSelf(IJsonImporterRegistry registry)
            {
                throw new NotSupportedException();
            }
        }
    }
}