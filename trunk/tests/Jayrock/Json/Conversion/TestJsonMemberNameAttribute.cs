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

namespace Jayrock.Tests.Json.Conversion
{
    #region Imports

    using System;
    using System.ComponentModel;
    using Jayrock.Json.Conversion;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonMemberNameAttribute
    {
        [ Test ]
        public void IsSerializable()
        {
            Assert.IsTrue(typeof(JsonMemberNameAttribute).IsSerializable);
        }

        [ Test ]
        public void DefaultInitializationYieldsEmptyName()
        {
            JsonMemberNameAttribute attribute = new JsonMemberNameAttribute();
            Assert.IsNotNull(attribute.Name);
            Assert.AreEqual(string.Empty, attribute.Name);
        }

        [ Test ]
        public void InitializingNullNameYieldsEmptyName()
        {
            JsonMemberNameAttribute attribute = new JsonMemberNameAttribute(null);
            Assert.IsNotNull(attribute.Name);
            Assert.AreEqual(string.Empty, attribute.Name);
        }

        [ Test ]
        public void InitializeName()
        {
            JsonMemberNameAttribute attribute = new JsonMemberNameAttribute("name");
            Assert.AreEqual("name", attribute.Name);
        }

        [ Test ]
        public void PropertyDescriptorNameCustomization()
        {
            TestPropertyDescriptor property = CreateTestProperty("foo");
            IPropertyDescriptorCustomization customization = new JsonMemberNameAttribute("bar");
            customization.Apply(property);
            Assert.AreEqual("bar", property.CustomizedName);
        }

        [ Test ]
        public void PropertyDescriptorNameCustomizationSkippedOnEmptyName()
        {
            TestPropertyDescriptor property = CreateTestProperty("foo");
            IPropertyDescriptorCustomization customization = new JsonMemberNameAttribute();
            customization.Apply(property);
            Assert.IsNull(property.CustomizedName);
        }

        [ Test ]
        [ ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotApplyToNullPropertyDescriptor()
        {
            IPropertyDescriptorCustomization customization = new JsonMemberNameAttribute();
            customization.Apply(null);
        }

        [ Test ]
        public void PascalCaseApplication()
        {
            TestNamingCase("pascalCase", NamingCase.Pascal, "PascalCase");
        }

        [ Test ]
        public void SingleLetterPascalCaseApplication()
        {
            TestNamingCase("p", NamingCase.Pascal, "P");
        }

        [ Test ]
        public void CamelCaseApplication()
        {
            TestNamingCase("CamelCase", NamingCase.Camel, "camelCase");
            TestNamingCase("C", NamingCase.Camel, "c");
        }

        [ Test ]
        public void SingleLetterCamelCaseApplication()
        {
            TestNamingCase("C", NamingCase.Camel, "c");
        }

        [ Test ]
        public void UpperCaseApplication()
        {
            TestNamingCase("upper", NamingCase.Upper, "UPPER");
        }

        [ Test ]
        public void SinlgleLetterUpperCaseApplication()
        {
            TestNamingCase("u", NamingCase.Upper, "U");
        }

        [ Test ]
        public void LowerCaseApplication()
        {
            TestNamingCase("LOWER", NamingCase.Lower, "lower");
        }

        [ Test ]
        public void SingleLetterLowerCaseApplication()
        {
            TestNamingCase("LOWER", NamingCase.Lower, "lower");
            TestNamingCase("L", NamingCase.Lower, "l");
        }

        [ Test ]
        public void NameCasingIsAppliedAfterNewName()
        {
            TestPropertyDescriptor property = CreateTestProperty("foo");
            JsonMemberNameAttribute attribute = new JsonMemberNameAttribute("bar");
            attribute.NamingCase = NamingCase.Upper;
            IPropertyDescriptorCustomization customization = attribute;
            customization.Apply(property);
            Assert.AreEqual("BAR", property.CustomizedName);
        }

        private static void TestNamingCase(string baseName, NamingCase testCase, string expected) 
        {
            JsonMemberNameAttribute attribute = new JsonMemberNameAttribute();
            attribute.NamingCase = testCase;
            TestPropertyDescriptor property = CreateTestProperty(baseName);
            IPropertyDescriptorCustomization customization = attribute;
            
            customization.Apply(property);
            
            Assert.AreEqual(expected, property.CustomizedName);
        }

        private static TestPropertyDescriptor CreateTestProperty(string baseName) 
        {
            TestPropertyDescriptor property = new TestPropertyDescriptor(baseName);
            Assert.AreEqual(baseName, property.Name);
            Assert.IsNull(property.CustomizedName);
            return property;
        }

        private sealed class TestPropertyDescriptor : PropertyDescriptor, IPropertyCustomization
        {
            public string CustomizedName;

            public TestPropertyDescriptor(string name)
                : base(name, null) {}

            public void SetName(string name)
            {
                CustomizedName = name;
            }

            public void SetType(Type type) { throw new NotImplementedException(); }
            public IPropertyImpl OverrideImpl(IPropertyImpl impl) { throw new NotImplementedException(); }

            #region Unimplemented members of PropertyDescriptor

            public override bool CanResetValue(object component) { throw new NotImplementedException(); }
            public override object GetValue(object component) { throw new NotImplementedException(); }
            public override void ResetValue(object component) { throw new NotImplementedException(); }
            public override void SetValue(object component, object value) { throw new NotImplementedException(); }
            public override bool ShouldSerializeValue(object component) { throw new NotImplementedException(); }
            public override Type ComponentType { get { throw new NotImplementedException(); } }
            public override bool IsReadOnly { get { throw new NotImplementedException(); } }
            public override Type PropertyType { get { throw new NotImplementedException(); } }

            #endregion
        }
    }
}
