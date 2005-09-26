namespace Jayrock.Json
{
    #region Imports

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
    }
}
