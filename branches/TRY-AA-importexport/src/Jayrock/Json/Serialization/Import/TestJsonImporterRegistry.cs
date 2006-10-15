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
    using Jayrock.Json.Serialization.Import.Importers;
    using Jayrock.Json.Serialization.Import;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonImporterRegistry
    {
        private JsonImporterRegistry _registry;
        private readonly Type _thing1Type = typeof(Thing1);
        private readonly Type _thing2Type = typeof(Thing2);
        private TestImporter _thing1Importer;
        private TestImporter _thing2Importer;

        [ SetUp ]
        public void Init()
        {
            _registry = new JsonImporterRegistry();
            _thing1Importer = new TestImporter(_thing1Type);
            _thing2Importer = new TestImporter(_thing2Type);
        }

        [ Test ]
        public void Registration()
        {
            _registry.Register(_thing1Importer);
            Assert.AreSame(_thing1Importer, _registry.Find(_thing1Type));
        }

        [ Test ]
        public void LastImporterRegistrationWins()
        {
            _registry.Register(_thing1Importer);
            Assert.AreSame(_thing1Importer, _registry.Find(_thing1Type));
            TestImporter anotherThing1Importer = new TestImporter(_thing1Type);
            _registry.Register(anotherThing1Importer);
            Assert.AreSame(anotherThing1Importer, _registry.Find(_thing1Type));
        }

        [ Test ]
        public void FirstImporterSetRegistrationWins()
        {
            _registry.Register(new TestImporterFamily(_thing1Importer));
            _registry.Register(new TestImporterFamily(_thing1Importer));
            Assert.AreSame(_thing1Importer, _registry.Find(_thing1Type));
        }

        [ Test ]
        public void InitialCountIsZero()
        {
            Assert.AreEqual(0, _registry.Count);
        }

        [ Test ]
        public void CountImporters()
        {
            _registry.Register(_thing1Importer);
            Assert.AreEqual(1, _registry.Count);

            _registry.Register(_thing2Importer);
            Assert.AreEqual(2, _registry.Count);
        }

        [ Test ]
        public void CountImpoterSets()
        {
            _registry.Register(new TestImporterFamily(_thing1Importer));
            Assert.AreEqual(1, _registry.Count);
        }

        [ Test ]
        public void EnumerateWhenEmpty()
        {
            Assert.AreEqual(0, EnumeratorHelper.List(_registry).Count);
        }

        [ Test ]
        public void EnumerateImporters()
        {
            _registry.Register(_thing1Importer);
            _registry.Register(_thing2Importer);
            IList list = EnumeratorHelper.List(_registry);
            Assert.AreEqual(2, list.Count);
            list.Remove(_thing1Importer);
            list.Remove(_thing2Importer);
            Assert.AreEqual(0, list.Count);
        }

        [ Test ]
        public void EnumerateImporterSets()
        {
            TestImporterFamily set1 = new TestImporterFamily();
            _registry.Register(set1);
            
            TestImporterFamily set2 = new TestImporterFamily();
            _registry.Register(set2);

            Assert.AreEqual(new object[] { set1, set2 }, CollectionHelper.ToArray(EnumeratorHelper.List(_registry)));
        }

        [ Test ]
        public void EnumerateRegistrations()
        {
            _registry.Register(_thing1Importer);

            TestImporterFamily importerFamily = new TestImporterFamily(_thing1Importer);
            _registry.Register(importerFamily);

            Assert.AreEqual(new object[] { _thing1Importer, importerFamily }, CollectionHelper.ToArray(EnumeratorHelper.List(_registry)));
        }

        [ Test ]
        public void CopyItemsToArray()
        {
            _registry.Register(_thing1Importer);

            TestImporterFamily importerFamily = new TestImporterFamily(_thing2Importer);
            _registry.Register(importerFamily);
            
            object[] items = new object[_registry.Count];
            _registry.CopyTo(items, 0);
            Assert.AreEqual(new object[] { _thing1Importer, importerFamily }, items);
        }
        
        [ Test ]
        public void CopyItemsToArrayAtNonZeroIndex()
        {
            _registry.Register(_thing1Importer);

            TestImporterFamily importerFamily = new TestImporterFamily(_thing2Importer);
            _registry.Register(importerFamily);
            
            object[] items = new object[2 + _registry.Count];
            _registry.CopyTo(items, 2);
            Assert.AreEqual(new object[] { null, null, _thing1Importer, importerFamily }, items);
        }
        
        [ Test ]
        public void CollectionNotSynchronized()
        {
            Assert.IsFalse(((ICollection) _registry).IsSynchronized);
        }

        [ Test ]
        public void CountNotAffectedByCaching()
        {
            _registry.Register(new TestImporterFamily(new TestImporter(_thing1Type)));
            _registry.Find(_thing1Type);
            Assert.AreEqual(1, _registry.Count);
        }

        [ Test ]
        public void RegistrationCacheInvaidatedWhenNewImporterRegistered()
        {
            _registry.Register(_thing1Importer);
            Assert.AreEqual(new object[] { _thing1Importer }, CollectionHelper.ToArray(_registry));
            _registry.Register(_thing2Importer);
            
            //
            // IMPORTANT!
            // We have to use EnumeratorHelper.List here because the order
            // of enumeration is not guaranteed to be same in which the
            // importers were registered! Instead we list the entries and 
            // then remove each expected instance. If the list becomes 
            // empty then all expectations were met!
            //
            
            IList list = EnumeratorHelper.List(_registry);
            Assert.AreEqual(2, list.Count);
            list.Remove(_thing1Importer);
            list.Remove(_thing2Importer);
            Assert.AreEqual(0, list.Count);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotFindNullType()
        {
            _registry.Find(null);
        }

        [ Test ]
        public void SyncRootIsNotSelf()
        {
            ICollection registry = _registry;
            object syncRoot = registry.SyncRoot;
            Assert.IsNotNull(syncRoot);
            Assert.AreNotSame(registry, registry.SyncRoot);
            Assert.AreSame(syncRoot,registry.SyncRoot);
        }

        private sealed class Thing1
        {
        }
        
        private sealed class Thing2
        {
        }

        private sealed class TestImporter : JsonImporterBase
        {
            public TestImporter(Type type) : 
                base(type) {}
        }

        private class TestImporterFamily : IJsonImporterFamily
        {
            private readonly IJsonImporter _importer;

            public TestImporterFamily() {}
            
            public TestImporterFamily(IJsonImporter importer)
            {
                _importer = importer;
            }

            public IJsonImporter Page(Type type)
            {
                return _importer != null  && _importer.OutputType == type ? _importer : null;
            }
        }
    }
}