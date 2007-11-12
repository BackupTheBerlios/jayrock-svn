#region License, Terms and Conditions
//
// Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
// Written by Atif Aziz (atif.aziz@skybow.com)
// Copyright (c) 2005 Atif Aziz. All rights reserved.
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

namespace Jayrock.Json.Conversion
{
    #region Imports

    using System;
    using System.ComponentModel;
    using System.Globalization;

    #endregion

    [ Serializable ]
    public enum NamingCase
    {
        None,
        Camel,      // worldWideWeb
        Pascal,     // WorldWideWeb
        Upper,      // WORLDWIDEWEB
        Lower       // worldwideweb
    }

    [ Serializable ]
    [ AttributeUsage(AttributeTargets.Property | AttributeTargets.Field) ]
    public sealed class JsonMemberNameAttribute : Attribute, IPropertyDescriptorCustomization
    {
        private string _name;
        private NamingCase _case;

        public JsonMemberNameAttribute() {}

        public JsonMemberNameAttribute(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return Mask.NullString(_name); }
            set { _name = value; }
        }

        public NamingCase NamingCase
        {
            get { return _case; }
            set { _case = value; }
        }

        void IPropertyDescriptorCustomization.Apply(PropertyDescriptor property)
        {
            if (property == null) 
                throw new ArgumentNullException("property");

            string name = Name.Length > 0 ? Name : property.Name;

            switch (NamingCase)
            {
                case NamingCase.Pascal:
                    name = char.ToUpper(name[0], CultureInfo.InvariantCulture) + name.Substring(1); break;
                case NamingCase.Camel:
                    name = char.ToLower(name[0], CultureInfo.InvariantCulture) + name.Substring(1); break;
                case NamingCase.Upper:
                    name = name.ToUpper(CultureInfo.InvariantCulture); break;
                case NamingCase.Lower:
                    name = name.ToLower(CultureInfo.InvariantCulture); break;
            }

            if (string.Compare(property.Name, name, false, CultureInfo.InvariantCulture) != 0)
            {
                IPropertyCustomization customization = (IPropertyCustomization) property;
                customization.SetName(name);
            }
        }
    }
}