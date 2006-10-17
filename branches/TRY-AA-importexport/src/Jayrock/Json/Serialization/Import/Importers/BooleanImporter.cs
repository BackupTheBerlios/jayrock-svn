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

namespace Jayrock.Json.Serialization.Import.Importers
{
    #region Imports

    using System;
    using System.Globalization;

    #endregion

    public sealed class BooleanImporter : JsonImporterBase
    {
        public BooleanImporter() : 
            base(typeof(bool)) { }

        protected override object ImportValue(JsonReader reader, object context)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            bool value;
            
            if (reader.TokenClass == JsonTokenClass.Number)
            {
                try
                {
                    value = Convert.ToInt64(reader.Text, CultureInfo.InvariantCulture) != 0;
                }
                catch (FormatException e)
                {
                    throw new JsonException(string.Format("The JSON Number {0} must be an integer to be convertible to System.Boolean.", reader.Text), e);
                }
            }
            else if (reader.TokenClass == JsonTokenClass.Boolean)
            {
                value = reader.Text == JsonBoolean.TrueText;
            }
            else
            {
                throw new JsonException(string.Format("Found {0} where expecting a JSON Boolean.", reader.TokenClass));
            }
            
            return value ? BooleanObject.True : BooleanObject.False;
        }
    }
}