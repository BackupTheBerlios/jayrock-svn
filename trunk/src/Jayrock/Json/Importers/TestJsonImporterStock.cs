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
    using System.Collections;
    using System.Threading;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonImporterStock
    {
        [ Test ]
        public void InStock()
        {
            AssertInStock(typeof(NumberImporter), typeof(byte));
            AssertInStock(typeof(NumberImporter), typeof(short));
            AssertInStock(typeof(NumberImporter), typeof(int));
            AssertInStock(typeof(NumberImporter), typeof(long));
            AssertInStock(typeof(NumberImporter), typeof(float));
            AssertInStock(typeof(NumberImporter), typeof(double));
            AssertInStock(typeof(DateTimeImporter), typeof(DateTime));
            AssertInStock(typeof(StringImporter), typeof(string));
            AssertInStock(typeof(BooleanImporter), typeof(bool));
            AssertInStock(typeof(AutoImporter), typeof(object));
            AssertInStock(typeof(ArrayImporter), typeof(object[]));
            AssertInStock(typeof(EnumImporter), typeof(System.Globalization.UnicodeCategory));
            AssertInStock(typeof(ImportableImporter), typeof(JsonObject));
            AssertInStock(typeof(ImportableImporter), typeof(IDictionary));
            AssertInStock(typeof(ImportableImporter), typeof(JsonArray));
            AssertInStock(typeof(ImportableImporter), typeof(IList));
            AssertInStock(typeof(ImportableImporter), typeof(ImportableThing));
        }

        [ Test, ExpectedException(typeof(JsonException)) ]
        public void CannotGetUnknown()
        {
            JsonImporterStock.Get(typeof(Enum));
        }

        [ Test ]
        public void AlwaysHasRegistry()
        {
            Assert.IsNotNull(JsonImporterStock.Registry);
        }

        [ Test, ExpectedException(typeof(ArgumentNullException)) ]
        public void CannotSetRegistryToNull()
        {
            JsonImporterStock.SetRegistry(null);
        }

        [ Test ]
        public void SetRegistry()
        {
            IJsonImporterRegistry oldRegistry = JsonImporterStock.Registry;
            
            try
            {
                JsonImporterRegistry newRegistry = new JsonImporterRegistry();
                JsonImporterStock.SetRegistry(newRegistry);
                Assert.AreSame(newRegistry, JsonImporterStock.Registry);
            }
            finally
            {
                // 
                // IMPORTANT! Do not remove, otherwise test elsewhere that
                // default on the default registry could fail.
                //
                
                JsonImporterStock.SetRegistry(oldRegistry);
            }
        }

        [ Test ]
        public void RegistryInitialization()
        {
            IJsonImporterRegistry oldRegistry = JsonImporterStock.Registry;
            
            try
            {
                TestEventNet net = new TestEventNet();
                JsonImporterStock.RegistryInitializing += new ObjectInitializationEventHandler(net.ObjectInitialization);
                JsonImporterRegistry newRegistry = new JsonImporterRegistry();
                JsonImporterStock.SetRegistry(newRegistry, true);
                Assert.IsTrue(net.HasEvent);
                Assert.AreSame(typeof(JsonImporterRegistry), net.EventSender);
                Assert.AreSame(newRegistry, ((ObjectInitializationEventArgs) net.EventArgs).Object);
            }
            finally
            {
                // 
                // IMPORTANT! Do not remove, otherwise test elsewhere that
                // default on the default registry could fail.
                //
                
                JsonImporterStock.SetRegistry(oldRegistry);
            }
        }

        [ Test ]
        public void RegistryIsPerThread()
        {
            ThreadTester test1 = new ThreadTester();
            test1.Run(new ThreadStart(test1.GetDefault));

            ThreadTester test2 = new ThreadTester();
            test2.Run(new ThreadStart(test2.GetDefault));
            
            Assert.AreNotSame(test1.Default, test2.Default);
        }

        [ Test ]
        public void FirstRegistryAccessRaisesInitializationEvent()
        {
            ThreadTester tester = new ThreadTester();
            tester.Run(new ThreadStart(tester.GetDefaultInitializationEvent));
            Assert.IsNotNull(tester.EventNet);
            Assert.IsTrue(tester.EventNet.HasEvent);
        }

        private class ThreadTester
        {
            public IJsonImporterRegistry Default;
            public TestEventNet EventNet;
            
            public void GetDefault()
            {
                Default = JsonImporterStock.Registry;
            }
            
            public void GetDefaultInitializationEvent()
            {
                EventNet = new TestEventNet();
                JsonImporterStock.RegistryInitializing += new ObjectInitializationEventHandler(EventNet.ObjectInitialization);
                GetDefault();
            }
            
            public void Run(ThreadStart test)
            {
                Thread thread = new Thread(new ThreadStart(test));
                thread.Start();
                thread.Join();
            }
        }

        private sealed class TestEventNet
        {
            public bool HasEvent;
            public object EventSender;
            public EventArgs EventArgs;

            public void ObjectInitialization(object sender, ObjectInitializationEventArgs args)
            {
                HasEvent = true;
                EventSender = sender;
                EventArgs = args;
            }
        }

        private static void AssertInStock(Type expected, Type type)
        {
            IJsonImporter importer = JsonImporterStock.Find(type);
            Assert.IsNotNull(importer , "{0} not in stock.", type.FullName);
            Assert.IsInstanceOfType(expected, importer, type.FullName);
        }

        private sealed class ImportableThing : IJsonImportable
        {
            public void Import(JsonReader reader)
            {
                throw new NotImplementedException();
            }
        }
    }
}