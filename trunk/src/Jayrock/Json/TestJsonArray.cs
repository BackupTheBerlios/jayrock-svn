namespace Jayrock.Json
{
    #region Imports

    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonArray
    {
        [ Test ]
        public void AddNullValue()
        {
            JsonArray a = new JsonArray();
            a.Add(null);
            Assert.AreEqual(1, a.Count);
            Assert.IsNull(a[0]);
        }
    }
}
