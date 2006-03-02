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

namespace Jayrock.Json.Rpc.Web
{
    #region Imports

    using System;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.IO;
    using System.Web;

    #endregion

    /// <remarks>
    /// See <a href="http://dojo.jot.com/SMD">Simple Method Description (SMD)</a> 
    /// for more information.
    /// </remarks>

    public sealed class JsonRpcSimpleMethodDescription : JsonRpcServiceFeature
    {
        private DateTime _lastModifiedTime;
        private bool _lastModifiedTimeInitialized;

        protected override void ProcessRequest()
        {
            if (!Modified())
            {
                Response.StatusCode = 304;
                return;
            }

            //
            // Generate the SMD object graph.
            //

            IRpcServiceDescriptor service = TargetService.GetDescriptor();

            JObject smd = new JObject();
            
            smd.Put("SMDVersion", ".1");
            smd.Put("objectName", JsonRpcServices.GetServiceName(TargetService));
            smd.Put("serviceType", "JSON-RPC");
            smd.Put("serviceURL", Request.FilePath); // TODO: Check whether this should be an absolute path from the protocol root.

            IRpcMethodDescriptor[] methods = service.GetMethods();

            if (methods.Length > 0) // TODO: Check if methods entry can be skipped if there are none.
            {
                JArray smdMethods = new JArray();

                foreach (IRpcMethodDescriptor method in methods)
                {
                    JObject smdMethod = new JObject();
                    smdMethod.Put("name", method.Name);
                
                    IRpcParameterDescriptor[] parameters = method.GetParameters();

                    if (parameters.Length > 0) // TODO: Check if parameters entry can be skipped if there are none.
                    {
                        JArray smdParameters = new JArray();

                        foreach (IRpcParameterDescriptor parameter in parameters)
                        {
                            JObject smdParameter = new JObject();
                            smdParameter.Put("name", parameter.Name);
                            smdParameters.Add(smdParameter);
                        }

                        smdMethod.Put("parameters", smdParameters);
                    }

                    smdMethods.Add(smdMethod);
                }

                smd.Put("methods", smdMethods);
            }

            //
            // Generate the response.
            //

            if (HasLastModifiedTime)
            {
                Response.Cache.SetCacheability(HttpCacheability.Public);
                Response.Cache.SetLastModified(LastModifiedTime);
            }

            Response.ContentType = "text/plain";

            Response.AppendHeader("Content-Disposition", 
                "attachment; filename=" + service.Name + ".smd");

            JsonTextWriter writer = new JsonTextWriter(Response.Output);
            writer.WriteValue(smd);
        }

        private bool Modified()
        {
            if (!HasLastModifiedTime)
                return true;

            string modifiedSinceHeader = Mask.NullString(Request.Headers["If-Modified-Since"]);
        
            if (modifiedSinceHeader.Length == 0)
                return true;

            DateTime modifiedSinceTime = InternetDate.Parse(modifiedSinceHeader);

            DateTime time = LastModifiedTime;
            time = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);

            if (time > modifiedSinceTime)
                return true;

            return false;
        }

        private bool HasLastModifiedTime
        {
            get { return LastModifiedTime > DateTime.MinValue; }
        }

        private DateTime LastModifiedTime
        {
            get
            {
                if (!_lastModifiedTimeInitialized)
                {
                    _lastModifiedTimeInitialized = true;

                    //
                    // The last modified time is determined by taking the
                    // last modified time of the physical file (for example,
                    // a DLL) representing the type's assembly.
                    //

                    Uri codeBase = new Uri(TargetService.GetType().Assembly.CodeBase);

                    if (codeBase.IsFile)
                    {
                        string path = codeBase.LocalPath;

                        if (File.Exists(path))
                        {
                            try
                            {
                                _lastModifiedTime = File.GetLastWriteTime(path);
                            }
                            catch (UnauthorizedAccessException) { /* ignored */ }
                            catch (IOException) { /* ignored */ }
                        }
                    }
                }

                return _lastModifiedTime;
            }
        }
    }
}
