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

namespace Jayrock.Json.Conversion.Import.Importers
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    public sealed class AutoImporter : JsonImporterBase
    {
        public AutoImporter() : 
            base(typeof(object)) {}

        public override object Import(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            reader.MoveToContent();
            
            if (reader.TokenClass == JsonTokenClass.String)
            {
                return reader.ReadString();
            }
            else if (reader.TokenClass == JsonTokenClass.Number)
            {
                return reader.ReadNumber();
            }
            else if (reader.TokenClass == JsonTokenClass.Boolean)
            {
                return reader.ReadBoolean();
            }
            else if (reader.TokenClass == JsonTokenClass.Null)
            {
                reader.Read();
                return null;
            }
            else if (reader.TokenClass == JsonTokenClass.Array)
            {
                JsonArray items = new JsonArray();
                items.Import(reader);
                return items;
            }
            else if (reader.TokenClass == JsonTokenClass.Object)
            {
                JsonObject o = new JsonObject();
                o.Import(reader);
                return o;
            }
            else 
            {
                throw new JsonException(string.Format("{0} not expected.", reader.TokenClass));
            }
        }
    }
}