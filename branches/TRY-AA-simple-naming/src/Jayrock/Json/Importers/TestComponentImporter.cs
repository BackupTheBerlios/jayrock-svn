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

    using System;
    using System.IO;
    using NetMatters;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestComponentImporter
    {
        [ Test ]
        public void ImportNull()
        {
            ComponentImporter importer = new ComponentImporter(typeof(object));
            Assert.IsNull(importer.Import(CreateReader("null")));
        }

        private sealed class Person
        {
            private int _id;
            private string _fullName;
            private Person _spouce;

            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }

            public string FullName
            {
                get { return _fullName; }
                set { _fullName = value; }
            }

            public Person Spouce
            {
                get { return _spouce; }
                set { _spouce = value; }
            }
        }

        [ Test ]
        public void ImportEmptyObject()
        {
            Person p = (Person) Import("{}");
            Assert.AreEqual(0, p.Id);
            Assert.IsNull(p.FullName, "FullName");
            Assert.IsNull(p.Spouce, "Spouce");
        }
                
        [ Test ]
        public void ImportObject()
        {
            Person p = (Person) Import("{ Id : 42, FullName : 'Bob'}");
            Assert.AreEqual(42, p.Id, "Id");
            Assert.AreEqual("Bob", p.FullName, "FullName");
            Assert.IsNull(p.Spouce, "Spouce");
        }

        [ Test ]
        public void ImportEmbeddedObjects()
        {
            Person p = (Person) Import(@"{
                Id : 42,
                FullName : 'Bob',
                Spouce: { 
                    FullName : 'Alice', 
                    Id       : 43,
                    Spouce   : null 
                } 
            }");
            Assert.AreEqual(42, p.Id, "Id");
            Assert.AreEqual("Bob", p.FullName, "FullName");
            Assert.IsNotNull(p.Spouce, "Spouce");
            p = p.Spouce;
            Assert.AreEqual(43, p.Id, "Id");
            Assert.AreEqual("Alice", p.FullName, "FullName");
            Assert.IsNull(p.Spouce, "Spouce");
        }
        
        [ Test ]
        public void YahooNewsSearch()
        {
            string text = @"
            /* Source: http://api.search.yahoo.com/NewsSearchService/V1/newsSearch?appid=YahooDemo&query=yahoo&results=3&language=en&output=json */
            {
                ""ResultSet"": {
                    ""totalResultsAvailable"": ""2393"",
                    ""totalResultsReturned"": 3,
                    ""firstResultPosition"": ""1"",
                    ""Result"": [
                        {
                            ""Title"": ""Yahoo invites its users to shoot ads"",
                            ""Summary"": "" Yahoo first encouraged consumers to create blogs and photo pages with text and pictures. Now, the Internet portal wants them to make advertisements, too. On Monday, Yahoo touts a new look for its front page by asking people to pull out the video camera, open up the editing software and create 12-second spot for Yahoo."",
                            ""Url"": ""http://news.yahoo.com/s/usatoday/20060717/tc_usatoday/yahooinvitesitsuserstoshootads"",
                            ""ClickUrl"": ""http://news.yahoo.com/s/usatoday/20060717/tc_usatoday/yahooinvitesitsuserstoshootads"",
                            ""NewsSource"": ""USATODAY.com via Yahoo! News"",
                            ""NewsSourceUrl"": ""http://news.yahoo.com/"",
                            ""Language"": ""en"",
                            ""PublishDate"": ""1153133816"",
                            ""ModificationDate"": ""1153134044""
                        },
                        {
                            ""Title"": ""Yahoo to launch new finance features"",
                            ""Summary"": "" Yahoo Inc. is beefing up the finance section of its Web site with more interactive stock charts and other features to help it maintain its longtime lead over rival financial information sites."",
                            ""Url"": ""http://news.yahoo.com/s/ap/20060717/ap_on_hi_te/yahoo_finance_2"",
                            ""ClickUrl"": ""http://news.yahoo.com/s/ap/20060717/ap_on_hi_te/yahoo_finance_2"",
                            ""NewsSource"": ""AP via Yahoo! News"",
                            ""NewsSourceUrl"": ""http://news.yahoo.com/"",
                            ""Language"": ""en"",
                            ""PublishDate"": ""1153134777"",
                            ""ModificationDate"": ""1153134920"",
                            ""Thumbnail"": {
                                ""Url"": ""http://us.news2.yimg.com/us.yimg.com/p/ap/20060714/vsthumb.8b1161b66b564adba0a5bbd6339c9379.media_summit_idet125.jpg"",
                                ""Height"": ""82"",
                                ""Width"": ""76""
                            }
                        }, 
                        {
                            ""Title"": ""Yahoo Finance revises charts, chat, other features"",
                            ""Summary"": "" Yahoo Inc. on Monday will unveil an upgraded version of its top-ranked financial information site that features new stock charting tools, improved investor chat rooms and financial video news."",
                            ""Url"": ""http://news.yahoo.com/s/nm/20060717/wr_nm/media_yahoo_finance_dc_2"",
                            ""ClickUrl"": ""http://news.yahoo.com/s/nm/20060717/wr_nm/media_yahoo_finance_dc_2"",
                            ""NewsSource"": ""Reuters via Yahoo! News"",
                            ""NewsSourceUrl"": ""http://news.yahoo.com/"",
                            ""Language"": ""en"",
                            ""PublishDate"": ""1153113288"",
                            ""ModificationDate"": ""1153113674""
                        }
                    ]
                }
            }";
            
            JsonTextReader reader = new JsonTextReader(new StringReader(text));
            (new ComponentImporter(typeof(YahooResponse), new FieldsToPropertiesProxyTypeDescriptor(typeof(YahooResponse)))).Register(reader.Importers);
            (new ComponentImporter(typeof(YahooResultSet), new FieldsToPropertiesProxyTypeDescriptor(typeof(YahooResultSet)))).Register(reader.Importers);
            (new ComponentImporter(typeof(YahooResult), new FieldsToPropertiesProxyTypeDescriptor(typeof(YahooResult)))).Register(reader.Importers);
            (new ComponentImporter(typeof(YahooThumbnail), new FieldsToPropertiesProxyTypeDescriptor(typeof(YahooThumbnail)))).Register(reader.Importers);
            
            YahooResponse response = (YahooResponse) reader.Get(typeof(YahooResponse));
            Assert.IsNotNull(response);
            
            YahooResultSet resultSet = response.ResultSet;
            Assert.IsNotNull(resultSet);
            Assert.AreEqual(2393,  resultSet.totalResultsAvailable);
            Assert.AreEqual(3,  resultSet.totalResultsReturned);
            Assert.AreEqual(1,  resultSet.firstResultPosition);
            Assert.AreEqual(3,  resultSet.Result.Length);
            
            YahooResult result = resultSet.Result[0];
            
            Assert.IsNotNull(result);
            Assert.AreEqual("Yahoo invites its users to shoot ads", result.Title);
            Assert.AreEqual(" Yahoo first encouraged consumers to create blogs and photo pages with text and pictures. Now, the Internet portal wants them to make advertisements, too. On Monday, Yahoo touts a new look for its front page by asking people to pull out the video camera, open up the editing software and create 12-second spot for Yahoo.", result.Summary);
            Assert.AreEqual("http://news.yahoo.com/s/usatoday/20060717/tc_usatoday/yahooinvitesitsuserstoshootads", result.Url);
            Assert.AreEqual("http://news.yahoo.com/s/usatoday/20060717/tc_usatoday/yahooinvitesitsuserstoshootads", result.ClickUrl);
            Assert.AreEqual("USATODAY.com via Yahoo! News", result.NewsSource);
            Assert.AreEqual("http://news.yahoo.com/", result.NewsSourceUrl);
            Assert.AreEqual("en", result.Language);
            Assert.AreEqual(1153133816, result.PublishDate);
            Assert.AreEqual(1153134044, result.ModificationDate);
            
            result = resultSet.Result[1];

            Assert.AreEqual("Yahoo to launch new finance features", result.Title);
            Assert.AreEqual(" Yahoo Inc. is beefing up the finance section of its Web site with more interactive stock charts and other features to help it maintain its longtime lead over rival financial information sites.", result.Summary);
            Assert.AreEqual("http://news.yahoo.com/s/ap/20060717/ap_on_hi_te/yahoo_finance_2", result.Url);
            Assert.AreEqual("http://news.yahoo.com/s/ap/20060717/ap_on_hi_te/yahoo_finance_2", result.ClickUrl);
            Assert.AreEqual("AP via Yahoo! News", result.NewsSource);
            Assert.AreEqual("http://news.yahoo.com/", result.NewsSourceUrl);
            Assert.AreEqual("en", result.Language);
            Assert.AreEqual(1153134777, result.PublishDate);
            Assert.AreEqual(1153134920, result.ModificationDate);
            Assert.AreEqual("http://us.news2.yimg.com/us.yimg.com/p/ap/20060714/vsthumb.8b1161b66b564adba0a5bbd6339c9379.media_summit_idet125.jpg", result.Thumbnail.Url);
            Assert.AreEqual(82, result.Thumbnail.Height);
            Assert.AreEqual(76, result.Thumbnail.Width);

            result = resultSet.Result[2];

            Assert.AreEqual("Yahoo Finance revises charts, chat, other features", result.Title);
            Assert.AreEqual(" Yahoo Inc. on Monday will unveil an upgraded version of its top-ranked financial information site that features new stock charting tools, improved investor chat rooms and financial video news.", result.Summary);
            Assert.AreEqual("http://news.yahoo.com/s/nm/20060717/wr_nm/media_yahoo_finance_dc_2", result.Url);
            Assert.AreEqual("http://news.yahoo.com/s/nm/20060717/wr_nm/media_yahoo_finance_dc_2", result.ClickUrl);
            Assert.AreEqual("Reuters via Yahoo! News", result.NewsSource);
            Assert.AreEqual("http://news.yahoo.com/", result.NewsSourceUrl);
            Assert.AreEqual("en", result.Language);
            Assert.AreEqual(1153113288, result.PublishDate);
            Assert.AreEqual(1153113674, result.ModificationDate);
        }
        
        private static object Import(string s)
        {
            Type expectedType = typeof(Person);
            JsonReader reader = CreateReader(s);
            IJsonImporterRegistry registry = reader.Importers;
            (new ComponentImporter(expectedType)).Register(registry);
            object o = reader.Get(expectedType);            
            Assert.IsNotNull(o);
            Assert.IsInstanceOfType(expectedType, o);
            return o;
        }

        private static JsonReader CreateReader(string s)
        {
            return new JsonTextReader(new StringReader(s));
        }
        
        //
        // NOTE: The default assignments on the following fields is
        // to maily shut off the following warning from the compiler:
        //
        // warning CS0649: Field '...' is never assigned to, and will always have its default value
        //
        // This warning is harmless. Since the C# 1.x compiler does
        // not support #pragma warning disable, we have to resort
        // to a more brute force method.
        //

        private class YahooResponse
        {
            public YahooResultSet ResultSet = null;
        }

        private class YahooResultSet
        {
            public int totalResultsAvailable = 0;
            public int totalResultsReturned = 0;
            public int firstResultPosition = 0;
            public YahooResult[] Result = null;
        }

        private class YahooResult
        {
            public string Title = null;
            public string Summary = null;
            public string Url = null;
            public string ClickUrl = null;
            public string NewsSource = null;
            public string NewsSourceUrl = null;
            public string Language = null;
            public long PublishDate = 0;
            public long ModificationDate = 0;
            public int Height = 0;
            public int Width = 0;
            public YahooThumbnail Thumbnail = null;
        }

        private class YahooThumbnail
        {
            public string Url = null;
            public int Height = 0;
            public int Width = 0;
        }
    }
}