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

namespace Jayrock.Json.Formatters
{
    #region Imports

    using System;
    using System.Data;
    using System.Diagnostics;

    #endregion

    public class DataSetFormatter : JsonFormatter
    {
        public override void Format(object o, JsonWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            DataSet dataSet = o as DataSet;

            if (dataSet != null)
                FormatDataSet(dataSet, writer);
            else
                base.Format(o, writer);
        }

        private static void FormatDataSet(DataSet dataSet, JsonWriter writer)
        {
            writer.WriteStartObject();
    
            foreach (DataTable table in dataSet.Tables)
            {
                writer.WriteMember(table.TableName);
                DataTableFormatter.FormatTable(table, writer);
            }
    
            writer.WriteEndObject();
        }
    }
}