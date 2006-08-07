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
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    internal sealed class JsonRpcServiceReflector
    {
        public static JsonRpcServiceClass FromType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            JsonRpcServiceClass.Builder builder = new JsonRpcServiceClass.Builder();
            BuildClass(builder, type);
            return builder.CreateClass();
        }

        private static void BuildClass(JsonRpcServiceClass.Builder builder, Type type)
        {
            /*
            //
            // Determine the service name, allowing customization via the
            // JsonRpcService attribute.
            //

            JsonRpcServiceAttribute serviceAttribute = (JsonRpcServiceAttribute) Attribute.GetCustomAttribute(type, typeof(JsonRpcServiceAttribute), true);

            if (serviceAttribute != null)
                builder.Name = serviceAttribute.Name.Trim();

            builder.Name = Mask.EmptyString(builder.Name, type.Name);
            */

            foreach (Attribute attribute in Attribute.GetCustomAttributes(type))
            {
                IBuilderAttribute builderAttribute = attribute as IBuilderAttribute;
                
                if (builderAttribute != null)
                    builderAttribute.BuildServiceClass(builder, type);
            }

            /*
            //
            // Get the description;
            //
            
            JsonRpcHelpAttribute helpAttribute = (JsonRpcHelpAttribute) Attribute.GetCustomAttribute(type, typeof(JsonRpcHelpAttribute), true);
            if (helpAttribute != null)  
                builder.Description = helpAttribute.Text;
            */
            
            if (builder.Name.Length == 0)
                builder.Name = type.Name;

            //
            // Get all the public instance methods on the type and create a
            // filtered table of those to expose from the service.
            //
            
            MethodInfo[] publicMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (MethodInfo method in publicMethods)
            {
                if (ShouldBuild(method))
                    BuildMethod(builder.DefineMethod(), method);
            }
        }

        private static bool ShouldBuild(MethodInfo method)
        {
            Debug.Assert(method != null);
            
            return !method.IsAbstract && Attribute.IsDefined(method, typeof(JsonRpcMethodAttribute));
        }

        private static void BuildMethod(JsonRpcMethod.Builder builder, MethodInfo method)
        {
            Debug.Assert(method != null);
            Debug.Assert(builder != null);

            //JsonRpcMethodAttribute attribute = (JsonRpcMethodAttribute) Attribute.GetCustomAttribute(method, typeof(JsonRpcMethodAttribute));
               
            builder.InternalName = method.Name;
            builder.ResultType = method.ReturnType;
            builder.Dispatcher = new Dispatcher(method);
            
            foreach (Attribute attribute in Attribute.GetCustomAttributes(method))
            {
                IBuilderAttribute builderAttribute = attribute as IBuilderAttribute;
                
                if (builderAttribute != null)
                    builderAttribute.BuildMethod(builder, method);
            }
            
            if (builder.Name.Length == 0)
                builder.Name = method.Name;

            /*
            //
            // Get the description;
            //
            
            JsonRpcHelpAttribute helpAttribute = (JsonRpcHelpAttribute) Attribute.GetCustomAttribute(method, typeof(JsonRpcHelpAttribute), true);
            if (helpAttribute != null)  
                builder.Description = helpAttribute.Text;

            //
            // Obsoleted method?
            //

            JsonRpcObsoleteAttribute obsolete = (JsonRpcObsoleteAttribute) CustomAttribute.Get(method, typeof(JsonRpcObsoleteAttribute), true);
            if (obsolete != null)
            {
                builder.IsObsolete = obsolete != null;
                builder.ObsoletionMessage = obsolete.Message;
            }
            */
            
            //
            // Build the method parameters.
            //

            foreach (ParameterInfo parameter in method.GetParameters())
                BuildParameter(builder.DefineParameter(), parameter);
        }

        private static void BuildParameter(JsonRpcParameter.Builder builder, ParameterInfo parameter)
        {
            Debug.Assert(parameter != null);
            Debug.Assert(builder != null);
            
            builder.Name = parameter.Name;
            builder.ParameterType = parameter.ParameterType;
            builder.Position = parameter.Position;
            builder.IsParamArray = parameter.IsDefined(typeof(ParamArrayAttribute), true);
        }

        private JsonRpcServiceReflector()
        {
            throw new NotSupportedException();
        }
        
        // TODO: Consider making serializable.

        private sealed class Dispatcher : IDispatcher
        {
            private readonly MethodInfo _method;

            public Dispatcher(MethodInfo method)
            {
                Debug.Assert(method != null);
                
                _method = method;
            }

            public object Invoke(IRpcService service, object[] args)
            {
                return _method.Invoke(service, args);
            }

            public IAsyncResult BeginInvoke(IRpcService service, object[] args, AsyncCallback callback, object asyncState)
            {
                throw new NotImplementedException();
            }

            public object EndInvoke(IAsyncResult asyncResult)
            {
                throw new NotImplementedException();
            }
        }
    }
}
