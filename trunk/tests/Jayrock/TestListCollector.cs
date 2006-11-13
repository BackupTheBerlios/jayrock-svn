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

namespace Jayrock
{
    #region Imports

    using System.Collections;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestListCollector
    {
        [ Test ]       
        public void Initialization()
        {
            ArrayList list = new ArrayList();
            ListCollector collector = new ListCollector(list);
            Assert.AreSame(list, collector.List);
        }

        [ Test ]       
        public void Collection()
        {
            ArrayList list = new ArrayList();
            ListCollector collector = new ListCollector(list);
            object o1 = new object();
            object o2 = new object();
            collector.Collect(o1);
            collector.Collect(o2);
            Assert.AreSame(o1, list[0]);
            Assert.AreSame(o2, list[1]);
        }

        [ Test ]       
        public void EmptyInitialization()
        {
            ListCollector collector = new ListCollector();
            Assert.IsNotNull(collector.List);
        }
    }
}