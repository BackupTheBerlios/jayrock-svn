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
    /// Provides data for events using <see cref="ObjectInitializationEventHandler"/>.
    /// </summary>

    [ Serializable ]
    public sealed class ObjectInitializationEventArgs : EventArgs
    {
        private readonly object _obj;

        public ObjectInitializationEventArgs(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            
            _obj = obj;
        }

        public object Object
        {
            get { return _obj; }
        }
    }
    
    /// <summary>
    /// Represent a method that participates in object initialization.
    /// </summary>

    public delegate void ObjectInitializationEventHandler(object sender, ObjectInitializationEventArgs args);
}