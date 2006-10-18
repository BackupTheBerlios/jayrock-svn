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

namespace Jayrock.Json.Serialization.Import
{
    #region Imports

    using System;
    using System.Collections;
    using System.Threading;
    using Jayrock.Json.Serialization.Import;
    using Jayrock.Json.Serialization.Import.Importers;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonImport
    {
        [ Test ]
        public void StockImporters()
        {
            AssertInStock(typeof(ByteImporter), typeof(byte));
            AssertInStock(typeof(Int16Importer), typeof(short));
            AssertInStock(typeof(Int32Importer), typeof(int));
            AssertInStock(typeof(Int64Importer), typeof(long));
            AssertInStock(typeof(SingleImporter), typeof(float));
            AssertInStock(typeof(DoubleImporter), typeof(double));
            AssertInStock(typeof(DecimalImporter), typeof(decimal));
            AssertInStock(typeof(DateTimeImporter), typeof(DateTime));
            AssertInStock(typeof(StringImporter), typeof(string));
            AssertInStock(typeof(BooleanImporter), typeof(bool));
            AssertInStock(typeof(AutoImporter), typeof(object));
            AssertInStock(typeof(ArrayImporter), typeof(object[]));
            AssertInStock(typeof(EnumImporter), typeof(System.Globalization.UnicodeCategory));
            AssertInStock(typeof(ImportableImporter), typeof(JsonObject));
            AssertInStock(typeof(ImportableImporter), typeof(IDictionary));
            AssertInStock(typeof(ImportableImporter), typeof(JsonArray));
            AssertInStock(typeof(ImportableImporter), typeof(IList));
            AssertInStock(typeof(ImportableImporter), typeof(ImportableThing));
        }

        private static void AssertInStock(Type expected, Type type)
        {
            IJsonImporter importer = JsonImport.GetImporter(type);
            Assert.IsInstanceOfType(expected, importer, type.FullName);
        }
        
        private sealed class ImportableThing : IJsonImportable
        {
            public void Import(JsonReader reader, object context)
            {
                throw new NotImplementedException();
            }
        }
    }
}