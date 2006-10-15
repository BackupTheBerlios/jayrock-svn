namespace Jayrock.Json.Serialization.Export
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using Jayrock.Json.Serialization.Export.Exporters;
    using NUnit.Framework;

    [ TestFixture ]
    public class TestJsonExport
    {
        [ Test ]
        public void StockExporters()
        {
            AssertInStock(typeof(ByteExporter), typeof(byte));
            AssertInStock(typeof(Int16Exporter), typeof(short));
            AssertInStock(typeof(Int32Exporter), typeof(int));
            AssertInStock(typeof(Int64Exporter), typeof(long));
            AssertInStock(typeof(SingleExporter), typeof(float));
            AssertInStock(typeof(DoubleExporter), typeof(double));
            AssertInStock(typeof(DateTimeExporter), typeof(DateTime));
            AssertInStock(typeof(StringExporter), typeof(string));
            AssertInStock(typeof(BooleanExporter), typeof(bool));
            AssertInStock(typeof(ComponentExporter), typeof(object));
            AssertInStock(typeof(EnumerableExporter), typeof(object[]));
            AssertInStock(typeof(NameValueCollectionExporter), typeof(NameValueCollection));
            // FIXME: Add case AssertInStock(typeof(EnumExporter), typeof(System.Globalization.UnicodeCategory));
            AssertInStock(typeof(ExportAwareExporter), typeof(JsonObject));
            AssertInStock(typeof(DictionaryExporter), typeof(Hashtable));
            AssertInStock(typeof(ExportAwareExporter), typeof(JsonArray));
            AssertInStock(typeof(EnumerableExporter), typeof(ArrayList));
            AssertInStock(typeof(ExportAwareExporter), typeof(ExportableThing));
        }

        private static void AssertInStock(Type expected, Type type)
        {
            IJsonExporter importer = JsonExport.GetExporter(type);
            Assert.IsInstanceOfType(expected, importer, type.FullName);
        }
        
        private sealed class ExportableThing : IJsonExportable
        {
            public void Export(JsonWriter writer, object context)
            {
                throw new NotImplementedException();
            }
        }
    }
}