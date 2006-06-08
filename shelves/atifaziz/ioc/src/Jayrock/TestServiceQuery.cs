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

    using System.ComponentModel.Design;
    using NUnit.Framework;

    #endregion

    [ TestFixture ]
    public class TestServiceQuery
    {
        [ Test ]
        public void FindUnavailableByName()
        {
            NamedServiceContainer container = new NamedServiceContainer();
            Assert.IsNull(ServiceQuery.FindByName(container, "Unknown"));
        }

        [ Test ]
        public void FindByName()
        {
            NamedServiceContainer container = new NamedServiceContainer();
            object service = new object();
            container.Add("test", service);
            Assert.AreSame(service, ServiceQuery.FindByName(container, "test"));
        }

        [ Test, ExpectedException(typeof(ServiceNotAvailableException)) ]
        public void GetUnavailableByName()
        {
            NamedServiceContainer container = new NamedServiceContainer();
            ServiceQuery.GetByName(container, "Unknown");
        }
    
        [ Test ]
        public void GetByName()
        {
            NamedServiceContainer container = new NamedServiceContainer();
            object service = new object();
            container.Add("test", service);
            Assert.AreSame(service, ServiceQuery.GetByName(container, "test"));
        }
 
        [ Test ]
        public void FindUnavailableByType()
        {
            ServiceContainer container = new ServiceContainer();
            Assert.IsNull(ServiceQuery.FindByType(container, typeof(object)));
        }

        [ Test ]
        public void FindByType()
        {
            ServiceContainer container = new ServiceContainer();
            object service = new object();
            container.AddService(typeof(object), service);
            Assert.AreSame(service, ServiceQuery.FindByType(container, typeof(object)));
        }

        [ Test, ExpectedException(typeof(ServiceNotAvailableException)) ]
        public void GetUnavailableByType()
        {
            ServiceContainer container = new ServiceContainer();
            ServiceQuery.GetByType(container, typeof(object));
        }
    
        [ Test ]
        public void GetByType()
        {
            ServiceContainer container = new ServiceContainer();
            object service = new object();
            container.AddService(typeof(object), service);
            Assert.AreSame(service, ServiceQuery.GetByType(container, typeof(object)));
        }
    }
}