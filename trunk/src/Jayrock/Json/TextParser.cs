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
    using System.Text;

    #endregion

    internal sealed class TextParser
    {
        private int _index;
        private string _source;
        
        public const char EOF = (char) 0;

        public TextParser() : this(null) {}

        public TextParser(string source)
        {
            Restart(source);
        }

        public int Index
        {
            get { return _index; }
        }

        public string Source
        {
            get { return _source; }
        }

        public void Restart(string source)
        {
            Restart(source, 0);
        }

        public void Restart(string source, int index)
        {
            _source = Mask.NullString(source);

            Debug.Assert(index >= 0);
            Debug.Assert(index <= _source.Length);

            _index = index;
        }

        /// <summary>
        /// Get the text up but not including the specified character or the
        /// end of line, whichever comes first.
        /// </summary>

        public string NextTo(char delimiter) 
        {
            StringBuilder sb = new StringBuilder();
            
            while (true)
            {
                char ch = Next();
                
                if (ch == delimiter || ch == EOF || ch == '\n' || ch == '\r') 
                {
                    if (ch != 0) 
                        Back();
                    
                    return sb.ToString().Trim();
                }
                
                sb.Append(ch);
            }
        }

        /// <summary>
        /// Back up one character. This provides a sort of lookahead capability,
        /// so that you can test for a digit or letter before attempting to
        /// parse the next number or identifier.
        /// </summary>
        
        public void Back()
        {
            if (_index > 0)
                _index -= 1;
        }

        /// <summary>
        /// Determine if the source string still contains characters that Next()
        /// can consume.
        /// </summary>
        /// <returns>true if not yet at the end of the source.</returns>
        
        public bool More()
        {
            return _index < _source.Length;
        }

        /// <summary>
        /// Get the next character in the source string.
        /// </summary>
        /// <returns>The next character, or 0 if past the end of the source string.</returns>
        
        public char Next()
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
        
        public string Next(int count)
        {
            int start = _index;
            int end = start + count;

            Debug.Assert(end < _source.Length);

            _index += count;
            return _source.Substring(start, count);
        }

        /// <summary>
        /// Return the characters up to the next close quote character.
        /// Backslash processing is done. The formal JSON format does not
        /// allow strings in single quotes, but an implementation is allowed to
        /// accept them.
        /// </summary>
        /// <param name="quote">The quoting character, either " or '</param>
        /// <returns>A String.</returns>
        
        public string NextString(char quote)
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
        /// Returns a printable string of this object.
        /// </summary>
        
        public override string ToString()
        {
            return " at charachter " + _index + " of " + _source;
        }
    }
}
