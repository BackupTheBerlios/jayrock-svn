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

    public sealed class EnumImporterFamily : ITypeImporterBinder
    {
        public ITypeImporter Bind(ImportContext context, Type type)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (type == null)
                throw new ArgumentNullException("type");

            return type.IsEnum && !type.IsDefined(typeof(FlagsAttribute), true) ? 
                   new EnumImporter(type) : null;
        }
    }
    
    public sealed class EnumImporter : TypeImporterBase
    {
        public EnumImporter(Type type) :
            base(type)
        {
            if (!type.IsEnum)
                throw new ArgumentException(string.Format("{0} does not inherit from System.Enum.", type));
            
            if (type.IsDefined(typeof(FlagsAttribute), true))
                throw new ArgumentException(string.Format("{0} is a bit field, which are not currently supported.", type));
        }

        protected override object ImportValue(ImportContext context, JsonReader reader)
        {
            string s = reader.ReadString().Trim();
        
            if (s.Length > 0)
            {
                char ch = s[0];
            
                if (Char.IsDigit(ch) || ch == '+' || ch == '-')
                    throw Error(s, null);
            }

            try
            {
                return Enum.Parse(OutputType, s, true);
            }
            catch (ArgumentException e)
            {
                //
                // Value is either an empty string ("") or only contains 
                // white space. Value is a name, but not one of the named
                // constants defined for the enumeration.
                //
            
                throw Error(s, e);
            }
        }

        private JsonException Error(string s, Exception e)
        {
            return new JsonException(string.Format("The value '{0}' cannot be imported as {1}.", DebugString.Format(s), OutputType.FullName), e);
        }
    }
}