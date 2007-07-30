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
    using System.ComponentModel.Design;
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
            PropertyDescriptor property = Thing.GetField1Property();
            Assert.IsInstanceOfType(typeof(IPropertyCustomization), property);
        }

        [ Test ]
        public void TypePropertyDescriptorSupportsCustomization()
        {
            PropertyDescriptor property = Thing.GetProperty1Property();
            Assert.IsInstanceOfType(typeof(IPropertyCustomization), property);
        }

        [ Test ]
        public void PropertyNameCustomization()
        {
            PropertyDescriptor property = Thing.GetField1Property();
            Assert.AreEqual("Field1", property.Name);
            IPropertyCustomization customization = (IPropertyCustomization) property;
            customization.SetName("FIELD1");
            Assert.AreEqual("FIELD1", property.Name);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotCustomizePropertyWithNullName()
        {
            PropertyDescriptor property = Thing.GetField1Property();
            ((IPropertyCustomization) property).SetName(null);
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotCustomizePropertyWithEmptyName()
        {
            PropertyDescriptor property = Thing.GetField1Property();
            ((IPropertyCustomization) property).SetName(string.Empty);
        }

        [ Test ]
        public void PropertyTypeCustomization()
        {
            PropertyDescriptor property = Thing.GetField1Property();
            Assert.AreEqual(typeof(object), property.PropertyType);
            IPropertyCustomization customization = (IPropertyCustomization) property;
            customization.SetType(typeof(string));
            Assert.AreEqual(typeof(string), property.PropertyType);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotCustomizePropertyWithNullType()
        {
            PropertyDescriptor property = Thing.GetField1Property();
            ((IPropertyCustomization) property).SetType(null);
        }

        [ Test ]
        public void PropertyGetterCustomization()
        {
            Thing thing = new Thing();
            PropertyDescriptor property = Thing.GetField1Property();
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
            PropertyDescriptor property = Thing.GetField1Property();
            ((IPropertyCustomization) property).OverrideImpl(null);
        }

        [ Test ]
        public void PropertySetterCustomization()
        {
            Thing thing = new Thing();
            PropertyDescriptor property = Thing.GetField1Property();
            Assert.IsNull(thing.Field1);
            IPropertyCustomization customization = (IPropertyCustomization) property;
            FakePropertyImpl impl = new FakePropertyImpl();
            impl.BaseImpl = customization.OverrideImpl(impl);
            property.SetValue(thing, "test");
            Assert.AreEqual(">>test", thing.Field1);
        }

        [ Test ]
        public void AddServiceToField()
        {
            AddServiceToServiceContainer((IServiceContainer) Thing.GetField1Property());
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotAddServiceTwiceToField()
        {
            CannotAddServiceTwiceToServiceContainer((IServiceContainer) Thing.GetField1Property());
        }

        [ Test ]
        public void AddThenRemoveServiceFromField()
        {
            AddThenRemoveService((IServiceContainer) Thing.GetField1Property());
        }

        [ Test ]
        public void AddServiceToProperty()
        {
            AddServiceToServiceContainer((IServiceContainer) Thing.GetProperty1Property());
        }

        [ Test, ExpectedException(typeof(ArgumentException)) ]
        public void CannotAddServiceTwiceToProperty()
        {
            CannotAddServiceTwiceToServiceContainer((IServiceContainer) Thing.GetProperty1Property());
        }

        [ Test ]
        public void AddThenRemoveServiceFromProperty()
        {
            AddThenRemoveService((IServiceContainer) Thing.GetProperty1Property());
        }

        [ Test ]
        public void ReadOnlyFieldHasReadOnlyDescriptor()
        {
            Assert.IsTrue(Thing.GetReadOnlyFieldProperty().IsReadOnly);
        }

        [ Test, ExpectedException(typeof(NotSupportedException)) ]
        public void CannotSetReadOnlyFieldViaDescriptor()
        {
            Thing.GetReadOnlyFieldProperty().SetValue(new Thing(), new object());
        }

        [ Test ]
        public void ReadOnlyPropertyHasReadOnlyDescriptor()
        {
            Assert.IsTrue(Thing.GetReadOnlyPropertyProperty().IsReadOnly);
        }

        [ Test, ExpectedException(typeof(NotSupportedException)) ]
        public void CannotSetReadOnlyPropertyViaDescriptor()
        {
            Thing.GetReadOnlyPropertyProperty().SetValue(new Thing(), new object());
        }

        private static void AddServiceToServiceContainer(IServiceContainer sc) 
        {
            object service = new object();
            Type serviceType = service.GetType();
            Assert.IsNull(sc.GetService(serviceType));
            sc.AddService(serviceType, service);
            Assert.AreSame(service, sc.GetService(serviceType));
        }
        
        private static void CannotAddServiceTwiceToServiceContainer(IServiceContainer sc) 
        {
            object service = new object();
            Type serviceType = service.GetType();
            Assert.IsNull(sc.GetService(serviceType));
            sc.AddService(serviceType, service);
            sc.AddService(serviceType, service);
        }

        private static void AddThenRemoveService(IServiceContainer sc) 
        {
            object service = new object();
            Type serviceType = service.GetType();
            Assert.IsNull(sc.GetService(serviceType));
            sc.AddService(serviceType, service);
            Assert.AreSame(service, sc.GetService(serviceType));
            sc.RemoveService(serviceType);
            Assert.IsNull(sc.GetService(serviceType));
        }

        public sealed class Thing
        {
            public object Field1;
            [ JsonIgnore ] public object Field2;
            public object Field3;
            public readonly object ReadOnlyField = null;

            public object Property1 { get { return null; } set { } }
            [ JsonIgnore ] public object Property2 { get { return null; } set { } }
            public object Property3 { get { return null; } set { } }
            public object ReadOnlyProperty { get { return null; } }
            
            public static PropertyDescriptor GetField1Property()
            {
                return CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("Field1"));
            }

            public static PropertyDescriptor GetReadOnlyFieldProperty()
            {
                return CustomTypeDescriptor.CreateProperty(typeof(Thing).GetField("ReadOnlyField"));
            }

            public static PropertyDescriptor GetProperty1Property()
            {
                return CustomTypeDescriptor.CreateProperty(typeof(Thing).GetProperty("Property1"));
            }

            public static PropertyDescriptor GetReadOnlyPropertyProperty()
            {
                return CustomTypeDescriptor.CreateProperty(typeof(Thing).GetProperty("ReadOnlyProperty"));
            }
        }
        
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