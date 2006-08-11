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
    using System.Diagnostics;
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

            bool isNotification = JsonNull.LogicallyEquals(id);
            
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

            if (JsonRpcTrace.Switch.TraceInfo)
                JsonRpcTrace.Info("Invoking method {1} on service {0}.", ServiceName, methodName);

            //
            // Invoke the method on the service and handle errors.
            //
    
            object error = null;
            object result = null;

            try
            {
                JsonRpcMethod method = _service.GetClass().GetMethodByName(methodName);
                object[] args = method.MapArguments(request["params"]);
                result = method.Invoke(_service, args);
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

            JsonObject response = new JsonObject();
            
            response["id"] = id;

            if (error != null)
                response["error"] = error;
            else
                response["result"] = result;

            return response;
        }

        protected virtual object ParseRequest(TextReader input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            JsonReader reader = (JsonReader) _serviceProvider.GetService(typeof(JsonReader));

            if (reader == null)
                reader = new JsonTextReader(input);
            
            JsonObject request = new JsonObject();
            JsonRpcMethod method = null;
            JsonReader paramsReader = null;
            object args = null;
            
            reader.ReadToken(JsonTokenClass.Object);
            
            while (reader.TokenClass != JsonTokenClass.EndObject)
            {
                string memberName = reader.ReadMember();
                
                switch (memberName)
                {
                    case "id" :
                    {
                        request["id"] = reader.DeserializeNext();
                        break;
                    }
                    case "method" :
                    {
                        string methodName = reader.ReadString();
                        request["method"] = methodName;
                        method = _service.GetClass().GetMethodByName(methodName);
                        
                        if (paramsReader != null)
                        {
                            //
                            // If the parameters were already read in and
                            // buffer, then deserialize them now that we know
                            // the method we're dealing with.
                            //
                            
                            args = ReadParameters(method, paramsReader);
                            paramsReader = null;
                        }
                        
                        break;
                    }
                    case "params" :
                    {
                        //
                        // Is the method already known? If so, then we can
                        // deserialize the parameters right away. Otherwise
                        // we record them until hopefully the method is
                        // encountered.
                        //
                        
                        if (method != null)
                        {
                            args = ReadParameters(method, reader);
                        }
                        else
                        {
                            JsonRecorder recorder = new JsonRecorder();
                            recorder.WriteValueFromReader(reader);
                            paramsReader = recorder.CreatePlayer();
                        }

                        break;
                    }
                }
            }
            
            reader.Read();

            if (args != null)
                request["params"] = args;
            
            return request;
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
                formatter.AddFormatter(typeof(DataSet), new DataSetFormatter(), true);
                formatter.AddFormatter(typeof(DataTable), new DataTableFormatter(), true);
                formatter.AddFormatter(typeof(DataView), new DataViewFormatter(), true);
                formatter.AddFormatter(typeof(DataRowView), new DataRowViewFormatter(), true);
                formatter.AddFormatter(typeof(DataRow), new DataRowFormatter(), true);
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

        private static object ReadParameters(JsonRpcMethod method, JsonReader reader)
        {
            Debug.Assert(method != null);
            Debug.Assert(reader != null);
            
            reader.MoveToContent();
            
            JsonRpcParameter[] parameters = method.GetParameters();
                            
            if (reader.TokenClass == JsonTokenClass.Array)
            {
                reader.Read();
                ArrayList argList = new ArrayList(parameters.Length);
                                
                // TODO: This loop could bomb when more args are supplied that parameters available.
                                                        
                for (int i = 0; i < parameters.Length && reader.TokenClass != JsonTokenClass.EndArray; i++)
                    argList.Add(reader.Get(parameters[i].ParameterType));

                reader.StepOut();
                return argList.ToArray();
            }
            else if (reader.TokenClass == JsonTokenClass.Object)
            {
                reader.Read();
                JsonObject argByName = new JsonObject();
                                
                while (reader.TokenClass != JsonTokenClass.EndObject)
                {
                    // TODO: Imporve this lookup.
                                    
                    JsonRpcParameter matchedParameter = null;

                    foreach (JsonRpcParameter parameter in parameters)
                    {
                        if (parameter.Name.Equals(reader.Text))
                        {
                            matchedParameter = parameter;
                            break;
                        }
                    }
                                    
                    reader.Read();

                    if (matchedParameter != null)
                        argByName.Put(matchedParameter.Name, reader.Get(matchedParameter.ParameterType));
                }
                                
                reader.Read();
                return argByName;
            }
            else
            {
                return reader.DeserializeNext();
            }
        }
    }
}
