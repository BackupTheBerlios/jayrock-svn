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
    using System.Globalization;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonReader
    {
        [ Test ]
        public void EOF()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().End();

            Assert.AreEqual(JsonToken.BOF, reader.Token);
            Assert.IsFalse(reader.Read());
            Assert.IsTrue(reader.EOF);
        }
        
        [ Test ]
        public void ReadAfterEOF()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().End();
            
            Assert.AreEqual(JsonToken.BOF, reader.Token);
            Assert.IsFalse(reader.Read());
            Assert.IsTrue(reader.EOF);
            Assert.IsFalse(reader.Read());
        }
        
        [ Test, ExpectedException(typeof(JsonException)) ]
        public void ReadUnexpectedToken()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().End();

            reader.ReadToken(JsonToken.Object);
        }
        
        [ Test ]
        public void ReadNull()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Null().End();
           
            reader.ReadNull();
            Assert.IsTrue(reader.EOF);
        }

        [ Test ]
        public void ReadString()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().String("hello").End();
           
            Assert.AreEqual("hello", reader.ReadString());
            Assert.IsTrue(reader.EOF);
        }

        [ Test ]
        public void ReadNumber()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Number(123456).End();
           
            Assert.AreEqual("123456", reader.ReadNumber());
            Assert.IsTrue(reader.EOF);
        }

        [ Test ]
        public void ReadBoolean()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Boolean(true).End();
           
            Assert.IsTrue(reader.ReadBoolean());
            Assert.IsTrue(reader.EOF);

            reader.Begin().Boolean(false).End();

            Assert.IsFalse(reader.ReadBoolean());
            Assert.IsTrue(reader.EOF);
        }

        [ Test ]
        public void ReadTypedNumber()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Array().
                Number(123).
                Number(456).
                Number(2.5).
                Number(4.2).
                Number(9.99m).
            EndArray().End();
           
            reader.ReadToken(JsonToken.Array);
            Assert.AreEqual(123, reader.ReadInt32());
            Assert.AreEqual(456L, reader.ReadInt64());
            Assert.AreEqual(2.5f, reader.ReadSingle());
            Assert.AreEqual(4.2, reader.ReadDouble());
            Assert.AreEqual(9.99m, reader.ReadDecimal());
            Assert.AreEqual(JsonToken.EndArray, reader.Token);
            Assert.IsFalse(reader.Read());
        }
        
        [ Test ]
        public void ReadMember()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Object().Member("mon", "Monday").EndObject().End();
           
            reader.ReadToken(JsonToken.Object);
            Assert.AreEqual("mon", reader.ReadMember());
            Assert.AreEqual("Monday", reader.ReadString());
            Assert.AreEqual(JsonToken.EndObject, reader.Token);
            Assert.IsFalse(reader.Read());
        }
        
        [ Test ]
        public void SkipArrayFromStart()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Array().String("one").String("two").String("three").EndArray().End();
            
            reader.MoveToContent();
            reader.StepOut();
            Assert.IsTrue(reader.EOF);
        }
        
        [ Test ]
        public void SkipArrayFromMiddle()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Array().String("one").String("two").String("three").EndArray().End();
            
            reader.ReadToken(JsonToken.Array);
            reader.ReadString();
            reader.ReadString();
            reader.StepOut();
            Assert.IsTrue(reader.EOF);
        }
        
        [ Test ]
        public void SkipArrayAtEnd()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Array().String("one").String("two").String("three").EndArray().End();
            
            reader.ReadToken(JsonToken.Array);
            reader.ReadString();
            reader.ReadString();
            reader.ReadString();
            Assert.AreEqual(JsonToken.EndArray, reader.Token);
            reader.StepOut();
            Assert.IsTrue(reader.EOF);
        }

        [ Test ]
        public void SkipFromWithinNestedArray()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Array()
                .String("one")
                .String("two").Array()
                    .String("three")
                    .String("four")
                    .EndArray()
                .String("five")
                .EndArray()
            .End();
            
            reader.ReadToken(JsonToken.Array);
            reader.ReadString();
            reader.ReadString();
            reader.StepOut();
            Assert.AreEqual("five", reader.ReadString());
        }

        [ Test ]
        public void SkipThroughNestedArrays()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin().Array()
                .String("one")
                .String("two").Array()
                    .String("three")
                    .String("four")
                    .EndArray()
                .String("five")
                .EndArray()
            .End();
            
            reader.MoveToContent();
            reader.StepOut();
            Assert.IsTrue(reader.EOF);
        }

        /*
        [ Test ]
        public void ReadStringArray()
        {
            MockedJsonReader reader = new MockedJsonReader();
            reader.Begin.Array().String("hello").String("world").EndArray().End();
            reader.ReadToken(JsonToken.Array);
            reader.Read();
            Assert.AreEqual("hello", reader.ReadString());
            Assert.AreEqual("world", reader.ReadString());
            Assert.AreEqual(JsonToken.EndArray, reader.Token);
            reader.Read();
            Assert.IsTrue(reader.EOF);
        }*/

        private sealed class ReadData
        {
            public readonly JsonToken Token;
            public readonly string Text;
            private readonly int _depthDelta;

            public ReadData(JsonToken token)
            {
                Token = token;
            }

            public ReadData(JsonToken token, string text)
            {
                Token = token;
                Text = text;
            }
        }
        
        private sealed class MockedJsonReader : JsonReader
        {
            private ReadData _data;
            private Queue _queue = new Queue();
            private int _depth = 0;

            public override JsonToken Token
            {
                get { return _data.Token; }
            }

            public override string Text
            {
                get { return _data.Text; }
            }

            public override int Depth
            {
                get { return _depth; }
            }

            public override JsonToken ReadToken()
            {
                if (Token == JsonToken.EndArray || Token == JsonToken.EndObject)
                    _depth--;

                _data = (ReadData) _queue.Dequeue();
                
                if (Token == JsonToken.Array || Token == JsonToken.Object)
                    _depth++;

                return Token;
            }

            private MockedJsonReader Append(ReadData data)
            {
                _queue.Enqueue(data);
                return this;
            }

            public MockedJsonReader Begin()
            {
                _data = new ReadData(JsonToken.BOF);
                _queue.Clear();
                return this;
            }

            public void End()
            {
                Append(new ReadData(JsonToken.EOF));
            }

            public MockedJsonReader Array()
            {
                return Append(new ReadData(JsonToken.Array));
            }

            public MockedJsonReader EndArray()
            {
                return Append(new ReadData(JsonToken.EndArray));
            }

            public MockedJsonReader String(string s)
            {
                return Append(new ReadData(JsonToken.String, s));
            }

            public MockedJsonReader String(string name, string s)
            {
                Append(new ReadData(JsonToken.Member, name));
                return Append(new ReadData(JsonToken.String, s));
            }

            public MockedJsonReader Number(int i)
            {
                return Append(new ReadData(JsonToken.Number, i.ToString(CultureInfo.InvariantCulture)));
            }

            public MockedJsonReader Number(double i)
            {
                return Append(new ReadData(JsonToken.Number, i.ToString(CultureInfo.InvariantCulture)));
            }

            public MockedJsonReader Number(decimal i)
            {
                return Append(new ReadData(JsonToken.Number, i.ToString(CultureInfo.InvariantCulture)));
            }
            
            public MockedJsonReader Boolean(bool b)
            {
                return Append(new ReadData(JsonToken.Boolean, b ? JsonReader.TrueText : JsonReader.FalseText));
            }

            public MockedJsonReader Object()
            {
                return Append(new ReadData(JsonToken.Object));
            }

            public MockedJsonReader EndObject()
            {
                return Append(new ReadData(JsonToken.EndObject));
            }

            public MockedJsonReader Member(string name, string value)
            {
                Append(new ReadData(JsonToken.Member, name));
                return Append(new ReadData(JsonToken.String, value));
            }

            public MockedJsonReader Null()
            {
                return Append(new ReadData(JsonToken.Null));
            }
        }
    }
}