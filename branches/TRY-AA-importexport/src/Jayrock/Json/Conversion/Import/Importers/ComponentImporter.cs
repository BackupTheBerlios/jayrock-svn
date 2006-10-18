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
    using System.ComponentModel;
    using System.Reflection;

    #endregion
    
    public sealed class ComponentImporterFamily : IJsonImporterFamily
    {
        public IJsonImporter Page(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return (type.IsPublic || type.IsNestedPublic) && 
                   !type.IsPrimitive && type.GetConstructor(Type.EmptyTypes) != null ? 
                   new ComponentImporter(type) : null;
        }
    }

    public sealed class ComponentImporter : JsonImporterBase
    {
        private readonly PropertyDescriptorCollection _properties;

        public ComponentImporter(Type type) :
            this(type, null) {}

        public ComponentImporter(Type type, ICustomTypeDescriptor typeDescriptor) :
            base(type)
        {
            if (typeDescriptor == null)
                typeDescriptor = new DefaultTypeDescriptor(type);
            
            _properties = typeDescriptor.GetProperties();
        }

        protected override object ImportValue(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            reader.ReadToken(JsonTokenClass.Object);
            
            object o = Activator.CreateInstance(OutputType);
            
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

        private sealed class DefaultTypeDescriptor : ICustomTypeDescriptor
        {
            private readonly PropertyDescriptorCollection _properties;
        
            public DefaultTypeDescriptor(Type type)
            {
                if (type == null) 
                    throw new ArgumentNullException("type");
            
                PropertyDescriptorCollection properties = new PropertyDescriptorCollection(null);
                
                //
                // Add public fields that are not read only and not 
                // constant literals.
                //
            
                foreach (FieldInfo field in type.GetFields())
                {
                    if (!field.IsInitOnly && !field.IsLiteral)
                        properties.Add(new TypeFieldDescriptor(field));
                }
                
                //
                // Add public properties that can be read and modified.
                //

                foreach (PropertyInfo propetry in type.GetProperties())
                {
                    if (propetry.CanRead && propetry.CanWrite)
                        properties.Add(new TypePropertyDescriptor(propetry));
                }

                _properties = properties;
            }

            public PropertyDescriptorCollection GetProperties()
            {
                return _properties;
            }

            #region Unimplemented ICustomTypeDescriptor members

            public AttributeCollection GetAttributes()
            {
                throw new NotImplementedException();
            }

            public string GetClassName()
            {
                throw new NotImplementedException();
            }

            public string GetComponentName()
            {
                throw new NotImplementedException();
            }

            public TypeConverter GetConverter()
            {
                throw new NotImplementedException();
            }

            public EventDescriptor GetDefaultEvent()
            {
                throw new NotImplementedException();
            }

            public PropertyDescriptor GetDefaultProperty()
            {
                throw new NotImplementedException();
            }

            public object GetEditor(Type editorBaseType)
            {
                throw new NotImplementedException();
            }

            public EventDescriptorCollection GetEvents()
            {
                throw new NotImplementedException();
            }

            public EventDescriptorCollection GetEvents(Attribute[] attributes)
            {
                throw new NotImplementedException();
            }

            public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                throw new NotImplementedException();
            }

            public object GetPropertyOwner(PropertyDescriptor pd)
            {
                throw new NotImplementedException();
            }

            #endregion

            /// <summary>
            /// A base <see cref="PropertyDescriptor"/> implementation for
            /// a type member (<see cref="MemberInfo"/>).
            /// </summary>

            private abstract class TypeMemberDescriptor : PropertyDescriptor
            {
                protected TypeMemberDescriptor(MemberInfo member) : 
                    base(member.Name, null) {}

                protected abstract MemberInfo Member { get; }
                
                public override bool Equals(object obj)
                {
                    TypeMemberDescriptor other = obj as TypeMemberDescriptor;
                    return other != null && other.Member.Equals(Member);
                }

                public override int GetHashCode() { return Member.GetHashCode(); }
                public override bool IsReadOnly { get { return false; } }
                public override void ResetValue(object component) {}
                public override bool CanResetValue(object component) { return false; }
                public override bool ShouldSerializeValue(object component) { return true; }
                public override Type ComponentType { get { return Member.DeclaringType; } }
                
                public abstract override Type PropertyType { get; }
                public abstract override object GetValue(object component);
                public abstract override void SetValue(object component, object value);
            }

            /// <summary>
            /// A <see cref="PropertyDescriptor"/> implementation around
            /// <see cref="FieldInfo"/>.
            /// </summary>

            private sealed class TypeFieldDescriptor : TypeMemberDescriptor
            {
                private readonly FieldInfo _field;

                public TypeFieldDescriptor(FieldInfo field) : 
                    base(field)
                {
                    _field = field;
                }

                protected override MemberInfo Member
                {
                    get { return _field; }
                }
                
                public override Type PropertyType
                {
                    get { return _field.FieldType; }
                }

                public override object GetValue(object component)
                {
                    return _field.GetValue(component);
                }

                public override void SetValue(object component, object value) 
                {
                    _field.SetValue(component, value); 
                    OnValueChanged(component, EventArgs.Empty);
                }
            }
            
            /// <summary>
            /// A <see cref="PropertyDescriptor"/> implementation around
            /// <see cref="PropertyInfo"/>.
            /// </summary>

            private sealed class TypePropertyDescriptor : TypeMemberDescriptor
            {
                private readonly PropertyInfo _property;

                public TypePropertyDescriptor(PropertyInfo property) : 
                    base(property)
                {
                    _property = property;
                }

                protected override MemberInfo Member
                {
                    get { return _property; }
                }

                public override Type PropertyType
                {
                    get { return _property.PropertyType; }
                }

                public override object GetValue(object component)
                {
                    return _property.GetValue(component, null);
                }

                public override void SetValue(object component, object value) 
                {
                    _property.SetValue(component, value, null); 
                    OnValueChanged(component, EventArgs.Empty);
                }
            }
        }
    }
}
