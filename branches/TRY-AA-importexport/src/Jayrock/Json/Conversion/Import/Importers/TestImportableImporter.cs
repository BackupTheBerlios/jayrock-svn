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

namespace Jayrock.Json.Conversion.Import.Importers
{
    #region Imports

    using System;
    using System.Collections;
    using Jayrock.Json.Conversion.Import;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestImportableImporter
    {
        private ImportAwareImporterFamily _importerFamily;
        
        [ SetUp ]
        public void Init()
        {
            _importerFamily = new ImportAwareImporterFamily();
        }

        [ Test ]
        public void FindNonImportableType()
        {
            Assert.IsNull(_importerFamily.Page(typeof(object)));
        }

        [ Test ]
        public void FindImportableType()
        {
            IJsonImporter importer = _importerFamily.Page(typeof(Thing));
            Assert.IsInstanceOfType(typeof(ImportableImporter), importer);
            Assert.IsNotNull(importer);
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
            JsonImporterCollection importers = new JsonImporterCollection();
            Assert.IsNull(importers.Find(type));
            ImportableImporter importer = new ImportableImporter(type);
            importers.Register(importer);
            Assert.AreSame(importer, importers.Find(type));
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