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
    using System.ComponentModel;

    #endregion

    public sealed class ComponentImporter : JsonImporter
    {
        private readonly Type _type;
        private readonly PropertyDescriptorCollection _properties;

        public ComponentImporter(Type type) :
            this(type, null) {}

        public ComponentImporter(Type type, ICustomTypeDescriptor typeDescriptor)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            _type = type;
            _properties = typeDescriptor == null ? 
                TypeDescriptor.GetProperties(type) : 
                typeDescriptor.GetProperties();
        }

        public override void RegisterSelf(IJsonImporterRegistry registry)
        {
            registry.Register(_type, this);
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            reader.ReadToken(JsonTokenClass.Object);
            
            object o = Activator.CreateInstance(_type);
            
            while (reader.TokenClass != JsonTokenClass.EndObject)
            {
                string memberName = reader.ReadMember();
               
                PropertyDescriptor property = _properties[memberName];
                
                if (property != null && !property.IsReadOnly)
                    property.SetValue(o, reader.ReadValue(property.PropertyType));
                else 
                    reader.Skip();
            }
         
            return o;
        }
    }
}
