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
            Assert.AreEqual("{\"Model\":\"350\",\"Year\":2000,\"Manufacturer\":\"BMW\",\"Color\":\"Silver\"}", Format(car));
        }

        [ Test ]
        public void NullPropertiesSkipped()
        {
            Car car = new Car();
            Assert.AreEqual("{\"Year\":0}", Format(car));
        }


        [ Test ]
        public void EmbeddedObjects()
        {
            Person albert = new Person();            
            albert.Id = 1;
            albert.FullName = "Albert White";
            Person snow = new Person();
            snow.Id = 2;
            snow.FullName = "Snow White";
            albert.Spouce = snow; // NOTE! Cyclic graphs not allowed.
            Assert.AreEqual("{\"Id\":1,\"FullName\":\"Albert White\",\"Spouce\":{\"Id\":2,\"FullName\":\"Snow White\"}}", Format(albert));
        }

        private static string Format(object o)
        {
            ComponentFormatter componentFormatter = new ComponentFormatter();

            CompositeFormatter compositeFormatter = new CompositeFormatter();
            compositeFormatter.AddFormatter(typeof(Car), componentFormatter);
            compositeFormatter.AddFormatter(typeof(Person), componentFormatter);

            JsonTextWriter writer = new JsonTextWriter();
            writer.ValueFormatter = compositeFormatter;

            writer.WriteValue(o);
            return writer.ToString();
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
    }
}