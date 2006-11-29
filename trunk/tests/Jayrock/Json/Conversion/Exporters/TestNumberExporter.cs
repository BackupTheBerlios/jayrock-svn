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

namespace Jayrock.Json.Conversion.Exporters
{
    #region Imports

    using System;
    using NUnit.Framework;

    #endregion

    public abstract class TestNumberExporter
    {
        [ Test ]
        public void InputType()
        {
            Assert.AreSame(SampleValue.GetType(), CreateExporter().InputType);
        }

        [ Test ]
        public void ExportNull()
        {
            JsonRecorder writer = new JsonRecorder();
            CreateExporter().Export(new ExportContext(), null, writer);
            writer.CreatePlayer().ReadNull();
        }

        [ Test ]
        public void ExportNumber()
        {
            JsonRecorder writer = new JsonRecorder();
            object sample = SampleValue;
            CreateExporter().Export(new ExportContext(), sample, writer);
            object actual = Convert.ChangeType(writer.CreatePlayer().ReadNumber(), sample.GetType());
            Assert.IsInstanceOfType(sample.GetType(), actual);
            Assert.AreEqual(sample, actual);
        }

        protected abstract object SampleValue { get; }
        protected abstract ITypeExporter CreateExporter();
    }

    [ TestFixture ]
    public class TestByteExporter : TestNumberExporter
    {
        protected override object SampleValue
        {
            get { return (byte) 123; }
        }

        protected override ITypeExporter CreateExporter()
        {
            return new ByteExporter();
        }
    }

    [ TestFixture ]
    public class TestInt16Exporter : TestNumberExporter
    {
        protected override object SampleValue
        {
            get { return (short) 1234; }
        }

        protected override ITypeExporter CreateExporter()
        {
            return new Int16Exporter();
        }
    }

    [ TestFixture ]
    public class TestInt32Exporter : TestNumberExporter
    {
        protected override object SampleValue
        {
            get { return 123456; }
        }

        protected override ITypeExporter CreateExporter()
        {
            return new Int32Exporter();
        }
    }

    [ TestFixture ]
    public class TestInt64Exporter : TestNumberExporter
    {
        protected override object SampleValue
        {
            get { return 9876543210L; }
        }

        protected override ITypeExporter CreateExporter()
        {
            return new Int64Exporter();
        }
    }
    
    [ TestFixture ]
    public class TestSingleExporter : TestNumberExporter
    {
        protected override object SampleValue
        {
            get { return 12.345f; }
        }

        protected override ITypeExporter CreateExporter()
        {
            return new SingleExporter();
        }
    }

    [ TestFixture ]
    public class TestDoubleExporter : TestNumberExporter
    {
        protected override object SampleValue
        {
            get { return 12.345m; }
        }

        protected override ITypeExporter CreateExporter()
        {
            return new DecimalExporter();
        }
    }

    /*
    [ TestFixture ]
    public class TestNumberExporter
    {
        [ Test ]
        public void Superclass()
        {
            Assert.IsInstanceOfType(typeof(JsonExporterBase), NumberExporter.Get(typeof(byte)));    
        }

        [ Test ]
        public void ExportByte()
        {
            Assert.AreEqual(123, Export((byte) 123).ReadNumber().ToByte());
        }

        [ Test ]
        public void ExportInt16()
        {
            Assert.AreEqual(1234, Export((short) 1234).ReadNumber().ToInt16());
        }

        [ Test ]
        public void ExportInt32()
        {
            Assert.AreEqual(123456, Export(123456).ReadNumber().ToInt32());
        }

        [ Test ]
        public void ExportInt64()
        {
            Assert.AreEqual(9876543210L, Export(9876543210L).ReadNumber().ToInt64());
        }

        [ Test ]
        public void ExportSingle()
        {
            Assert.AreEqual(12.345f, Export(12.345f).ReadNumber().ToSingle());
        }

        [ Test ]
        public void ExportDouble()
        {
            Assert.AreEqual(12.345e123, Export(12.345e123).ReadNumber().ToDouble());
        }

        [ Test ]
        public void ExportDecimal()
        {
            Assert.AreEqual(12.345m, Export(12.345m).ReadNumber().ToDecimal());
        }

        private static JsonReader Export(object value)
        {
            JsonRecorder writer = new JsonRecorder();
            NumberExporter.Get(value.GetType()).Export(value, writer);
            return writer.CreatePlayer();
        }
    }
    */
}