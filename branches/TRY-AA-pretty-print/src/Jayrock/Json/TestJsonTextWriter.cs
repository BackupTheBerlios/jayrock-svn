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

    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonTextWriter
    {
        [ Test ]
        public void Blank()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            Assert.AreEqual(string.Empty, writer.ToString());
        }

        [ Test ]
        public void WriteString()
        {
            WriteString("\"Hello\"", "Hello");
            WriteString("\"Hello World\"", "Hello World");
            WriteString("\"And before he parted, he said, \\\"Goodbye, people!\\\"\"", "And before he parted, he said, \"Goodbye, people!\"");
            WriteString("\"Hello\\tWorld\"", "Hello\tWorld");
            WriteString("\"Hello\\u0000World\"", "Hello" + ((char) 0) + "World");
        }

        private void WriteString(string expected, string value)
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteString(value);
            Assert.AreEqual(expected, writer.ToString());
        }

        [ Test ]
        public void WriteNumber()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteNumber(123);
            Assert.AreEqual("123", writer.ToString());
        }

        [ Test ]
        public void WriteNull()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteNull();
            Assert.AreEqual("null", writer.ToString());
        }

        [ Test ]
        public void WriteBoolean()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteBoolean(true);
            Assert.AreEqual("true", writer.ToString());
            
            writer = new JsonTextWriter(new StringWriter());
            writer.WriteBoolean(false);
            Assert.AreEqual("false", writer.ToString());
        }

        [ Test ]
        public void WriteEmptyArray()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteArray(new object[0]);
            Assert.AreEqual("[]", writer.ToString());
        }

        [ Test ]
        public void WriteArray()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteArray(new object[] { 123, "Hello \"Old\" World", true });
            Assert.AreEqual("[123,\"Hello \\\"Old\\\" World\",true]", writer.ToString());
        }

        [ Test ]
        public void WriteEmptyObject()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteStartObject();
            writer.WriteEndObject();
            Assert.AreEqual("{}", writer.ToString());
        }

        [ Test ]
        public void WriteObject()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteStartObject();
            writer.WriteMember("Name");
            writer.WriteString("John Doe");
            writer.WriteMember("Salary");
            writer.WriteNumber(123456789);
            writer.WriteEndObject();
            Assert.AreEqual("{\"Name\":\"John Doe\",\"Salary\":123456789}", writer.ToString());
        }

        [ Test ]
        public void WriteNullValue()
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteValue(JNull.Value);
            Assert.AreEqual("null", writer.ToString());
        }

        [ Test ]
        public void WriteValue()
        {
            Assert.AreEqual("123", WriteValue((byte) 123), "Byte");
            Assert.AreEqual("\"123\"", WriteValue((sbyte) 123), "Short byte");
            Assert.AreEqual("123", WriteValue((short) 123), "Short integer");
            Assert.AreEqual("123", WriteValue(123), "Integer");
            Assert.AreEqual("123", WriteValue(123L), "Long integer");
            Assert.AreEqual("123", WriteValue(123m), "Decimal");
        }

        [ Test ]
        public void WriteObjectArray()
        {
            JObject o = new JObject();
            o.Put("one", 1);
            o.Put("two", 2);
            o.Put("three", 3);
            Assert.AreEqual("[{\"one\":1,\"two\":2,\"three\":3},{\"one\":1,\"two\":2,\"three\":3},{\"one\":1,\"two\":2,\"three\":3}]", WriteValue(new object[] { o, o, o }));
        }

        [ Test ]
        public void WriteNestedArrays()
        {
            int[] inner = new int[] { 1, 2, 3 };
            int[][] outer = new int[][] { inner, inner, inner };
            Assert.AreEqual("[[1,2,3],[1,2,3],[1,2,3]]", WriteValue(outer));
        }

        private static string WriteValue(object value)
        {
            JsonTextWriter writer = new JsonTextWriter(new StringWriter());
            writer.WriteValue(value);
            return writer.ToString();
        }
    }
}
