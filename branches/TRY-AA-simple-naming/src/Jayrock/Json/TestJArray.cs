namespace Jayrock.Json
{
    #region Imports

    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJArary
    {
        [ Test ]
        public void AddNullValue()
        {
            JArray a = new JArray();
            a.Add(null);
            Assert.AreEqual(1, a.Count);
            Assert.IsNull(a[0]);
        }
    }
}
