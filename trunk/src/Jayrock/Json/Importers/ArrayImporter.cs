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
    using System.Diagnostics;
    using System.Text;

    #endregion
    
    public sealed class ArrayImporterFactory : IJsonImporterFactory
    {
        public IJsonImporter Create(Type type)
        {
            return new ArrayImporter(type);
        }

        public void Register(IJsonImporterRegistry registry)
        {
            registry.RegisterFactory(typeof(Array), this);
        }
    }

    public sealed class ArrayImporter : JsonImporter
    {
        private readonly Type _arrayType;

        public ArrayImporter() : this(null) {}

        public ArrayImporter(Type arrayType)
        {
            if (arrayType == null)
                arrayType = typeof(object[]);
            
            if (!arrayType.IsArray)
                throw new ArgumentException(string.Format("{0} is not an array.", arrayType.FullName), "arrayType");
            
            if (arrayType.GetArrayRank() != 1)
                throw new ArgumentException(string.Format("{0} is not one-dimension array. Multi-dimensional arrays are not supported.", arrayType.FullName), "arrayType");

            _arrayType = arrayType;
        }

        public override void Register(IJsonImporterRegistry registry)
        {
            Type elementType = _arrayType.GetElementType();
            
            //
            // For sake of convenience, if the element type does not have an
            // importer already registered then we'll check if the stock has
            // one. If yes, then we'll auto-register it here at the same time
            // as registering the importer for the array type. This allows 
            // simple types like array of integers to be handles without
            // requiring the user to register the element type and
            // then the array, which can seem like extra steps for the most
            // common cases.
            //
            
            IJsonImporter importer = registry.Find(elementType);
            
            if (importer == null)
            {
                importer = JsonImporterStock.Find(elementType);
                if (importer != null)
                    registry.Register(elementType, importer);
            }

            registry.Register(_arrayType, this);
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader.Token != JsonToken.Array)
                throw new JsonSerializationException(string.Format("Found {0} where expecting an array.", reader.Token));

            reader.Read();

            Type elementType = _arrayType.GetElementType();
            ArrayList list = new ArrayList();

            while (reader.Token != JsonToken.EndArray)
                list.Add(reader.Get(elementType));

            return list.ToArray(elementType);
        }
    }
}