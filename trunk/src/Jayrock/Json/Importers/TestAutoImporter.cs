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

    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestAutoImporter
    {
        [ Test ]
        public void EmptyMemberName()
        {
            ParserOutput output = new ParserOutput();
            output.StartObject();
            output.ObjectPut(string.Empty, string.Empty);
            JObject o = (JObject) output.EndObject();
            Assert.AreEqual(1, o.Count);
            Assert.AreEqual(string.Empty, o[string.Empty]);
        }

        [ Test ]
        public void ParseNumber()
        {
            Assert.AreEqual(123, Parse("123"));
        }

        [ Test ]
        public void ParseString()
        {
            // TODO: Move some of these test to JsonTextWriter since these have more to do with parsing than importing.
            
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
            Assert.IsNull(Parse("null"));
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
            JObject o = (JObject) Parse("{}");
            Assert.IsNotNull(o);
            Assert.AreEqual(0, o.Count);
        }

        [ Test ]
        public void ParseObject()
        {
            JObject article = (JObject) Parse(@"
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
            Assert.IsTrue(article.Contains("Abstract"));
            Assert.IsNull(article["Abstract"]);
            
            JObject author = (JObject) article["Author"];
            Assert.IsNotNull(author);
            Assert.AreEqual(2, author.Count);
            Assert.AreEqual("John Doe", author["Name"]);
            Assert.AreEqual("john.doe@example.com", author["E-Mail Address"]);

            JArray references = (JArray) article["References"];
            Assert.IsNotNull(references);
            Assert.AreEqual(1, references.Length);

            JObject reference = (JObject) references[0];
            Assert.IsNotNull(reference);
            Assert.AreEqual(2, reference.Count);
            Assert.AreEqual("JSON RPC", reference["Title"]);
            Assert.AreEqual("http://www.json-rpc.org/", reference["Link"]);
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void ParseUnclosedComment()
        {
            Parse(@"/* This is an unclosed comment");
        }

        [ Test, ExpectedException(typeof(ParseException)) ]
        public void ParseUnterminatedString()
        {
            Parse("\"Hello World'");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotParseEmpty()
        {
            Parse(string.Empty);
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void ParseBadNumber()
        {
            Parse("1234S6");
        }

        private object Parse(string s)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(s));
            object value = JsonImporterStock.Auto.Import(reader);
            Assert.IsTrue(reader.EOF);
            return value;
        }
    }
}