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

namespace Jayrock.Json.Importers
{
    #region Imports

    using System;
    using System.Globalization;
    using System.Xml;

    #endregion

    public sealed class DateTimeImporter : JsonImporter
    {
        public override void Register(IJsonImporterRegistry registry)
        {
            registry.Register(typeof(DateTime), this);
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader.Token == JsonToken.String)
            {
                return XmlConvert.ToDateTime(reader.Text);
            }
            else if (reader.Token == JsonToken.Number)
            {
                try
                {
                    return UnixTime.ToDateTime(Convert.ToInt64(reader.Text, CultureInfo.InvariantCulture));
                }
                catch (FormatException e)
                {
                    throw new JsonSerializationException(null, e); // TODO: Supply an exception message.
                }
                catch (OverflowException e)
                {
                    throw new JsonSerializationException(null, e); // TODO: Supply an exception message.
                }
            }
            else
            {
                throw new JsonSerializationException(string.Format("Found {0} where expecting a string in ISO 8601 time format or a number expressed in Unix time.", reader.Token));
            }
        }
    }
}