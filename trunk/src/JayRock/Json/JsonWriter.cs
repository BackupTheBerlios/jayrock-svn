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

    public abstract class JsonWriter
    {
        private IJsonFormatter _valueFormatter;
        private static readonly JsonFormatter _defaultFormatter = new JsonFormatter();

        public abstract void WriteStartObject();
        public abstract void WriteEndObject();
        public abstract void WriteMember(string name);
        public abstract void WriteStartArray();
        public abstract void WriteEndArray();
        public abstract void WriteString(string value);
        public abstract void WriteNumber(string value);
        public abstract void WriteBoolean(bool value);
        public abstract void WriteNull();
        
        public virtual IJsonFormatter ValueFormatter
        {
            get
            {
                if (_valueFormatter == null)
                    return _defaultFormatter;

                return _valueFormatter;
            }
            
            set { _valueFormatter = value; }
        }

        public virtual void Flush() { }

        public virtual void WriteNumber(byte value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumber(short value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumber(int value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumber(long value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public virtual void WriteNumber(decimal value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumber(float value)
        {
            if (float.IsNaN(value))
                throw new ArgumentOutOfRangeException("value");

            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public virtual void WriteNumber(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentOutOfRangeException("value");

            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public virtual void WriteArray(IEnumerable values)
        {
            WriteArray(values, this);
        }

        public virtual void WriteArray(IEnumerable values, JsonWriter valueWriter)
        {
            if (values == null)
            {
                WriteNull();
            }
            else
            {
                WriteStartArray();

                foreach (object value in values)
                    valueWriter.WriteValue(value);

                WriteEndArray();
            }
        }

        public virtual void WriteValue(object value)
        {
            ValueFormatter.Format(value, this);
        }
    }
}