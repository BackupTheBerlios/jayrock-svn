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

namespace Jayrock.Json.Conversion.Importers
{
    #region Imports

    using System;
    using System.Globalization;
    using System.Xml;

    #endregion

    public sealed class DateTimeImporter : TypeImporterBase
    {
        public DateTimeImporter() : 
            base(typeof(DateTime)) {}

        protected override object ImportValue(ImportContext context, JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            string text = reader.Text;

            if (reader.TokenClass == JsonTokenClass.String)
            {
                try
                {
                    return XmlConvert.ToDateTime(text);
                }
                catch (FormatException e)
                {
                    throw new JsonException("Error importing JSON String as System.DateTime.", e);
                }
            }
            else if (reader.TokenClass == JsonTokenClass.Number)
            {
                long time;

                try
                {
                    time = Convert.ToInt64(text, CultureInfo.InvariantCulture);
                }
                catch (FormatException e)
                {
                    throw NumberError(e, text);
                }
                catch (OverflowException e)
                {
                    throw NumberError(e, text);
                }

                try
                {
                    return UnixTime.ToDateTime(time);
                }
                catch (ArgumentException e)
                {
                    throw NumberError(e, text);
                }
            }
            else
            {
                throw new JsonException(string.Format("Found {0} where expecting a JSON String in ISO 8601 time format or a JSON Number expressed in Unix time.", reader.TokenClass));
            }
        }

        private static JsonException NumberError(Exception e, string text)
        {
            return new JsonException(string.Format("Error importing JSON Number {0} as System.DateTime.", text), e);
        }
    }
}