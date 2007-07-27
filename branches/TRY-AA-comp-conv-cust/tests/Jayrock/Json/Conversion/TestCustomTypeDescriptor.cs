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

namespace Jayrock.Json.Conversion
{
    #region Imports

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestCustomTypeDescriptor
    {
        [ Test ]
        public void MembersWithIgnoreAttributeExcluded()
        {
            CustomTypeDescriptor thingType = new CustomTypeDescriptor(typeof(Thing));
            PropertyDescriptorCollection properties = thingType.GetProperties();
            Assert.AreEqual(4, properties.Count);
            Assert.IsNull(properties.Find("Field2", true));
            Assert.IsNull(properties.Find("Property2", true));
        }

        [ Test ]
        public void PropertyDescriptorsSupportCustomization()
        {
            CustomTypeDescriptor thingType = new CustomTypeDescriptor(typeof(Thing));
            PropertyDescriptorCollection properties = thingType.GetProperties();
            Assert.AreNotEqual(0, properties.Count);
            foreach (PropertyDescriptor property in properties)
                Assert.IsInstanceOfType(typeof(IPropertyCustomization), property, "Property = {0}", property.Name);
        }

        [ Test ]
        public void PropertyNameCustomization()
        {
            CustomTypeDescriptor thingType = new CustomTypeDescriptor(typeof(CustomizedThing));
            PropertyDescriptorCollection properties = thingType.GetProperties();
            Assert.AreEqual(3, properties.Count);
            PropertyDescriptor property = properties[0];
            Assert.AreEqual("FIELD", property.Name);
            Assert.AreSame(property, properties.Find("FIELD", false));
        }

        [ Test ]
        public void PropertyTypeCustomization()
        {
            CustomTypeDescriptor thingType = new CustomTypeDescriptor(typeof(CustomizedThing));
            PropertyDescriptorCollection properties = thingType.GetProperties();
            PropertyDescriptor time = properties.Find("time", false);
            Assert.IsNotNull(time);
            Assert.AreEqual(typeof(string), time.PropertyType);
        }

        [ Test ]
        public void PropertyGetterCustomization()
        {
            CustomTypeDescriptor thingType = new CustomTypeDescriptor(typeof(CustomizedThing));
            PropertyDescriptorCollection properties = thingType.GetProperties();
            PropertyDescriptor text = properties.Find("text", false);
            Assert.IsNotNull(text);
            CustomizedThing thing = new CustomizedThing();
            thing.Text = "test";
            Assert.AreEqual("<<test", text.GetValue(thing));
        }

        [ Test ]
        public void PropertySetterCustomization()
        {
            CustomTypeDescriptor thingType = new CustomTypeDescriptor(typeof(CustomizedThing));
            PropertyDescriptorCollection properties = thingType.GetProperties();
            PropertyDescriptor text = properties.Find("text", false);
            Assert.IsNotNull(text);
            CustomizedThing thing = new CustomizedThing();
            text.SetValue(thing, "test");
            Assert.AreEqual(">>test", thing.Text);
        }

        public sealed class Thing
        {
            public object Field1;
            [ JsonIgnore ] public object Field2;
            [ DummyAttribute ] public object Field3;

            public object Property1 { get { return null; } set { } }
            [ JsonIgnore ] public object Property2 { get { return null; } set { } }
            public object Property3 { get { return null; } set { } }
        }
        
        [ AttributeUsage(AttributeTargets.Field)]
        private sealed class DummyAttribute : Attribute {}
 
        public sealed class CustomizedThing
        {
            [UpperName] public object Field;
            [StringType] public DateTime Time;
            [Chevron] public string Text;
        }

        [ AttributeUsage(AttributeTargets.Field)]
        private sealed class UpperNameAttribute : Attribute, IPropertyDescriptorCustomization
        {
            public void Apply(PropertyDescriptor property)
            {
                IPropertyCustomization customization = (IPropertyCustomization) property;
                customization.SetName(property.Name.ToUpper(CultureInfo.InvariantCulture));
            }
        }

        [ AttributeUsage(AttributeTargets.Field)]
        private sealed class StringTypeAttribute : Attribute, IPropertyDescriptorCustomization
        {
            public void Apply(PropertyDescriptor property)
            {
                IPropertyCustomization customization = (IPropertyCustomization) property;
                customization.SetType(typeof(string));
            }
        }

        [ AttributeUsage(AttributeTargets.Field)]
        private sealed class ChevronAttribute : Attribute, IPropertyDescriptorCustomization, IPropertyImpl
        {
            private IPropertyImpl _baseImpl;

            public void Apply(PropertyDescriptor property)
            {
                IPropertyCustomization customization = (IPropertyCustomization) property;
                _baseImpl = customization.OverrideImpl(this);
                Assert.AreSame(property, _baseImpl);
            }

            public object GetValue(object obj)
            {
                return "<<" + _baseImpl.GetValue(obj);
            }

            public void SetValue(object obj, object value)
            {
                _baseImpl.SetValue(obj, ">>" + value);
            }
        }
    }
}