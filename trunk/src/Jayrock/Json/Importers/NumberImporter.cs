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

    public sealed class NumberImporter : JsonImporter
    {
        internal static NumberImporter Byte = new NumberImporter(typeof(byte), new Converter(ConvertToByte));
        internal static NumberImporter Int16 = new NumberImporter(typeof(short), new Converter(ConvertToInt16));
        internal static NumberImporter Int32 = new NumberImporter(typeof(int), new Converter(ConvertToInt32));
        internal static NumberImporter Int64 = new NumberImporter(typeof(long), new Converter(ConvertToInt64));
        internal static NumberImporter Single = new NumberImporter(typeof(float), new Converter(ConvertToSingle));
        internal static NumberImporter Double = new NumberImporter(typeof(double), new Converter(ConvertToDouble));
        internal static NumberImporter Decimal = new NumberImporter(typeof(decimal), new Converter(ConvertToDecimal));

        private delegate object Converter(string s);
        
        private readonly Converter _converter;
        private readonly Type _type;

        private NumberImporter(Type type, Converter converter)
        {
            Debug.Assert(type != null);
            Debug.Assert(type.IsValueType);
            Debug.Assert(converter != null);

            _type = type;
            _converter = converter;
        }

        public override void RegisterSelf(IJsonImporterRegistry registry)
        {
            registry.Register(_type, this);
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            if (reader.TokenClass != JsonTokenClass.Number && reader.TokenClass != JsonTokenClass.String)
                throw new JsonException(string.Format("Found {0} where expecting a number.", reader.TokenClass));

            string text = reader.Text;
            
            try
            {
                return _converter(text);
            }
            catch (FormatException e)
            {
                throw NumberError(e, text);
            }
            catch (OverflowException e)
            {
                throw NumberError(e, text);
            }
        }

        private Exception NumberError(Exception e, string text)
        {
            return new JsonException(string.Format("Error importing JSON Number {0} as {1}.", text, _type.FullName), e);
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