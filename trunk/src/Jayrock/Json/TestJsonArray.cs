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
    #region Imports

    using System.IO;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestJsonArray
    {
        [ Test ]
        public void AddNullValue()
        {
            JsonArray a = new JsonArray();
            a.Add(null);
            Assert.AreEqual(1, a.Count);
            Assert.IsNull(a[0]);
        }

        [ Test ]
        public void Import()
        {
            JsonArray a = new JsonArray();
            a.Import(new JsonTextReader(new StringReader("[123,'Hello World',true]")));
            Assert.AreEqual(3, a.Length);
            Assert.AreEqual(123, (int) (JsonNumber) a[0]);
            Assert.AreEqual("Hello World", a[1]);
            Assert.AreEqual(true, a[2]);
        }

        [ Test ]
        public void ContentsClearedBeforeImporting()
        {
            JsonArray a = new JsonArray();
            a.Add(new object());
            Assert.AreEqual(1, a.Length);
            a.Import(new JsonTextReader(new StringReader("[123]")));
            Assert.AreEqual(1, a.Length);
        }
        
        [ Test ]
        public void ImportIsExceptionSafe()
        {
            JsonArray a = new JsonArray();
            object o = new object();
            a.Add(o);
            
            try
            {
                a.Import(new JsonTextReader(new StringReader("[123,456,")));
            }
            catch (JsonException)
            {
            }
            
            Assert.AreEqual(1, a.Count);
            Assert.AreSame(o, a[0]);
        }
    }
}
