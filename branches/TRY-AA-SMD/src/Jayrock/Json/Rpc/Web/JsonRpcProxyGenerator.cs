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

    public sealed class JsonRpcProxyGenerator : JsonRpcServiceFeature
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

            if (HasLastModifiedTime)
            {
                Response.Cache.SetCacheability(HttpCacheability.Public);
                Response.Cache.SetLastModified(LastModifiedTime);
            }

            IRpcServiceDescriptor service = TargetService.GetDescriptor();

            Response.ContentType = "text/javascript";
            Response.AppendHeader("Content-Disposition", 
                "attachment; filename=" + service.Name + "Proxy.js");

            IndentedTextWriter writer = new IndentedTextWriter(Response.Output);

            writer.WriteLine("// This JavaScript was automatically generated by");
            writer.Write("// ");
            writer.WriteLine(GetType().AssemblyQualifiedName);
            writer.Write("// on ");
            DateTime now = DateTime.Now;
            TimeZone timeZone = TimeZone.CurrentTimeZone;
            writer.Write(now.ToLongDateString());
            writer.Write(" at ");
            writer.Write(now.ToLongTimeString());
            writer.Write(" (");
            writer.Write(timeZone.IsDaylightSavingTime(now) ? 
                timeZone.DaylightName : timeZone.StandardName);
            writer.WriteLine(")");
            writer.WriteLine();

            string version = Mask.EmptyString(Request.QueryString["v"], "1").Trim();

            if (version.Equals("2"))
                Version2(service, new Uri(Request.Url.GetLeftPart(UriPartial.Path)), writer);
            else
                Version1(service, new Uri(Request.Url.GetLeftPart(UriPartial.Path)), writer);
        }

        private static void Version1(IRpcServiceDescriptor service, Uri url, IndentedTextWriter writer)
        {
            Debug.Assert(service != null);
            Debug.Assert(url!= null);
            Debug.Assert(!url.IsFile);
            Debug.Assert(writer != null);

            writer.WriteLine("// Proxy version 1.0");
            writer.WriteLine();

            writer.Write("function ");
            writer.Write(service.Name);
            writer.WriteLine("(url)");
            writer.WriteLine("{");
            writer.Indent++;
    
            IRpcMethodDescriptor[] methods = service.GetMethods();
            string[] methodNames = new string[methods.Length];
    
            for (int i = 0; i < methods.Length; i++)
            {
                IRpcMethodDescriptor method = methods[i];
                methodNames[i] = method.Name;

                string summary = JsonRpcHelpAttribute.GetText(method.AttributeProvider);
                if (summary.Length > 0)
                {
                    // TODO: What to do if /* and */ appear in the summary?

                    writer.Write("/* ");
                    writer.Write(summary);
                    writer.WriteLine(" */");
                    writer.WriteLine();
                }

                writer.Write("this[\"");
                writer.Write(method.Name);
                writer.Write("\"] = function(");

                IRpcParameterDescriptor[] parameters = method.GetParameters();
                
                foreach (IRpcParameterDescriptor parameter in parameters)
                {
                    writer.Write(parameter.Name);
                    writer.Write(", ");
                }

                writer.WriteLine("callback)");
                writer.WriteLine("{");
                writer.Indent++;

                writer.Write("return call(\"");
                writer.Write(method.Name);
                writer.Write("\", [");

                foreach (IRpcParameterDescriptor parameter in parameters)
                {
                    if (parameter.Position > 0)
                        writer.Write(',');
                    writer.Write(' ');
                    writer.Write(parameter.Name);
                }

                writer.WriteLine(" ], callback);");

                writer.Indent--;
                writer.WriteLine("}");
                writer.WriteLine();
            }
    
            writer.Write("var url = typeof(url) === 'string' ? url : '");
            writer.Write(url);
            writer.WriteLine("';");
            writer.WriteLine(@"var self = this;
    var nextId = 0;

    function call(method, params, callback)
    {
        var request = { id : nextId++, method : method, params : params };
        return callback == null ? 
            callSync(method, request) : callAsync(method, request, callback);
    }

    function callSync(method, request)
    {
        var http = newHTTP();
        http.open('POST', url, false, self.httpUserName, self.httpPassword);
        setupHeaders(http, method);
        http.send(JSON.stringify(request));
        if (http.status != 200)
            throw { message : http.status + ' ' + http.statusText, toString : function() { return message; } };
        var response = JSON.eval(http.responseText);
        if (response.error != null) throw response.error;
        return response.result;
    }

    function callAsync(method, request, callback)
    {
        var http = newHTTP();
        http.open('POST', url, true, self.httpUserName, self.httpPassword);
        setupHeaders(http, method);
        http.onreadystatechange = function() { http_onreadystatechange(http, callback); }
        http.send(JSON.stringify(request));
        return request.id;
    }

    function setupHeaders(http, method)
    {
        http.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
        http.setRequestHeader('X-JSON-RPC', method);
    }

    function http_onreadystatechange(sender, callback)
    {
        if (sender.readyState == /* complete */ 4)
        {
            var response = sender.status == 200 ? 
                JSON.eval(sender.responseText) : {};
            
            response.xmlHTTP = sender;
                
            callback(response);
        }
    }

    function newHTTP()
    {
        return typeof(ActiveXObject) === 'function' ? 
            new ActiveXObject('Microsoft.XMLHTTP') : /* IE 5 */
            new XMLHttpRequest(); /* Safari 1.2, Mozilla 1.0/Firefox, and Netscape 7 */
    }");
    
            writer.Indent--;
            writer.WriteLine("}");
    
            writer.WriteLine();
            writer.Write(service.Name);
            writer.Write(".rpcMethods = ");
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.WriteArray(methodNames);
            writer.WriteLine(";");
        }

        private void Version2(IRpcServiceDescriptor service, Uri url, IndentedTextWriter writer)
        {
            Debug.Assert(service != null);
            Debug.Assert(url!= null);
            Debug.Assert(!url.IsFile);
            Debug.Assert(writer != null);
 
            writer.WriteLine("// Proxy version 2.0");
            writer.WriteLine();
 
            writer.Write("var ");
            writer.Write(service.Name);
            writer.Write(@" = function()
{
    var nextId = 0;

    var proxy = {

        url : """);
            writer.Write(url);
            writer.Write(@""",
        rpc : {");
            writer.WriteLine();
            writer.Indent += 3;
    
            IRpcMethodDescriptor[] methods = service.GetMethods();
            
            string[] methodNames = new string[methods.Length];
            for (int i = 0; i < methods.Length; i++)
                methodNames[i] = methods[i].Name;
            
            Array.Sort(methodNames, methods);
    
            for (int i = 0; i < methods.Length; i++)
            {
                IRpcMethodDescriptor method = methods[i];

                writer.WriteLine();

                string summary = JsonRpcHelpAttribute.GetText(method.AttributeProvider);
                if (summary.Length > 0)
                {
                    // TODO: What to do if /* and */ appear in the summary?

                    writer.Write("/* ");
                    writer.Write(summary);
                    writer.WriteLine(" */");
                    writer.WriteLine();
                }

                writer.Write('\"');
                writer.Write(method.Name);
                writer.Write("\" : function(");

                IRpcParameterDescriptor[] parameters = method.GetParameters();
                
                foreach (IRpcParameterDescriptor parameter in parameters)
                {
                    writer.Write(parameter.Name);
                    writer.Write(", ");
                }

                writer.WriteLine("callback) {");
                writer.Indent++;

                writer.Write("return new Call(\"");
                writer.Write(method.Name);
                writer.Write("\", [");

                foreach (IRpcParameterDescriptor parameter in parameters)
                {
                    if (parameter.Position > 0)
                        writer.Write(',');
                    writer.Write(' ');
                    writer.Write(parameter.Name);
                }

                writer.WriteLine(" ], callback);");

                writer.Indent--;
                writer.Write("}");
                if (i < (methods.Length - 1))
                    writer.Write(',');
                writer.WriteLine();
            }
    
            writer.Indent--;
            writer.WriteLine(@"}
    }

    function Call(method, params, callback)
    {
        this.url = proxy.url;
        this.callback = callback;
        this.request = 
        { 
            id     : ++nextId, 
            method : method, 
            params : params 
        };
    }
    
    Call.prototype.call = function(channel) { channel(this); }
    
    return proxy;
}();");
    
            writer.Indent--;
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
