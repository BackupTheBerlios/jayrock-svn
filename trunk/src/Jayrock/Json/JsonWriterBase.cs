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
        private JsonTokenClass _currentBracket;
        private Stack _brackets;
        
        public JsonWriterBase()
        {
            CurrentBracket = JsonTokenClass.BOF;
        }
        
        public sealed override int Depth
        {
            get { return Brackets.Count; }
        }

        public sealed override void WriteStartObject()
        {
            EnsureNotEnded();

            if (CurrentBracket != JsonTokenClass.BOF)
                EnsureMemberOnObjectBracket();
            
            PushBracket(JsonTokenClass.Object);
            WriteStartObjectImpl();
        }

        public sealed override void WriteEndObject()
        {
            if (CurrentBracket != JsonTokenClass.Object)
                throw new JsonException("JSON Object tail not expected at this time.");
            
            WriteEndObjectImpl();
            PopBracket();
        }

        public sealed override void WriteMember(string name)
        {
            if (CurrentBracket != JsonTokenClass.Object)
                throw new JsonException("A JSON Object member is not valid inside a JSON Array.");

            WriteMemberImpl(name);
            CurrentBracket = JsonTokenClass.Member;
        }

        public sealed override void WriteStartArray()
        {
            EnsureNotEnded();

            if (CurrentBracket != JsonTokenClass.BOF)
                EnsureMemberOnObjectBracket();
            
            PushBracket(JsonTokenClass.Array);
            WriteStartArrayImpl();
        }

        public sealed override void WriteEndArray()
        {
            if (_currentBracket != JsonTokenClass.Array)
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
            if (CurrentBracket == JsonTokenClass.Member) 
                CurrentBracket = JsonTokenClass.Object;
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
        
        public JsonTokenClass CurrentBracket
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

        private void PushBracket(JsonTokenClass newBracket)
        {
            Debug.Assert(newBracket == JsonTokenClass.Array || newBracket == JsonTokenClass.Object);
            
            Brackets.Push(CurrentBracket == JsonTokenClass.Member ? JsonTokenClass.Object : CurrentBracket);
            CurrentBracket = newBracket;
        }
        
        private void PopBracket()
        {
            JsonTokenClass bracket = (JsonTokenClass) Brackets.Pop();

            if (bracket == JsonTokenClass.BOF)
                bracket = JsonTokenClass.EOF;
            
            CurrentBracket = bracket;
        }

        private void EnsureMemberOnObjectBracket() 
        {
            if (CurrentBracket == JsonTokenClass.Object)
                throw new JsonException("A JSON member value inside a JSON object must be preceded by its member name.");
        }

        private void EnsureNotEnded()
        {
            if (CurrentBracket == JsonTokenClass.EOF)
                throw new JsonException("JSON text has already been ended.");
        }
    }
}