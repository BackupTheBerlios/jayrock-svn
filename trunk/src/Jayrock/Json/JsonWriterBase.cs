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

    using System.Collections;
    using System.Diagnostics;

    #endregion

    /// <summary>
    /// Base implementation of <see cref="JsonWriter"/> that can be used
    /// as a starting point for sub-classes of <see cref="JsonWriter"/>.
    /// </summary>

    public abstract class JsonWriterBase : JsonWriter
    {
        private JsonTokenClass _currentBracket;
        private Stack _brackets;
        
        public JsonWriterBase()
        {
            _brackets = new Stack(4);
            _currentBracket = JsonTokenClass.BOF;
        }
        
        public sealed override int Depth
        {
            get { return _brackets.Count; }
        }

        public sealed override void WriteStartObject()
        {
            EnsureNotEnded();
            EnterBracket(JsonTokenClass.Object);
            WriteStartObjectImpl();
        }

        public sealed override void WriteEndObject()
        {
            EnsureStructural();

            if (_currentBracket != JsonTokenClass.Object)
                throw new JsonException("JSON Object tail not expected at this time.");
            
            WriteEndObjectImpl();
            ExitBracket();
        }

        public sealed override void WriteMember(string name)
        {
            EnsureStructural();
            
            if (_currentBracket != JsonTokenClass.Object)
                throw new JsonException("A JSON Object member is not valid inside a JSON Array.");
            
            WriteMemberImpl(name);
        }

        public sealed override void WriteStartArray()
        {
            EnsureNotEnded();
            EnterBracket(JsonTokenClass.Array);
            WriteStartArrayImpl();
        }

        public sealed override void WriteEndArray()
        {
            EnsureStructural();

            if (_currentBracket != JsonTokenClass.Array)
                throw new JsonException("JSON Array tail not expected at this time.");
            
            WriteEndArrayImpl();
            ExitBracket();
        }

        public sealed override void WriteString(string value)
        {
            EnsureStructural();
            WriteStringImpl(value);
        }

        public sealed override void WriteNumber(string value)
        {
            EnsureStructural();
            WriteNumberImpl(value);
        }

        public sealed override void WriteBoolean(bool value)
        {
            EnsureStructural();
            WriteBooleanImpl(value);
        }

        public sealed override void WriteNull()
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