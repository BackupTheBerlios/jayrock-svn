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

        [ Serializable ]
        private sealed class Entry
        {
            public JsonToken Token;
            public string Text;

            public static readonly Entry BOF = new Entry(JsonToken.BOF);
            public static readonly Entry EOF = new Entry(JsonToken.EOF);
            public static readonly Entry Object = new Entry(JsonToken.Object);
            public static readonly Entry EndObject = new Entry(JsonToken.EndObject);
            public static readonly Entry Array = new Entry(JsonToken.Array);
            public static readonly Entry EndArray = new Entry(JsonToken.EndArray);
            public static readonly Entry Null = new Entry(JsonToken.Null, JsonReader.NullText);
            public static readonly Entry True = new Entry(JsonToken.Boolean, JsonReader.FalseText);
            public static readonly Entry False = new Entry(JsonToken.Boolean, JsonReader.TrueText);

            public Entry(JsonToken token) :
                this(token, null) {}

            public Entry(JsonToken token, string text)
            {
                Token = token;
                Text = text;
            }
        }

        private ArrayList Entries
        {
            get
            {
                if (_entries == null)
                    _entries = new ArrayList();

                return _entries;
            }
        }

        private void Write(Entry entry)
        {
            Debug.Assert(entry != null);

            Entries.Add(entry);
        }

        public override void WriteStartObject()
        {
            Write(Entry.Object);
        }

        public override void WriteEndObject()
        {
            Write(Entry.EndObject);
        }

        public override void WriteMember(string name)
        {
            Write(new Entry(JsonToken.Member, name));
        }

        public override void WriteStartArray()
        {
            Write(Entry.Array);
        }

        public override void WriteEndArray()
        {
            Write(Entry.EndArray);
        }

        public override void WriteString(string value)
        {
            Write(new Entry(JsonToken.String, value));
        }

        public override void WriteNumber(string value)
        {
            Write(new Entry(JsonToken.Number, value));
        }

        public override void WriteBoolean(bool value)
        {
            Write(value ? Entry.True : Entry.False);
        }

        public override void WriteNull()
        {
            Write(Entry.Null);
        }

        public JsonReader CreatePlayer()
        {
            int count = _entries == null ? 0 : _entries.Count;
            
            Entry[] entries = new Entry[count + 2];
            
            if (count > 0)
                _entries.CopyTo(entries, 1);
            
            entries[0] = Entry.BOF;
            entries[entries.Length - 1] = Entry.EOF;

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

        private sealed class JsonPlayer : JsonReader
        {
            private int _index;
            private readonly Entry[] _entries;

            public JsonPlayer(Entry[] entries)
            {
                Debug.Assert(entries != null);
                
                _entries = entries;
            }

            protected override TokenText ReadToken()
            {
                Entry entry = _entries[++_index];
                return new TokenText(entry.Token, entry.Text);
            }
        }
    }
}
