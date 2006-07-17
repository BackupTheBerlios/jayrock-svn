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
    using System.Collections.Specialized;

    #endregion

    public class NameValueCollectionImporter : TypeImporter
    {
        public override void Register(ITypeImporterRegistry registry)
        {
            registry.Register(typeof(NameValueCollection), this);
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            if (!reader.MoveToContent())
                throw new JsonSerializationException("Unexpected EOF.");
            
            //
            // If got a null then that's what we return without further ado.
            //
            
            if (reader.Token == JsonToken.Null)
                return null;
            
            //
            // Reader must be sitting on an object.
            //

            if (reader.Token != JsonToken.Object)
                throw new JsonSerializationException("Expecting object.");
            
            //
            // Create the NameValueCollection object being deserialized.
            // If a hint was supplied, then that's what we will create
            // here because it could be that the caller wants to 
            // return a subtype of NameValueCollection.
            //
            
            NameValueCollection collection = CreateCollection();
            
            //
            // Loop through all members of the object.
            //

            while (reader.ReadToken() == JsonToken.Member)
            {
                string name = reader.Text;

                reader.Read();
                
                //
                // If the value is an array, then it's a multi-value 
                // entry otherwise a single-value one.
                //

                if (reader.Token == JsonToken.Array)
                {
                    while (reader.ReadToken() != JsonToken.EndArray)
                        collection.Add(name, GetValueAsString(reader));
                }
                else
                {
                    collection.Add(name, GetValueAsString(reader));    
                }
            }
            
            if (reader.Token != JsonToken.EndObject)
                throw new JsonSerializationException("Expecting end of object.");

            return collection;
        }

        protected virtual string GetValueAsString(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
           
            switch (reader.Token)
            {
                case JsonToken.String :
                    return reader.Text;
                case JsonToken.Boolean :
                case JsonToken.Number :
                    return reader.Text;
                case JsonToken.Null :
                    return null;
            }
            
            throw new JsonSerializationException(string.Format("Cannot deserialize a value of type {0} for storing in a NameValueCollection instance.", reader.Token));
        }        

        protected virtual NameValueCollection CreateCollection()
        {
            return new NameValueCollection();
        }
    }
}