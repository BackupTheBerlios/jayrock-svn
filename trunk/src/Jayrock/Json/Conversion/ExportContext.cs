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

namespace Jayrock.Json.Conversion
{
    #region Imports

    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using Jayrock.Json.Conversion.Exporters;

    #endregion

    [ Serializable ]
    public class ExportContext
    {
        ExporterCollection _exporters;

        public virtual void Export(object value, JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (JsonNull.LogicallyEquals(value))
            {
                writer.WriteNull();
            }
            else
            {
                IExporter exporter = FindExporter(value.GetType());

                if (exporter != null)
                    exporter.Export(this, value, writer);
                else
                    writer.WriteString(value.ToString());
            }
        }

        public virtual void Register(IExporter exporter)
        {
            if (exporter == null)
                throw new ArgumentNullException("exporter");

            Exporters.Put(exporter);
        }

        public virtual IExporter FindExporter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            IExporter exporter = Exporters[type];

            if (exporter != null)
                return exporter;
            
            exporter = FindCompatibleExporter(type);

            if (exporter != null)
            {
                Register(exporter);
                return exporter;
            }
            
            return null;
        }

        private IExporter FindCompatibleExporter(Type type)
        {
            Debug.Assert(type != null);

            if (typeof(IJsonExportable).IsAssignableFrom(type))
                return new ExportAwareExporter(type);

            if (type.IsClass && type != typeof(object))
            {
                IExporter exporter = FindBaseExporter(type.BaseType, type);
                if (exporter != null)
                    return exporter;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
                return new DictionaryExporter(type);

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return new EnumerableExporter(type);
            
            if ((type.IsPublic || type.IsNestedPublic) &&
                !type.IsPrimitive && type.GetConstructor(Type.EmptyTypes) != null)
            {
                return new ComponentExporter(type);
            }

            return new StringExporter(type);
        }

        private IExporter FindBaseExporter(Type baseType, Type actualType)
        {
            Debug.Assert(baseType != null);
            Debug.Assert(actualType != null);

            if (baseType == typeof(object))
                return null;

            IExporter exporter = Exporters[baseType];

            if (exporter == null)
                return FindBaseExporter(baseType.BaseType, actualType);

            return (IExporter) Activator.CreateInstance(exporter.GetType(), new object[] { actualType });
        }
 
        private ExporterCollection Exporters
        {
            get
            {
                if (_exporters == null)
                {
                    ExporterCollection exporters = new ExporterCollection();

                    exporters.Add(new ByteExporter());
                    exporters.Add(new Int16Exporter());
                    exporters.Add(new Int32Exporter());
                    exporters.Add(new Int64Exporter());
                    exporters.Add(new SingleExporter());
                    exporters.Add(new DoubleExporter());
                    exporters.Add(new DecimalExporter());
                    exporters.Add(new StringExporter());
                    exporters.Add(new BooleanExporter());
                    exporters.Add(new DateTimeExporter());
                    exporters.Add(new ByteArrayExporter());
                    exporters.Add(new DataRowViewExporter());
                    exporters.Add(new NameValueCollectionExporter());
                    exporters.Add(new DataSetExporter());
                    exporters.Add(new DataTableExporter());
                    exporters.Add(new DataRowExporter());
                    exporters.Add(new ControlExporter());

                    IList typeList = (IList) ConfigurationSettings.GetConfig("jayrock/json.conversion.exporters");

                    if (typeList != null && typeList.Count > 0)
                    {
                        foreach (Type type in typeList)
                            exporters.Add((IExporter) Activator.CreateInstance(type));
                    }

                    _exporters = exporters;
                }

                return _exporters;
            }
        }
    }
}