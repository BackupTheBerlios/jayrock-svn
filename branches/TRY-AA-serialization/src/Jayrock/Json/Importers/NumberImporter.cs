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
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    public sealed class NumberImporter : TypeImporter
    {
        internal static NumberImporter Byte = new NumberImporter(new Converter(ConvertToByte));
        internal static NumberImporter Int16 = new NumberImporter(new Converter(ConvertToInt16));
        internal static NumberImporter Int32 = new NumberImporter(new Converter(ConvertToInt32));
        internal static NumberImporter Int64 = new NumberImporter(new Converter(ConvertToInt64));
        internal static NumberImporter Single = new NumberImporter(new Converter(ConvertToSingle));
        internal static NumberImporter Double = new NumberImporter(new Converter(ConvertToDouble));
        internal static NumberImporter Decimal = new NumberImporter(new Converter(ConvertToDecimal));

        private delegate object Converter(string s);
        
        private readonly Converter _converter;
        
        private NumberImporter(Converter converter)
        {
            Debug.Assert(converter != null);
            
            _converter = converter;
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            reader.MoveToContent();
            
            if (reader.Token != JsonToken.Number)
                throw new JsonSerializationException(string.Format("Found {0} where expecting a number.", reader.Token));

            try
            {
                return _converter(reader.Text);
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

        private static object ConvertToByte(string s) { return Convert.ToByte(s, CultureInfo.InvariantCulture); }
        private static object ConvertToInt16(string s) { return Convert.ToInt16(s, CultureInfo.InvariantCulture); }
        private static object ConvertToInt32(string s) { return Convert.ToInt32(s, CultureInfo.InvariantCulture); }
        private static object ConvertToInt64(string s) { return Convert.ToInt64(s, CultureInfo.InvariantCulture); }
        private static object ConvertToSingle(string s) { return Convert.ToSingle(s, CultureInfo.InvariantCulture); }
        private static object ConvertToDouble(string s) { return Convert.ToDouble(s, CultureInfo.InvariantCulture); }
        private static object ConvertToDecimal(string s) { return Convert.ToDecimal(s, CultureInfo.InvariantCulture); }
    }
}