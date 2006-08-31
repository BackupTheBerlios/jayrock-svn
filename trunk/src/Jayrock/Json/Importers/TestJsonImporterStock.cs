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
    public class TestJsonImporterStock
    {
        [ Test ]
        public void InStock()
        {
            AssertInStock(typeof(NumberImporter), typeof(byte));
            AssertInStock(typeof(NumberImporter), typeof(short));
            AssertInStock(typeof(NumberImporter), typeof(int));
            AssertInStock(typeof(NumberImporter), typeof(long));
            AssertInStock(typeof(NumberImporter), typeof(float));
            AssertInStock(typeof(NumberImporter), typeof(double));
            AssertInStock(typeof(DateTimeImporter), typeof(DateTime));
            AssertInStock(typeof(StringImporter), typeof(string));
            AssertInStock(typeof(BooleanImporter), typeof(bool));
            AssertInStock(typeof(AutoImporter), typeof(object));
            AssertInStock(typeof(TypedArrayImporter), typeof(object[]));
            AssertInStock(typeof(TypedEnumImporter), typeof(System.Globalization.UnicodeCategory));
            AssertInStock(typeof(ImportableImporter), typeof(JsonObject));
            AssertInStock(typeof(ImportableImporter), typeof(IDictionary));
            AssertInStock(typeof(ImportableImporter), typeof(JsonArray));
            AssertInStock(typeof(ImportableImporter), typeof(IList));
            AssertInStock(typeof(ImportableImporter), typeof(ImportableThing));
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotGetUnknown()
        {
            JsonImporterStock.Get(typeof(Enum));
        }

        private static void AssertInStock(Type expected, Type type)
        {
            IJsonImporter importer = JsonImporterStock.Find(type);
            Assert.IsNotNull(importer , "{0} not in stock.", type.FullName);
            Assert.IsInstanceOfType(expected, importer, type.FullName);
        }

        private sealed class ImportableThing : IJsonImportable
        {
            public void Import(JsonReader reader)
            {
                throw new NotImplementedException();
            }
        }
    }
}