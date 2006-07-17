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
    using System.Reflection;

    #endregion

    public sealed class ObjectImporter : TypeImporter
    {
        private readonly Type _type;
        private WeakReference _properties;

        public ObjectImporter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            _type = type;
        }

        public override void Register(ITypeImporterRegistry registry)
        {
            registry.Register(_type, this);
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            if (reader.Token != JsonToken.Object)
                throw new JsonSerializationException(string.Format("Found {0} where expecting an object.", reader.Token));
            
            reader.Read();
            object o = Activator.CreateInstance(_type);
            
            PropertyDescriptorCollection properties = null;
           
            while (reader.Token != JsonToken.EndObject)
            {
                string memberName = reader.Text;
                reader.Read();

                if (properties == null)
                    properties = Properties;
                
                PropertyDescriptor property = properties[memberName];
                
                if (property != null)
                    property.SetValue(o, reader.Get(property.PropertyType));
            }
         
            return o;
        }

        private PropertyDescriptorCollection Properties
        {
            get
            {
                PropertyDescriptorCollection properties = null;

                if (_properties != null)
                    properties = (PropertyDescriptorCollection) _properties.Target;

                if (properties == null)
                {
                    properties = TypeDescriptor.GetProperties(_type);
                    _properties = new WeakReference(properties);
                }

                return properties;
            }
        }
    }
}