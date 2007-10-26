namespace JsonConversionsDemo
{
    #region Imports

    using System.IO;
    using Jayrock.Json;

    #endregion

    /// <summary>
    /// Facade for working with JsonReader and JsonWriter implementations
    /// that work with JSON text.
    /// </summary>

    public sealed class JsonText
    {
        public static JsonReader CreateReader(string text)
        {
            return new JsonTextReader(new StringReader(text));
        }

        public static JsonWriter CreateWriter(TextWriter writer)
        {
            return CreateWriter(writer, false);
        }

        public static JsonWriter CreatePrettyWriter(TextWriter writer)
        {
            return CreateWriter(writer, true);
        }

        public static JsonWriter CreateWriter(TextWriter writer, bool format)
        {
            JsonTextWriter jsonw = new JsonTextWriter(writer);
            jsonw.PrettyPrint = format;
            return jsonw;
        }

        private JsonText() { }
    }
}