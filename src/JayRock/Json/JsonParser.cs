#region License, Terms and Conditions
//
// JayRock - A JSON-RPC implementation for the Microsoft .NET Framework
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

namespace JayRock.Json
{
    #region Imports

    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// A JsonParser takes a source string and extracts characters and tokens
    /// from it. It is used by the JObject and JArray constructors to parse JSON
    /// source strings.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Public Domain 2002 JSON.org, ported to C# by Are Bjolseth (teleplan.no)
    /// and re-adapted by Atif Aziz (www.raboof.com)</para>
    /// </remarks>

    public sealed class JsonParser
    {
        private int _index;
        private string _source;
        private IParserOutput _output;
        
        private const char EOF = (char) 0;

        public JsonParser() : this(null) {}

        public JsonParser(IParserOutput output)
        {
            _index = 0;
            _source = string.Empty;
            _output = output;
        }

        public object Parse(TextReader sourceReader)
        {
            if (sourceReader == null)
                throw new ArgumentNullException("sourceReader");

            return Parse(sourceReader.ReadToEnd());
        }

        public object Parse(string source)
        {
            _index = 0;
            _source = Mask.NullString(source);
            return Parse();
        }

        private object Parse()
        {
            //
            // Fault-in an AST strategy if one wasn't supplied thus far.
            //

            if (_output == null)
                _output = new ParserOutput();

            //
            // Parse and return the next object found.
            //

            return NextObject();
        }

        /// <summary>
        /// Back up one character. This provides a sort of lookahead capability,
        /// so that you can test for a digit or letter before attempting to
        /// parse the next number or identifier.
        /// </summary>
        
        private void Back()
        {
            if (_index > 0)
                _index -= 1;
        }

        /// <summary>
        /// Determine if the source string still contains characters that Next()
        /// can consume.
        /// </summary>
        /// <returns>true if not yet at the end of the source.</returns>
        
        private bool More()
        {
            return _index < _source.Length;
        }

        /// <summary>
        /// Get the next character in the source string.
        /// </summary>
        /// <returns>The next character, or 0 if past the end of the source string.</returns>
        
        private char Next()
        {
            if (!More())
                return EOF;

            return _source[_index++];
        }

        /// <summary>
        /// Get the next count characters.
        /// </summary>
        /// <param name="count">The number of characters to take.</param>
        /// <returns>A string of count characters.</returns>
        
        private string Next(int count)
        {
            int start = _index;
            int end = start + count;
            
            if (end >= _source.Length)
                throw new ArgumentOutOfRangeException("count");

            _index += count;
            return _source.Substring(start, count);
        }

        /// <summary>
        /// Get the next char in the string, skipping whitespace
        /// and comments (slashslash and slashstar).
        /// </summary>
        /// <returns>A character, or 0 if there are no more characters.</returns>
        
        private char NextClean()
        {
            while (true) 
            {
                char ch = Next();

                if (ch == '/') 
                {
                    switch (Next()) 
                    {
                        case '/' :
                        {
                            do 
                            {
                                ch = Next();
                            } 
                            while (ch != '\n' && ch != '\r' && ch != EOF);
                            break;
                        }
                        case '*' :
                        {
                            while (true) 
                            {
                                ch = Next();

                                if (ch == EOF) 
                                    throw new ParseException("Unclosed comment.");

                                if (ch == '*') 
                                {
                                    if (Next() == '/') 
                                        break;
                                    
                                    Back();
                                }
                            }
                            break;
                        }
                        default :
                        {
                            Back();
                            return '/';
                        }
                    }
                } 
                else if (ch == EOF || ch > ' ') 
                {
                    return ch;
                }
            }
        }

        /// <summary>
        /// Return the characters up to the next close quote character.
        /// Backslash processing is done. The formal JSON format does not
        /// allow strings in single quotes, but an implementation is allowed to
        /// accept them.
        /// </summary>
        /// <param name="quote">The quoting character, either " or '</param>
        /// <returns>A String.</returns>
        
        private string NextString(char quote)
        {
            char ch;
            StringBuilder sb = new StringBuilder();
            
            while (true)
            {
                ch = Next();

                if ((ch == EOF) || (ch == '\n') || (ch == '\r')) 
                    throw new ParseException("Unterminated string.");

                if (ch == '\\')
                {
                    ch = Next();

                    switch (ch)
                    {
                        case 'b': // Backspace
                            sb.Append('\b');
                            break;
                        case 't': // Horizontal tab
                            sb.Append('\t');
                            break;
                        case 'n':  // Newline
                            sb.Append('\n');
                            break;
                        case 'f':  // Form feed
                            sb.Append('\f');
                            break;
                        case 'r':  // Carriage return
                            sb.Append('\r');
                            break;
                        case 'u':
                            // TODO: Review
                            //sb.append((char)Integer.parseInt(next(4), 16)); // 16 == radix, ie. hex
                            int iascii = int.Parse(Next(4),NumberStyles.HexNumber);
                            sb.Append((char)iascii);
                            break;
                        default:
                            sb.Append(ch);
                            break;
                    }
                }
                else
                {
                    if (ch == quote)
                        return sb.ToString();

                    sb.Append(ch);
                }
            }
        }

        /// <summary>
        /// Gets the next value as object. The value can be a Boolean, a Double,
        /// an Integer, an object array, a JObject, a String, or 
        /// JObject.NULL.
        /// </summary>
        
        private object NextObject()
        {
            char ch = NextClean();

            //
            // String
            //

            if (ch == '"' || ch == '\'') 
                return _output.ToStringPrimitive(NextString(ch));

            //
            // Object
            //

            if (ch == '{') 
            {
                Back();
                return ParseObject();
            }

            //
            // JSON Array
            //

            if (ch == '[')
            {
                Back();
                return ParseArray();
            }

            StringBuilder sb = new StringBuilder();
            char b = ch;
            
            while (ch >= ' ' && ch != ':' && ch != ',' && ch != ']' && ch != '}' && ch != '/')
            {
                sb.Append(ch);
                ch = Next();
            }

            Back();

            string s = sb.ToString().Trim();
            
            if (s == "true")
                return _output.TruePrimitive;
            
            if (s == "false")
                return _output.FalsePrimitive;

            if (s == "null")
                return _output.NullPrimitive;
            
            if ((b >= '0' && b <= '9') || b == '.' || b == '-' || b == '+') 
            {
                object number = _output.ToNumberPrimitive(s);

                if (number == null)
                    throw new ParseException(string.Format("Cannot convert '{0}' to a number.", s));

                return number;
            }

            if (s.Length == 0)
                throw new ParseException("Missing value.");

            return s;
        }

        /// <summary>
        /// Returns a printable string of this object.
        /// </summary>
        
        public override string ToString()
        {
            return " at charachter " + _index + " of " + _source;
        }

        private object ParseArray()
        {
            if (NextClean() != '[') 
                throw new ParseException("An array must start with '['.");

            _output.StartArray();
            
            if (NextClean() != ']') 
            {
                Back();

                bool end = false;

                do
                {
                    _output.ArrayPut(NextObject());

                    switch (NextClean()) 
                    {
                        case ',' :
                        {
                            if (NextClean() == ']') 
                                end = true;
                            else
                                Back();

                            break;
                        }

                        case ']' :
                        {
                            end = true; 
                            break;
                        }
    					
                        default :
                            throw new ParseException("Expected a ',' or ']'.");
                    }
                }
                while (!end);
            }

            return _output.EndArray();
        }

        private object ParseObject()
        {
            if (Next() == '%') 
                Unescape();

            Back();

            if (NextClean() != '{') 
                throw new ParseException("An object must begin with '{'.");

            _output.StartObject();

            char ch = NextClean();

            while (ch != '}')
            {
                string memberName;

                switch (ch) 
                {
                    case JsonParser.EOF :
                        throw new ParseException("An object must end with '}'.");
                    
                    case '}' :
                        continue;
                    
                    default :
                    {
                        Back();
                        memberName = NextObject().ToString();
                        break;
                    }
                }

                if (NextClean() != ':') 
                    throw new ParseException("Expected a ':' after a key.");

                object memberValue = NextObject();

                _output.ObjectPut(memberName, memberValue);

                switch (ch = NextClean())
                {
                    case ',' :
                    {
                        if ((ch = NextClean()) == '}') 
                            continue;

                        Back();
                        break;
                    }
                    
                    case '}' :
                        continue;

                    default :
                        throw new ParseException("Expected a ',' or '}'.");
                }

                ch = NextClean();
            }

            return _output.EndObject();
        }

        /// <summary>
        /// Unescape the source text. Convert %hh sequences to single characters,
        /// and convert plus to space. There are Web transport systems that insist on
        /// doing unnecessary URL encoding. This provides a way to undo it.
        /// </summary>
        
        private void Unescape()
        {
            _source = Unescape(_source);
        }

        /// <summary>
        /// Convert %hh sequences to single characters, and convert plus to
        /// space.
        /// </summary>
        
        private static string Unescape(string s)
        {
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
                        ch = (char) (lo * 16 + hi);
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
