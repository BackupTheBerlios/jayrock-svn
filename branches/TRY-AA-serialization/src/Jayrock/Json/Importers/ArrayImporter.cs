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
    using System.Collections;

    #endregion

    public sealed class ArrayImporter : TypeImporter
    {
        private readonly Type _elementType;

        public ArrayImporter() : this(null) {}

        public ArrayImporter(Type elementType)
        {
            if (elementType == null)
                elementType = typeof(object);
            
            if (elementType.IsArray)
                throw new ArgumentException("Element type itself cannot be yet another array.", "elementType");
            
            _elementType = elementType;
        }

        public Type ElementType
        {
            get { return _elementType; }
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader.Token != JsonToken.Array)
                throw new JsonSerializationException(string.Format("Found {0} where expecting an array.", reader.Token));

            reader.Read();
            ArrayList list = new ArrayList();

            while (reader.Token != JsonToken.EndArray)
                list.Add(reader.Get(_elementType));
         
            return list.ToArray(_elementType);
        }
    }
}