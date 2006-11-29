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
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    public abstract class NumberImporterBase : TypeImporterBase
    {
        protected NumberImporterBase(Type type) :
            base(type) {}
 
        protected override object ImportValue(ImportContext context, JsonReader reader)
        {
            if (context == null)
                throw new ArgumentNullException("reader");

            if (reader == null)
                throw new ArgumentNullException("reader");
            
            if (reader.TokenClass != JsonTokenClass.Number && reader.TokenClass != JsonTokenClass.String)
                throw new JsonException(string.Format("Found {0} where expecting a number.", reader.TokenClass));

            string text = reader.Text;
            
            try
            {
                return ConvertFromString(text);
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

        protected abstract object ConvertFromString(string s);

        private Exception NumberError(Exception e, string text)
        {
            return new JsonException(string.Format("Error importing JSON Number {0} as {1}.", text, OutputType.FullName), e);
        }
    }
    
    public class ByteImporter : NumberImporterBase
    {
        public ByteImporter() : 
            base(typeof(byte)) {}

        protected override object ConvertFromString(string s)
        {
            return Convert.ToByte(s, CultureInfo.InvariantCulture);
        }
    }

    public class Int16Importer : NumberImporterBase
    {
        public Int16Importer() : 
            base(typeof(short)) {}

        protected override object ConvertFromString(string s)
        {
            return Convert.ToInt16(s, CultureInfo.InvariantCulture);
        }
    }

    public class Int32Importer : NumberImporterBase
    {
        public Int32Importer() : 
            base(typeof(int)) {}

        protected override object ConvertFromString(string s)
        {
            return Convert.ToInt32(s, CultureInfo.InvariantCulture);
        }
    }

    public class Int64Importer : NumberImporterBase
    {
        public Int64Importer() : 
            base(typeof(long)) {}

        protected override object ConvertFromString(string s)
        {
            return Convert.ToInt64(s, CultureInfo.InvariantCulture);
        }
    }

    public class SingleImporter : NumberImporterBase
    {
        public SingleImporter() : 
            base(typeof(float)) {}

        protected override object ConvertFromString(string s)
        {
            return Convert.ToSingle(s, CultureInfo.InvariantCulture);
        }
    }

    public class DoubleImporter : NumberImporterBase
    {
        public DoubleImporter() : 
            base(typeof(double)) {}

        protected override object ConvertFromString(string s)
        {
            return Convert.ToDouble(s, CultureInfo.InvariantCulture);
        }
    }

    public class DecimalImporter : NumberImporterBase
    {
        public DecimalImporter() : 
            base(typeof(decimal)) {}

        protected override object ConvertFromString(string s)
        {
            return Convert.ToDecimal(s, CultureInfo.InvariantCulture);
        }
    }
}