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
    using System;
    using NUnit.Framework;

    [ TestFixture ]
    public class TestJsonWriter
    {
        private JsonWriter _writer;
        
        [ SetUp ]
        public void Init()
        {
            _writer = new StubJsonWriter();
        }

        [ Test ]
        public void StartingDepthIsZero()
        {
            Assert.AreEqual(0, _writer.Depth);
        }
       
        [ Test ]
        public void DepthIncreasesInsideObject()
        {
            _writer.WriteStartObject();
            Assert.AreEqual(1, _writer.Depth);
            _writer.WriteStartObject();
            Assert.AreEqual(2, _writer.Depth);
            _writer.WriteEndObject();
            Assert.AreEqual(1, _writer.Depth);
            _writer.WriteEndObject();
            Assert.AreEqual(0, _writer.Depth);
        }

        [ Test ]
        public void DepthIncreasesInsideArray()
        {
            _writer.WriteStartArray();
            Assert.AreEqual(1, _writer.Depth);
            _writer.WriteStartArray();
            Assert.AreEqual(2, _writer.Depth);
            _writer.WriteEndArray();
            Assert.AreEqual(1, _writer.Depth);
            _writer.WriteEndArray();
            Assert.AreEqual(0, _writer.Depth);
        }
        
        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotWriteObjectAfterRoot()
        {
            _writer.WriteStartObject();
            _writer.WriteEndObject();
            _writer.WriteStartObject();
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotWriteArrayAfterRoot()
        {
            _writer.WriteStartObject();
            _writer.WriteEndObject();
            _writer.WriteStartArray();
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotWriteMemberInsideArray()
        {
            _writer.WriteStartArray();
            _writer.WriteMember("Member");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotEndObjectWithArray()
        {
            _writer.WriteStartObject();
            _writer.WriteEndArray();
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotEndArrayWithObject()
        {
            _writer.WriteStartArray();
            _writer.WriteEndObject();
        }

        private sealed class StubJsonWriter : JsonWriter
        {
            protected override void WriteStartObjectImpl()
            {
            }

            protected override void WriteEndObjectImpl()
            {
            }

            protected override void WriteMemberImpl(string name)
            {
                throw new NotImplementedException();
            }

            protected override void WriteStartArrayImpl()
            {
            }

            protected override void WriteEndArrayImpl()
            {
            }

            protected override void WriteStringImpl(string value)
            {
                throw new NotImplementedException();
            }

            protected override void WriteNumberImpl(string value)
            {
                throw new NotImplementedException();
            }

            protected override void WriteBooleanImpl(bool value)
            {
                throw new NotImplementedException();
            }

            protected override void WriteNullImpl()
            {
                throw new NotImplementedException();
            }
        }
    }
}