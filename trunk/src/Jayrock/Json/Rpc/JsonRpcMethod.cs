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
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    #endregion

    [ Serializable ]
    public sealed class JsonRpcMethod
    {
        private readonly JsonRpcServiceClass _class;
        private readonly string _name;
        private readonly string _internalName;
        private readonly Type _resultType;
        private JsonRpcParameter[] _parameters;
        private readonly IDispatcher _dispatcher;
        private readonly string _description;
        private readonly Attribute[] _attributes;

        internal JsonRpcMethod(JsonRpcMethodBuilder methodBuilder, JsonRpcServiceClass clazz)
        {
            Debug.Assert(methodBuilder != null);
            Debug.Assert(clazz != null);
            
            _name = methodBuilder.Name;
            _internalName = Mask.EmptyString(methodBuilder.InternalName, methodBuilder.Name);
            _resultType = methodBuilder.ResultType;
            _description = methodBuilder.Description;
            _dispatcher = methodBuilder.Dispatcher;
            _attributes = DeepCopy(methodBuilder.GetCustomAttributes());
            _class = clazz;
            
            JsonRpcParameterBuilder[] parameterBuilders = methodBuilder.GetParameters();
            _parameters = new JsonRpcParameter[parameterBuilders.Length];
            int paramIndex = 0;

            foreach (JsonRpcParameterBuilder parameterBuilder in parameterBuilders)
                _parameters[paramIndex++] = new JsonRpcParameter(parameterBuilder, this);
        }

        public string Name
        {
            get { return _name; }
        }

        public string InternalName
        {
            get { return _internalName; }
        }
        
        public JsonRpcParameter[] GetParameters()
        {
            //
            // IMPORTANT! Never return the private array instance since the
            // caller could modify its state and compromise the integrity as
            // well as the assumptions made in this implementation.
            //

            return (JsonRpcParameter[]) _parameters.Clone();
        }

        public Type ResultType
        {
            get { return _resultType; }
        }
        
        public string Description
        {
            get { return _description; }
        }

        public JsonRpcServiceClass ServiceClass
        {
            get { return _class; }
        }
        
        public Attribute[] GetCustomAttributes()
        {
            return DeepCopy(_attributes);
        }

        public Attribute FindFirstCustomAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            foreach (Attribute attribute in _attributes)
            {
                if (TypesMatch(type, attribute.GetType()))
                    return (Attribute) ((ICloneable) attribute).Clone();
            }
            
            return null;
        }

        public object Invoke(IRpcService service, object[] args)
        {
            return _dispatcher.Invoke(service, args);
        }

        /// <remarks>
        /// The default implementation calls Invoke synchronously and returns
        /// an IAsyncResult that also indicates that the operation completed
        /// synchronously. If a callback was supplied, it will be called 
        /// before BeginInvoke returns. Also, if Invoke throws an exception, 
        /// it is delayed until EndInvoke is called to retrieve the results.
        /// </remarks>

        public IAsyncResult BeginInvoke(IRpcService service, object[] args, AsyncCallback callback, object asyncState)
        {
            return _dispatcher.BeginInvoke(service, args, callback, asyncState);
        }

        public object EndInvoke(IAsyncResult asyncResult)
        {
            return _dispatcher.EndInvoke(asyncResult);
        }

        /// <summary>
        /// Determines if the method accepts variable number of arguments or
        /// not. A method is designated as accepting variable arguments by
        /// annotating the last parameter of the method with the JsonRpcParams
        /// attribute.
        /// </summary>

        public bool HasParamArray
        {
            get
            {
                return _parameters.Length > 0 && 
                    _parameters[_parameters.Length - 1].IsParamArray;
            }
        }

        public object[] MapArguments(object argsObject)
        {
            object[] args;
            IDictionary argsMap = argsObject as IDictionary;

            if (argsMap != null)
            {
                JsonObject namedArgs = new JsonObject(argsMap);
                
                args = new object[_parameters.Length];

                for (int i = 0; i < _parameters.Length; i++)
                {
                    args[i] = namedArgs[_parameters[i].Name];
                    namedArgs.Remove(_parameters[i].Name);
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
                        
                        if (index < _parameters.Length)
                            args[index] = entry.Value;
                    }
                }

                return args;
            }
            else
            {
                args = CollectionHelper.ToArray((ICollection) argsObject);
            }

            return TransposeVariableArguments(args);
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

        public object[] TransposeVariableArguments(object[] args)
        {
            //
            // If the method does not have take variable arguments then just
            // return the arguments array verbatim.
            //

            if (!HasParamArray)
                return args;

            int parameterCount = _parameters.Length;

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

        private static bool TypesMatch(Type expected, Type actual)
        {
            Debug.Assert(expected != null);
            Debug.Assert(actual != null);
            
            //
            // If the expected type is sealed then use a quick check by
            // comparing types for equality. Otherwise, use the slow
            // approach to determine type compatibility be their
            // relationship.
            //
            
            return expected.IsSealed ? 
                   expected.Equals(actual) : 
                   expected.IsAssignableFrom(actual);
        }

        private static Attribute[] DeepCopy(Attribute[] originals)
        {
            Attribute[] copies = new Attribute[originals.Length];
            originals.CopyTo(copies, 0);
            
            //
            // Deep copy each returned attribute since attributes
            // are generally not known to be read-only.
            //
            
            for (int i = 0; i < copies.Length; i++)
                copies[i] = (Attribute) ((ICloneable) copies[i]).Clone();
            
            return copies;
        }
    }
}
