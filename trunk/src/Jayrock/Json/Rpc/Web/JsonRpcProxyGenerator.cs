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

    #endregion

    public sealed class JsonRpcProxyGenerator : JsonRpcServiceFeature
    {
        protected override void ProcessRequest()
        {
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
            
            writer.Write("function ");
            writer.Write(service.Name);
            writer.WriteLine("Service(url)");
            writer.WriteLine("{");
            writer.Indent++;

            foreach (IRpcMethodDescriptor method in service.GetMethods())
            {
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

                writer.Write("return call('");
                writer.Write(method.Name);
                writer.Write("', [");

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
            writer.Write(Request.FilePath);
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
        http.open('POST', url, false);
        setupHeaders(http, method);
        http.send(JSON.stringify(request));
        if (http.status != 200)
            throw { message : http.status + ' ' + http.statusText, toString : function() { return message; } };
        var response = JSON.parse(http.responseText);
        if (response.error != null) throw response.error;
        return response.result;
    }

    function callAsync(method, request, callback)
    {
        var http = newHTTP();
        http.open('POST', url, true);
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
            callback(JSON.parse(sender.responseText));
    }

    function newHTTP()
    {
        return typeof(ActiveXObject) === 'function' ? 
            new ActiveXObject('Microsoft.XMLHTTP') : /* IE 5 */
            new XMLHttpRequest(); /* Safari 1.2, Mozilla 1.0/Firefox, and Netscape 7 */
    }
");
            
            writer.Indent--;
            writer.WriteLine("}");
        }
    }
}