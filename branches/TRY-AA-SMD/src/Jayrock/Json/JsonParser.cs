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
    using System.Diagnostics;
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
        private IParserOutput _output;
        
        private const char EOF = (char) 0;

        public JsonParser() : this(null) {}

        public JsonParser(IParserOutput output)
        {
            _output = output;
        }

        public object Parse(TextReader sourceReader)
        {
            if (sourceReader == null)
                throw new ArgumentNullException("sourceReader");

            return Parse(new TextParser(sourceReader.ReadToEnd()));
        }

        public object Parse(string source)
        {
            return Parse(new TextParser(source));
        }

        private object Parse(TextParser parser)
        {
            Debug.Assert(parser != null);

            //
            // Fault-in an AST strategy if one wasn't supplied thus far.
            //

            if (_output == null)
                _output = new ParserOutput();

            //
            // Parse and return the next object found.
            //

            return NextObject(parser);
        }

        /// <summary>
        /// Get the next char in the string, skipping whitespace
        /// and comments (slashslash and slashstar).
        /// </summary>
        /// <returns>A character, or 0 if there are no more characters.</returns>
        
        private char NextClean(TextParser parser)
        {
            Debug.Assert(parser != null);

            while (true) 
            {
                char ch = parser.Next();

                if (ch == '/') 
                {
                    switch (parser.Next()) 
                    {
                        case '/' :
                        {
                            do 
                            {
                                ch = parser.Next();
                            } 
                            while (ch != '\n' && ch != '\r' && ch != EOF);
                            break;
                        }
                        case '*' :
                        {
                            while (true) 
                            {
                                ch = parser.Next();

                                if (ch == EOF) 
                                    throw new ParseException("Unclosed comment.");

                                if (ch == '*') 
                                {
                                    if (parser.Next() == '/') 
                                        break;
                                    
                                    parser.Back();
                                }
                            }
                            break;
                        }
                        default :
                        {
                            parser.Back();
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
        /// Gets the next value as object. The value can be a Boolean, a Double,
        /// an Integer, an object array, a JObject, a String, or 
        /// JObject.NULL.
        /// </summary>
        
        private object NextObject(TextParser parser)
        {
            Debug.Assert(parser != null);

            char ch = NextClean(parser);

            //
            // String
            //

            if (ch == '"' || ch == '\'') 
                return _output.ToStringPrimitive(parser.NextString(ch));

            //
            // Object
            //

            if (ch == '{') 
            {
                parser.Back();
                return ParseObject(parser);
            }

            //
            // JSON Array
            //

            if (ch == '[')
            {
                parser.Back();
                return ParseArray(parser);
            }

            StringBuilder sb = new StringBuilder();
            char b = ch;
            
            while (ch >= ' ' && ch != ':' && ch != ',' && ch != ']' && ch != '}' && ch != '/')
            {
                sb.Append(ch);
                ch = parser.Next();
            }

            parser.Back();

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

        private object ParseArray(TextParser parser)
        {
            Debug.Assert(parser != null);

            if (NextClean(parser) != '[') 
                throw new ParseException("An array must start with '['.");

            _output.StartArray();
            
            if (NextClean(parser) != ']') 
            {
                parser.Back();

                bool end = false;

                do
                {
                    _output.ArrayPut(NextObject(parser));

                    switch (NextClean(parser)) 
                    {
                        case ',' :
                        {
                            if (NextClean(parser) == ']') 
                                end = true;
                            else
                                parser.Back();

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

        private object ParseObject(TextParser parser)
        {
            Debug.Assert(parser != null);

            if (parser.Next() == '%')
                parser.Restart(Unescape(parser.Source), parser.Index);

            parser.Back();

            if (NextClean(parser) != '{') 
                throw new ParseException("An object must begin with '{'.");

            _output.StartObject();

            char ch = NextClean(parser);

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
                        parser.Back();
                        memberName = NextObject(parser).ToString();
                        break;
                    }
                }

                if (NextClean(parser) != ':') 
                    throw new ParseException("Expected a ':' after a key.");

                object memberValue = NextObject(parser);

                _output.ObjectPut(memberName, memberValue);

                switch (ch = NextClean(parser))
                {
                    case ',' :
                    {
                        if ((ch = NextClean(parser)) == '}') 
                            continue;

                        parser.Back();
                        break;
                    }
                    
                    case '}' :
                        continue;

                    default :
                        throw new ParseException("Expected a ',' or '}'.");
                }

                ch = NextClean(parser);
            }

            return _output.EndObject();
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
