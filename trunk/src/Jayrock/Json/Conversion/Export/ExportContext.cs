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
    using System.Threading;
    using Jayrock.Json.Conversion.Export.Exporters;

    #endregion

    [ Serializable ]
    public class ExportContext
    {
        private ITypeExporterBinder _exporterBinder;

        private static object _defaultExporterBinder;
        
        public ITypeExporterBinder ExporterBinder
        {
            get
            {
                if (_exporterBinder == null)
                    _exporterBinder = DefaultExporterBinder;
                
                return _exporterBinder;
            }
            
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                
                _exporterBinder = value;
            }
        }

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
                Type valueType = value.GetType();
                ITypeExporter exporter = ExporterBinder.Bind(this, valueType);

                if (exporter != null)
                    exporter.Export(this, value, writer);
                else 
                    writer.WriteString(value.ToString());
            }
        }

        private static ITypeExporterBinder DefaultExporterBinder
        {
            get
            {
                if (_defaultExporterBinder == null)
                {
                    TypeExporterCollection bindings = new TypeExporterCollection();
                    
                    bindings.Add(new ByteExporter());
                    bindings.Add(new Int16Exporter());
                    bindings.Add(new Int32Exporter());
                    bindings.Add(new Int64Exporter());
                    bindings.Add(new SingleExporter());
                    bindings.Add(new DoubleExporter());
                    bindings.Add(new DecimalExporter());
                    bindings.Add(new StringExporter());
                    bindings.Add(new BooleanExporter());
                    bindings.Add(new DateTimeExporter());
                    bindings.Add(new DataRowViewExporter());
                    
                    TypeExporterBinderCollection binders = new TypeExporterBinderCollection();
                    
                    binders.Add(bindings);
                    binders.Add(new ExportAwareExporterFamily());
                    binders.Add(new NameValueCollectionExporterFamily());
                    binders.Add(new DataSetExporterFamily());
                    binders.Add(new DataTableExporterFamily());
                    binders.Add(new DataRowExporterFamily());
                    binders.Add(new ControlExporterFamily());
                    binders.Add(new DictionaryExporterFamily());
                    binders.Add(new EnumerableExporterFamily());
                    binders.Add(new ComponentExporterFamily());

                    Interlocked.CompareExchange(ref _defaultExporterBinder, binders, null);
                }
                
                return (ITypeExporterBinder) _defaultExporterBinder;
            }
        }
    }
}