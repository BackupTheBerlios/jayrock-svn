#region License, Terms and Conditions
//
// Jayrock - JSON and JSON-RPC for Microsoft .NET Framework and Mono
// Written by Atif Aziz (atif.aziz@skybow.com)
// Copyright (c) 2005 Atif Aziz. All rights reserved.
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
    using System.IO;
    using System.Security;
    using System.Web;
    using Jayrock.Services;

    #endregion

    public sealed class JsonRpcPythonProxyGenerator : JsonRpcProxyGeneratorBase
    {
        private const string _docQuotes = "\"\"\"";

        public JsonRpcPythonProxyGenerator(IService service) : 
            base(service) { }

        protected override string ClientFileName
        {
            get { return Service.GetClass().Name + "Proxy.py"; }
        }

        protected override void WriteProxy(IndentedTextWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            if (writer == null)
                throw new ArgumentNullException("writer");
            
            WriteProlog(writer);
            
            ServiceClass serviceClass = Service.GetClass();
            WriteClass(writer, serviceClass);

            foreach (Method method in serviceClass.GetMethods())
                WriteMethod(writer, method);
            
            WriteClassTail(writer, serviceClass);

            WriteEpilog(writer);
        }

        private void WriteProlog(IndentedTextWriter writer)
        {
            writer.WriteLine("import simplejson");
            writer.WriteLine("import urllib");
            writer.WriteLine();
        }

        private void WriteClass(IndentedTextWriter writer, ServiceClass serviceClass)
        {
            writer.Write("class ");
            writer.Write(serviceClass.Name);
            writer.WriteLine(":");
            writer.Indent++;
            
            writer.Write(_docQuotes);
            writer.Indent--;

            if (serviceClass.Description.Length > 0)
            {
                writer.WriteLine(serviceClass.Description);
                writer.WriteLine();
            }
            
            writer.WriteLine("This Python class was automatically generated by");
            writer.WriteLine(GetType().AssemblyQualifiedName);

            DateTime now = DateTime.Now;
            writer.Write("on ");
            writer.Write(now.ToLongDateString());
            writer.Write(" at ");
            writer.Write(now.ToLongTimeString());

            writer.Write(" (");
            TimeZone timeZone = TimeZone.CurrentTimeZone;
            writer.Write(timeZone.IsDaylightSavingTime(now) ? 
                         timeZone.DaylightName : timeZone.StandardName);
            writer.WriteLine(")");
            
            writer.WriteLine(_docQuotes);
            writer.Indent++;
            
            Uri url = new Uri(Request.Url.GetLeftPart(UriPartial.Path));
            writer.WriteLine(@"def __init__(self, url = '" + url + @"'):
        self.url = url
        self.__id = 0");
            
            writer.WriteLine();
        }

        private void WriteMethod(IndentedTextWriter writer, Method method)
        {
            string clientMethodName = method.Name.Replace(".", "_");

            writer.Write("def ");
            writer.Write(clientMethodName);
            writer.Write("(self");

            Parameter[] parameters = method.GetParameters();
                
            foreach (Parameter parameter in parameters)
            {
                writer.Write(", ");
                writer.Write(parameter.Name);
            }

            writer.WriteLine("):");
            writer.Indent++;

            if (method.Description.Length > 0)
            {
                // TODO: What to do if /* and */ appear in the summary?

                writer.Write(_docQuotes);
                writer.WriteLine(method.Description);
                writer.WriteLine(_docQuotes);
            }

            writer.Write("return self.__call('");
            writer.Write(method.Name);
            writer.Write("', (");

            foreach (Parameter parameter in parameters)
            {
                writer.Write(parameter.Name);
                writer.Write(", ");
            }

            writer.WriteLine("))");
            writer.Indent--;
            writer.WriteLine();
        }

        private void WriteClassTail(IndentedTextWriter writer, ServiceClass serviceClass)
        {
            writer.WriteLine(@"def __call(self, method, params):
        self.__id = self.__id + 1
        response = simplejson.loads(urllib.urlopen(self.url, urllib.urlencode([('JSON-RPC', simplejson.dumps({ 'id' : self.__id, 'method' : method, 'params' : params }))])).read())
        if response.has_key('error'): raise Error(None, response)
        return response['result']
");
            
            writer.Indent--;
        }

        private void WriteEpilog(IndentedTextWriter writer)
        {
            writer.WriteLine(@"class Error(Exception):
    """"""Exception raised when an error occurs calling a JSON-RPC service.""""""
    def __init__(self, message = None, response = None):
        self.error = response['error']
        self.response = response
        if message: self.message = str(message)
        else: self.message = self.error['message']");
        }
    }
}