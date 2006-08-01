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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Collections;
    using System.Globalization;

    #endregion
    
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only 
    /// access to JSON data. 
    /// </summary>

    public abstract class JsonReader
    {
        private IJsonImporterRegistry _importers;
        private TokenText _token;
        private int _depth;

        public const string TrueText = "true";
        public const string FalseText = "false";
        public const string NullText = "null";

        public JsonReader()
        {
            _token = new TokenText(JsonToken.BOF);
        }
 
        /// <summary>
        /// Gets the current token.
        /// </summary>

        public JsonToken Token
        {
            get { return _token.Token; }
        }

        /// <summary>
        /// Gets the current token text, if applicable. Otherwise returns an 
        /// empty string. Note that for tokens like 
        /// <see cref="JsonToken.String"/>, an empty string return from this
        /// property actually signifies a zero-length string.
        /// </summary>

        public string Text
        {
            get { return _token.Text; }
        }

        /// <summary>
        /// Return the current level of nesting as the reader encounters
        /// nested objects and arrays.
        /// </summary>

        public int Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Indicates whether the reader has reached the end of input source.
        /// </summary>

        public bool EOF
        {
            get { return Token == JsonToken.EOF; }
        }

        /// <summary>
        /// Reads the next token and returns true if one was found.
        /// </summary>

        public bool Read()
        {
            if (!EOF)
            {
                if (Token == JsonToken.EndObject || Token == JsonToken.EndArray)
                    _depth--;

                _token = ReadToken();

                if (Token == JsonToken.Object || Token == JsonToken.Array)
                    _depth++;
            }
            
            return !EOF;
        }

        /// <summary>
        /// Reads the next token and returns it.
        /// </summary>

        protected abstract TokenText ReadToken();

        /// <summary>
        /// Reads the next token ensuring that it matches the specified 
        /// token. If not, an exception is thrown.
        /// </summary>

        public string ReadToken(JsonToken token)
        {
            MoveToContent();
            
            if (Token != token)
                throw new JsonException(string.Format("Found {0} where {1} was expected.", Token.ToString(), token.ToString()));
            
            string s = Text;
            Read();
            return s;
        }

        /// <summary>
        /// Reads the next token, ensures it is a String and returns its 
        /// text. If the next token is not a String, then an exception
        /// is thrown instead.
        /// </summary>

        public string ReadString()
        {
            return ReadToken(JsonToken.String);
        }
        
        public bool ReadBoolean()
        {
            return ReadToken(JsonToken.Boolean) == JsonReader.TrueText;
        }

        public string ReadNumber()
        {
            return ReadToken(JsonToken.Number);
        }

        public int ReadInt32()
        {
            return int.Parse(ReadNumber(), CultureInfo.InvariantCulture);
        }

        public long ReadInt64()
        {
            return long.Parse(ReadNumber(), CultureInfo.InvariantCulture);
        }
    
        public float ReadSingle()
        {
            return float.Parse(ReadNumber(), CultureInfo.InvariantCulture);
        }

        public double ReadDouble()
        {
            return double.Parse(ReadNumber(), CultureInfo.InvariantCulture);
        }

        public decimal ReadDecimal()
        {
            return decimal.Parse(ReadNumber(), CultureInfo.InvariantCulture);
        }

        public void ReadNull()
        {
            ReadToken(JsonToken.Null);
        }

        public string ReadMember()
        {
            return ReadToken(JsonToken.Member);
        }

        public void StepOut()
        {
            int depth = Depth;
            
            if (depth == 0)
                throw new InvalidOperationException();

            while (Depth >= depth)
                Read();
        }

        /// <summary>
        /// Ensures that the reader is positioned on content (a JSON value) 
        /// ready to be read. If the reader is already aligned on the start
        /// of a value then no further action is taken.
        /// </summary>
        /// <returns>Return true if content was found. Otherwise false to 
        /// indicate EOF.</returns>

        public bool MoveToContent()
        {
            JsonToken current = Token;

            if (current == JsonToken.BOF || current == JsonToken.EndArray || current == JsonToken.EndObject)
                return Read();

            return !EOF;
        }
        
        /// <summary>
        /// Deserializes the next object from JSON data.
        /// </summary>

        public object DeserializeNext()
        {
            return DeserializeNext(null);
        }

        /// <summary>
        /// Deserializes the next object from JSON data using a give type 
        /// system.
        /// </summary>
        
        public virtual object DeserializeNext(IParserOutput output)
        {
            if (output == null)
                output = new ParserOutput();

            MoveToContent();

            switch (Token)
            {
                case JsonToken.String: return output.ToStringPrimitive(ReadString());
                case JsonToken.Number: return output.ToNumberPrimitive(ReadNumber());
                case JsonToken.Boolean : return ReadBoolean();
                case JsonToken.Null : ReadNull(); return output.NullPrimitive;

                case JsonToken.Array :
                {
                    Read();
                    output.StartArray();
                    
                    while (Token != JsonToken.EndArray)
                        output.ArrayPut(DeserializeNext(output));
                    
                    Read();
                    return output.EndArray();
                }

                case JsonToken.Object :
                {
                    Read();
                    output.StartObject();

                    while (Token != JsonToken.EndObject)
                    {
                        string name = ReadMember();
                        output.ObjectPut(name, DeserializeNext(output));
                    }
                    
                    Read();
                    return output.EndObject();
                }

                case JsonToken.EOF : throw new JsonException("Unexpected EOF.");
                default : throw new JsonException(string.Format("{0} not expected.", this.Token));
            }
        }

        public IJsonImporterRegistry Importers
        {
            get
            {
                if (_importers == null)
                    _importers = new JsonImporterRegistry();
                
                return _importers;
            }
            
            set
            {
                if (value == null) 
                    throw new ArgumentNullException("value");
                
                _importers = value;
            }
        }

        public object Get(Type type)
        {
            IJsonImporter importer = Importers.Find(type);
            
            if (importer == null)
                throw new JsonException(string.Format("Don't know how to read the type {0} from JSON.", type.FullName)); // TODO: Review the choice of exception type here.
            
            return importer.Import(this);
        }

        public override string ToString()
        {
            return _token.ToString();
        }

        protected struct TokenText
        {
            private readonly JsonToken _token;
            private readonly string _text;

            public TokenText(JsonToken token) : 
                this(token, null) {}

            public TokenText(JsonToken token, string text)
            {
                _token = token;
                _text = Mask.NullString(text);
            }

            public JsonToken Token
            {
                get { return _token; }
            }

            public string Text
            {
                get { return _text; }
            }
        
            public override string ToString()
            {
                if (Token == JsonToken.Member ||
                    Token == JsonToken.String ||
                    Token == JsonToken.Number ||
                    Token == JsonToken.Boolean)
                {
                    return Token.ToString() + " = " + Text;
                }
                else
                {
                    return Token.ToString();
                }
            }
        }
    }
}