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
    using System.Collections;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestCompositeFormatter
    {
        [ Test ]
        public void ExactSelection()
        {
            JsonTextWriter writer = new JsonTextWriter();
            CompositeFormatter compositeFormatter = new CompositeFormatter();
            compositeFormatter.AddFormatter(typeof(object), new TestFormatter());
            IJsonFormatter formatter = compositeFormatter.SelectFormatter(typeof(object));
            formatter.Format(new object(), writer);
            Assert.AreEqual("\"(object)\"", writer.ToString());
        }

        [ Test ]
        public void WideSelection()
        {
            CompositeFormatter compositeFormatter = new CompositeFormatter();
            StubFormatter formatter = new StubFormatter();
            compositeFormatter.AddFormatter(typeof(IList), formatter, true);
            Assert.IsNull(compositeFormatter.SelectFormatter(typeof(Guid)));
            Assert.AreSame(formatter, compositeFormatter.SelectFormatter(typeof(ArrayList)));
            Assert.AreSame(formatter, compositeFormatter.SelectFormatter(typeof(int[])));
        }

        private sealed class StubFormatter : IJsonFormatter
        {
            public void Format(object o, JsonWriter writer)
            {
            }
        }

        private sealed class TestFormatter : IJsonFormatter
        {
            public void Format(object o, JsonWriter writer)
            {
                writer.WriteString("(object)");
            }
        }
    }
}
