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

    using System.Collections;
    using System.IO;
    using Jayrock.Json.Conversion;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestDelimitedListParser
    {
        [ Test ]
        public void RowToArray()
        {
            Assert.AreEqual(Json("['1','2','3']"), ParseRowToArray("1,2,3"));
        }

        [ Test ]
        public void RowToObject()
        {
            Assert.AreEqual(Json("{x:'1',y:'2',z:'3'}"), ParseRowToObject(new string[] { "x", "y", "z" }, "1,2,3"));
        }

        [ Test ]
        public void TwoRows()
        {
            Assert.AreEqual(Json("[{x:'1',y:'2',z:'3'},{x:'4',y:'5',z:'6'}]"), Parse("x,y,z\n1,2,3\n4,5,6"));
        }

        private static string Json(string s)
        {
            return JsonConvert.Import(s).ToString();
        }

        private static object ParseRowToArray(string s)
        {
            DelimitedListParser parser = new DelimitedListParser();
            return parser.ParseRowToArray(s).ToString();
        }

        private static object ParseRowToObject(string[] names, string s)
        {
            DelimitedListParser parser = new DelimitedListParser();
            return parser.ParseRowToObject(names, s).ToString();
        }

        private static object Parse(string s)
        {
            DelimitedListParser parser = new DelimitedListParser();
            return parser.Parse(s).ToString();
        }
    }
}
