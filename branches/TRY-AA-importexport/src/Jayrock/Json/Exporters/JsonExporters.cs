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

namespace Jayrock.Json.Exporters
{
    #region Imports

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    internal sealed class JsonExporters
    {
        private static JsonExporterCollection _exporters;
        private static object _lock = new object();
        
        public static IJsonExporter Find(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            FaultExporters();
            return _exporters.Find(type);
        }

        private static void FaultExporters()
        {
            if (_exporters != null)
                return;
            
            lock (_lock)
            {
                if (_exporters != null)
                    return;
                
                ICollection exporters = null;

                /*
                Configuration config = Configuration.Load();
                
                if (config != null)
                    exporters = config.Exporters;*/

                if (exporters != null && exporters.Count > 0)
                {
                    exporters = EnsureObjects(exporters);
                }
                else
                {
                    exporters = new object[]
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
                    
                int index = 0;
                JsonExporterCollection actualExporter = new JsonExporterCollection();
                
                foreach (object item in exporters)
                {
                    if (item == null)
                        continue;
                    
                    if (!Register(actualExporter, item))
                        throw new ConfigurationException(string.Format("Item at index {0} of JSON exporters list is not a valid JSON exporter type (expected {1} or {2} when actually found {3}).", index.ToString(CultureInfo.InvariantCulture), typeof(IJsonExporter).FullName, typeof(IJsonExporterFamily).FullName, item.GetType().FullName));

                    index++;
                }
                
                _exporters = actualExporter;
            }
        }

        private static IList EnsureObjects(ICollection typeSpecs)
        {
            Debug.Assert(typeSpecs != null);
            
            ArrayList objectList = new ArrayList(typeSpecs.Count);
            
            foreach (string typeSpec in typeSpecs)
            {
                if (Mask.NullString(typeSpec).Length == 0)
                    continue;

                objectList.Add(Activator.CreateInstance(Compat.GetType(typeSpec)));
            }
            
            return objectList;
        }

        private static bool Register(JsonExporterCollection exporters, object item)
        {
            Debug.Assert(exporters != null);
            Debug.Assert(item != null);
            
            IJsonExporter exporter = item as IJsonExporter;
                    
            if (exporter == null)
            {
                IJsonExporterFamily family = item as IJsonExporterFamily;
                        
                if (family != null)
                    exporters.Register(family);
                else
                    return false;
            }
            else
            {
                exporters.Register(exporter);
            }
            
            return true;
        }

        private JsonExporters()
        {
            throw new NotSupportedException();
        }
    }
}