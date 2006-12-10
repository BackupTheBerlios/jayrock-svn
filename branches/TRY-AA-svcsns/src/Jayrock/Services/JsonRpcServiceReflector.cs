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

namespace Jayrock.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using Jayrock.JsonRpc;

    internal sealed class JsonRpcServiceReflector
    {
        public static JsonRpcServiceClass FromType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            
            JsonRpcServiceClassBuilder builder = new JsonRpcServiceClassBuilder();
            BuildClass(builder, type);
            return builder.CreateClass();
        }

        private static void BuildClass(JsonRpcServiceClassBuilder builder, Type type)
        {
            //
            // Build via attributes.
            //

            object[] attributes = type.GetCustomAttributes(typeof(IServiceClassReflector), true);
            foreach (IServiceClassReflector reflector in attributes)
                reflector.Build(builder, type);
            
            //
            // Fault in the type name if still without name.
            //

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

        private static void BuildMethod(JsonRpcMethodBuilder builder, MethodInfo method)
        {
            Debug.Assert(method != null);
            Debug.Assert(builder != null);

            builder.InternalName = method.Name;
            builder.ResultType = method.ReturnType;
            builder.Handler = new MethodDispatcher(method);
            
            //
            // Build via attributes.
            //
            
            object[] attributes = method.GetCustomAttributes(typeof(Attribute), true);
            foreach (Attribute attribute in attributes)
            {
                IMethodReflector reflector = attribute as IMethodReflector;
                
                if (reflector != null)
                    reflector.Build(builder, method);
                else 
                    builder.AddCustomAttribute(attribute);
            }
            
            //
            // Fault in the method name if still without name.
            //
            
            if (builder.Name.Length == 0)
                builder.Name = method.Name;

            //
            // Build the method parameters.
            //

            foreach (ParameterInfo parameter in method.GetParameters())
                BuildParameter(builder.DefineParameter(), parameter);
        }

        private static void BuildParameter(JsonRpcParameterBuilder builder, ParameterInfo parameter)
        {
            Debug.Assert(parameter != null);
            Debug.Assert(builder != null);
            
            builder.Name = parameter.Name;
            builder.ParameterType = parameter.ParameterType;
            builder.Position = parameter.Position;
            builder.IsParamArray = parameter.IsDefined(typeof(ParamArrayAttribute), true);

            //
            // Build via attributes.
            //
            
            object[] attributes = parameter.GetCustomAttributes(typeof(IParameterReflector), true);
            foreach (IParameterReflector reflector in attributes)
                reflector.Build(builder, parameter);
        }

        private JsonRpcServiceReflector()
        {
            throw new NotSupportedException();
        }

        [ Serializable ]
        private sealed class MethodDispatcher : IMethodImpl
        {
            private readonly MethodInfo _method;

            public MethodDispatcher(MethodInfo method)
            {
                Debug.Assert(method != null);
                
                _method = method;
            }

            public object Invoke(IService service, object[] args)
            {
                if (service == null)
                    throw new ArgumentNullException("service");

                try
                {
                    return _method.Invoke(service, args);
                }
                catch (ArgumentException e)
                {
                    throw TranslateException(e);
                }
                catch (TargetParameterCountException e)
                {
                    throw TranslateException(e);
                }
                catch (TargetInvocationException e)
                {
                    throw TranslateException(e);
                }
            }

            public IAsyncResult BeginInvoke(IService service, object[] args, AsyncCallback callback, object asyncState)
            {
                if (service == null)
                    throw new ArgumentNullException("service");

                SynchronousAsyncResult asyncResult;

                try
                {
                    object result = Invoke(service, args);
                    asyncResult = SynchronousAsyncResult.Success(asyncState, result);
                }
                catch (Exception e)
                {
                    asyncResult = SynchronousAsyncResult.Failure(asyncState, e);
                }

                if (callback != null)
                    callback(asyncResult);
                
                return asyncResult;
            }

            public object EndInvoke(IAsyncResult asyncResult)
            {
                if (asyncResult == null)
                    throw new ArgumentException("asyncResult");

                SynchronousAsyncResult ar = asyncResult as SynchronousAsyncResult;

                if (ar == null)
                    throw new ArgumentException("asyncResult", "IAsyncResult object did not come from the corresponding async method on this type.");

                try
                {
                    //
                    // IMPORTANT! The End method on SynchronousAsyncResult will 
                    // throw an exception if that's what Invoke did when 
                    // BeginInvoke called it. The unforunate side effect of this is
                    // the stack trace information for the exception is lost and 
                    // reset to this point. There seems to be a basic failure in the 
                    // framework to accommodate for this case more generally. One 
                    // could handle this through a custom exception that wraps the 
                    // original exception, but this assumes that an invocation will 
                    // only throw an exception of that custom type. We need to 
                    // think more about this.
                    //

                    return ar.End("Invoke");
                }
                catch (ArgumentException e)
                {
                    throw TranslateException(e);
                }
                catch (TargetParameterCountException e)
                {
                    throw TranslateException(e);
                }
                catch (TargetInvocationException e)
                {
                    throw TranslateException(e);
                }
            }

            private Exception TranslateException(ArgumentException e)
            {
                //
                // The type of the parameter does not match the signature
                // of the method or constructor reflected by this
                // instance.
                //

                return new InvocationException(e);
            }

            private static Exception TranslateException(TargetParameterCountException e)
            {
                //
                // The parameters array does not have the correct number of
                // arguments.
                //

                return new InvocationException(e.Message, e);
            }

            private static Exception TranslateException(TargetInvocationException e)
            {
                return new TargetMethodException(e.InnerException);
            }
        }
    }

    internal interface IServiceClassReflector
    {
        void Build(JsonRpcServiceClassBuilder builder, Type type);
    }

    internal interface IMethodReflector
    {
        void Build(JsonRpcMethodBuilder builder, MethodInfo method);
    }

    internal interface IParameterReflector
    {
        void Build(JsonRpcParameterBuilder builder, ParameterInfo parameter);
    }
}

