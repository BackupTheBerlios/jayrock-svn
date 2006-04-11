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
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only 
    /// access to JSON data over JSON text. 
    /// </summary>

    public sealed class JsonTextReader : JsonReader
    {
        private readonly TextParser _parser;
        private Stack _stack;
        private JsonToken _token;
        private string _text;
        private int _depth;

        private const char NIL = (char) 0;

        private delegate JsonToken Continuation();

        private Continuation _methodParse;
        private Continuation _methodParseArrayFirst;
        private Continuation _methodParseArrayNext;
        private Continuation _methodParseObjectMember;
        private Continuation _methodParseObjectMemberValue;
        private Continuation _methodParseNextMember;

        public JsonTextReader(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            _parser = new TextParser(reader.ReadToEnd());
            _token = JsonToken.BOF;
            _stack = new Stack();
            _stack.Push(ParseMethod);
        }

        /// <summary>
        /// Gets the current token.
        /// </summary>

        public override JsonToken Token
        {
            get { return _token; }
        }

        /// <summary>
        /// Gets the current token text, if applicable. Otherwise returns an 
        /// empty string. Note that for tokens like 
        /// <see cref="JsonToken.String"/>, an empty string return from this
        /// property actually signifies a zero-length string.
        /// </summary>

        public override string Text
        {
            get { return Mask.NullString(_text); }
        }

        /// <summary>
        /// Return the current level of nesting as the reader encounters
        /// nested objects and arrays.
        /// </summary>

        public override int Depth
        {
            get { return _depth; }
        }

        /// <summary>
        /// Reads the next token and returns it.
        /// </summary>

        public override JsonToken ReadToken()
        {
            _text = null;

            if (_stack == null || _stack.Count == 0)
            {
                _stack = null;
                _token = JsonToken.EOF;
            }
            else
            {
                ((Continuation) _stack.Pop())();
            }

            return _token;
        }

        /// <summary>
        /// Parses the next token from the input and returns it.
        /// </summary>

        private JsonToken Parse()
        {
            char ch = NextClean();

            //
            // String
            //

            if (ch == '"' || ch == '\'')
            {
                // TODO: TextParser.NextString throw ParseException when we should throw JsonException!
                return Yield(JsonToken.String, _parser.NextString(ch));
            }

            //
            // Object
            //

            if (ch == '{')
            {
                _parser.Back();
                return ParseObject();
            }

            //
            // JSON Array
            //

            if (ch == '[')
            {
                _parser.Back();
                return ParseArray();
            }

            //
            // Handle unquoted text. This could be the values true, false, or
            // null, or it can be a number. An implementation (such as this one)
            // is allowed to also accept non-standard forms.
            //
            // Accumulate characters until we reach the end of the text or a
            // formatting character.
            // 

            StringBuilder sb = new StringBuilder();
            char b = ch;

            while (ch >= ' ' && ",:]}/\\\"[{;=#".IndexOf(ch) < 0) 
            {
                sb.Append(ch);
                ch = _parser.Next();
            }

            _parser.Back();

            string s = sb.ToString().Trim();

            if (s.Length == 0)
                throw new JsonException("Missing value.");

            if (s == JsonTextReader.TrueText || s == JsonTextReader.FalseText)
                return Yield(JsonToken.Boolean, s);

            if (s == JsonTextReader.NullText)
                return Yield(JsonToken.Null, s);

            if ((b >= '0' && b <= '9') || b == '.' || b == '-' || b == '+')
            {
                double unused;
                if (!double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out unused))
                    throw new JsonException(string.Format("The text '{0}' has the incorrect syntax for a number.", s));

                return Yield(JsonToken.Number, s);
            }

            return Yield(JsonToken.String, s);
        }

        private Continuation ParseMethod
        {
            get
            {
                if (_methodParse == null) _methodParse = new Continuation(Parse);
                return _methodParse;
            }
        }

        /// <summary>
        /// Parses expecting an array in the source.
        /// </summary>

        private JsonToken ParseArray()
        {
            if (NextClean() != '[')
                throw new JsonException("An array must start with '['.");

            return Yield(JsonToken.Array, ParseArrayFirstMethod);
        }

        /// <summary>
        /// Parses the first element of an array or the end of the array if
        /// it is empty.
        /// </summary>

        private JsonToken ParseArrayFirst()
        {
            if (NextClean() == ']')
                return Yield(JsonToken.EndArray);

            _parser.Back();

            _stack.Push(ParseArrayNextMethod);
            return Parse();
        }

        private Continuation ParseArrayFirstMethod
        {
            get
            {
                if (_methodParseArrayFirst == null) _methodParseArrayFirst = new Continuation(ParseArrayFirst);
                return _methodParseArrayFirst;
            }
        }

        /// <summary>
        /// Parses the next element in the array.
        /// </summary>

        private JsonToken ParseArrayNext()
        {
            switch (NextClean())
            {
                case ',':
                {
                    if (NextClean() == ']')
                        return Yield(JsonToken.EndArray);
                    else
                        _parser.Back();

                    break;
                }

                case ']':
                {
                    return Yield(JsonToken.EndArray);
                }

                default:
                    throw new JsonException("Expected a ',' or ']'.");
            }

            _stack.Push(ParseArrayNextMethod);
            return Parse();
        }

        private Continuation ParseArrayNextMethod
        {
            get
            {
                if (_methodParseArrayNext == null) _methodParseArrayNext = new Continuation(ParseArrayNext);
                return _methodParseArrayNext;
            }
        }

        /// <summary>
        /// Parses expecting an object in the source.
        /// </summary>

        private JsonToken ParseObject()
        {
            if (_parser.Next() == '%')
                _parser.Restart(Unescape(_parser.Source), _parser.Index);

            _parser.Back();

            if (NextClean() != '{')
                throw new JsonException("An object must begin with '{'.");

            return Yield(JsonToken.Object, ParseObjectMemberMethod);
        }

        /// <summary>
        /// Parses the first member name of the object or the end of the array
        /// in case of an empty object.
        /// </summary>

        private JsonToken ParseObjectMember()
        {
            char ch = NextClean();

            if (ch == '}')
                return Yield(JsonToken.EndObject);

            if (ch == JsonTextReader.NIL)
                throw new JsonException("An object must end with '}'.");

            _parser.Back();
            Parse();
            // TODO: Check return from NextObject;
            return Yield(JsonToken.Member, Text, ParseObjectMemberValueMethod);
        }

        private Continuation ParseObjectMemberMethod
        {
            get
            {
                if (_methodParseObjectMember == null) _methodParseObjectMember = new Continuation(ParseObjectMember);
                return _methodParseObjectMember;
            }
        }

        private JsonToken ParseObjectMemberValue()
        {
            char ch = NextClean();

            if (ch == '=')
            {
                if (_parser.Next() != '>')
                    _parser.Back();
            }
            else if (ch != ':')
                throw new JsonException("Expected a ':' after a key.");

            _stack.Push(ParseNextMemberMethod);
            return Parse();
        }

        private Continuation ParseObjectMemberValueMethod
        {
            get
            {
                if (_methodParseObjectMemberValue == null) _methodParseObjectMemberValue = new Continuation(ParseObjectMemberValue);
                return _methodParseObjectMemberValue;
            }
        }

        private JsonToken ParseNextMember()
        {
            char ch = NextClean();

            switch (ch)
            {
                case ';':
                case ',':
                    {
                        if ((ch = NextClean()) == '}')
                            return Yield(JsonToken.EndObject);
                        break;
                    }

                case '}':
                    return Yield(JsonToken.EndObject);

                default:
                    throw new JsonException("Expected a ',' or '}'.");
            }

            _parser.Back();
            Parse();
            // TODO: Check return from NextObject;
            return Yield(JsonToken.Member, Text, ParseObjectMemberValueMethod);
        }

        private Continuation ParseNextMemberMethod
        {
            get
            {
                if (_methodParseNextMember == null) _methodParseNextMember = new Continuation(ParseNextMember);
                return _methodParseNextMember;
            }
        }

        /// <summary> 
        /// Yields control back to the reader's user while updating the
        /// reader with the new found token.
        /// </summary>

        private JsonToken Yield(JsonToken token)
        {
            return Yield(token, null, null);
        }

        /// <summary> 
        /// Yields control back to the reader's user while updating the
        /// reader with the new found token and its text.
        /// </summary>

        private JsonToken Yield(JsonToken token, string text)
        {
            return Yield(token, text, null);
        }

        /// <summary> 
        /// Yields control back to the reader's user while updating the
        /// reader with the new found token and the next continuation 
        /// point into the reader.
        /// </summary>

        private JsonToken Yield(JsonToken token, Continuation continuation)
        {
            return Yield(token, null, continuation);
        }

        /// <summary> 
        /// Yields control back to the reader's user while updating the
        /// reader with the new found token, its text and the next 
        /// continuation point into the reader.
        /// </summary>
        /// <remarks>
        /// By itself, this method cannot affect the stack such tha control 
        /// is returned back to the reader's user. This must be done by 
        /// Yield's caller by way of explicit return.
        /// </remarks>

        private JsonToken Yield(JsonToken token, string text, Continuation continuation)
        {
            switch (token)
            {
                case JsonToken.Object:
                case JsonToken.Array:
                    _depth++;
                    break;
                case JsonToken.EndObject:
                case JsonToken.EndArray:
                    _depth--;
                    break;
            }

            _text = text;
            _token = token;

            if (continuation != null)
                _stack.Push(continuation);
            
            return token;
        }
 
        /// <summary>
        /// Get the next char in the string, skipping whitespace
        /// and comments (slashslash and slashstar).
        /// </summary>
        /// <returns>A character, or 0 if there are no more characters.</returns>
        
        private char NextClean()
        {
            Debug.Assert(_parser != null);

            while (true)
            {
                char ch = _parser.Next();

                if (ch == '/')
                {
                    switch (_parser.Next())
                    {
                        case '/':
                        {
                            do
                            {
                                ch = _parser.Next();
                            } while (ch != '\n' && ch != '\r' && ch != NIL);
                            break;
                        }
                        case '*':
                        {
                            while (true)
                            {
                                ch = _parser.Next();

                                if (ch == NIL)
                                    throw new JsonException("Unclosed comment.");

                                if (ch == '*')
                                {
                                    if (_parser.Next() == '/')
                                        break;

                                    _parser.Back();
                                }
                            }
                            break;
                        }
                        default:
                        {
                            _parser.Back();
                            return '/';
                        }
                    }
                }
                else if (ch == '#') 
                {
                    do 
                    {
                        ch = _parser.Next();
                    } 
                    while (ch != '\n' && ch != '\r' && ch != NIL);
                }
                else if (ch == NIL || ch > ' ')
                {
                    return ch;
                }
            }
        }

        /// <summary>
        /// Convert %hh sequences to single characters, and convert plus to
        /// space.
        /// </summary>
        
        private static string Unescape(string s)
        {
            s = Mask.NullString(s);

            int length = s.Length;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                char ch = s[i];

                if (ch == '+')
                {
                    ch = ' ';
                }
                else if (ch == '%' && (i + 2 < length))
                {
                    int lo = ParseHexChar(s[i + 1]);
                    int hi = ParseHexChar(s[i + 2]);

                    if (lo >= 0 && hi >= 0)
                    {
                        ch = (char) (lo*16 + hi);
                        i += 2;
                    }
                }

                sb.Append(ch);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get the hex value of a character (base16).
        /// </summary>
        /// <returns>
        /// An integer between 0 and 15, or -1 if ch was not a hex digit.
        /// </returns>
        
        private static int ParseHexChar(char ch)
        {
            if (ch >= '0' && ch <= '9')
                return ch - '0';

            if (ch >= 'A' && ch <= 'F')
                return ch + 10 - 'A';

            if (ch >= 'a' && ch <= 'f')
                return ch + 10 - 'a';

            return -1;
        }
    }
}