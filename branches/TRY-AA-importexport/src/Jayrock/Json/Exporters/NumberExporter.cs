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

namespace Jayrock.Json.Exporters
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    public abstract class NumberExporterBase : JsonExporterBase
    {
        protected NumberExporterBase(Type inputType) : 
            base(inputType) {}
        
        protected override void SubExport(object value, JsonWriter writer)
        {
            string s;

            try
            {
                s = ConvertToString(value);
            }
            catch (InvalidCastException e)
            {
                throw new JsonException(e.Message, e);
            }

            writer.WriteNumber(s);
        }
        
        protected abstract string ConvertToString(object value);
    }
    
    public class ByteExporter : NumberExporterBase
    {
        public ByteExporter() : 
            base(typeof(byte)) {}

        protected override string ConvertToString(object value)
        {
            return ((byte) value).ToString(CultureInfo.InvariantCulture);
        }
    }

    public class Int16Exporter : NumberExporterBase
    {
        public Int16Exporter() : 
            base(typeof(short)) {}

        protected override string ConvertToString(object value)
        {
            return ((short) value).ToString(CultureInfo.InvariantCulture);
        }
    }

    public class Int32Exporter : NumberExporterBase
    {
        public Int32Exporter() : 
            base(typeof(int)) {}

        protected override string ConvertToString(object value)
        {
            return ((int) value).ToString(CultureInfo.InvariantCulture);
        }
    }

    public class Int64Exporter : NumberExporterBase
    {
        public Int64Exporter() : 
            base(typeof(long)) {}

        protected override string ConvertToString(object value)
        {
            return ((long) value).ToString(CultureInfo.InvariantCulture);
        }
    }

    public class SingleExporter : NumberExporterBase
    {
        public SingleExporter() : 
            base(typeof(float)) {}

        protected override string ConvertToString(object value)
        {
            return ((float) value).ToString(CultureInfo.InvariantCulture);
        }
    }

    public class DoubleExporter : NumberExporterBase
    {
        public DoubleExporter() : 
            base(typeof(double)) {}

        protected override string ConvertToString(object value)
        {
            return ((double) value).ToString(CultureInfo.InvariantCulture);
        }
    }

    public class DecimalExporter : NumberExporterBase
    {
        public DecimalExporter() : 
            base(typeof(decimal)) {}

        protected override string ConvertToString(object value)
        {
            return ((decimal) value).ToString(CultureInfo.InvariantCulture);
        }
    }

    /*
    [ Obsolete ]
    public sealed class NumberExporter : JsonExporterBase
    {
        internal static NumberExporter Byte = new NumberExporter(typeof(byte), new Converter(FormatByte));
        internal static NumberExporter Int16 = new NumberExporter(typeof(short), new Converter(FormatInt16));
        internal static NumberExporter Int32 = new NumberExporter(typeof(int), new Converter(FormatInt32));
        internal static NumberExporter Int64 = new NumberExporter(typeof(long), new Converter(FormatInt64));
        internal static NumberExporter Single = new NumberExporter(typeof(float), new Converter(FormatSingle));
        internal static NumberExporter Double = new NumberExporter(typeof(double), new Converter(FormatDouble));
        internal static NumberExporter Decimal = new NumberExporter(typeof(decimal), new Converter(FormatDecimal));
        
        private readonly Converter _converter;

        private delegate string Converter(IConvertible value);

        private NumberExporter(Type inputType, Converter converter) : 
            base(inputType)
        {
            Debug.Assert(converter != null);
            
            _converter = converter;
        }
        
        protected override void SubExport(object o, JsonWriter writer)
        {
            string s;

            try
            {
                s = _converter((IConvertible) o);
            }
            catch (OverflowException e)
            {
                throw new JsonException(e.Message, e);
            }

            writer.WriteNumber(s);
        }
        
        public static NumberExporter Get(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte: return NumberExporter.Byte;
                case TypeCode.Int16: return NumberExporter.Int16;
                case TypeCode.Int32: return NumberExporter.Int32;
                case TypeCode.Int64: return NumberExporter.Int64;
                case TypeCode.Single: return NumberExporter.Single;
                case TypeCode.Double: return NumberExporter.Double;
                case TypeCode.Decimal: return NumberExporter.Decimal;
                
                default:
                    throw new ArgumentException("type");
            }
        }

        private static string FormatByte(IConvertible value) { return value.ToByte(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
        private static string FormatInt16(IConvertible value) { return value.ToInt16(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
        private static string FormatInt32(IConvertible value) { return value.ToInt32(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
        private static string FormatInt64(IConvertible value) { return value.ToInt64(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
        private static string FormatSingle(IConvertible value) { return value.ToSingle(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
        private static string FormatDouble(IConvertible value) { return value.ToDouble(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
        private static string FormatDecimal(IConvertible value) { return value.ToDouble(CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
    }
    */
}
