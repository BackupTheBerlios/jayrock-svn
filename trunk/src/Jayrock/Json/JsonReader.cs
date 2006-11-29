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

    #endregion

    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only 
    /// access to JSON data. 
    /// </summary>

    public abstract class JsonReader
    {
        /// <summary>
        /// Reads the next token and returns true if one was found.
        /// </summary>

        public abstract bool Read();

        /// <summary>
        /// Gets the current token.
        /// </summary>

        public abstract JsonToken Token { get; }

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

        public abstract int Depth { get; }
        
        /// <summary>
        /// Closes the reader and releases any underlying resources 
        /// associated with the reader.
        /// </summary>
        
        public virtual void Close() {}

        /// <summary>
        /// Returns a <see cref="String"/> that represents the state of the 
        /// instance.
        /// </summary>

        public override string ToString()
        {
            return Token.ToString();
        }
        
        /// <summary>
        /// Indicates whether the reader has reached the end of input source.
        /// </summary>

        public bool EOF
        {
            get { return TokenClass == JsonTokenClass.EOF; }
        }

        /// <summary>
        /// Reads the next token ensuring that it matches the specified 
        /// token. If not, an exception is thrown.
        /// </summary>

        public string ReadToken(JsonTokenClass token)
        {
            MoveToContent();
            
            if (TokenClass != token)
                throw new JsonException(string.Format("Found {0} where {1} was expected.", TokenClass, token));
            
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

        /// <summary>
        /// Reads the next token, ensures it is a Boolean and returns its 
        /// value. If the next token is not a Boolean, then an exception 
        /// is thrown instead.
        /// </summary>

        public bool ReadBoolean()
        {
            return ReadToken(JsonTokenClass.Boolean) == JsonBoolean.TrueText;
        }

        /// <summary>
        /// Reads the next token, ensures it is a Number and returns its 
        /// text representation. If the next token is not a Number, then 
        /// an exception is thrown instead.
        /// </summary>

        public JsonNumber ReadNumber()
        {
            return new JsonNumber(ReadToken(JsonTokenClass.Number));
        }

        /// <summary>
        /// Reads the next token, ensures it is a Null. If the next token 
        /// is not a Null, then an exception is thrown instead.
        /// </summary>
        
        public void ReadNull()
        {
            ReadToken(JsonTokenClass.Null);
        }

        /// <summary>
        /// Reads the next token, ensures it is Member (of an object) and 
        /// returns its text. If the next token is not a Member, then an 
        /// exception is thrown instead.
        /// </summary>

        public string ReadMember()
        {
            return ReadToken(JsonTokenClass.Member);
        }
        
        /// <summary>
        /// Steps out of the current depth to the level immediately above. 
        /// Usually this skips the current Object or Array being read, 
        /// including all nested structures.
        /// </summary>
        
        public void StepOut()
        {
            int depth = Depth;
            
            if (depth == 0)
                throw new InvalidOperationException();

            while (Depth > depth || (TokenClass != JsonTokenClass.EndObject && TokenClass != JsonTokenClass.EndArray))
                Read();
            
            Read(/* past tail */);
        }

        /// <summary>
        /// Skips through the next item. If it is an Object or Array, then
        /// the entire object or array is skipped. If it is a scalar value
        /// then just that value is skipped. If the reader is on an object
        /// member then the member and its value is skipped.
        /// </summary>

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
    }
}
