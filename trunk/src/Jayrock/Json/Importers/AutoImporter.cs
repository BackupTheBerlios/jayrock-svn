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

namespace Jayrock.Json.Importers
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Globalization;

    #endregion

    public sealed class AutoImporter : JsonImporter
    {
        public override void Register(IJsonImporterRegistry registry)
        {
            registry.Register(typeof(object), this);
        }

        protected override object SubImport(JsonReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            reader.MoveToContent();
            
            if (reader.TokenClass == JsonTokenClass.String)
            {
                return reader.Text;
            }
            else if (reader.TokenClass == JsonTokenClass.Number)
            {
                return NumberFromString(reader.Text);
            }
            else if (reader.TokenClass == JsonTokenClass.Boolean)
            {
                return reader.Text == JsonReader.TrueText;
            }
            else if (reader.TokenClass == JsonTokenClass.Null)
            {
                return null;
            }
            else if (reader.TokenClass == JsonTokenClass.Array)
            {
                reader.Read();
                JArray values = new JArray();
                
                while (reader.TokenClass != JsonTokenClass.EndArray)
                    values.Add(Import(reader));

                return values;
            }
            else if (reader.TokenClass == JsonTokenClass.Object)
            {
                reader.Read();
                JObject o = new JObject();

                while (reader.TokenClass != JsonTokenClass.EndObject)
                    o.Put(reader.ReadMember(), Import(reader));
                
                return o;
            }
            else 
            {
                throw new JsonException(string.Format("{0} not expected.", reader.TokenClass));
            }
        }

        private static object NumberFromString(string s)
        {
            Debug.Assert(s != null);

            //
            // Try first parsing as a 32-bit integer. If that doesn't work
            // then try as a double. In the worst case, where it cannot
            // fit in a double, the straight text version is returned.
            //

            try 
            {
                return Convert.ToInt32(s, CultureInfo.InvariantCulture);
            } 
            catch (FormatException) { /* ignore */ }
            catch (OverflowException) { /* ignore */ }

            try 
            {
                return Convert.ToInt64(s, CultureInfo.InvariantCulture);
            } 
            catch (FormatException) { /* ignored */ }
            catch (OverflowException) { /* ignored */ }

            try 
            {
                return Convert.ToDouble(s, CultureInfo.InvariantCulture);
            } 
            catch (FormatException e)
            {
                throw new JsonException(e.Message, e);
            }
            catch (OverflowException) { /* ignore */ }
                
            return s;
        }
    }
}