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

namespace Jayrock.Json
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;
    using Jayrock.Json.Importers;
    using NUnit.Framework;

    public interface ITypeImporter
    {
        object Import(JsonReader reader);
    }

    public interface IJsonImporter    
    {
        object Import(JsonReader reader, Type typeHint);
    }

    public class JsonSerializer : JsonFormatter, IJsonImporter
    {
        //
        // The following two statics are only used as an optimization so that we
        // don't create a boxed Boolean each time the True and False properties
        // are evaluated. Instead we keep returning a reference to the same
        // immutable value. This should put much less pressure on the GC.
        //
               
        private readonly static object _trueObject = true;
        private readonly static object _falseObject = false;
        
        private IDictionary _importers;

        public IDictionary Importers
        {
            get
            {
                if (_importers == null)
                    _importers = new Hashtable();
                
                return _importers;
            }
            
            set
            {
                if (value == null) 
                    throw new ArgumentNullException("value");
                
                _importers = value;
            }
        }
        
        public object Import(JsonReader reader, Type typeHint)
        {
            IJsonImporter importer = (IJsonImporter) Importers[typeHint];          
            
            if (importer == null)
                throw new JsonSerializationException(string.Format("Don't know how to import {0} values.", typeHint.FullName));
            
            return importer.Import(reader, typeHint);
        }

        #region OLD CODE

        /*

        public object ReadType(JsonReader reader, Type typeHint)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            if (!reader.MoveToContent())
                throw new JsonSerializationException("Unexpected EOF.");
            
            if (reader.Token == JsonToken.Member)
                throw new JsonSerializationException("Cannot deserialize from inside an object.");
                        
            switch (reader.Token)
            {
                case JsonToken.Null :
                    return GetNull(reader, typeHint);
                    
                case JsonToken.Number :
                    return GetNumber(reader, typeHint);
                    
                case JsonToken.Boolean :
                    return GetBoolean(reader, typeHint);
                    
                case JsonToken.String :
                    return GetValueAsString(reader, typeHint);
            
                case JsonToken.Array :
                    return GetArray(reader, typeHint);

                case JsonToken.Object :
                    return GetObject(reader, typeHint);
            }
            
            return null;
        }

        protected virtual object GetNull(JsonReader reader, Type typeHint)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            return null;
        }

        protected object GetBoolean(JsonReader reader, Type typeHint)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            return reader.Text == JsonReader.TrueText ? _trueObject : _falseObject;
        }

        protected virtual object GetValueAsString(JsonReader reader, Type typeHint)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            return reader.Text;
        }

        protected virtual object GetNumber(JsonReader reader, Type typeHint)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            try
            {
                if (typeHint != null)
                {
                    switch (Type.GetTypeCode(typeHint))
                    {
                        case TypeCode.Byte: return Convert.ToByte(reader.Text, CultureInfo.InvariantCulture);
                        case TypeCode.Int16: return Convert.ToInt16(reader.Text, CultureInfo.InvariantCulture);
                        case TypeCode.Int32: return Convert.ToInt32(reader.Text, CultureInfo.InvariantCulture);
                        case TypeCode.Int64: return Convert.ToInt64(reader.Text, CultureInfo.InvariantCulture);
                        case TypeCode.Single: return Convert.ToSingle(reader.Text, CultureInfo.InvariantCulture);
                        case TypeCode.Double: return Convert.ToDouble(reader.Text, CultureInfo.InvariantCulture);
                        case TypeCode.Decimal: return Convert.ToDecimal(reader.Text, CultureInfo.InvariantCulture);
                    }
                }

                //
                // Try first parsing as a 32-bit integer. If that doesn't work
                // then just assume it is a double.
                //

                try 
                {
                    return Convert.ToInt32(reader.Text, CultureInfo.InvariantCulture);
                } 
                catch (FormatException) {}
                catch (OverflowException) {}
                
                try 
                {
                    return Convert.ToInt64(reader.Text, CultureInfo.InvariantCulture);
                } 
                catch (FormatException) {}
                catch (OverflowException) {}

                return Convert.ToDouble(reader.Text, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new JsonSerializationException(null, e); // TODO: Supply an exception message.
            }
            catch (OverflowException e)
            {
                throw new JsonSerializationException(null, e); // TODO: Supply an exception message.
            }
        }

        protected object GetArray(JsonReader reader, Type typeHint)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            
            if (typeHint != null)
            {
                if (!typeHint.IsArray)
                {
                    typeHint = null;
                }
                else
                {
                    if (typeHint.GetArrayRank() > 1)
                        throw new ArgumentException(string.Format("The hinted array type, {0}, cannot be multi-dimensional.", typeHint.FullName), "typeHint");
                }
            }

            Type elementType = typeHint.GetElementType();
            ArrayList list = new ArrayList();

            while (reader.ReadToken() != JsonToken.EndArray)
                list.Add(ReadType(reader, elementType));
         
            return list.ToArray(elementType);
        }

        private object GetObject(JsonReader reader, Type typeHint)
        {
            throw new NotImplementedException();
        }
        */

        #endregion
    }

    [ TestFixture ]
	public class TestJsonSerializer
	{
        [ Test ]
        public void GetByte()
        {
            AssertImport((byte) 123, "123");
        }

        [ Test, ExpectedException(typeof(JsonSerializationException)) ]
        public void ByteOverflow()
        {
            AssertImport((byte) 123, "456");
        }

        [ Test, ExpectedException(typeof(JsonSerializationException)) ]
        public void BadByte()
        {
            AssertImport((byte) 100, "ABC");
        }

        [ Test ]
        public void GetShort()
        {
            AssertImport((short) 456, "456");
        }

        private static void AssertImport(object expected, string input)
        {
            TypeImporterCollection importers = new TypeImporterCollection();
            importers.Add(typeof(byte), TypeImporterStock.Byte);
            importers.Add(typeof(short), TypeImporterStock.Int16);
            JsonTextReader reader = new JsonTextReader(new StringReader(input));
            reader.TypeImporterLocator = importers;
            Type expectedType = expected.GetType();
            object o = reader.Get(expectedType);
            Assert.IsInstanceOfType(expectedType, o);
            Assert.AreEqual(expected, o);
        }

        [ Test ]
	    public void Serialization()
	    {
	        JsonReader reader = new JsonTextReader(new StringReader(@"{
                id       : 1,
                lastName : 'Aziz',
                firstName: 'Atif',
                birthday : '1971-05-17', 
                children : [ {
                        id       : 3,
                        lastName : 'Aziz',
                        firstName: 'Faris',
                        birthday : '2000-12-04'
                    }, {
                        id       : 4,
                        lastName : 'Aziz',
                        firstName: 'Faiz',
                        birthday : '2003-09-14' 
                    }
                ],
                spouce: {
                    id       : 2,
                    lastName : 'Shafiq',
                    firstName: 'Veeda',
                    birthday : '1972-09-28'
                }
            }"));

	        Person person = DeserializePerson(reader);

	        Assert.AreEqual(1, person.Id);
	        Assert.AreEqual("Atif", person.FirstName); 
	        Assert.AreEqual("Aziz", person.LastName);
	        Assert.AreEqual(new DateTime(1971, 5, 17), person.Birthday);
	        
	        Person spouce = person.Spouce;
	        Assert.IsNotNull(spouce);

            Assert.AreEqual(2, spouce.Id);
            Assert.AreEqual("Veeda", spouce.FirstName); 
            Assert.AreEqual("Shafiq", spouce.LastName);
            Assert.AreEqual(new DateTime(1972, 9, 28), spouce.Birthday);
	        
	        Assert.AreEqual(2, person.Children.Count);

            Person child = (Person) person.Children[0];
            Assert.IsNotNull(child);

            Assert.AreEqual(3, child.Id);
            Assert.AreEqual("Faris", child.FirstName); 
            Assert.AreEqual("Aziz", child.LastName);
            Assert.AreEqual(new DateTime(2000, 12, 4), child.Birthday);

            child = (Person) person.Children[1];

	        Assert.AreEqual(4, child.Id);
            Assert.AreEqual("Faiz", child.FirstName); 
            Assert.AreEqual("Aziz", child.LastName);
            Assert.AreEqual(new DateTime(2003, 9, 14), child.Birthday);
        
            JsonTextWriter writer = new JsonTextWriter();
            Serialize(person, writer);
	        Assert.AreEqual("{\"id\":1,\"firstName\":\"Atif\",\"lastName\":\"Aziz\",\"birthday\":\"1971-05-17T00:00:00.0000000+02:00\",\"spouce\":{\"id\":2,\"firstName\":\"Veeda\",\"lastName\":\"Shafiq\",\"birthday\":\"1972-09-28T00:00:00.0000000+02:00\"},\"children\":[{\"id\":3,\"firstName\":\"Faris\",\"lastName\":\"Aziz\",\"birthday\":\"2000-12-04T00:00:00.0000000+01:00\"},{\"id\":4,\"firstName\":\"Faiz\",\"lastName\":\"Aziz\",\"birthday\":\"2003-09-14T00:00:00.0000000+02:00\"}]}", writer.ToString());
        }
        
        private void CompileSerializer(ICustomTypeDescriptor typeModel)
        {
            PropertyDescriptorCollection properties = typeModel.GetProperties();

            foreach (DictionaryEntry entry in properties)
            {
                string name = (string) entry.Key;
                PropertyDescriptor property = (PropertyDescriptor) entry.Value;
                Type propertyType = property.PropertyType;
                AttributeCollection attributes = property.Attributes;
                
            }
        }
        
        private static void Serialize(Person person, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteMember("id");
            writer.WriteNumber(person.Id);
            writer.WriteMember("firstName");
            writer.WriteString(person.FirstName);
            writer.WriteMember("lastName");
            writer.WriteString(person.LastName);
            writer.WriteMember("birthday");
            writer.WriteString(XmlConvert.ToString(person.Birthday));
            if (person.Spouce != null)
            {
                writer.WriteMember("spouce");
                Serialize(person.Spouce, writer);
            }
            if (person.Children != null && person.Children.Count > 0)
            {
                writer.WriteMember("children");
                writer.WriteStartArray();
                foreach (Person child in person.Children)
                    Serialize(child, writer);
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }

        private static Person DeserializePerson(JsonReader reader)
        {
            Person person = new Person();

            reader.MoveToContent();
            
            if (reader.Token != JsonToken.Object)
                throw new Exception("Expecting object.");

            while (reader.ReadToken() != JsonToken.EndObject)
            {
                Debug.Assert(reader.Token == JsonToken.Member);
                
                switch (reader.Text)
                {                    
                    case "id" :
                        person.Id = reader.ReadInt32();
                        break;
                        
                    case "firstName" :
                        person.FirstName = reader.ReadString();
                        break;
                        
                    case "lastName" :
                        person.LastName = reader.ReadString();
                        break;
                        
                    case "birthday" :
                        person.Birthday = XmlConvert.ToDateTime(reader.ReadString());
                        break;
                        
                    case "spouce" :
                        reader.Read();
                        person.Spouce = DeserializePerson(reader);
                        break;
                        
                    case "children":
                        reader.ReadToken(JsonToken.Array);
                        while (reader.ReadToken() != JsonToken.EndArray)
                            person.Children.Add(DeserializePerson(reader));
                        break;
                        
                    default : 
                        
                        int depth = reader.Depth;
                        do reader.Read(); while (reader.Depth > depth);
                        break;
                }
            }
            
            return person;
        }

        [ Serializable ]
        private sealed class Person
        {
            private int _id;
            private string _firstName;
            private string _lastName;
            private DateTime _birthday;
            private Person _spouce;
            private readonly ArrayList _children = new ArrayList(3);

            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }

            public string FirstName
            {
                get { return Mask.NullString(_firstName); }
                set { _firstName = value; }
            }

            public string LastName
            {
                get { return Mask.NullString(_lastName); }
                set { _lastName = value; }
            }

            public DateTime Birthday
            {
                get { return _birthday; }
                set { _birthday = value; }
            }

            public Person Spouce
            {
                get { return _spouce; }
                set { _spouce = value; }
            }

            public IList Children
            {
                get { return _children; }
            }

            public override string ToString()
            {
                return LastName.Length > 0 ? FirstName + " " + LastName : FirstName;
            }
        }
	}
}
