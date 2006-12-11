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

    #endregion

    /// <summary>
    /// Base implementation of <see cref="JsonWriter"/> that can be used
    /// as a starting point for sub-classes of <see cref="JsonWriter"/>.
    /// </summary>

    public abstract class JsonWriterBase : JsonWriter
    {
        private Stack _brackets;
        private JsonWriterBracket _currentBracket;
        
        public JsonWriterBase()
        {
            CurrentBracket = JsonWriterBracket.Pending;
        }

        public sealed override int Depth
        {
            get { return Brackets.Count; }
        }

        public sealed override JsonWriterBracket Bracket
        {
            get { return CurrentBracket; }
        }

        public sealed override void WriteStartObject()
        {
            EnsureNotEnded();

            if (CurrentBracket != JsonWriterBracket.Pending)
                EnsureMemberOnObjectBracket();
            
            PushBracket(JsonWriterBracket.Object);
            WriteStartObjectImpl();
        }

        public sealed override void WriteEndObject()
        {
            if (CurrentBracket != JsonWriterBracket.Object)
                throw new JsonException("JSON Object tail not expected at this time.");
            
            WriteEndObjectImpl();
            PopBracket();
        }

        public sealed override void WriteMember(string name)
        {
            if (CurrentBracket != JsonWriterBracket.Object)
                throw new JsonException("A JSON Object member is not valid inside a JSON Array.");

            WriteMemberImpl(name);
            CurrentBracket = JsonWriterBracket.Member;
        }

        public sealed override void WriteStartArray()
        {
            EnsureNotEnded();

            if (CurrentBracket != JsonWriterBracket.Pending)
                EnsureMemberOnObjectBracket();
            
            PushBracket(JsonWriterBracket.Array);
            WriteStartArrayImpl();
        }

        public sealed override void WriteEndArray()
        {
            if (_currentBracket != JsonWriterBracket.Array)
                throw new JsonException("JSON Array tail not expected at this time.");
            
            WriteEndArrayImpl();
            PopBracket();
        }

        public sealed override void WriteString(string value)
        {
            EnsureMemberOnObjectBracket();
            WriteStringImpl(value);
            PostScalarWrite();
        }

        private void PostScalarWrite()
        {
            if (CurrentBracket == JsonWriterBracket.Member) 
                CurrentBracket = JsonWriterBracket.Object;
        }

        public sealed override void WriteNumber(string value)
        {
            EnsureMemberOnObjectBracket();
            WriteNumberImpl(value);
            PostScalarWrite();
        }

        public sealed override void WriteBoolean(bool value)
        {
            EnsureMemberOnObjectBracket();
            WriteBooleanImpl(value);
            PostScalarWrite();
        }

        public sealed override void WriteNull()
        {
            EnsureMemberOnObjectBracket();
            WriteNullImpl();
            PostScalarWrite();
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

        private JsonWriterBracket CurrentBracket
        {
            get { return _currentBracket; }
            set { _currentBracket = value; }
        }

        private Stack Brackets
        {
            get
            {
                if (_brackets == null)
                    _brackets = new Stack(6);
                
                return _brackets;
            }
        }

        private void PushBracket(JsonWriterBracket newBracket)
        {
            Debug.Assert(newBracket == JsonWriterBracket.Array || newBracket == JsonWriterBracket.Object);
            
            Brackets.Push(CurrentBracket == JsonWriterBracket.Member ? JsonWriterBracket.Object : CurrentBracket);
            CurrentBracket = newBracket;
        }
        
        private void PopBracket()
        {
            JsonWriterBracket bracket = (JsonWriterBracket) Brackets.Pop();

            if (bracket == JsonWriterBracket.Pending)
                bracket = JsonWriterBracket.EOF;
            
            CurrentBracket = bracket;
        }

        private void EnsureMemberOnObjectBracket() 
        {
            if (CurrentBracket == JsonWriterBracket.Object)
                throw new JsonException("A JSON member value inside a JSON object must be preceded by its member name.");
        }

        private void EnsureNotEnded()
        {
            if (CurrentBracket == JsonWriterBracket.EOF)
                throw new JsonException("JSON text has already been ended.");
        }
    }
}