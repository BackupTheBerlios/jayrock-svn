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
    using System.Collections.Specialized;
    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestArrayImporter
    {
        [ Test ]
        public void ImportNull()
        {
            ArrayImporter importer = new ArrayImporter(typeof(object));
            Assert.IsNull(importer.Import(CreateReader("null")));
        }

        [ Test ]
        public void ImportEmptyArray()
        {
            AssertImport(new int[0], "[]");
        }

        [ Test ]
        public void ImportInt32Array()
        {
            AssertImport(new int[] { 123, 789, 456 }, "[ 123, 789, 456 ]");
        }
        
        [ Test ]
        public void ImportStringArray()
        {
            AssertImport(new string[] { "see no evil", "hear no evil", "speak no evil" }, 
                "[ 'see no evil', 'hear no evil', 'speak no evil' ]");
        }

        [ Test ]
        public void ImportDateArray()
        {
            AssertImport(new DateTime[] { new DateTime(1999, 12, 31), new DateTime(2000, 1, 1),  }, "[ '1999-12-31', '2000-01-01' ]");
        }
        
        private static void AssertImport(Array expected, string s)
        {
            TypeImporterCollection importers = new TypeImporterCollection();

            Type expectedType = expected.GetType();
            importers.Add(expectedType.GetElementType(), TypeImporterStock.Get(expectedType.GetElementType()));
            importers.Add(expectedType, new ArrayImporter(expectedType.GetElementType()));

            JsonReader reader = CreateReader(s);
            reader.TypeImporterLocator = importers;
            object o = reader.Get(expectedType);

            if (expected == null)
                Assert.IsNull(o);
            
            Assert.AreEqual(expected, o);
        }

        private static JsonReader CreateReader(string s)
        {
            return new JsonTextReader(new StringReader(s));
        }
    }
}