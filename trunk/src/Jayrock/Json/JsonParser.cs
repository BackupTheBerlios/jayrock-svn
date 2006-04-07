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
    /// Parses and deserializes JSON data into an object graph.
    /// </summary>

    [ Obsolete("Use JsonReader.DeserializeNext instead.") ]
    public sealed class JsonParser
    {
        private IParserOutput _output;

        public JsonParser() : this(null) {}

        public JsonParser(IParserOutput output)
        {
            _output = output;
        }

        public object Parse(string s)
        {
            return Parse(new StringReader(s));
        }

        public object Parse(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            JsonReader jsonReader = new JsonTextReader(reader);
            return Parse(jsonReader);
        }

        public object Parse(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            //
            // Fault-in an AST strategy if one wasn't supplied thus far.
            //

            if (_output == null)
                _output = new ParserOutput();

            return reader.DeserializeNext(_output);
        }
    }
}
