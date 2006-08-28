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
    using System.Collections;
    using System.ComponentModel;
    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestComponentFormatter
    {
        [ Test ]
        public void EmptyObject()
        {
            Assert.AreEqual("\"System.Object\"", Format(new object()));
        }

        [ Test ]
        public void PublicProperties()
        {
            Car car = new Car();            
            car.Manufacturer = "BMW";
            car.Model = "350";
            car.Year = 2000;
            car.Color = "Silver";

            Test(new JsonObject(
                new string[] { "Manufacturer", "Model", "Year", "Color" },
                new object[] { car.Manufacturer, car.Model, car.Year, car.Color }), car);
        }

        [ Test ]
        public void NullPropertiesSkipped()
        {
            Car car = new Car();

            JsonReader reader = FormatForReading(car);
            reader.ReadToken(JsonTokenClass.Object);
            Assert.AreEqual("Year", reader.ReadMember());
            Assert.AreEqual(0, (int) reader.ReadNumber());
            Assert.AreEqual(JsonTokenClass.EndObject, reader.TokenClass);
        }

        [ Test ]
        public void EmbeddedObjects()
        {
            Person snow = new Person();
            snow.Id = 2;
            snow.FullName = "Snow White";

            Person albert = new Person();
            albert.Id = 1;
            albert.FullName = "Albert White";
            albert.Spouce = snow; // NOTE! Cyclic graphs not allowed.

            Test(new JsonObject(
                new string[] { "Id", "FullName", "Spouce" },
                new object[] { albert.Id, albert.FullName, 
                    /* Spouce */ new JsonObject(
                        new string[] { "Id", "FullName" },
                        new object[] { snow.Id, snow.FullName })}), albert);
        }

        [ Test ]
        public void CustomPropertiesInternally()
        {
            Point point = new Point(123, 456);
            JsonTextWriter writer = new JsonTextWriter();
            writer.WriteValue(point);
            Assert.AreEqual("{\"X\":123,\"Y\":456}", writer.ToString());
        }

        [ Test ]
        public void TypeSpecific()
        {
            Person john = new Person();
            john.Id = 123;
            john.FullName = "John Doe";
            
            Car beamer = new Car();            
            beamer.Manufacturer = "BMW";
            beamer.Model = "350";
            beamer.Year = 2000;
            beamer.Color = "Silver";

            OwnerCars johnCars = new OwnerCars();
            johnCars.Owner = john;
            johnCars.Cars.Add(beamer);

            JsonObject test = new JsonObject(
                new string[] { "Owner", "Cars" }, 
                new object[] {
                    /* Owner */ new JsonObject(
                        new string[] { "Id", "FullName" }, 
                        new object[] { john.Id,  john.FullName }),
                    /* Cars */ new object[] {
                        new JsonObject(
                            new string[] { "Manufacturer", "Model", "Year", "Color" }, 
                            new object[] { beamer.Manufacturer, beamer.Model, beamer.Year, beamer.Color })
                    }
                });

            Test(test, johnCars);
        }

        private static string Format(object o)
        {
            CompositeFormatter compositeFormatter = new CompositeFormatter();
            compositeFormatter.AddFormatter(typeof(Car), new ComponentFormatter());
            compositeFormatter.AddFormatter(typeof(Person), new ComponentFormatter());
            compositeFormatter.AddFormatter(typeof(OwnerCars), new ComponentFormatter());

            JsonTextWriter writer = new JsonTextWriter();
            writer.ValueFormatter = compositeFormatter;

            writer.WriteValue(o);
            return writer.ToString();
        }

        private static JsonReader FormatForReading(object o)
        {
            return new JsonTextReader(new StringReader(Format(o)));
        }

        private static void Test(JsonObject expected, object actual)
        {
            JsonReader reader = FormatForReading(actual);
            TestObject(expected, reader, "(root)");
            Assert.IsFalse(reader.Read(), "Expected EOF.");
        }

        private static void TestObject(JsonObject expected, JsonReader reader, string path)
        {
            reader.MoveToContent();
            reader.ReadToken(JsonTokenClass.Object);
            
            while (reader.TokenClass != JsonTokenClass.EndObject)
            {
                string name = reader.ReadMember();
                object value = expected[name];
                expected.Remove(name);
                TestValue(value, reader, path + "/" + name);
            }
            
            Assert.AreEqual(0, expected.Count);
            reader.Read();
        }

        private static void TestArray(Array expectations, JsonReader reader, string path)
        {
            reader.MoveToContent();
            reader.ReadToken(JsonTokenClass.Array);

            for (int i = 0; i < expectations.Length; i++)
                TestValue(expectations.GetValue(i), reader, path + "/" + i);

            Assert.AreEqual(JsonTokenClass.EndArray, reader.TokenClass);
            reader.Read();
        }

        private static void TestValue(object expected, JsonReader reader, string path)
        {
            if (JsonNull.LogicallyEquals(expected))
            {
                Assert.AreEqual(JsonTokenClass.Null, reader.TokenClass, path);
            }
            else
            {
                TypeCode expectedType = Type.GetTypeCode(expected.GetType());

                if (expectedType == TypeCode.Object)
                {
                    if (expected.GetType().IsArray)
                        TestArray((Array) expected, reader, path);
                    else
                        TestObject((JsonObject) expected, reader, path);
                }
                else
                {
                    switch (expectedType)
                    {
                        case TypeCode.String : Assert.AreEqual(expected, reader.ReadString(), path); break;
                        case TypeCode.Int32  : Assert.AreEqual(expected, (int) reader.ReadNumber(), path); break;
                        default : Assert.Fail("Don't know how to handle {0} values.", expected.GetType()); break;
                    }
                }
            }
        }

        private sealed class Car
        {
            private string _manufacturer;
            private string _model;
            private int _year;
            private string _color;

            public string Manufacturer
            {
                get { return _manufacturer; }
                set { _manufacturer = value; }
            }

            public string Model
            {
                get { return _model; }
                set { _model = value; }
            }

            public int Year
            {
                get { return _year; }
                set { _year = value; }
            }

            public string Color
            {
                get { return _color; }
                set { _color = value; }
            }
        }

        private sealed class Person
        {
            private int _id;
            private string _fullName;
            private Person _spouce;

            public int Id
            {
                get { return _id; }
                set { _id = value; }
            }

            public string FullName
            {
                get { return _fullName; }
                set { _fullName = value; }
            }

            public Person Spouce
            {
                get { return _spouce; }
                set { _spouce = value; }
            }
        }

        private sealed class OwnerCars
        {
            private Person _owner;
            private ArrayList _cars = new ArrayList();

            public Person Owner
            {
                get { return _owner; }
                set { _owner = value; }
            }

            public ArrayList Cars
            {
                get { return _cars; }
            }
        }

        private struct  Point : IJsonFormattable
        {
            private int _x;
            private int _y;

            private static readonly PropertyDescriptorCollection _properties;

            static Point()
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Point));
                _properties = new PropertyDescriptorCollection(null);
                _properties.Add(properties["X"]);
                _properties.Add(properties["Y"]);
            }

            public Point(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public int X { get { return _x; } }
            public int Y { get { return _y; } }
            public string XYString { get { return X + "," + Y; } }

            public void Format(JsonWriter writer)
            {
                ComponentFormatter formatter = new ComponentFormatter(_properties);
                formatter.Format(this, writer);
            }
        }
    }
}
