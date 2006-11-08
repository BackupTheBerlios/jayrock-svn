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
    using System.Diagnostics;
    using System.Globalization;
    using Jayrock.Json.Conversion.Export;
    using Jayrock.Json.Conversion.Export.Exporters;

    #endregion

    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only means of 
    /// emitting JSON data.
    /// </summary>

    public abstract class JsonWriter
    {
        private JsonTokenClass _currentBracket;
        private Stack _brackets;
        
        public JsonWriter()
        {
            _brackets = new Stack(4);
            _currentBracket = JsonTokenClass.BOF;
        }
        
        public int Depth
        {
            get { return _brackets.Count; }
        }

        public void WriteStartObject()
        {
            EnsureNotEnded();
            EnterBracket(JsonTokenClass.Object);
            WriteStartObjectImpl();
        }

        public void WriteEndObject()
        {
            EnsureStructural();

            if (_currentBracket != JsonTokenClass.Object)
                throw new JsonException("JSON Object tail not expected at this time.");
            
            WriteEndObjectImpl();
            ExitBracket();
        }

        public void WriteMember(string name)
        {
            EnsureStructural();
            
            if (_currentBracket != JsonTokenClass.Object)
                throw new JsonException("A JSON Object member is not valid inside a JSON Array.");
            
            WriteMemberImpl(name);
        }

        public void WriteStartArray()
        {
            EnsureNotEnded();
            EnterBracket(JsonTokenClass.Array);
            WriteStartArrayImpl();
        }

        public void WriteEndArray()
        {
            EnsureStructural();

            if (_currentBracket != JsonTokenClass.Array)
                throw new JsonException("JSON Array tail not expected at this time.");
            
            WriteEndArrayImpl();
            ExitBracket();
        }

        public void WriteString(string value)
        {
            EnsureStructural();
            WriteStringImpl(value);
        }

        public void WriteNumber(string value)
        {
            EnsureStructural();
            WriteNumberImpl(value);
        }

        public void WriteBoolean(bool value)
        {
            EnsureStructural();
            WriteBooleanImpl(value);
        }

        public void WriteNull()
        {
            EnsureStructural();
            WriteNullImpl();
        }
        
        //
        // Actual methods that need to be implemented by the subclass.
        // These methods do not need to check for the structural 
        // integrity since this is checked by this base implementation.
        //
        
        protected abstract void WriteStartObjectImpl();
        protected abstract void WriteEndObjectImpl();
        protected abstract void WriteMemberImpl(string name);
        protected abstract void WriteStartArrayImpl();
        protected abstract void WriteEndArrayImpl();
        protected abstract void WriteStringImpl(string value);
        protected abstract void WriteNumberImpl(string value);
        protected abstract void WriteBooleanImpl(bool value);
        protected abstract void WriteNullImpl();
        
        public virtual void Flush() { }

        public void WriteNumber(byte value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(short value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(int value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(long value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public void WriteNumber(decimal value)
        {
            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(float value)
        {
            if (float.IsNaN(value))
                throw new ArgumentOutOfRangeException("value");

            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }

        public void WriteNumber(double value)
        {
            if (double.IsNaN(value))
                throw new ArgumentOutOfRangeException("value");

            WriteNumber(value.ToString(CultureInfo.InvariantCulture));
        }
        
        public void WriteStringArray(IEnumerable values)
        {
            if (values == null)
            {
                WriteNull();
            }
            else
            {
                WriteStartArray();
                        
                foreach (object value in values)
                {
                    if (JsonNull.LogicallyEquals(value))
                        WriteNull();
                    else
                        WriteString(value.ToString());
                }
                        
                WriteEndArray();
            }
        }
        
        public void WriteStringArray(params string[] values)
        {
            if (values == null)
            {
                WriteNull();
            }
            else
            {
                WriteStartArray();
                        
                foreach (string value in values)
                {
                    if (JsonNull.LogicallyEquals(value))
                        WriteNull();
                    else
                        WriteString(value);
                }
                        
                WriteEndArray();
            }
        }
        
        public void WriteValueFromReader(JsonReader reader) // FIXME: Make virtual
        {
            if (reader == null)            
                throw new ArgumentNullException("reader");

            if (!reader.MoveToContent())
                return;

            if (reader.TokenClass == JsonTokenClass.String)
            {
                WriteString(reader.Text); 
            }
            else if (reader.TokenClass == JsonTokenClass.Number)
            {
                WriteNumber(reader.Text);
            }
            else if (reader.TokenClass == JsonTokenClass.Boolean)
            {
                WriteBoolean(reader.Text == JsonBoolean.TrueText); 
            }
            else if (reader.TokenClass == JsonTokenClass.Null)
            {
                WriteNull();
            }
            else if (reader.TokenClass == JsonTokenClass.Array)
            {
                WriteStartArray();
                reader.Read();

                while (reader.TokenClass != JsonTokenClass.EndArray)
                    WriteValueFromReader(reader);

                WriteEndArray();
            }
            else if (reader.TokenClass == JsonTokenClass.Object)
            {
                reader.Read();
                WriteStartObject();
                    
                while (reader.TokenClass != JsonTokenClass.EndObject)
                {
                    WriteMember(reader.ReadMember());
                    WriteValueFromReader(reader);
                }

                WriteEndObject();
            }
            else 
            {
                throw new JsonException(string.Format("{0} not expected.", reader.TokenClass));
            }

            reader.Read();
        }
 
        private void EnterBracket(JsonTokenClass newBracket)
        {
            Debug.Assert(newBracket == JsonTokenClass.Array || newBracket == JsonTokenClass.Object);
            
            _brackets.Push(_currentBracket);
            _currentBracket = newBracket;
        }
        
        private void ExitBracket()
        {
            JsonTokenClass bracket = (JsonTokenClass) _brackets.Pop();

            if (bracket == JsonTokenClass.BOF)
                bracket = JsonTokenClass.EOF;
            
            _currentBracket = bracket;
        }

        private void EnsureStructural()
        {
            /*
            if (Depth == 0)
                throw new JsonException("A JSON Object or Array has not been started.");
            */                
        }
 
        private void EnsureNotEnded()
        {
            if (_currentBracket == JsonTokenClass.EOF)
                throw new JsonException("JSON text has already been ended.");
        }
    }
}