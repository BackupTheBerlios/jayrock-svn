#region License, Terms and Conditions
//
// JayRock - A JSON-RPC implementation for the Microsoft .NET Framework
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

namespace JayRock
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    /// <summary>
    /// Helper methods to read custom attributes given a custom attribute
    /// provider.
    /// </summary>

    internal sealed class CustomAttribute
    {
        public static object Get(ICustomAttributeProvider provider, Type attributeType)
        {
            return Get(provider, attributeType, true);
        }

        public static object Get(ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            Debug.Assert(provider != null);

            object[] attributes = provider.GetCustomAttributes(attributeType, inherit);

            if (attributes.Length == 0)
                return null;

            if (attributes.Length > 1)
                throw new AmbiguousMatchException();

            return attributes[0];
        }

        public static object[] GetArray(ICustomAttributeProvider provider, Type attributeType)
        {
            return GetArray(provider, attributeType, true);
        }

        public static object[] GetArray(ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            Debug.Assert(provider != null);

            return provider.GetCustomAttributes(attributeType, inherit);
        }

        public static bool IsDefined(ICustomAttributeProvider provider, Type attributeType)
        {
            return provider.IsDefined(attributeType, true);
        }

        public static bool IsDefined(ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            Debug.Assert(provider != null);

            return IsDefined(provider, attributeType);
        }
        
        private CustomAttribute()
        {
            throw new NotSupportedException();
        }
    }
}
