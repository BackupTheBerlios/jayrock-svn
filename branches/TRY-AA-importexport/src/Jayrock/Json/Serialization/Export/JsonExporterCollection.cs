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

namespace Jayrock.Json.Serialization.Export
{
    #region Imports

    using System;
    using System.Collections;

    #endregion

    public sealed class JsonExporterCollection : JsonTraderCollection, IJsonFormatter // FIXME: Remove
    {
        public IJsonExporter Find(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            return (IJsonExporter) BaseFind(type);
        }

        public void Register(IJsonExporter exporter)
        {
            if (exporter == null)
                throw new ArgumentNullException("exporter");
            
            Register(exporter.InputType, exporter);
        }

        public void Register(IJsonExporterFamily family)
        {
            if (family == null)
                throw new ArgumentNullException("family");
            
            RegisterFamily(family);
        }

        protected override object Page(object family, Type type)
        {
            return ((IJsonExporterFamily) family).Page(type);
        }

        public void Format(object o, JsonWriter writer)
        {
            if (o == null)
                writer.WriteNull();
            else
                Find(o.GetType()).Export(null, o);
        }
    }
}