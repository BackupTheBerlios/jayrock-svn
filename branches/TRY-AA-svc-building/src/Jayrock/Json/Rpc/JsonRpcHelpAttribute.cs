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
    using System.Reflection;

    #endregion

    [ Serializable ]
    [ AttributeUsage(AttributeTargets.All) ]
    public sealed class JsonRpcHelpAttribute : Attribute, IServiceClassBuilderAttribute, IMethodBuilderAttribute
    {
        private string _text;

        public JsonRpcHelpAttribute() {}

        public JsonRpcHelpAttribute(string text)
        {
            _text = text;
        }

        public string Text
        {
            get { return Mask.NullString(_text); }
            set { _text = value; }
        }

        public static JsonRpcHelpAttribute Get(ICustomAttributeProvider attributeProvider)
        {
            if (attributeProvider == null)
                return null;

            return (JsonRpcHelpAttribute) CustomAttribute.Get(attributeProvider, typeof(JsonRpcHelpAttribute));
        }
    
        internal static string GetText(ICustomAttributeProvider attributeProvider)
        {
            if (attributeProvider == null)
                return string.Empty;

            JsonRpcHelpAttribute attribute = Get(attributeProvider);
            return attribute != null ? attribute.Text : string.Empty;
        }

        void IServiceClassBuilderAttribute.Build(JsonRpcServiceClass.Builder builder, Type type)
        {
            builder.Description = Text;
        }

        void IMethodBuilderAttribute.Build(JsonRpcMethod.Builder builder, MethodInfo method)
        {
            builder.Description = Text;
        }
    }
}
