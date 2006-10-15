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

    #endregion

    [ Serializable ]
    public sealed class JsonRecorder : JsonWriter
    {
        private ArrayList _entries;

        private ArrayList Entries
        {
            get
            {
                if (_entries == null)
                    _entries = new ArrayList();

                return _entries;
            }
        }

        private void Write(JsonToken token)
        {
            Entries.Add(token);
        }

        protected override void WriteStartObjectImpl()
        {
            Write(JsonToken.Object());
        }

        protected override void WriteEndObjectImpl()
        {
            Write(JsonToken.EndObject());
        }

        protected override void WriteMemberImpl(string name)
        {
            Write(JsonToken.Member(name));
        }

        protected override void WriteStartArrayImpl()
        {
            Write(JsonToken.Array());
        }

        protected override void WriteEndArrayImpl()
        {
            Write(JsonToken.EndArray());
        }

        protected override void WriteStringImpl(string value)
        {
            Write(JsonToken.String(value));
        }

        protected override void WriteNumberImpl(string value)
        {
            Write(JsonToken.Number(value));
        }

        protected override void WriteBooleanImpl(bool value)
        {
            Write(JsonToken.Boolean(value));
        }

        protected override void WriteNullImpl()
        {
            Write(JsonToken.Null());
        }

        public JsonReader CreatePlayer()
        {
            int count = _entries == null ? 0 : _entries.Count;
            
            JsonToken[] entries = new JsonToken[count + 2];
            
            if (count > 0)
                _entries.CopyTo(entries, 1);
            
            entries[0] = JsonToken.BOF();
            entries[entries.Length - 1] = JsonToken.EOF();

            return new JsonPlayer(entries);
        }

        public void Playback(JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            writer.WriteValueFromReader(CreatePlayer());
        }

        public static JsonRecorder Record(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            JsonRecorder recorder = new JsonRecorder();
            recorder.WriteValueFromReader(reader);
            return recorder;
        }

        [ Serializable ]
        private sealed class JsonPlayer : JsonReader
        {
            private int _index;
            private readonly JsonToken[] _entries;

            public JsonPlayer(JsonToken[] entries)
            {
                Debug.Assert(entries != null);
                
                _entries = entries;
            }

            protected override JsonToken ReadTokenImpl()
            {
                return _entries[++_index];
            }
        }
    }
}
