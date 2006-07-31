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
    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonTextReader
    {
        private JsonTextReader _reader;

        [ TearDown ]
        public void Dispose()
        {
            _reader = null;
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void Blank()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(string.Empty));
            reader.Read();
        }

        [ Test ]
        public void BOF()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(string.Empty));
            Assert.AreEqual(JsonToken.BOF, reader.Token);
        }

        [ Test ]
        public void String()
        {
            CreateReader("'Hello World'");
            AssertTokenText(JsonToken.String, "Hello World");
            AssertEOF();
        }

        [ Test ]
        public void Number()
        {
            CreateReader("123");
            AssertTokenText(JsonToken.Number, "123");
            AssertEOF();
        }

        [ Test ]
        public void Null()
        {
            CreateReader("null");
            AssertTokenText(JsonToken.Null, "null");
            AssertEOF();
        }

        [ Test ]
        public void BooleanTrue()
        {
            CreateReader("true");
            AssertTokenText(JsonToken.Boolean, "true");
            AssertEOF();
        }
        
        [ Test ]
        public void BooleanFalse()
        {
            CreateReader("false");
            AssertTokenText(JsonToken.Boolean, "false");
            AssertEOF();
        }

        [ Test ]
        public void EmptyArray()
        {
            CreateReader("[]");
            AssertToken(JsonToken.Array);
            AssertToken(JsonToken.EndArray);
            AssertEOF();
        }

        [ Test ]
        public void ArrayWithOneNumber()
        {
            CreateReader("[ 123 ]");
            AssertToken(JsonToken.Array);
            AssertTokenText(JsonToken.Number, "123");
            AssertToken(JsonToken.EndArray);
            AssertEOF();
        }

        [ Test ]
        public void ArrayWithPrimitives()
        {
            CreateReader("[ 123, 'string', true, false, null ]");
            AssertToken(JsonToken.Array);
            AssertTokenText(JsonToken.Number, "123");
            AssertTokenText(JsonToken.String, "string");
            AssertTokenText(JsonToken.Boolean, "true");
            AssertTokenText(JsonToken.Boolean, "false");
            AssertToken(JsonToken.Null);
            AssertToken(JsonToken.EndArray);
            AssertEOF();
        }

        [ Test ]
        public void EmptyObject()
        {
            CreateReader("{}");
            AssertToken(JsonToken.Object);
            AssertToken(JsonToken.EndObject);
            AssertEOF();
        }

        [ Test ]
        public void ObjectWithOneMember()
        {
            CreateReader("{ 'num' : 123 }");
            AssertToken(JsonToken.Object);
            AssertTokenText(JsonToken.Member, "num");
            AssertTokenText(JsonToken.Number, "123");
            AssertToken(JsonToken.EndObject);
            AssertEOF();
        }

        [ Test ]
        public void ObjectWithPrimitiveMembers()
        {
            CreateReader("{ m1 : 123, m2 : 'string', m3 : true, m4 : false, m5 : null }");
            AssertToken(JsonToken.Object);
            AssertMember("m1", JsonToken.Number, "123");
            AssertMember("m2", "string");
            AssertMember("m3", JsonToken.Boolean, "true");
            AssertMember("m4", JsonToken.Boolean, "false");
            AssertMember("m5", JsonToken.Null);
            AssertToken(JsonToken.EndObject);
            AssertEOF();
        }

        [ Test ]
        public void Complex()
        {
            CreateReader(@"
                {'menu': {
                    'header': 'SVG Viewer',
                    'items': [
                        {'id': 'Open'},
                        {'id': 'OpenNew', 'label': 'Open New'},
                        null,
                        {'id': 'ZoomIn', 'label': 'Zoom In'},
                        {'id': 'ZoomOut', 'label': 'Zoom Out'},
                        {'id': 'OriginalView', 'label': 'Original View'},
                        null,
                        {'id': 'Quality'},
                        {'id': 'Pause'},
                        {'id': 'Mute'}
                    ]
                }}");

            AssertToken(JsonToken.Object);
            AssertMember("menu", JsonToken.Object);
            AssertMember("header", "SVG Viewer");
            AssertMember("items", JsonToken.Array);
            
            AssertToken(JsonToken.Object);
            AssertMember("id", "Open");
            AssertToken(JsonToken.EndObject);
            
            AssertToken(JsonToken.Object);
            AssertMember("id", "OpenNew");
            AssertMember("label", "Open New");
            AssertToken(JsonToken.EndObject);

            AssertToken(JsonToken.Null);

            AssertToken(JsonToken.Object);
            AssertMember("id", "ZoomIn");
            AssertMember("label", "Zoom In");
            AssertToken(JsonToken.EndObject);

            AssertToken(JsonToken.Object);
            AssertMember("id", "ZoomOut");
            AssertMember("label", "Zoom Out");
            AssertToken(JsonToken.EndObject);

            AssertToken(JsonToken.Object);
            AssertMember("id", "OriginalView");
            AssertMember("label", "Original View");
            AssertToken(JsonToken.EndObject);

            AssertToken(JsonToken.Null);
            
            AssertToken(JsonToken.Object);
            AssertMember("id", "Quality");
            AssertToken(JsonToken.EndObject);

            AssertToken(JsonToken.Object);
            AssertMember("id", "Pause");
            AssertToken(JsonToken.EndObject);

            AssertToken(JsonToken.Object);
            AssertMember("id", "Mute");
            AssertToken(JsonToken.EndObject);
            
            AssertToken(JsonToken.EndArray);
            AssertToken(JsonToken.EndObject);
            AssertToken(JsonToken.EndObject);
            AssertEOF();
        }

        [ Test ]
        public void OneLevelDepth()
        {
         
            CreateReader("[]");
            Assert.AreEqual(0, _reader.Depth);
            AssertToken(JsonToken.Array);
            Assert.AreEqual(1, _reader.Depth);
            AssertToken(JsonToken.EndArray);
            Assert.AreEqual(1, _reader.Depth);
            AssertEOF();
            Assert.AreEqual(0, _reader.Depth);
        }

        [ Test ]
        public void TwoLevelDepth()
        {         
            CreateReader("[{}]");
            Assert.AreEqual(0, _reader.Depth);

            AssertToken(JsonToken.Array);
            Assert.AreEqual(1, _reader.Depth);

            AssertToken(JsonToken.Object);
            Assert.AreEqual(2, _reader.Depth);
            AssertToken(JsonToken.EndObject);
            Assert.AreEqual(2, _reader.Depth);
            
            AssertToken(JsonToken.EndArray);
            Assert.AreEqual(1, _reader.Depth);

            AssertEOF();
            Assert.AreEqual(0, _reader.Depth);
        }
        
        [ Test ]
        public void NestedDepths()
        {
            CreateReader("[{a:[{}]}]");

            const int maxDepth = 4;
            for (int i = 0; i < maxDepth; i++)
            {
                Assert.AreEqual(i, _reader.Depth);
                
                if (i % 2 == 0)
                {
                    AssertToken(JsonToken.Array);
                }
                else
                {
                    AssertToken(JsonToken.Object);
                    if (i < (maxDepth - 1))
                        AssertToken(JsonToken.Member);
                }

                Assert.AreEqual(i + 1, _reader.Depth);
            }

            for (int i = 0; i < maxDepth; i++)
            {
                AssertToken(i % 2 == 0 ? JsonToken.EndObject : JsonToken.EndArray);
                Assert.AreEqual(maxDepth - i, _reader.Depth);
            }

            AssertEOF();
            Assert.AreEqual(0, _reader.Depth);
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void BadNumber()
        {
            CreateReader("123-45").Read();
        }

        [ Test, ExpectedException(typeof(ParseException)) ]
        public void UnterminatedString()
        {
            CreateReader("'string").Read();
        }

        [ Test ]
        public void UnquotedStringsMayWork()
        {
            CreateReader("hello");
            Assert.AreEqual("hello", _reader.ReadString());
        }

        [ Test ]
        public void Shred()
        {
            JsonReader reader = CreateReader(@"
                {'web-app': {
                'servlet': [   
                    {
                    'servlet-name': 'cofaxCDS',
                    'servlet-class': 'org.cofax.cds.CDSServlet',
                    'init-param': {
                        'configGlossary:installationAt': 'Philadelphia, PA',
                        'configGlossary:adminEmail': 'ksm@pobox.com',
                        'configGlossary:poweredBy': 'Cofax',
                        'configGlossary:poweredByIcon': '/images/cofax.gif',
                        'configGlossary:staticPath': '/content/static',
                        'templateProcessorClass': 'org.cofax.WysiwygTemplate',
                        'templateLoaderClass': 'org.cofax.FilesTemplateLoader',
                        'templatePath': 'templates',
                        'templateOverridePath': '',
                        'defaultListTemplate': 'listTemplate.htm',
                        'defaultFileTemplate': 'articleTemplate.htm',
                        'useJSP': false,
                        'jspListTemplate': 'listTemplate.jsp',
                        'jspFileTemplate': 'articleTemplate.jsp',
                        'cachePackageTagsTrack': 200,
                        'cachePackageTagsStore': 200,
                        'cachePackageTagsRefresh': 60,
                        'cacheTemplatesTrack': 100,
                        'cacheTemplatesStore': 50,
                        'cacheTemplatesRefresh': 15,
                        'cachePagesTrack': 200,
                        'cachePagesStore': 100,
                        'cachePagesRefresh': 10,
                        'cachePagesDirtyRead': 10,
                        'searchEngineListTemplate': 'forSearchEnginesList.htm',
                        'searchEngineFileTemplate': 'forSearchEngines.htm',
                        'searchEngineRobotsDb': 'WEB-INF/robots.db',
                        'useDataStore': true,
                        'dataStoreClass': 'org.cofax.SqlDataStore',
                        'redirectionClass': 'org.cofax.SqlRedirection',
                        'dataStoreName': 'cofax',
                        'dataStoreDriver': 'com.microsoft.jdbc.sqlserver.SQLServerDriver',
                        'dataStoreUrl': 'jdbc:microsoft:sqlserver://LOCALHOST:1433;DatabaseName=goon',
                        'dataStoreUser': 'sa',
                        'dataStorePassword': 'dataStoreTestQuery',
                        'dataStoreTestQuery': 'SET NOCOUNT ON;select test=\'test\';',
                        'dataStoreLogFile': '/usr/local/tomcat/logs/datastore.log',
                        'dataStoreInitConns': 10,
                        'dataStoreMaxConns': 100,
                        'dataStoreConnUsageLimit': 100,
                        'dataStoreLogLevel': 'debug',
                        'maxUrlLength': 500}},
                    {
                    'servlet-name': 'cofaxEmail',
                    'servlet-class': 'org.cofax.cds.EmailServlet',
                    'init-param': {
                    'mailHost': 'mail1',
                    'mailHostOverride': 'mail2'}},
                    {
                    'servlet-name': 'cofaxAdmin',
                    'servlet-class': 'org.cofax.cds.AdminServlet'},
                 
                    {
                    'servlet-name': 'fileServlet',
                    'servlet-class': 'org.cofax.cds.FileServlet'},
                    {
                    'servlet-name': 'cofaxTools',
                    'servlet-class': 'org.cofax.cms.CofaxToolsServlet',
                    'init-param': {
                        'templatePath': 'toolstemplates/',
                        'log': 1,
                        'logLocation': '/usr/local/tomcat/logs/CofaxTools.log',
                        'logMaxSize': '',
                        'dataLog': 1,
                        'dataLogLocation': '/usr/local/tomcat/logs/dataLog.log',
                        'dataLogMaxSize': '',
                        'removePageCache': '/content/admin/remove?cache=pages&id=',
                        'removeTemplateCache': '/content/admin/remove?cache=templates&id=',
                        'fileTransferFolder': '/usr/local/tomcat/webapps/content/fileTransferFolder',
                        'lookInContext': 1,
                        'adminGroupID': 4,
                        'betaServer': true}}],
                'servlet-mapping': {
                    'cofaxCDS': '/',
                    'cofaxEmail': '/cofaxutil/aemail/*',
                    'cofaxAdmin': '/admin/*',
                    'fileServlet': '/static/*',
                    'cofaxTools': '/tools/*'},
                 
                'taglib': {
                    'taglib-uri': 'cofax.tld',
                    'taglib-location': '/WEB-INF/tlds/cofax.tld'}}}");

            ArrayList items = new ArrayList();

            while (reader.Read())
            {
                if (reader.Token == JsonToken.Member && reader.Text == "servlet-name")
                {
                    reader.Read();
                    items.Add(reader.ReadString());
                }
            }

            Assert.AreEqual(new string[] { "cofaxCDS", "cofaxEmail", "cofaxAdmin", "fileServlet", "cofaxTools" }, items.ToArray(typeof(string)));
        }

        private void AssertToken(JsonToken token)
        {
            AssertTokenText(token, null);
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void UnterminatedObject()
        {
            JsonReader reader = CreateReader("{x:1,y:2");
            reader.MoveToContent();
            reader.StepOut();
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void UnterminatedArray()
        {
            JsonReader reader = CreateReader("[1,2");
            reader.MoveToContent();
            reader.StepOut();
        }
        
        [ Test, ExpectedException(typeof(JsonException)) ]
        public void MissingObjectMember()
        {
            JsonReader reader = CreateReader("{x:1,/*y:2*/,z:3}");
            reader.MoveToContent();
            reader.StepOut();
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void MissingObjectMemberNameValueDelimiter()
        {
            JsonReader reader = CreateReader("{x 1}");
            reader.MoveToContent();
            reader.StepOut();
        }

        private void AssertTokenText(JsonToken token, string text)
        {
            Assert.IsTrue(_reader.Read());
            Assert.AreEqual(token, _reader.Token, "Found {0} (with text \x201c{1}\x201d) when expecting {2} (with text \x201c{3}\x201d).", _reader.Token, _reader.Text, token, text);
            if (text != null)
                Assert.AreEqual(text, _reader.Text);
        }

        private void AssertMember(string name, JsonToken valueToken)
        {
            AssertMember(name, valueToken, null);
        }

        private void AssertMember(string name, string value)
        {
            AssertMember(name, JsonToken.String, value);
        }
        
        private void AssertMember(string name, JsonToken valueToken, string valueText)
        {
            AssertTokenText(JsonToken.Member, name);
            AssertTokenText(valueToken, valueText);
        }

        private void AssertEOF()
        {
            Assert.IsFalse(_reader.Read(), "Expected EOF.");
            Assert.AreEqual(JsonToken.EOF, _reader.Token);
            Assert.AreEqual(string.Empty, _reader.Text);
        }

        private JsonReader CreateReader(string s)
        {
            _reader = new JsonTextReader(new StringReader(s));
            return _reader;
        }
    }
}
