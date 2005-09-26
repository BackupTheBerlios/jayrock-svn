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
    using System.Collections;

    #endregion

    public class CompositeFormatter : JsonFormatter
    {
        private readonly Hashtable _formatters = new Hashtable();
        private readonly Hashtable _lazyFormatters = new Hashtable();

        public void AddFormatter(Type type, IJsonFormatter formatter)
        {
            AddFormatter(type, formatter,  false);
        }

        public virtual void AddFormatter(Type type, IJsonFormatter formatter, bool includeSubclasses)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (formatter == null)
                throw new ArgumentNullException("formatter");

            if (type.IsInterface || type.IsAbstract)
                includeSubclasses = true;

            if (includeSubclasses)
                _lazyFormatters.Add(type, formatter);
            else
                _formatters.Add(type, formatter);
        }

        public virtual void RemoveFormatter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _formatters.Remove(type);
        }

        public virtual IJsonFormatter SelectFormatter(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            IJsonFormatter formatter = (IJsonFormatter) _formatters[type];
            
            if (formatter == null)
            {
                formatter = LazySelectFormatter(type);

                if (formatter != null)
                    AddFormatter(type, formatter);
            }

            return formatter;
        }

        private IJsonFormatter LazySelectFormatter(Type type)
        {
            foreach (DictionaryEntry entry in _lazyFormatters)
            {
                Type wideType = (Type) entry.Key;
                    
                if (wideType.IsAssignableFrom(type))
                    return (IJsonFormatter) entry.Value;
            }

            return null;
        }

        protected override void FormatCustom(object o, JsonWriter writer)
        {
            IJsonFormatter formatter = SelectFormatter(o.GetType());
                
            if (formatter != null)
                formatter.Format(o, writer);
            else
                base.FormatCustom(o, writer);
        }
    }
}