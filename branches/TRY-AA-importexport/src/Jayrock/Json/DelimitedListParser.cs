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
    using System.Collections;

    #endregion

    public sealed class DelimitedListParser
    {
        private IParserOutput _output;
        
        public DelimitedListParser() : this(null) {}

        public DelimitedListParser(IParserOutput output)
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

        public object ParseRowToArray(string source)
        {
            return ParseRowToArray(new TextParser(source));
        }

        public object ParseRowToObject(string[] names, string source)
        {
            return ParseRowToObject(new TextParser(source), names);
        }

        private IParserOutput Output
        {
            get
            {
                //
                // Fault-in an AST strategy if one wasn't supplied initially.
                //

                if (_output == null)
                    _output = new ParserOutput();

                return _output;
            }
        }

        private object Parse(TextParser parser)
        {
            Debug.Assert(parser != null);

            //
            // Get the names from the first row. If no names are found then
            // bail out with a null.
            //

            ICollection nameCollection = (ICollection) ParseRowToArray(parser);

            if (nameCollection == null)
                return null;

            //
            // Turn the values in the names collection into an array of
            // string. Note, we need to handle this quite generically since
            // we cannot assume what objects were returned by the 
            // IParserOutput implementation.
            //

            int i = 0;
            string[] names = new string[nameCollection.Count];

            foreach (object nameValue in nameCollection)
                names[i++] = nameValue.ToString();

            //
            // Finally, build an array of objects, where each object comes
            // from a row, and then return the resulting array.
            //

            IParserOutput output = Output;
            output.StartArray();

            object item = ParseRowToObject(parser, names);

            while (item != null)
            {
                output.ArrayPut(item);
                item = ParseRowToObject(parser, names);
            }

            return output.EndArray();
        }

        private object ParseRowToArray(TextParser parser) 
        {
            Debug.Assert(parser != null);

            string value = GetValue(parser);

            if (value == null)
                return null;

            IParserOutput output = Output;
            output.StartArray();

            do
            {
                output.ArrayPut(value);

                while (true)
                {
                    char ch = parser.Next();

                    if (ch == ',') 
                    {
                        break;
                    }
                    else if (ch != ' ') 
                    {
                        if (ch == '\n' || ch == '\r' || ch == TextParser.EOF) 
                            return output.EndArray();

                        throw new ParseException("Bad character '" + ch.ToString() + "' (" + ((int) ch).ToString(CultureInfo.InvariantCulture) + ").");
                    }
                }

                value = GetValue(parser);
            }
            while (value != null);

            return output.EndArray();
        }

        /// <summary>
        /// Parses the values from a row into an object given a set of names
        /// to use for the members.
        /// </summary>

        private object ParseRowToObject(TextParser parser, string[] names)
        {
            Debug.Assert(parser != null);

            if (names == null)
                return null;

            ICollection collection = (ICollection) ParseRowToArray(parser);
            
            if (collection == null)
                return null;
            
            object[] values = new object[collection.Count];
            collection.CopyTo(values, 0);

            IParserOutput output = Output;
            output.StartObject();

            for (int i = 0; i < Math.Min(names.Length, values.Length); i++)
                output.ObjectPut(names[i], values[i]);

            return output.EndObject();
        }

        /// <summary>
        /// Get the next value. The value can be wrapped in quotes. 
        /// The value can be empty.
        /// </summary>

        private string GetValue(TextParser parser) 
        {
            char ch;
            
            do 
            {
                ch = parser.Next();
            } 
            while (ch <= ' ' && ch != TextParser.EOF);

            switch (ch) 
            {
                case TextParser.EOF:
                    return null;
                case '"':
                case '\'':
                    return parser.NextString(ch);
                case ',':
                    parser.Back();
                    return "";
                default:
                    parser.Back();
                    return parser.NextTo(',');
            }
        }
    }
}
