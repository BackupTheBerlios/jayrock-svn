namespace Jayrock.Json
{
    #region Imports

    using System.Collections;
    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestParserOutput
    {
        [ Test ]
        public void ArrayInsideObject()
        {
            ParserOutput output = new ParserOutput();
            Assert.IsNull(output.TestCurrentArray, "No current array initially.");
            Assert.IsNull(output.TestCurrentObject, "No current object initially.");
            Assert.AreEqual(0, output.TestStack.Count, "Stack is empty initially.");

            output.StartObject();
            Assert.IsNotNull(output.TestCurrentObject, "Has object when creating an object.");
            Assert.IsNull(output.TestCurrentArray, "No array when creating an object.");
            Assert.AreEqual(0, output.TestStack.Count, "Stack empty at top-level." );

            output.StartArray();
            Assert.IsNotNull(output.TestCurrentArray, "Has array when creating an array, even while inside creating an object.");
            Assert.IsNull(output.TestCurrentObject, "No object when creating an array, even while inside creating an object.");
            Assert.AreEqual(1, output.TestStack.Count, "One object pending on stack.");

            output.EndArray();
            Assert.IsNotNull(output.TestCurrentObject, "Back to working with an object after finishing the nested array.");
            Assert.IsNull(output.TestCurrentArray, "No array when back to creating the last object.");
            Assert.AreEqual(0, output.TestStack.Count, "Stack empty since returned to top-level.");
            
            output.EndObject();
            Assert.IsNull(output.TestCurrentArray, "No current array when done.");
            Assert.IsNull(output.TestCurrentObject, "No current object when done.");
            Assert.AreEqual(0, output.TestStack.Count, "Stack is empty when done.");
        }

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
            return reader.DeserializeNext();
        }
    }
}
