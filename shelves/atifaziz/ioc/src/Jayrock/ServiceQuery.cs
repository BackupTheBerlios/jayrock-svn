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
    using System;

    public sealed class ServiceQuery
    {
        public static object FindByName(NamedServiceContainer container, string name)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            return container.Get(name);
        }

        public static object GetByName(NamedServiceContainer container, string name)
        {
            object service = FindByName(container, name);

            if (service == null)
                ServiceNotAvailableException.ThrowFormatted(name);

            return service;
        }

        public static object FindByType(IServiceProvider serviceProvider, Type serviceType)
        {
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            return serviceProvider.GetService(serviceType);
        }

        public static object GetByType(IServiceProvider serviceProvider, Type serviceType)
        {
            object service = FindByType(serviceProvider, serviceType);

            if (service == null)
                ServiceNotAvailableException.ThrowFormatted(serviceType);

            return service;
        }

        private ServiceQuery()
        {
            throw new NotImplementedException();
        }
    }
}