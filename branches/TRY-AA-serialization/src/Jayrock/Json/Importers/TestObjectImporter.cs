namespace Jayrock.Json.Importers
{
    using System;
    using System.IO;
    using NUnit.Framework;

    [ TestFixture ]
    public class TestObjectImporter
    {
        [ Test ]
        public void ImportNull()
        {
            ObjectImporter importer = new ObjectImporter(typeof(object));
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
            Person p = (Person) Import("{Id:42,FullName:'Bob'}");
            Assert.AreEqual(42, p.Id, "Id");
            Assert.AreEqual("Bob", p.FullName, "FullName");
            Assert.IsNull(p.Spouce, "Spouce");
        }

        [ Test ]
        public void ImportEmbeddedObejects()
        {
            Person p = (Person) Import("{Id:42,FullName:'Bob',Spouce:{FullName:'Alice',Id:43,Spouce:null}}");
            Assert.AreEqual(42, p.Id, "Id");
            Assert.AreEqual("Bob", p.FullName, "FullName");
            Assert.IsNotNull(p.Spouce, "Spouce");
            p = p.Spouce;
            Assert.AreEqual(43, p.Id, "Id");
            Assert.AreEqual("Alice", p.FullName, "FullName");
            Assert.IsNull(p.Spouce, "Spouce");
        }

        private static object Import(string s)
        {
            Type expectedType = typeof(Person);
            JsonReader reader = CreateReader(s);
            ITypeImporterRegistry registry = reader.TypeImporterRegistry;
            (new ObjectImporter(expectedType)).Register(registry);
            TypeImporterStock.Int32.Register(registry);
            TypeImporterStock.String.Register(registry);
            object o = reader.Get(expectedType);            
            Assert.IsNotNull(o);
            Assert.IsInstanceOfType(expectedType, o);
            return o;
        }

        private static JsonReader CreateReader(string s)
        {
            return new JsonTextReader(new StringReader(s));
        }

    }
}