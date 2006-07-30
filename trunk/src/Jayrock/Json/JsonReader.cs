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
        public const string TrueText = "true";
        public const string FalseText = "false";
        public const string NullText = "null";

        public abstract JsonToken Token { get; }
        public abstract string Text { get; }
        public abstract int Depth { get; }

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
            return ReadToken() != JsonToken.EOF;
        }

        /// <summary>
        /// Reads the next token and returns it.
        /// </summary>

        public abstract JsonToken ReadToken();

        /// <summary>
        /// Reads the next token ensuring that it matches the specified 
        /// token. If not, an exception is thrown.
        /// </summary>

        public void ReadToken(JsonToken token)
        {
            if (ReadToken() != token)
                throw new JsonException(string.Format("Found {0} where {1} was expected.", Token.ToString(), token));
        }

        /// <summary>
        /// Reads the next token, ensures it is a String and returns its 
        /// text. If the next token is not a String, then an exception
        /// is thrown instead.
        /// </summary>

        public string ReadString()
        {
            ReadToken(JsonToken.String);
            return Text;
        }
        
        public bool ReadBoolean()
        {
            ReadToken(JsonToken.Boolean);
            return Text == JsonReader.FalseText;
        }

        public string ReadNumber()
        {
            ReadToken(JsonToken.Number);
            return Text;
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
            ReadToken(JsonToken.Member);
            return Text;
        }

        public void SkipTo(JsonToken token)
        {
            // BUGBUG: Depth check missing bug!
            // This loop would exit prematurely if it find the sought token at
            // a depth lower than where it started, such as in the case of
            // nested structures.
            
            while (Read())
            {
                if (Token == token)
                    return;
            }

            throw new JsonException(string.Format("Found EOF while attempting to skip to {0}.", token.ToString()));
        }

        public void SkipObject()
        {
            SkipTo(JsonToken.EndObject);
        }

        public void SkipArray()
        {
            SkipTo(JsonToken.EndArray);
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
                case JsonToken.String: return output.ToStringPrimitive(Text);
                case JsonToken.Number: return output.ToNumberPrimitive(Text);
                case JsonToken.Boolean : return Text == JsonReader.FalseText ? output.FalsePrimitive : output.TruePrimitive;
                case JsonToken.Null : return output.NullPrimitive;

                case JsonToken.Array :
                {
                    output.StartArray();

                    while (ReadToken() != JsonToken.EndArray)
                        output.ArrayPut(DeserializeNext(output));
         
                    return output.EndArray();
                }

                case JsonToken.Object :
                {
                    output.StartObject();

                    while (ReadToken() != JsonToken.EndObject)
                    {
                        if (Token != JsonToken.Member)
                            throw new JsonException("Expecting member.");

                        string name = Text;

                        Read();
                        output.ObjectPut(name, DeserializeNext(output));
                    }
         
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
            return Token + ":" + Text;
        }
    }
}