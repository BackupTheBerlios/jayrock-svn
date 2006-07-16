namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Runtime.Serialization;

    #endregion

    [ Serializable ]
    public class JsonSerializationException : System.ApplicationException
    {
        private const string _defaultMessage = "An error occurred while serializing or deserializing JSON.";

        public JsonSerializationException() : 
            this(null) {}

        public JsonSerializationException(string message) : 
            base(Mask.NullString(message, _defaultMessage), null) {}

        public JsonSerializationException(string message, Exception innerException) :
            base(Mask.NullString(message, _defaultMessage), innerException) {}

        protected JsonSerializationException(SerializationInfo info, StreamingContext context) :
            base(info, context) {}
    }
}