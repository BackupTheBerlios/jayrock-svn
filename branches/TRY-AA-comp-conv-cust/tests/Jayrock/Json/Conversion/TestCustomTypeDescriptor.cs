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
        public void TypeFieldDescriptorSupportsCustomization()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            Assert.IsInstanceOfType(typeof(IPropertyCustomization), property);
        }

        [ Test ]
        public void TypePropertyDescriptorSupportsCustomization()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetProperty("Property1"));
            Assert.IsInstanceOfType(typeof(IPropertyCustomization), property);
        }

        [ Test ]
        public void PropertyNameCustomization()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            Assert.AreEqual("Field1", property.Name);
            IPropertyCustomization customization = (IPropertyCustomization) property;
            customization.SetName("FIELD1");
            Assert.AreEqual("FIELD1", property.Name);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotCustomizePropertyWithNullName()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            ((IPropertyCustomization) property).SetName(null);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotCustomizePropertyWithEmptyName()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            ((IPropertyCustomization) property).SetName(string.Empty);
        }

        [ Test ]
        public void PropertyTypeCustomization()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            Assert.AreEqual(typeof(object), property.PropertyType);
            IPropertyCustomization customization = (IPropertyCustomization) property;
            customization.SetType(typeof(string));
            Assert.AreEqual(typeof(string), property.PropertyType);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotCustomizePropertyWithNullType()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            ((IPropertyCustomization) property).SetType(null);
        }

        [ Test ]
        public void PropertyGetterCustomization()
        {
            Thing thing = new Thing();
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            const string testValue = "test";
            thing.Field1 = testValue;
            Assert.AreEqual(testValue, thing.Field1);
            IPropertyCustomization customization = (IPropertyCustomization) property;
            FakePropertyImpl impl = new FakePropertyImpl();
            impl.BaseImpl = customization.OverrideImpl(impl);
            Assert.IsNotNull(impl.BaseImpl);
            Assert.AreEqual("<<" + testValue, property.GetValue(thing));
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotCustomizePropertyWithNullImpl()
        {
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            ((IPropertyCustomization) property).OverrideImpl(null);
        }

        [ Test ]
        public void PropertySetterCustomization()
        {
            Thing thing = new Thing();
            PropertyDescriptor property = CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            Assert.IsNull(thing.Field1);
            IPropertyCustomization customization = (IPropertyCustomization) property;
            FakePropertyImpl impl = new FakePropertyImpl();
            impl.BaseImpl = customization.OverrideImpl(impl);
            property.SetValue(thing, "test");
            Assert.AreEqual(">>test", thing.Field1);
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

        private sealed class FakePropertyImpl : IPropertyImpl
        {
            public IPropertyImpl BaseImpl;
            
            public object GetValue(object obj)
            {
                return "<<" + BaseImpl.GetValue(obj);
            }

            public void SetValue(object obj, object value)
            {
                BaseImpl.SetValue(obj, ">>" + value);
            }
        }
    }
}