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

namespace Jayrock.Json.Rpc
{
    #region Imports

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;

    #endregion

    public class JsonRpcService : Component, IRpcService
    {
        private RpcServiceDescriptor _descriptor;

        public virtual IRpcServiceDescriptor GetDescriptor()
        {
            if (_descriptor == null)
                _descriptor = RpcServiceDescriptor.GetDescriptor(this.GetType());

            return _descriptor;
        }

        /// <remarks>
        /// Provides introspection capabilities. 
        /// See http://scripts.incutio.com/xmlrpc/introspection.html.
        /// </remarks>

        [ JsonRpcMethod("system.listMethods") ]
        [ JsonRpcHelp("Returns an array of method names implemented by this service.") ]
        public virtual string[] SystemListMethods()
        {
            IRpcMethodDescriptor[] methods = GetDescriptor().GetMethods();
            string[] names = new string[methods.Length];
            
            for (int i = 0; i < methods.Length; i++)
                names[i] = methods[i].Name;

            return names;
        }

        [ JsonRpcMethod("system.version") ]
        [ JsonRpcHelp("Returns the version JSON-RPC server implementation using the major, minor, build and revision format.") ]
        public virtual string SystemVersion()
        {
            return typeof(JsonRpcService).Assembly.GetName().Version.ToString();
        }

        [ JsonRpcMethod("system.about") ]
        [ JsonRpcHelp("Returns a summary about the JSON-RPC server implementation for display purposes.") ]
        public virtual string SystemAbout()
        {
            StringWriter writer = new StringWriter();

            //
            // Write out the assembly name and version.
            //
            
            Assembly assembly = typeof(JsonRpcService).Assembly;
            AssemblyName name = assembly.GetName();

            writer.Write(name.Name);
            writer.Write(", ");
            writer.Write(name.Version.ToString());

            //
            // Write out the build configuration, and if available, the 
            // assembly file version.
            //

            writer.Write(" (");

            AssemblyConfigurationAttribute configuration = (AssemblyConfigurationAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyConfigurationAttribute));
            
            Debug.Assert(configuration != null);
            Debug.Assert(Mask.NullString(configuration.Configuration).Length > 0);

            writer.Write(configuration.Configuration);
            writer.Write(" build");

            //
            // Display the file version. Ideally, this could be obtained from
            // FileVersionInfo, but that requires a link demand of full trust
            // that we don't want to require from the application. Using the
            // reflection attribute allows this method to work in partial 
            // trust cases.
            //

            AssemblyFileVersionAttribute version = (AssemblyFileVersionAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));

            if (version != null)
            {
                try
                {
                    string versionDisplay = (new Version(version.Version)).ToString();
                    writer.Write(' ');
                    writer.Write(versionDisplay);
                }
                catch (ArgumentException) { /* version has fewer than two components or more than four components. */ }
                catch (FormatException) { /* At least one component of version does not parse to an integer. */ }
                catch (OverflowException) { /* At least one component of version caused an overflow. */ }
            }

            writer.WriteLine(")");

            //
            // Write out the copyright notice, if available.
            //

            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));

            if (copyright != null && Mask.NullString(copyright.Copyright).Length > 0)
                writer.WriteLine(copyright.Copyright);

            //
            // Write out project home page location.
            //

            writer.WriteLine("For more information, visit http://jayrock.berlios.de/");

            return writer.GetStringBuilder().ToString();
        }
    }
}
