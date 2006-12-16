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

namespace Jayrock.JsonRpc.Web
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Security;
    using System.Web;
    using Jayrock.Json;
    using Jayrock.Services;

    #endregion

    public sealed class JsonRpcProxyGenerator : JsonRpcProxyGeneratorBase
    {
        public JsonRpcProxyGenerator(IService service) : 
            base(service) {}

        protected override string ContentType
        {
            get { return "text/javascript"; }
        }

        protected override string ClientFileName
        {
            get { return Service.GetClass().Name + "Proxy.js"; }
        }
        
        protected override void WriteProxy(IndentedTextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

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

            Uri url = new Uri(Request.Url.GetLeftPart(UriPartial.Path));
            
            if (version.Equals("2"))
                Version2(Service.GetClass(), url, writer);
            else
                Version1(Service.GetClass(), url, writer);
        }

        private static void Version1(ServiceClass service, Uri url, IndentedTextWriter writer)
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
    
            Method[] methods = service.GetMethods();
            string[] methodNames = new string[methods.Length];
    
            for (int i = 0; i < methods.Length; i++)
            {
                Method method = methods[i];
                methodNames[i] = method.Name;

                if (method.Description.Length > 0)
                {
                    // TODO: What to do if /* and */ appear in the summary?

                    writer.Write("/* ");
                    writer.Write(method.Description);
                    writer.WriteLine(" */");
                    writer.WriteLine();
                }

                writer.Write("this[\"");
                writer.Write(method.Name);
                writer.Write("\"] = function(");

                Parameter[] parameters = method.GetParameters();
                
                foreach (Parameter parameter in parameters)
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

                foreach (Parameter parameter in parameters)
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
        if (typeof(window) != 'undefined' && window.XMLHttpRequest)
            return new XMLHttpRequest(); /* IE7, Safari 1.2, Mozilla 1.0/Firefox, and Netscape 7 */
        else
            return new ActiveXObject('Microsoft.XMLHTTP'); /* WSH and IE 5 to IE 6 */
    }");
    
            writer.Indent--;
            writer.WriteLine("}");
    
            writer.WriteLine();
            writer.Write(service.Name);
            writer.Write(".rpcMethods = ");
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            jsonWriter.WriteStringArray(methodNames);
            writer.WriteLine(";");
        }

        private void Version2(ServiceClass service, Uri url, IndentedTextWriter writer)
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
    
            Method[] methods = service.GetMethods();
            
            string[] methodNames = new string[methods.Length];
            for (int i = 0; i < methods.Length; i++)
                methodNames[i] = methods[i].Name;
            
            Array.Sort(methodNames, methods);
    
            for (int i = 0; i < methods.Length; i++)
            {
                Method method = methods[i];

                writer.WriteLine();

                if (method.Description.Length > 0)
                {
                    // TODO: What to do if /* and */ appear in the summary?

                    writer.Write("/* ");
                    writer.Write(method.Description);
                    writer.WriteLine(" */");
                    writer.WriteLine();
                }

                writer.Write('\"');
                writer.Write(method.Name);
                writer.Write("\" : function(");

                Parameter[] parameters = method.GetParameters();
                
                foreach (Parameter parameter in parameters)
                {
                    writer.Write(parameter.Name);
                    writer.Write(", ");
                }

                writer.WriteLine("callback) {");
                writer.Indent++;

                writer.Write("return new Call(\"");
                writer.Write(method.Name);
                writer.Write("\", [");

                foreach (Parameter parameter in parameters)
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
    
    Call.prototype.call = function(channel) { return channel(this); }
    
    return proxy;
}();");
    
            writer.Indent--;
        }

        protected override void WriteClass(IndentedTextWriter writer, ServiceClass serviceClass)
        {
            throw new NotImplementedException();
        }

        protected override void WriteMethod(IndentedTextWriter writer, Method method)
        {
            throw new NotImplementedException();
        }

        protected override void WriteClassTail(IndentedTextWriter writer, ServiceClass serviceClass)
        {
            throw new NotImplementedException();
        }
    }
}
