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
    using System.Globalization;
    using System.Reflection;

    #endregion

    public sealed class JsonRpcServices
    {
        public static string GetServiceName(IRpcService service)
        {
            return GetServiceName(service, "(anonymous)");
        }

        /// <summary>
        /// Returns the name of the service or an anonymous default if it does
        /// not have a name.
        /// </summary>

        public static string GetServiceName(IRpcService service, string anonymousName)
        {
            if (service == null)
                throw new ArgumentNullException("service");

            string name = null;
    
            IRpcServiceClass clazz = service.GetClass();
    
            if (clazz != null)
                name = clazz.Name;
    
            return Mask.EmptyString(name, anonymousName);
        }

        public static object[] MapArguments(IRpcMethod method, object argsObject)
        {
            object[] args;
            IDictionary argsMap = argsObject as IDictionary;

            if (argsMap != null)
            {
                JObject namedArgs = new JObject(argsMap);
                
                IRpcParameter[] parameters = method.GetParameters();
                args = new object[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    args[i] = namedArgs[parameters[i].Name];
                    namedArgs.Remove(parameters[i].Name);
                }

                foreach (DictionaryEntry entry in namedArgs)
                {
                    if (entry.Key == null)
                        continue;

                    string key = entry.Key.ToString();
                    
                    char ch1;
                    char ch2;

                    if (key.Length == 2)
                    {
                        ch1 = key[0];
                        ch2 = key[1];
                    }
                    else if (key.Length == 1)
                    {
                        ch1 = '0';
                        ch2 = key[0];
                    }
                    else
                    {
                        continue;
                    }

                    if (ch1 >= '0' && ch1 < '9' &&
                        ch2 >= '0' && ch2 < '9')
                    {
                        int index = int.Parse(key, NumberStyles.Number, CultureInfo.InvariantCulture);
                        
                        if (index < parameters.Length)
                            args[index] = entry.Value;
                    }
                }

                return args;
            }
            else
            {
                args = CollectionHelper.ToArray((ICollection) argsObject);
            }

            return TransposeVariableArguments(method, args);
        }

        /// <summary>
        /// Takes an array of arguments that are designated for a method and
        /// transposes them if the target method supports variable arguments (in
        /// other words, the last parameter is annotated with the JsonRpcParams
        /// attribute). If the method does not support variable arguments then
        /// the input array is returned verbatim. 
        /// </summary>

        // TODO: Allow args to be null to represent empty arguments.
        // TODO: Allow parameter conversions

        public static object[] TransposeVariableArguments(IRpcMethod method, object[] args)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            //
            // If the method does not have take variable arguments then just
            // return the arguments array verbatim.
            //

            if (!HasMethodVariableArguments(method))
                return args;

            int parameterCount = method.GetParameters().Length;

            object[] varArgs = null;
            
            //
            // The variable argument may already be setup correctly as an
            // array. If so then the formal and actual parameter count will
            // match here.
            //
            
            if (args.Length == parameterCount)
            {
                object lastArg = args[args.Length - 1];

                if (lastArg != null)
                {
                    //
                    // Is the last argument already set up as an object 
                    // array ready to be received as the variable arguments?
                    //
                    
                    varArgs = lastArg as object[];
                    
                    if (varArgs == null)
                    {
                        //
                        // Is the last argument an array of some sort? If so 
                        // then we convert it into an array of objects since 
                        // that is what we support right now for variable 
                        // arguments.
                        //
                        // TODO: Allow variable arguments to be more specific type, such as array of integers.
                        // TODO: Don't make a copy if one doesn't have to be made. 
                        // For example if the types are compatible on the receiving end.
                        //
                        
                        Array lastArrayArg = lastArg as Array;
                        
                        if (lastArrayArg != null && lastArrayArg.GetType().GetArrayRank() == 1)
                        {
                            varArgs = new object[lastArrayArg.Length];
                            Array.Copy(lastArrayArg, varArgs, varArgs.Length);
                        }
                    }
                }
            }

            //
            // Copy out the extra arguments into a new array that represents
            // the variable parts.
            //

            if (varArgs == null)
            {
                varArgs = new object[(args.Length - parameterCount) + 1];
                Array.Copy(args, parameterCount - 1, varArgs, 0, varArgs.Length);
            }

            //
            // Setup a new array of arguments that has a copy of the fixed
            // arguments followed by the variable arguments array setup above.
            //

            object[] transposedArgs = new object[parameterCount];
            Array.Copy(args, transposedArgs, parameterCount - 1);
            transposedArgs[transposedArgs.Length - 1] = varArgs;
            return transposedArgs;
        }

        /// <summary>
        /// Determines if the method accepts variable number of arguments or
        /// not. A method is designated as accepting variable arguments by
        /// annotating the last parameter of the method with the JsonRpcParams
        /// attribute.
        /// </summary>

        public static bool HasMethodVariableArguments(IRpcMethod method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            IRpcParameter[] parameters = method.GetParameters();
            int parameterCount = parameters.Length;

            if (parameterCount == 0)
                return false;

            IRpcParameter lastParameter = parameters[parameterCount - 1];
            ICustomAttributeProvider attributeProvider = lastParameter.AttributeProvider;
            return attributeProvider != null && 
                   CustomAttribute.IsDefined(attributeProvider, typeof(ParamArrayAttribute));
        }

        public static object GetResult(IDictionary response)
        {
            if (response == null)
                throw new ArgumentNullException("response");

            object errorObject = response["error"];

            if (errorObject != null)
            {
                IDictionary error = errorObject as IDictionary;
                
                string message = null;

                if (!JNull.LogicallyEquals(error))
                {
                    object messageObject = error["message"];
                    
                    if (!JNull.LogicallyEquals(messageObject))
                        message = messageObject.ToString();
                }
                else
                    message = error.ToString();
   
                throw new JsonRpcException(message);
            }

            if (!response.Contains("result"))
                throw new ArgumentException("Response object is not valid because it does not contain the expected 'result' member.");

            return response["result"];
        }

        private JsonRpcServices()
        {
            throw new NotSupportedException();
        }
    }
}
