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
    using System.IO;

    #endregion

    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only means of 
    /// emitting JSON data formatted as JSON text (RFC 4627).
    /// </summary>

    public class JsonTextWriter : JsonWriter
    {
        private readonly TextWriter _writer;

        private Stack _stack;
        private object _currentBracket = _noBracket;

        private static readonly object _noBracket = Bracket.None;
        private static readonly object _newObjectBracket = Bracket.NewObject;
        private static readonly object _runningObjectBracket = Bracket.RunningObject;
        private static readonly object _newArrayBracket = Bracket.NewArray;
        private static readonly object _runningArrayBracket = Bracket.RunningArray;

        private enum Bracket
        {
            None,
            NewObject,
            RunningObject,
            NewArray,
            RunningArray
        }

        public JsonTextWriter() :
            this(null) {}

        public JsonTextWriter(TextWriter writer)
        {
            _writer = writer != null ? writer : new StringWriter();
        }

        public TextWriter InnerWriter
        {
            get { return _writer; }
        }

        protected override void WriteStartObjectImpl()
        {
            BeforeWrite();
            EnterBracket(_newObjectBracket);

            _writer.Write('{');
        }

        protected override void WriteEndObjectImpl()
        {
            _writer.Write('}');
            ExitBracket();
        }

        protected override void WriteMemberImpl(string name)
        {
            if (_currentBracket == _runningObjectBracket)
                _writer.Write(',');
            
            WriteString(name);
            _writer.Write(':');
            _currentBracket = _runningObjectBracket;
        }

        protected override void WriteStringImpl(string value)
        {
            BracketedWrite(JsonString.Enquote(value));
        }

        protected override void WriteNumberImpl(string value)
        {
            BracketedWrite(value);
        }

        protected override void WriteBooleanImpl(bool value)
        {
            BracketedWrite(value ? "true" : "false");
        }

        protected override void WriteNullImpl()
        {
            BracketedWrite(JsonNull.Text);
        }

        protected override void WriteStartArrayImpl()
        {
            BeforeWrite();
            EnterBracket(_newArrayBracket);

            _writer.Write('[');
        }

        protected override void WriteEndArrayImpl()
        {
            _writer.Write(']');            
            ExitBracket();
        }

        private void BracketedWrite(string text)
        {
            BeforeWrite();
            _writer.Write(text);
            AfterWrite();
        }

        private void EnterBracket(object bracket)
        {
            if (_stack == null)
                _stack = new Stack(10);

            _stack.Push(_currentBracket);
            _currentBracket = bracket;
        }

        private void ExitBracket()
        {
            _currentBracket = _stack.Pop();
            AfterWrite();
        }

        private void BeforeWrite()
        {
            if (_currentBracket == _runningArrayBracket)
                _writer.Write(',');
        }

        private void AfterWrite()
        {
            _currentBracket = _currentBracket == _newObjectBracket ? 
                _runningObjectBracket : 
                _runningArrayBracket;
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        public override string ToString()
        {
            StringWriter stringWriter = _writer as StringWriter;
            return stringWriter != null ? 
                stringWriter.ToString() : base.ToString();
        }
    }
}
