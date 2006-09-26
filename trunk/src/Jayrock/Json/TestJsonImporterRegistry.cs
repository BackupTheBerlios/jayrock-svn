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
    using System.Collections;
    using Jayrock.Json.Importers;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonImporterRegistry
    {
        private JsonImporterRegistry _registry;
        private readonly Type _thingType = typeof(Thing);

        [ SetUp ]
        public void Init()
        {
            _registry = new JsonImporterRegistry();
        }

        [ Test ]
        public void Registration()
        {
            TestImporter importer = new TestImporter(_thingType);
            _registry.Register(importer);
            Assert.AreSame(importer, _registry.Find(_thingType));
        }

        [ Test ]
        public void LastRegistrationWins()
        {
            Type type = _thingType;
            
            TestImporter importer1 = new TestImporter(type);
            _registry.Register(importer1);
            Assert.AreSame(importer1, _registry.Find(type));

            TestImporter importer2 = new TestImporter(type);
            _registry.Register(importer2);
            Assert.AreSame(importer2, _registry.Find(type));
        }

        private sealed class Thing
        {
        }
        
        private sealed class TestImporter : JsonImporterBase
        {
            public TestImporter(Type type) : 
                base(type) {}
        }
    }
}