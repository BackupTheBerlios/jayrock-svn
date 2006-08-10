namespace Jayrock.Json.Importers
{
    using System.IO;
    using NUnit.Framework;

    [ TestFixture ]
    public class TestBooleanImporter
    {
        [ Test ]
        public void ImportNull()
        {
            Assert.IsNull(Import("null"));
        }

        [ Test ]
        public void ImportTrue()
        {
            AssertImport(true, "true");
        }

        [ Test ]
        public void ImportFalse()
        {
            AssertImport(false, "false");
        }

        [ Test ]
        public void ImportNonZeroNumber()
        {
            AssertImport(true, "123");
        }

        [ Test ]
        public void ImportZeroNumber()
        {
            AssertImport(false, "0");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportFractionalNumbers()
        {
            Import("0.5");
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportString()
        {
            Import("'true'");
        }
       
        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportArray()
        {
            Import("[]");
        }
        
        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotImportObject()
        {
            Import("{}");
        }
        
        private static void AssertImport(bool expected, string input)
        {
            object o = Import(input);
            Assert.IsInstanceOfType(typeof(bool), o);
            Assert.AreEqual(expected, o);
        }

        private static object Import(string input)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(input));
            object o = JsonImporterStock.Boolean.Import(reader);
            Assert.IsTrue(reader.EOF, "Reader must be at EOF.");
            return o;
        }
    }
}