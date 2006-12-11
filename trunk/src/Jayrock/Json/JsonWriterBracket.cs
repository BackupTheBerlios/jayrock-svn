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
    using System.Runtime.Serialization;

    #endregion

    [ Serializable ]
    public sealed class JsonWriterBracket : IObjectReference
    {
        public static readonly JsonWriterBracket Pending = new JsonWriterBracket("Pending");
        public static readonly JsonWriterBracket Array = new JsonWriterBracket("Array");
        public static readonly JsonWriterBracket Object = new JsonWriterBracket("Object");
        public static readonly JsonWriterBracket Member = new JsonWriterBracket("Member");
        public static readonly JsonWriterBracket EOF = new JsonWriterBracket("EOF");
            
        public static readonly ICollection All = new JsonWriterBracket[] { Pending, Array, Object, Member, EOF };
            
        private readonly string _name;
          
        private JsonWriterBracket(string name) 
        {
            Debug.Assert(name != null);
                
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
            
        public override string ToString()
        {
            return Name;
        }
            
        object IObjectReference.GetRealObject(StreamingContext context)
        {
            foreach (JsonWriterBracket bracket in All)
            {
                if (string.CompareOrdinal(bracket.Name, Name) == 0)
                    return bracket;
            }
                
            throw new SerializationException(string.Format("{0} is not a valid {1} instance.", Name, typeof(JsonWriterBracket).FullName));
        }
    }
}