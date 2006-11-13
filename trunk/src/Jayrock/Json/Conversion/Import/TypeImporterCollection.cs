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

namespace Jayrock.Json.Conversion.Import
{
    #region Imports
    
    using System;
    using System.Collections;

    #endregion
    
    // FIXME: Turn TypeImporterCollection into a real keyed-collection.

    [ Serializable ]
    public sealed class TypeImporterCollection : ITypeImporterBinder 
    {
        private Hashtable _importerByType;
        
        public void Add(ITypeImporter importer)
        {
            if (importer == null)
                throw new ArgumentNullException("importer");
            
            ImporterByType.Add(importer.OutputType, importer);
        }

        public ITypeImporter Bind(ImportContext context, Type type)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            if (type == null)
                throw new ArgumentNullException("type");
            
            return (ITypeImporter) ImporterByType[type];
        }

        private Hashtable ImporterByType
        {
            get
            {
                if (_importerByType == null)
                    _importerByType = new Hashtable();
                
                return _importerByType;
            }
        }
    }
}