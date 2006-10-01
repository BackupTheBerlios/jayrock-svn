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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Collections;
    using System.Diagnostics;
    using Jayrock.Json.Importers;

    #endregion
    
    public sealed class JsonImporterRegistry : IJsonImporterRegistry
    {        
        private readonly Hashtable _importerByType = new Hashtable();
        private ArrayList _importerSetList;
        
        public IJsonImporter Find(Type type)
        {
            //
            // First look up in the table of specific type registrations.
            //
            
            IJsonImporter importer = (IJsonImporter) _importerByType[type];
            
            if (importer != null)
                return importer;

            //
            // No importer found using an exact matching type, so we ask
            // the set of "chained" locators to find one.
            //
        
            foreach (IJsonImporterSet importerSet in ImporterSets)
            {
                importer = importerSet.Page(type);
            
                if (importer != null)
                    break;
            }
            
            //
            // If an import was found, then register it so that it we don't
            // have to go through the same trouble of locating it again.
            //
            
            if (importer != null)
                Register(importer);
            
            return importer;
        }

        public void Register(IJsonImporter importer)
        {
            if (importer == null)
                throw new ArgumentNullException("importer");
            
            _importerByType[importer.OutputType] = importer;
        }

        public void Register(IJsonImporterSet set)
        {
            if (set == null)
                throw new ArgumentNullException("set");
            
            ImporterSets.Add(set);
        }
        
        private ArrayList ImporterSets
        {
            get
            {
                if (_importerSetList == null)
                    _importerSetList = new ArrayList(4);
                
                return _importerSetList;
            }
        }
    }
}