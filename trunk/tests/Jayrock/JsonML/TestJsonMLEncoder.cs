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

namespace Jayrock.JsonML
{
    #region Imports

    using System;
    using System.IO;
    using System.Xml;
    using Json;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonMLEncoder
    {
        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void EncodeDoesNotAcceptNullXmlReader()
        {
            JsonMLCodec.Encode(null, new JsonRecorder());
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void EncodeDoesNotAcceptNullJsonWriter()
        {
            JsonMLCodec.Encode(new XmlTextReader("http://www.example.com/"), null);
        }

        [ Test ]
        public void EncodeOnSingleEmptyElement()
        {
            JsonReader reader = Encode("<root />");
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("root", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void EncodeOnSingleEmptyElementWithAttributes()
        {
            JsonReader reader = Encode("<root a1='v1' a2='v2' />");
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("root", reader.ReadString());
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("a1", reader.ReadMember());
            Assert.AreEqual("v1", reader.ReadString());
            Assert.AreEqual("a2", reader.ReadMember());
            Assert.AreEqual("v2", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndObject);
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void EncodeOnSingleElementWithAttributesAndContent()
        {
            JsonReader reader = Encode("<root a1='v1' a2='v2'>content</root>");
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("root", reader.ReadString());
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("a1", reader.ReadMember());
            Assert.AreEqual("v1", reader.ReadString());
            Assert.AreEqual("a2", reader.ReadMember());
            Assert.AreEqual("v2", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndObject);
            Assert.AreEqual("content", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void EncodeOnNestedEmptyElements()
        {
            JsonReader reader = Encode(@"
                <root>
                    <child1 />
                    <child2 />
                    <child3 />
                </root>");
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("root", reader.ReadString());
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("child1", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("child2", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("child3", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void EncodeOnInterspersedElementsAndText()
        {
            JsonReader reader = Encode(@"<root>text1<e1 />text2<e2 />text3</root>");
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("root", reader.ReadString());
            Assert.AreEqual("text1", reader.ReadString());
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("e1", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            Assert.AreEqual("text2", reader.ReadString());
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("e2", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            Assert.AreEqual("text3", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void EncodeCDataHandling()
        {
            JsonReader reader = Encode("<e><![CDATA[<content>]]></e>");
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("e", reader.ReadString());
            Assert.AreEqual("<content>", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test ]
        public void EncodeOnComplexXml()
        {
            JsonReader reader = Encode(
                "<a ichi='1' ni='2'><b>The content of b</b> and " +
                "<c san='3'>The content of c</c><d>do</d><e></e>" + 
                "<d>re</d><f/><d>mi</d></a>");

            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("a", reader.ReadString());
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("ichi", reader.ReadMember());
            Assert.AreEqual("1", reader.ReadString());
            Assert.AreEqual("ni", reader.ReadMember());
            Assert.AreEqual("2", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndObject);
            
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("b", reader.ReadString());
            Assert.AreEqual("The content of b", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            
            Assert.AreEqual(" and ", reader.ReadString());
            
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("c", reader.ReadString());
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("san", reader.ReadMember());
            Assert.AreEqual("3", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndObject);
            Assert.AreEqual("The content of c", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("d", reader.ReadString());
            Assert.AreEqual("do", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
            
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("e", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);

            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("d", reader.ReadString());
            Assert.AreEqual("re", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);

            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("f", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);

            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("d", reader.ReadString());
            Assert.AreEqual("mi", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);

            reader.ReadToken(JsonTokenClass.EndArray); // a
        }

        [ Test ]
        public void EncodeSkipsComments()
        {
            JsonReader reader = Encode("<e><!--comment--></e>");
            reader.ReadToken(JsonTokenClass.Array);
            Assert.AreEqual("e", reader.ReadString());
            reader.ReadToken(JsonTokenClass.EndArray);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void EncodeExpectsXmlReaderOnElement()
        {
            XmlTextReader reader = new XmlTextReader(new StringReader("<e>text</e>"));
            reader.Read();
            reader.Read();
            JsonMLCodec.Encode(reader, new JsonRecorder());
        }

        private static JsonReader Encode(string xml)
        {
            JsonRecorder writer = new JsonRecorder();
            JsonMLCodec.Encode(new XmlTextReader(new StringReader(xml)), writer);
            return writer.CreatePlayer();
        }
    }
}