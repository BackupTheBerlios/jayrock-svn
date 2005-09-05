#region License, Terms and Conditions
//
// JayRock - A JSON-RPC implementation for the Microsoft .NET Framework
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

namespace JayRock.Json.Rpc
{
    #region Imports

    using System;
    using System.Collections;

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
    
            IRpcServiceDescriptor descriptor = service.GetDescriptor();
    
            if (descriptor != null)
                name = descriptor.Name;
    
            return Mask.EmptyString(name, anonymousName);
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

        public static object[] TransposeVariableArguments(IRpcMethodDescriptor method, object[] args)
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
            
            if (args.Length >= parameterCount)
            {
                object lastArg = args[args.Length - 1];

                if (lastArg != null)
                {
                    varArgs = lastArg as object[];
                
                    if (varArgs == null)
                    {
                        ICollection collection = lastArg as ICollection;    
                    
                        if (collection != null)
                        {
                            varArgs = new object[collection.Count];
                            collection.CopyTo(varArgs, 0);
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

        public static bool HasMethodVariableArguments(IRpcMethodDescriptor method)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            IRpcParameterDescriptor[] parameters = method.GetParameters();
            int parameterCount = parameters.Length;

            if (parameterCount == 0)
                return false;

            IRpcParameterDescriptor lastParameter = parameters[parameterCount - 1];
            return JsonRpcParamsAttribute.IsDefined(lastParameter.AttributeProvider);
        }

        private JsonRpcServices()
        {
            throw new NotSupportedException();
        }
    }
}
