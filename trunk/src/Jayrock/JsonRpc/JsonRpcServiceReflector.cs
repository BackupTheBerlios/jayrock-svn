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

namespace Jayrock.JsonRpc
{
    #region Imports

    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using Jayrock.Services;

    #endregion

    internal sealed class JsonRpcServiceReflector
    {
        private static readonly Hashtable _classByTypeCache = Hashtable.Synchronized(new Hashtable());
        
        public static ServiceClass FromType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            ServiceClass clazz = (ServiceClass) _classByTypeCache[type];

            if (clazz == null)
            {
                clazz = BuildFromType(type);
                _classByTypeCache[type] = clazz;
            }

            return clazz;
        }

        private static ServiceClass BuildFromType(Type type)
        {
            ServiceClassBuilder builder = new ServiceClassBuilder();
            BuildClass(builder, type);
            return builder.CreateClass();
        }

        private static void BuildClass(ServiceClassBuilder builder, Type type)
        {
            //
            // Build...
            //

            IServiceClassReflector reflector = (IServiceClassReflector) FindCustomAttribute(type, typeof(IServiceClassReflector), true);

            if (reflector == null)
                reflector = new JsonRpcServiceAttribute();

            reflector.Build(builder, type);

            //
            // Fault in the type name if still without name.
            //

            if (builder.Name.Length == 0)
                builder.Name = type.Name;

            //
            // Modify...
            //

            object[] modifiers = type.GetCustomAttributes(typeof(IServiceClassModifier), true);
            foreach (IServiceClassModifier modifier in modifiers)
                modifier.Modify(builder, type);
        }

        internal static bool ShouldBuild(MethodInfo method)
        {
            Debug.Assert(method != null);

            return !method.IsAbstract && method.IsDefined(typeof(IMethodReflector), true);
        }

        internal static void BuildMethod(MethodBuilder builder, MethodInfo method)
        {
            Debug.Assert(method != null);
            Debug.Assert(builder != null);

            builder.InternalName = method.Name;
            builder.ResultType = method.ReturnType;
            builder.Handler = new TypeMethodImpl(method);

            //
            // Build...
            //

            IMethodReflector reflector = (IMethodReflector) FindCustomAttribute(method, typeof(IMethodReflector), true);

            if (reflector == null)
                reflector = new JsonRpcMethodAttribute();

            reflector.Build(builder, method);

            //
            // Fault in the method name if still without name.
            //

            if (builder.Name.Length == 0)
                builder.Name = method.Name;

            //
            // Modify...
            //

            object[] attributes = method.GetCustomAttributes(typeof(Attribute), true);
            foreach (Attribute attribute in attributes)
            {
                IMethodModifier modifier = attribute as IMethodModifier;

                if (modifier != null)
                    modifier.Modify(builder, method);
                else if (!(attribute is IMethodReflector))
                    builder.AddCustomAttribute(attribute);
            }
        }

        internal static void BuildParameter(ParameterBuilder builder, ParameterInfo parameter)
        {
            Debug.Assert(parameter != null);
            Debug.Assert(builder != null);

            //
            // Build...
            //
            
            builder.Name = parameter.Name;
            builder.ParameterType = parameter.ParameterType;
            builder.Position = parameter.Position;
            builder.IsParamArray = parameter.IsDefined(typeof(ParamArrayAttribute), true);

            //
            // Modify...
            //
            
            object[] modifiers = parameter.GetCustomAttributes(typeof(IParameterModifier), true);
            foreach (IParameterModifier modifier in modifiers)
                modifier.Modify(builder, parameter);
        }

        private static object FindCustomAttribute(ICustomAttributeProvider provider, Type attributeType, bool inherit)
        {
            object[] attributes = provider.GetCustomAttributes(attributeType, inherit);
            return attributes.Length > 0 ? attributes[0] : null;
        }

        private JsonRpcServiceReflector()
        {
            throw new NotSupportedException();
        }
    }

    internal interface IServiceClassReflector
    {
        void Build(ServiceClassBuilder builder, Type type);
    }

    internal interface IServiceClassModifier
    {
        void Modify(ServiceClassBuilder builder, Type type);
    }

    internal interface IMethodReflector
    {
        void Build(MethodBuilder builder, MethodInfo method);
    }

    internal interface IMethodModifier
    {
        void Modify(MethodBuilder builder, MethodInfo method);
    }

    internal interface IParameterReflector
    {
        void Build(ParameterBuilder builder, ParameterInfo parameter);
    }

    internal interface IParameterModifier
    {
        void Modify(ParameterBuilder builder, ParameterInfo parameter);
    }
}
