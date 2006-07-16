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

namespace Jayrock.Json.Importers
{
    #region Imports

    using System;

    #endregion

    public sealed class BooleanImporter : TypeImporter
    {
        //
        // The following two statics are only used as an optimization so that we
        // don't create a boxed Boolean each time the True and False properties
        // are evaluated. Instead we keep returning a reference to the same
        // immutable value. This should put much less pressure on the GC.
        //
               
        private readonly static object _trueObject = true;
        private readonly static object _falseObject = false;

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader.Token != JsonToken.Boolean)
                throw new JsonSerializationException(string.Format("Found {0} where expecting a string.", reader.Token));
            
            return reader.Text == JsonReader.TrueText ? _trueObject : _falseObject;
        }
    }
}