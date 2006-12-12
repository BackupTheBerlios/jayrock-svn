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
    using System.CodeDom;
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    public sealed class JsonCodeWriter : JsonWriterBase
    {
        private readonly CodeVariableReferenceExpression _variable;
        private readonly ICollector _collector;

        public JsonCodeWriter(CodeVariableReferenceExpression variable, ICollector collector)
        {
            if (variable == null)
                throw new ArgumentNullException("variable");

            if (collector == null)
                throw new ArgumentNullException("collector");

            _variable = variable;
            _collector = collector;
        }

        protected override void WriteStartObjectImpl()
        {
            Append(MethodBase.GetCurrentMethod());
        }

        protected override void WriteEndObjectImpl()
        {
            Append(MethodBase.GetCurrentMethod());
        }

        protected override void WriteMemberImpl(string name)
        {
            Append(MethodBase.GetCurrentMethod(), name);
        }

        protected override void WriteStartArrayImpl()
        {
            Append(MethodBase.GetCurrentMethod());
        }

        protected override void WriteEndArrayImpl()
        {
            Append(MethodBase.GetCurrentMethod());
        }

        protected override void WriteStringImpl(string value)
        {
            Append(MethodBase.GetCurrentMethod(), value);
        }

        protected override void WriteNumberImpl(string value)
        {
            Append(MethodBase.GetCurrentMethod(), value);
        }

        protected override void WriteBooleanImpl(bool value)
        {
            Append(MethodBase.GetCurrentMethod(), value);
        }

        protected override void WriteNullImpl()
        {
            Append(MethodBase.GetCurrentMethod());
        }

        private void Append(MethodBase method)
        {
            Append(method, null);
        }

        private void Append(MethodBase method, object value)
        {
            Debug.Assert(method != null);

            CodeMethodInvokeExpression call = new CodeMethodInvokeExpression(_variable, method.Name);

            if (value != null)
                call.Parameters.Add(new CodePrimitiveExpression(value));
            
            _collector.Collect(new CodeExpressionStatement(call));
        }
    }
}