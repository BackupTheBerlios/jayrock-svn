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

namespace Jayrock.Json.Conversion.Export
{
    #region Imports

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using Jayrock.Json.Conversion.Export.Exporters;

    #endregion

    [ Serializable ]
    public sealed class JsonExporterCollection : JsonConverterCollection
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

        protected override ICollection GetDefaultConfiguration()
        {
            return new object[]
            {
                new ByteExporter(),
                new Int16Exporter(),
                new Int32Exporter(),
                new Int64Exporter(),
                new SingleExporter(),
                new DoubleExporter(),
                new DecimalExporter(),
                new StringExporter(),
                new BooleanExporter(),
                new DateTimeExporter(),
                
                new ExportAwareExporterFamily(),
                new NameValueCollectionExporterFamily(),
                new DictionaryExporterFamily(),
                new EnumerableExporterFamily(),
                new ComponentExporterFamily()
            };
        }

        protected override void Register(object item)
        {
            if (item == null)
                throw new ArgumentNullException("item");
            
            IJsonExporter exporter = item as IJsonExporter;
                    
            if (exporter != null)
            {
                Register(exporter);
            }
            else
            {
                IJsonExporterFamily family = item as IJsonExporterFamily;
                        
                if (family == null)
                    throw new ArgumentException(string.Format("The type {0} is not a valid JSON exporter. Expected {1} or {2}.", typeof(IJsonExporter).FullName, typeof(IJsonExporterFamily).FullName, item.GetType().FullName));
                
                Register(family);
            }
        }
    }
}