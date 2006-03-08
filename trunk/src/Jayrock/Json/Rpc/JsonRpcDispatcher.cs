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
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Data;
    using System.IO;
    using System.Web.UI;
    using Jayrock.Json.Formatters;

    #endregion

    /// <summary>
    /// The workhorse of the JSON-RPC implementation to parse the request,
    /// invoke methods on a service and write out the response.
    /// </summary>

    // TODO: Add async processing.

    public class JsonRpcDispatcher
    {
        private readonly IRpcService _service;
        private readonly IServiceProvider _serviceProvider;
        private string _serviceName;
        private bool _localExecution;

        public JsonRpcDispatcher(IRpcService service) :
            this(service, null) {}

        public JsonRpcDispatcher(IRpcService service, IServiceProvider serviceProvider)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            _service = service;

            if (serviceProvider == null)
            {
                //
                // No service provider supplied so check if the RPC service
                // itself is our service provider.
                //

                serviceProvider = service as IServiceProvider;    

                //
                // If no service provider found so far, then create a default
                // one.
                //

                if (serviceProvider == null)
                    serviceProvider = new ServiceContainer();
            }

            _serviceProvider = serviceProvider;
        }

        internal void SetLocalExecution()
        {
            // TODO: Need to make this public but through a more generic set of options.

            _localExecution = true;
        }

        private string ServiceName
        {
            get
            {
                if (_serviceName == null)
                    _serviceName = JsonRpcServices.GetServiceName(_service);

                return _serviceName;
            }
        }

        public virtual string Process(string request)
        {
            StringWriter writer = new StringWriter();
            Process(new StringReader(request), writer);
            return writer.ToString();
        }

        public virtual void Process(TextReader input, TextWriter output)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (output == null)
                throw new ArgumentNullException("output");

            IDictionary request = (IDictionary) ParseRequest(input);
            IDictionary response = Invoke(request);
            WriteResponse(response, output);
        }

        public virtual IDictionary Invoke(IDictionary request)
        {
            if (request == null)
                throw new ArgumentNullException();

            //
            // Get the ID of the request.
            //

            object id = request["id"];

            //
            // If the ID is not there or was not set then this is a notification
            // request from the client that does not expect any response. Right
            // now, we don't support this.
            //

            bool isNotification = id == null || JNull.Value.Equals(id);
            
            if (isNotification)
                throw new NotSupportedException("Notification are not yet supported.");

            if (JsonRpcTrace.TraceInfo)
                JsonRpcTrace.Info("Received request with the ID {0}.", id.ToString());

            //
            // Get the method name and arguments.
            //
    
            string methodName = Mask.NullString((string) request["method"]);

            if (methodName.Length == 0)
                throw new JsonRpcException("No method name supplied for this request.");

            object[] args = CollectionHelper.ToArray((ICollection) request["params"]);
    
            if (JsonRpcTrace.Switch.TraceInfo)
                JsonRpcTrace.Info("Invoking method {1} on service {0}.", ServiceName, methodName);

            //
            // Invoke the method on the service and handle errors.
            //
    
            object error = null;
            object result = null;

            try
            {
                result = _service.Invoke(methodName, args);
            }
            catch (MethodNotFoundException e)
            {
                error = OnError(e);
            }
            catch (InvocationException e)
            {
                error = OnError(e);
            }
            catch (TargetMethodException e)
            {
                error = OnError(e.InnerException);
            }
            catch (Exception e)
            {
                if (JsonRpcTrace.Switch.TraceError)
                    JsonRpcTrace.Error(e);

                throw;
            }

            //
            // Setup and return the response object.
            //

            JObject response = new JObject();
            
            response["id"] = id;

            if (error != null)
                response["error"] = error;
            else
                response["result"] = result != null ? result : JNull.Value;

            return response;
        }

        protected virtual object ParseRequest(TextReader input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            JsonParser parser = (JsonParser) _serviceProvider.GetService(typeof(JsonParser));

            if (parser == null)
                parser = new JsonParser();

            return parser.Parse(input);
        }

        protected virtual void WriteResponse(object response, TextWriter output)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            if (output == null)
                throw new ArgumentNullException("output");
            
            JsonWriter writer = (JsonWriter) _serviceProvider.GetService(typeof(JsonWriter));
            
            if (writer == null)
            {
                CompositeFormatter formatter = new CompositeFormatter();

                formatter.AddFormatter(typeof(DateTime), new DateTimeFormatter());
                formatter.AddFormatter(typeof(DataSet), new DataSetFormatter());
                formatter.AddFormatter(typeof(DataTable), new DataTableFormatter());
                formatter.AddFormatter(typeof(DataRowView), new DataRowViewFormatter());
                formatter.AddFormatter(typeof(DataRow), new DataRowFormatter());
                formatter.AddFormatter(typeof(NameValueCollection), new NameValueCollectionFormatter(), true);
                formatter.AddFormatter(typeof(Control), new ControlFormatter(), true);

                writer = new JsonTextWriter(output);
                writer.ValueFormatter = formatter;
            }

            writer.WriteValue(response);
        }

        protected virtual object OnError(Exception e)
        {
            if (JsonRpcTrace.Switch.TraceError)
                JsonRpcTrace.Error(e);

            return JsonRpcError.FromException(e, _localExecution);
        }
    }
}