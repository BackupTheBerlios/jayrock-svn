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

namespace Jayrock.Json.Formatters
{
    #region Imports

    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    #endregion

    public sealed class ComponentFormatter : JsonFormatter
    {
        private PropertyDescriptorCollection _properties;

        public ComponentFormatter() {}

        public ComponentFormatter(PropertyDescriptorCollection properties)
        {
            _properties = properties;
        }

        protected override void FormatOther(object o, JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (JNull.LogicallyEquals(o))
            {
                FormatNull(o, writer);
                return;
            }

            writer.WriteStartObject();

            if (_properties == null)
                _properties = TypeDescriptor.GetProperties(o);

            foreach (PropertyDescriptor property in _properties)
            {
                // TODO: Allow someone to indicate via an attribute (e.g. JsonIgnore) that a property should be excluded.

                object value = property.GetValue(o);
                
                if (!JNull.LogicallyEquals(value))
                {
                    writer.WriteMember(property.Name);
                    writer.WriteValue(value);
                }
            }

            writer.WriteEndObject();
        }
    }
}
