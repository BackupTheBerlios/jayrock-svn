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

    public sealed class JsonMLDecoder
    {
        public void Decode(JsonReader reader, XmlWriter writer)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (writer == null) throw new ArgumentNullException("writer");

            reader.ReadToken(JsonTokenClass.Array);

            if (reader.TokenClass == JsonTokenClass.EndArray)
            {
                if (writer.WriteState != WriteState.Element && writer.WriteState != WriteState.Content)
                    throw new JsonMLException("Missing root element.");
            }
            else
            {
                writer.WriteStartElement(reader.ReadString());

                //
                // If there is a second element that is a JSON object then
                // it represents the element attributes.
                //

                if (reader.TokenClass == JsonTokenClass.Object)
                {
                    reader.Read();

                    while (reader.TokenClass != JsonTokenClass.EndObject)
                    {
                        string name = reader.ReadMember();

                        if (reader.TokenClass == JsonTokenClass.Object ||
                            reader.TokenClass == JsonTokenClass.Array)
                        {
                            throw new JsonMLException(
                                "Attribute value cannot be structural, such as a JSON object or array.");
                        }

                        writer.WriteAttributeString(name,
                            reader.TokenClass == JsonTokenClass.Null ? 
                                string.Empty : reader.Text);

                        reader.Read();
                    }

                    reader.Read();
                }

                //
                // Process any remaining elements as child elements
                // and text content.
                //

                while (reader.TokenClass != JsonTokenClass.EndArray)
                {
                    if (reader.TokenClass == JsonTokenClass.Object)
                    {
                        throw new JsonMLException(
                            "Found JSON object when expecting " + 
                            "either a JSON array representing a child element " + 
                            "or a JSON string representing text content.");
                    }
                    else if (reader.TokenClass == JsonTokenClass.Array)
                    {   
                        Decode(reader, writer);
                    }
                    else if (reader.TokenClass == JsonTokenClass.Null)
                    {
                        reader.Read();
                    }
                    else
                    {
                        writer.WriteString(reader.Text);
                        reader.Read();
                    }
                }

                writer.WriteEndElement();
            }

            reader.Read();
        }
    }
}