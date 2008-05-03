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
    public class TestJsonMLDecoder
    {
        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void DecodeDoesNotAcceptNullJsonReader()
        {
            JsonMLCodec.Decode(null, new XmlTextWriter(new StringWriter()));
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void DecodeDoesNotAcceptNullXmlWriter()
        {
            JsonMLCodec.Decode(new JsonRecorder().CreatePlayer(), null);
        }

        [ Test ]
        public void DecodeOnSingleElement()
        {
            Assert.AreEqual("<root />", Decode("[root]"));
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void DecodeExpectsJsonArray()
        {
            Decode("{}");
        }

        [ Test ]
        public void DecodeOnSingleElementWithAttributes()
        {
            Assert.AreEqual("<root a='1' b='2' />", Decode("[root,{a:1,b:2}]"));
        }

        [ Test, ExpectedException(typeof(JsonMLException)) ]
        public void DecodeThrowsWhenAttributeIsObject()
        {
            Decode("[root,{bad:{}}]");
        }

        [ Test, ExpectedException(typeof(JsonMLException)) ]
        public void DecodeThrowsWhenAttributeIsArray()
        {
            Decode("[root,{bad:[]}]");
        }

        [ Test ]
        public void DecodeOnAttributeWithNullYieldsEmptyXmlAttributeValue()
        {
            Assert.AreEqual("<root nullattr='' />", Decode("[root,{nullattr:null}]"));
        }

        [ Test ]
        public void DecodeOnSingleElementWithAttributesAndContent()
        {
            Assert.AreEqual("<root a='1' b='2'>content</root>", Decode("[root,{a:1,b:2},content]"));
        }

        [ Test ]
        public void DecodeOnNestedEmptyElements()
        {
            Assert.AreEqual("<root><child1 /><child2 /><child3 /></root>", Decode("[root,[child1],[child2],[child3]]"));
        }

        [ Test ]
        public void DecodeOnInterspersedElementsAndText()
        {
            Assert.AreEqual("<root>text1<e1 />text2<e2 />text3</root>", Decode("[root,text1,[e1],text2,[e2],text3]"));
        }

        [ Test ]
        public void DecodeOnComplexJson()
        {
            Assert.AreEqual(
                "<a ichi='1' ni='2'><b>The content of b</b> and " +
                "<c san='3'>The content of c</c><d>do</d><e />" +
                "<d>re</d><f /><d>mi</d></a>",
                Decode(@"
                    [a, {ichi: 1, ni: 2},
                        [b, The content of b],
                        ' and ',
                        [c, {san: 3}, The content of c],
                        [d, do],
                        [e],
                        [d, re],
                        [f],
                        [d, mi]
                    ]"));
        }

        [ Test ]
        public void DecodeOnElementWithEmptyAttributesObject()
        {
            Assert.AreEqual("<e>text</e>", Decode("[e,{},text]"));
        }

        [ Test ]
        public void DecodeOnElementPermitsChildNodes()
        {
            Assert.AreEqual("<e>abcdef</e>", Decode("[e,abc,[],def]"));
            Assert.AreEqual("<e />", Decode("[e,[]]"));
            Assert.AreEqual("<e a='1' />", Decode("[e,{a:1},[]]"));
        }

        [ Test, ExpectedException(typeof(JsonMLException)) ]
        public void DecodeDoesNotLikeEmptyRootElement()
        {
            Decode("[]");
        }

        [ Test ]
        public void DecodeAllowsEmptyRootElementAsLongAsXmlWriterIsStarted()
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            writer.WriteStartElement("root");
            new JsonMLDecoder().Decode(new JsonTextReader(new StringReader("[]")), writer);
        }

        [ Test, ExpectedException(typeof(JsonMLException)) ]
        public void DecodeDoesNotLikeObjectsAfterAttributes()
        {
            Decode("[e,{a:1},text,{/*bad*/}]");
        }

        [ Test ]
        public void DecodeIgnoresNullElements()
        {
            Assert.AreEqual("<e>text</e>", Decode("[e,null,text,null]"));
        }

        private static string Decode(string json)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(sw);
            writer.QuoteChar = '\'';
            JsonMLCodec.Decode(new JsonTextReader(new StringReader(json)), writer);
            return sw.ToString();
        }
    }
}