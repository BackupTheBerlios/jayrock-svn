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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Collections;
    using System.Globalization;

    #endregion

    public class JsonFormatter : IJsonFormatter
    {
        public virtual void Format(object o, JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (JsonNull.LogicallyEquals(o))
            {
                FormatNull(o, writer);
                return;
            }

            //
            // Handle primitive mapping specially.
            //

            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte    : FormatByte((byte) o, writer);       return;
                case TypeCode.Int16   : FormatInt16((short) o, writer);     return;
                case TypeCode.Int32   : FormatInt32((int) o, writer);       return;
                case TypeCode.Int64   : FormatInt64((long) o, writer);      return;
                case TypeCode.Decimal : FormatDecimal((decimal) o, writer); return;
                case TypeCode.Single  : FormatSingle((float) o, writer);    return;
                case TypeCode.Double  : FormatDouble((double) o, writer);   return;
                case TypeCode.Boolean : FormatBoolean((bool) o, writer);    return;
                case TypeCode.String  : FormatString((string) o, writer);   return;
            }
                    
            //
            // For all other types, go through a method that can be
            // overridden by subtypes.
            //

            FormatOther(o, writer);
        }

        protected virtual void FormatOther(object o, JsonWriter writer)
        {
            //
            // If the value is is JSON-aware then let it do the job.
            //

            IJsonFormattable jsonFormattable = o as IJsonFormattable;

            if (jsonFormattable != null)
            {
                FormatFormattable(jsonFormattable, writer);
                return;
            }

            //
            // If the value is a dictionary then encode it as a JSON
            // object.
            //

            IDictionary dictionary = o as IDictionary;

            if (dictionary != null)
            {
                FormatDictionary(dictionary, writer);
                return;
            }

            //
            // If the value is enumerable then encode it as a JSON
            // array.
            //

            IEnumerable enumerable = o as IEnumerable;

            if (enumerable != null)
            {
                FormatEnumerable(enumerable, writer);
                return;
            }

            //
            // For all other types, write out its string representation as a
            // simple JSON string.
            //
            
            writer.WriteString(Convert.ToString(o, CultureInfo.InvariantCulture));
        }

        protected virtual void FormatNull(object value, JsonWriter writer)
        {
            writer.WriteNull();
        }

        protected virtual void FormatFormattable(IJsonFormattable formattable, JsonWriter writer)
        {
            formattable.Format(writer);
        }

        protected virtual void FormatDictionary(IDictionary dictionary, JsonWriter writer)
        {
            writer.WriteStartObject();
    
            foreach (DictionaryEntry entry in dictionary)
            {
                writer.WriteMember(entry.Key.ToString());
                writer.WriteValue(entry.Value);
            }
    
            writer.WriteEndObject();
        }

        protected virtual void FormatEnumerable(IEnumerable enumerable, JsonWriter writer)
        {
            writer.WriteArray(enumerable);
        }

        protected virtual void FormatByte(byte value, JsonWriter writer)
        {
            writer.WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void FormatInt16(short value, JsonWriter writer)
        {
            writer.WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void FormatInt32(int value, JsonWriter writer)
        {
            writer.WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
    
        protected virtual void FormatInt64(long value, JsonWriter writer)
        {
            writer.WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
    
        protected virtual void FormatDecimal(decimal value, JsonWriter writer)
        {
            writer.WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
    
        protected virtual void FormatSingle(float value, JsonWriter writer)
        {
            writer.WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
    
        protected virtual void FormatDouble(double value, JsonWriter writer)
        {
            writer.WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        protected virtual void FormatBoolean(bool value, JsonWriter writer)
        {
            writer.WriteBoolean(value);
        }
    
        protected virtual void FormatString(string value, JsonWriter writer)
        {
            writer.WriteString(value);
        }
    }
}
