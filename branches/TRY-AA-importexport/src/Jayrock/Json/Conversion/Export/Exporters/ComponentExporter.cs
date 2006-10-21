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

namespace Jayrock.Json.Conversion.Export.Exporters
{
    #region Imports

    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    #endregion
    
    /// <summary>
    /// Dispenses exporters for top-level and nested types that are 
    /// public and which have default constructors.
    /// </summary>
    
    public sealed class ComponentExporterFamily : IJsonExporterFamily
    {
        public IJsonExporter Page(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return (type.IsPublic || type.IsNestedPublic) && 
                   !type.IsPrimitive && type.GetConstructor(Type.EmptyTypes) != null ? 
                   new ComponentExporter(type) : null;
        }
    }

    public sealed class ComponentExporter : JsonExporterBase
    {
        private readonly PropertyDescriptorCollection _properties; // TODO: Review thread-safety of PropertyDescriptorCollection

        public ComponentExporter(Type inputType) :
            this(inputType, (ICustomTypeDescriptor) null) {}

        public ComponentExporter(Type inputType, ICustomTypeDescriptor typeDescriptor) :
            this(inputType, typeDescriptor != null ? 
                            typeDescriptor.GetProperties() : (new CustomTypeDescriptor(inputType)).GetProperties()) {}

        private ComponentExporter(Type inputType, PropertyDescriptorCollection properties) : 
            base(inputType)
        {
            Debug.Assert(properties != null);
            
            _properties = properties;
        }

        protected override void ExportValue(object value, JsonWriter writer)
        {
            Debug.Assert(value != null);
            Debug.Assert(writer != null);
            
            if (_properties.Count == 0)
            {
                writer.WriteString(value.ToString());
            }
            else
            {
                writer.WriteStartObject();

                foreach (PropertyDescriptor property in _properties)
                {
                    object propertyValue = property.GetValue(value);
                
                    if (!JsonNull.LogicallyEquals(propertyValue))
                    {
                        writer.WriteMember(property.Name);
                        writer.WriteValue(propertyValue);
                    }
                }

                writer.WriteEndObject();
            }
        }
    }
}
