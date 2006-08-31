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

namespace Jayrock.Json.Importers
{
    #region Imports

    using System;
    using System.Collections;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestImportableImporter
    {
        private ImportableBaseImporter _importer;
        
        [ SetUp ]
        public void Init()
        {
            _importer = new ImportableBaseImporter();
        }

        [ Test ]
        public void FindNonImportableType()
        {
            Assert.IsNull(_importer.Find(typeof(object)));
        }

        [ Test ]
        public void FindImportableType()
        {
            IJsonImporter importer = _importer.Find(typeof(Thing));
            Assert.IsInstanceOfType(typeof(ImportableImporter), importer);
            Assert.IsNotNull(importer);
        }
        
        [ Test ]
        public void LocatorRegistration()
        {
            JsonImporterRegistry registry = new JsonImporterRegistry();
            Assert.IsFalse(CollectionHelper.ToList(registry.Items).Contains(_importer));
            _importer.RegisterSelf(registry);
            Assert.IsTrue(CollectionHelper.ToList(registry.Items).Contains(_importer));
        }
        
        [ Test ]
        public void ImportTellsObjectToImportSelf()
        {
            ImportableImporter importer = new ImportableImporter(typeof(Thing));
            JsonRecorder writer = new JsonRecorder();
            writer.WriteString(string.Empty);
            Thing thing = (Thing) importer.Import(writer.CreatePlayer());
            Assert.IsTrue(thing.ImportCalled);
        }

        [ Test ]
        public void ImportNull()
        {
            ImportableImporter importer = new ImportableImporter(typeof(Thing));
            JsonRecorder writer = new JsonRecorder();
            writer.WriteNull();
            Assert.IsNull(importer.Import(writer.CreatePlayer()));
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotInitWithNullType()
        {
            new ImportableImporter(null);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotSendNullReaderToImport()
        {
            ImportableImporter importer = new ImportableImporter(typeof(Thing));
            importer.Import(null);
        }
        
        [ Test ]
        public void Registration()
        {
            Type type = typeof(Thing);
            JsonImporterRegistry registry = new JsonImporterRegistry(JsonImporterStock.None);
            Assert.IsNull(registry.Find(type));
            ImportableImporter importer = new ImportableImporter(type);
            importer.RegisterSelf(registry);
            Assert.AreSame(importer, registry.Find(type));
        }

        private sealed class Thing : IJsonImportable
        {
            public bool ImportCalled;
            
            public void Import(JsonReader reader)
            {
                ImportCalled = true;
            }
        }
    }
}