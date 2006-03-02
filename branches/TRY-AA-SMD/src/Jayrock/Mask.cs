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

	using System;

	#endregion

    /// <summary>
    /// Provides masking services where one value masks another given a test.
    /// </summary>

    internal sealed class Mask
    {
        public static string NullString(string actual) 
        {
            return IfStringEquals(actual, null, string.Empty);
        }

        public static string EmptyString(string actual, string onEmpty) 
        {
            return Mask.NullString(actual).Length == 0 ? onEmpty : actual;
        }

        public static string IfStringEquals(string actual, string test, string newValue)
        {
            return string.Equals(actual, test) ? newValue : actual;
        }

        private Mask()
        {
            throw new NotSupportedException();
        }
    }
}
