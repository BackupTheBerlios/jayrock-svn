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
    using Jayrock.Json.Serialization.Import.Importers;
    using Jayrock.Json.Serialization.Import;

    #endregion
    
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only 
    /// access to JSON data. 
    /// </summary>

    public abstract class JsonReader
    {
        private JsonToken _token;
        private int _depth;

        public JsonReader()
        {
            _token = JsonToken.BOF();
        }

        /// <summary>
        /// Gets the current token.
        /// </summary>

        public JsonToken Token
        {
            get { return _token; }
        }
        
        /// <summary>
        /// Gets the class of the current token.
        /// </summary>

        public JsonTokenClass TokenClass
        {
            get { return Token.Class; }
        }
        
        /// <summary>
        /// Gets the text of the current token.
        /// </summary>

        public string Text
        {
            get { return Token.Text; }
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
            get { return TokenClass == JsonTokenClass.EOF; }
        }

        /// <summary>
        /// Reads the next token and returns true if one was found.
        /// </summary>

        public bool Read()
        {
            if (!EOF)
            {
                if (TokenClass == JsonTokenClass.EndObject || TokenClass == JsonTokenClass.EndArray)
                    _depth--;

                _token = ReadTokenImpl();

                if (TokenClass == JsonTokenClass.Object || TokenClass == JsonTokenClass.Array)
                    _depth++;
            }
            
            return !EOF;
        }

        /// <summary>
        /// Reads the next token and returns it.
        /// </summary>
        
        protected abstract JsonToken ReadTokenImpl();

        /// <summary>
        /// Reads the next token ensuring that it matches the specified 
        /// token. If not, an exception is thrown.
        /// </summary>

        public string ReadToken(JsonTokenClass token)
        {
            MoveToContent();
            
            if (TokenClass != token)
                throw new JsonException(string.Format("Found {0} where {1} was expected.", TokenClass.ToString(), token.ToString()));
            
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
            return ReadToken(JsonTokenClass.String);
        }
        
        public bool ReadBoolean()
        {
            return ReadToken(JsonTokenClass.Boolean) == JsonBoolean.TrueText;
        }

        public JsonNumber ReadNumber()
        {
            return new JsonNumber(ReadToken(JsonTokenClass.Number));
        }

        public void ReadNull()
        {
            ReadToken(JsonTokenClass.Null);
        }

        public string ReadMember()
        {
            return ReadToken(JsonTokenClass.Member);
        }

        public void StepOut()
        {
            int depth = Depth;
            
            if (depth == 0)
                throw new InvalidOperationException();

            while (Depth > depth || (TokenClass != JsonTokenClass.EndObject && TokenClass != JsonTokenClass.EndArray))
                Read();
            
            Read(/* past tail */);
        }
        
        public void Skip()
        {
            if (!MoveToContent())
                return;
            
            if (TokenClass == JsonTokenClass.Object || TokenClass == JsonTokenClass.Array)
            {
                StepOut();
            }
            else if (TokenClass == JsonTokenClass.Member)
            {
                Read(/* member */);
                Skip(/* value */);
            }
            else
            {
                Read(/* scalar */);
            }
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
            JsonTokenClass current = TokenClass;

            if (current == JsonTokenClass.BOF || current == JsonTokenClass.EndArray || current == JsonTokenClass.EndObject)
                return Read();

            return !EOF;
        }
        
        /// <summary>
        /// Deserializes the next object from JSON data.
        /// </summary>

        [ Obsolete("Use JsonReader.ReadValue() instead.") ]
        public object DeserializeNext()
        {
            return DeserializeNext(null);
        }

        /// <summary>
        /// Deserializes the next object from JSON data using a give type 
        /// system.
        /// </summary>
        
        [ Obsolete("Use JsonReader.ReadValue() instead for similar functionality.") ]
        public virtual object DeserializeNext(IParserOutput output)
        {
            if (output == null)
                output = new ParserOutput();

            MoveToContent();

            if (TokenClass == JsonTokenClass.String)
            {
                return output.ToStringPrimitive(ReadString());
            }
            else if (TokenClass == JsonTokenClass.Number)
            {
                return output.ToNumberPrimitive(ReadNumber().ToString());
            }
            else if (TokenClass == JsonTokenClass.Boolean)
            {
                return ReadBoolean();
            }
            else if (TokenClass == JsonTokenClass.Null)
            {
                ReadNull(); 
                return output.NullPrimitive;
            }
            else if (TokenClass == JsonTokenClass.Array)
            {
                Read();
                output.StartArray();
                
                while (TokenClass != JsonTokenClass.EndArray)
                    output.ArrayPut(DeserializeNext(output));
                
                Read();
                return output.EndArray();
            }
            else if (TokenClass == JsonTokenClass.Object)
            {
                Read();
                output.StartObject();

                while (TokenClass != JsonTokenClass.EndObject)
                {
                    string name = ReadMember();
                    output.ObjectPut(name, DeserializeNext(output));
                }
                
                Read();
                return output.EndObject();
            }
            else 
            {
                throw new JsonException(string.Format("{0} not expected.", TokenClass));
            }
        }

        public object ReadValue()
        {
            return ReadValue(null);
        }

        public virtual object ReadValue(Type type)
        {
            return JsonImport.Import(this, type);
        }

        public override string ToString()
        {
            return _token.ToString();
        }
    }
}