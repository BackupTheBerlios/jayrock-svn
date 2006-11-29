namespace Jayrock.Json.Conversion
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Data;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using Jayrock.Json.Conversion.Exporters;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestExportContext
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
            AssertInStock(typeof(ControlExporter), typeof(Control));
            AssertInStock(typeof(ControlExporter), typeof(HtmlControl));
            AssertInStock(typeof(ControlExporter), typeof(HtmlImage));
            AssertInStock(typeof(DataSetExporter), typeof(DataSet));
            AssertInStock(typeof(DataSetExporter), typeof(MyDataSet));
            AssertInStock(typeof(DataTableExporter), typeof(DataTable));
            AssertInStock(typeof(DataTableExporter), typeof(MyDataTable));
            AssertInStock(typeof(DataRowExporter), typeof(DataRow));
            AssertInStock(typeof(DataRowExporter), typeof(MyDataRow));
            AssertInStock(typeof(DataRowViewExporter), typeof(DataRowView));
        }

        private static void AssertInStock(Type expected, Type type)
        {
            ExportContext context = new ExportContext();
            ITypeExporter exporter = context.FindExporter(type);
            Assert.IsNotNull(exporter, "No exporter found for {0}", type.FullName);
            Assert.IsInstanceOfType(expected, exporter, type.FullName);
        }
        
        private sealed class ExportableThing : IJsonExportable
        {
            public void Export(ExportContext context, JsonWriter writer)
            {
                throw new NotImplementedException();
            }
        }

        private sealed class MyDataSet : DataSet
        {
        }
        
        private sealed class MyDataTable : DataTable
        {
        }
        
        private sealed class MyDataRow : DataRow
        {
            public MyDataRow(DataRowBuilder builder) : 
                base(builder) {}
        }        
    }
}