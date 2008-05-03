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

namespace Jayrock.JsonML
{
    #region Imports

    using System;
    using System.Xml;
    using Json;

    #endregion

    //
    // The following BNF grammar represents how XML-based markup (e.g. XHTML) 
    // is encoded into JsonML:
    //
    //  element 
    //      = '[' tag-name ',' attributes ',' element-list ']' 
    //      | '[' tag-name ',' attributes ']' 
    //      | '[' tag-name ',' element-list ']' 
    //      | '[' tag-name ']' 
    //      | json-string 
    //      ; 
    //  tag-name 
    //      = json-string 
    //      ; 
    //  attributes 
    //      = '{' attribute-list '}' 
    //      | '{' '}' 
    //      ; 
    //  attribute-list 
    //      = attribute ',' attribute-list 
    //      | attribute 
    //      ; 
    //  attribute 
    //      = attribute-name ':' attribute-value 
    //      ; 
    //  attribute-name 
    //      = json-string 
    //      ; 
    //  attribute-value 
    //      = json-string 
    //      ; 
    //  element-list 
    //      = element ',' element-list 
    //      | element 
    //      ; 
    //

    public sealed class JsonMLEncoder
    {
        public void Encode(XmlReader reader, JsonWriter writer)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (writer == null) throw new ArgumentNullException("writer");

            if (reader.MoveToContent() != XmlNodeType.Element)
                throw new ArgumentException(null, "reader");

            writer.WriteStartArray();
            writer.WriteString(reader.Name);

            //
            // Write attributes
            //

            if (reader.MoveToFirstAttribute())
            {
                writer.WriteStartObject();

                do
                {
                    writer.WriteMember(reader.Name);
                    writer.WriteString(reader.Value);
                } 
                while (reader.MoveToNextAttribute());

                writer.WriteEndObject();
                reader.MoveToElement();
            }

            bool isEmpty = reader.IsEmptyElement;

            if (!isEmpty)
            {
                reader.Read();

                //
                // Write child nodes (text, CDATA and element)
                //

                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
                    {
                        writer.WriteString(reader.Value);
                        reader.Read();
                    }
                    else if (reader.NodeType == XmlNodeType.Element)
                    {
                        Encode(reader, writer);
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }

            writer.WriteEndArray();
            reader.Read();
        }
    }
}
