#region License, Terms and Conditions
//
// JayRock - A JSON-RPC implementation for the Microsoft .NET Framework
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

namespace JayRock.Json
{
    #region Imports

    using System.Collections;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonParser
    {
        [ Test ]
        public void ParseNumber()
        {
            Assert.AreEqual(123, Parse("123"));
        }

        [ Test ]
        public void ParseString()
        {
            Assert.AreEqual("Hello World", Parse("\"Hello World\""), "Double-quoted string.");
            Assert.AreEqual("Hello World", Parse("'Hello World'"), "Single-quoted string.");
            Assert.AreEqual("Hello 'World'", Parse("\"Hello 'World'\""));
            Assert.AreEqual("Hello \"World\"", Parse("'Hello \"World\"'"));
        }

        [ Test ]
        public void ParseBoolean()
        {
            Assert.AreEqual(true, Parse("true"));
            Assert.AreEqual(false, Parse("false"));
        }

        [ Test ]
        public void ParseNull()
        {
            Assert.AreEqual(JNull.Value, Parse("null"));
        }

        [ Test ]
        public void ParseEmptyArray()
        {
            JArray values = (JArray) Parse("[]");
            Assert.IsNotNull(values);
            Assert.AreEqual(0, values.Length);
        }

        [ Test ]
        public void ParseArray()
        {
            JArray values = (JArray) Parse("[123,'Hello World',true]");
            Assert.IsNotNull(values);
            Assert.AreEqual(3, values.Length);
            Assert.AreEqual(123, values[0]);
            Assert.AreEqual("Hello World", values[1]);
            Assert.AreEqual(true, values[2]);
        }

        [ Test ]
        public void ParseEmptyObject()
        {
            IDictionary o = (IDictionary) Parse("{}");
            Assert.IsNotNull(o);
            Assert.AreEqual(0, o.Count);
        }

        [ Test ]
        public void ParseObject()
        {
            IDictionary article = (IDictionary) Parse(@"
                /* Article */ {
                    Title : 'Introduction to JSON',
                    Rating : 2,
                    Abstract : null,
                    Author : {
                        Name : 'John Doe',
                        'E-Mail Address' : 'john.doe@example.com' 
                    },
                    References : [
                        { Title : 'JSON RPC', Link : 'http://www.json-rpc.org/' }
                    ]
                }");

            Assert.IsNotNull(article);
            Assert.AreEqual(5, article.Count);
            Assert.AreEqual("Introduction to JSON", article["Title"]);
            Assert.AreEqual(2, article["Rating"]);
            Assert.AreEqual(JNull.Value, article["Abstract"]);
            
            IDictionary author = (IDictionary) article["Author"];
            Assert.IsNotNull(author);
            Assert.AreEqual(2, author.Count);
            Assert.AreEqual("John Doe", author["Name"]);
            Assert.AreEqual("john.doe@example.com", author["E-Mail Address"]);

            JArray references = (JArray) article["References"];
            Assert.IsNotNull(references);
            Assert.AreEqual(1, references.Length);

            IDictionary reference = (IDictionary) references[0];
            Assert.IsNotNull(reference);
            Assert.AreEqual(2, reference.Count);
            Assert.AreEqual("JSON RPC", reference["Title"]);
            Assert.AreEqual("http://www.json-rpc.org/", reference["Link"]);
        }

        [ Test, ExpectedException(typeof(ParseException)) ]
        public void ParseUnclosedComment()
        {
            Parse(@"/* This is an unclosed comment");
        }

        [ Test, ExpectedException(typeof(ParseException)) ]
        public void ParseUnterminatedString()
        {
            Parse("\"Hello World'");
        }

        [ Test, ExpectedException(typeof(ParseException)) ]
        public void CannotParseEmpty()
        {
            Parse(string.Empty);
        }

        [ Test ]
        public void Reparsing()
        {
            JsonParser parser = new JsonParser();
            Assert.AreEqual("Hello", parser.Parse("\"Hello\""));
            Assert.AreEqual(123, parser.Parse("123"));
        }

        [ Test, ExpectedException(typeof(ParseException)) ]
        public void ParseBadNumber()
        {
            Parse("1234S6");
        }

        private object Parse(string s)
        {
            JsonParser parser = new JsonParser();
            return parser.Parse(s);
        }
    }
}
