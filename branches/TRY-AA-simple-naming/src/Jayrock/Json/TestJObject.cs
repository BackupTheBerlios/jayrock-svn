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
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJObject
    {
        [ Test ]
        public void Names()
        {
            JObject o = new JObject();
            o.Put("one", 1);
            o.Put("two", 2);
            ICollection names = o.Names;
            IEnumerator e = names.GetEnumerator();
            e.MoveNext(); Assert.AreEqual("one", e.Current);
            e.MoveNext(); Assert.AreEqual("two", e.Current);
        }

        [ Test ]
        public void InitWithKeyValuePairs()
        {
            JObject o = new JObject(new string[] { "one", "two", }, new object[] { 1, 2 });
            Assert.AreEqual(2, o.Count);
            Assert.AreEqual(1, o["one"]);
            Assert.AreEqual(2, o["two"]);
        }

        [ Test ]
        public void InitWithKeyValuePairsAccumulates()
        {
            JObject o = new JObject(new string[] { "one", "two", "three", "two" }, new object[] { 1, 2, 3, 4 });
            Assert.AreEqual(3, o.Count);
            Assert.AreEqual(1, o["one"]);
            IList two = o["two"] as IList;
            Assert.IsNotNull(two, "Member 'two' should be an ordered list of accumulated values.");
            Assert.AreEqual(2, two.Count, "Count of values under 'two'.");
            Assert.AreEqual(2, two[0]);
            Assert.AreEqual(4, two[1]);
            Assert.AreEqual(3, o["three"]);
        }

        [ Test ]
        public void InitWithExtraKeys()
        {
            JObject o = new JObject(new string[] { "one", "two", "three" }, new object[] { 1, 2 });
            Assert.AreEqual(3, o.Count);
            Assert.AreEqual(1, o["one"]);
            Assert.AreEqual(2, o["two"]);
            Assert.IsTrue(JNull.LogicallyEquals(o["three"]));
        }

        [ Test ]
        public void InitWithNullValues()
        {
            JObject o = new JObject(new string[] { "one", "two", "three" }, null);
            Assert.AreEqual(3, o.Count);
            Assert.IsTrue(JNull.LogicallyEquals(o["one"]));
            Assert.IsTrue(JNull.LogicallyEquals(o["two"]));
            Assert.IsTrue(JNull.LogicallyEquals(o["three"]));
        }

        [ Test ]
        public void InitWithExtraValues()
        {
            JObject o = new JObject(new string[] { "one", "two" }, new object[] { 1, 2, 3, 4 });
            Assert.AreEqual(2, o.Count);
            Assert.AreEqual(1, o["one"]);
            IList two = (IList) o["two"];
            Assert.AreEqual(3, two.Count, "Count of values under 'two'.");
            Assert.AreEqual(2, two[0]);
            Assert.AreEqual(3, two[1]);
            Assert.AreEqual(4, two[2]);
        }

        [ Test ]
        public void InitWithNullKeys()
        {
            JObject o = new JObject(null, new object[] { 1, 2, 3, 4 });
            Assert.AreEqual(1, o.Count);
            IList values = (IList) o[""];
            Assert.AreEqual(4, values.Count, "Count of values.");
            Assert.AreEqual(1, values[0]);
            Assert.AreEqual(2, values[1]);
            Assert.AreEqual(3, values[2]);
            Assert.AreEqual(4, values[3]);
        }
    }
}
