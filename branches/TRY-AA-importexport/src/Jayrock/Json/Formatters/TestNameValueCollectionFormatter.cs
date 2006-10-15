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

namespace Jayrock.Json.Formatters
{
    #region Imports

    using System;
    using System.Collections.Specialized;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestNameValueCollectionFormatter
    {
        [ Test ]
        public void Empty()
        {
            Assert.AreEqual("{}", Format(new NameValueCollection()));
        }

        [ Test ]
        public void OneNameValue()
        {
            NameValueCollection collection = new NameValueCollection();
            collection.Add("foo", "bar");
            Assert.AreEqual("{\"foo\":\"bar\"}", Format(collection));
        }

        [ Test ]
        public void EmptyName()
        {
            NameValueCollection collection = new NameValueCollection();
            collection.Add("", "bar");
            Assert.AreEqual("{\"\":\"bar\"}", Format(collection));
        }

        [ Test ]
        public void EmptyValue()
        {
            NameValueCollection collection = new NameValueCollection();
            collection.Add("foo", "");
            Assert.AreEqual("{\"foo\":\"\"}", Format(collection));
        }

        [ Test ]
        public void NullValue()
        {
            NameValueCollection collection = new NameValueCollection();
            collection.Add("foo", null);
            Assert.AreEqual("{\"foo\":null}", Format(collection));
        }

        [ Test ]
        public void ValuesArray()
        {
            NameValueCollection collection = new NameValueCollection();
            collection.Add("foo", "bar1");
            collection.Add("foo", "bar2");
            collection.Add("foo", "bar3");
            Assert.AreEqual("{\"foo\":[\"bar1\",\"bar2\",\"bar3\"]}", Format(collection));
        }

        [ Test ]
        public void ManyEntries()
        {
            NameValueCollection collection = new NameValueCollection();
            collection.Add("foo1", "bar1");
            collection.Add("foo2", "bar2");
            collection.Add("foo3", "bar3");
            Assert.AreEqual("{\"foo1\":\"bar1\",\"foo2\":\"bar2\",\"foo3\":\"bar3\"}", Format(collection));
        }

        private static string Format(object o)
        {
            JsonTextWriter writer = new JsonTextWriter();
            JsonExport.Export(o, writer);
            return writer.ToString();
        }
   }
}