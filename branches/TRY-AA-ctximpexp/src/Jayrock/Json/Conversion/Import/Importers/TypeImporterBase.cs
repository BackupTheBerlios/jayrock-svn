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

namespace Jayrock.Json.Conversion.Import.Importers
{
    #region Imports

    using System;
    using Jayrock.Json.Conversion.Import;

    #endregion

    public abstract class TypeImporterBase : ITypeImporter
    {
        private readonly Type _outputType;

        protected TypeImporterBase(Type outputType)
        {
            if (outputType == null)
                throw new ArgumentNullException("outputType");
            
            _outputType = outputType;
        }

        public Type OutputType
        {
            get { return _outputType; }
        }

        public virtual object Import(ImportContext context, JsonReader reader)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            if (!reader.MoveToContent())
                throw new JsonException("Unexpected EOF.");
            
            object o = null;
            
            if (reader.TokenClass != JsonTokenClass.Null)
                o = ImportValue(context, reader);
            
            reader.Read();
            return o;
        }

        protected virtual object ImportValue(ImportContext context, JsonReader reader)
        {
            throw new NotImplementedException(); // FIXME: not implemented
        }
    }
}