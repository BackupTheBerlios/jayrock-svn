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
    public class TestNameValueCollectionImporter
    {
        [ Test ]
        public void ImportNull()
        {
            Assert.IsNull(UncheckImport("null"));
        }
        
        [ Test ]
        public void ImportEmpty()
        {
            NameValueCollection collection = Import("{}");
            Assert.AreEqual(0, collection.Count);
        }

        [ Test ]
        public void ImportOneNameValue()
        {
            NameValueCollection collection = Import("{\"foo\":\"bar\"}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("bar", collection["foo"]);
        }
        
        [ Test ]
        public void ImportEmptyName()
        {
            NameValueCollection collection = Import("{\"\":\"bar\"}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("bar", collection[""]);
        }

        [ Test ]
        public void ImportEmptyValue()
        {
            NameValueCollection collection = Import("{\"foo\":\"\"}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("", collection["foo"]);
        }

        [ Test ]
        public void ImportNullValue()
        {
            NameValueCollection collection = Import("{\"foo\":null}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual("foo", collection.Keys[0]);
            Assert.AreEqual(null, collection.Get(0));
        }

        [ Test ]
        public void ImportValuesArray()
        {
            NameValueCollection collection = Import("{\"foo\":[\"bar1\",\"bar2\",\"bar3\"]}");
            Assert.AreEqual(1, collection.Count);
            Assert.AreEqual(new string[] { "bar1", "bar2", "bar3" }, collection.GetValues("foo"));
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportObjectValue()
        {
            Import("{\"foo\":{}}");
        }

        private static NameValueCollection UncheckImport(string s)
        {
            JsonReader reader = new JsonTextReader(new StringReader(s));
            IJsonImporter importer = new NameValueCollectionImporter();
            return (NameValueCollection) importer.Import(reader);
        }

        private static NameValueCollection Import(string s)
        {
            object o = UncheckImport(s);
            Assert.IsNotNull(o);
            Assert.IsInstanceOfType(typeof(NameValueCollection), o);
            return (NameValueCollection) o;
        }
    }
}