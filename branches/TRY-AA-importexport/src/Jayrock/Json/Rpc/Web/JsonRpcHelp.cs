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

namespace Jayrock.Json.Rpc.Web
{
    #region Imports

    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    #endregion

    internal sealed class JsonRpcHelp : JsonRpcPage
    {
        public JsonRpcHelp(IService service) : 
            base(service) {}

        protected override void AddHeader()
        {
            Control header = AddDiv(Body, null);
            header.ID = "Header";

            AddGeneric(header, "h1", null, Title);

            base.AddHeader();
        }

        protected override void AddContent()
        {
            Control content = AddDiv(Body, null);
            content.ID = "Content";
            
            if (ServiceClass.Description.Length > 0)
                AddPara(content, "service-help", ServiceClass.Description);

            Control para = AddPara(content, "intro", null);
            AddLiteral(para, "The following ");
            AddLink(para, "JSON-RPC", "http://www.json-rpc.org/");
            AddLiteral(para, " methods are supported (try these using the ");
            AddLink(para, JsonRpcServices.GetServiceName(Service) + " test page", Request.FilePath + "?test");
            AddLiteral(para, "):");

            HtmlGenericControl methodList = new HtmlGenericControl("dl");
            content.Controls.Add(methodList);

            foreach (JsonRpcMethod method in SortedMethods)
                AddMethod(methodList, method);

            base.AddContent ();
        }

        private void AddMethod(Control parent, JsonRpcMethod method)
        {
            JsonRpcObsoleteAttribute obsolete = (JsonRpcObsoleteAttribute) method.FindFirstCustomAttribute(typeof(JsonRpcObsoleteAttribute));
            
            Control methodTerm = AddGeneric(parent, "dt", obsolete == null ? "method" : "method obsolete-method");
            AddSpan(methodTerm, "method-name", method.Name);
            AddSignature(methodTerm, method);

            if (method.Description.Length > 0 || obsolete != null)
            {
                AddGeneric(parent, "dd", "method-summary", method.Description);

                if (obsolete != null)
                    AddSpan(parent, "obsolete-message", " This method has been obsoleted. " + obsolete.Message);
            }
        }

        private static void AddSignature(Control parent, JsonRpcMethod method)
        {
            Control methodSignatureSpan = AddSpan(parent, "method-sig", null);
            AddSpan(methodSignatureSpan, "method-param-open", "(");
    
            JsonRpcParameter[] parameters = method.GetParameters();
            foreach (JsonRpcParameter parameter in parameters)
            {
                if (parameter.Position > 0)
                    AddSpan(methodSignatureSpan, "method-param-delim", ", ");

                AddSpan(methodSignatureSpan, "method-param", parameter.Name);
            }
    
            AddSpan(methodSignatureSpan, "method-param-close", ")");
        }

        private static Control AddGeneric(Control parent, string tagName, string className)
        {
            return AddGeneric(parent, tagName, className, null);
        }

        private static Control AddGeneric(Control parent, string tagName, string className, string innerText)
        {
            HtmlGenericControl control = new HtmlGenericControl(tagName);
            
            if (Mask.NullString(className).Length > 0) 
                control.Attributes["class"] = className;
            
            if (Mask.NullString(innerText).Length > 0) 
                control.InnerText = innerText;
            
            parent.Controls.Add(control);
            return control;
        }

        private static Control AddPara(Control parent, string className, string innerText)
        {
            return AddGeneric(parent, "p", className, innerText);
        }

        private static Control AddSpan(Control parent, string className, string innerText)
        {
            return AddGeneric(parent, "span", className, innerText);
        }
    
        private static Control AddDiv(Control parent, string className)
        {
            return AddGeneric(parent, "div", className);
        }

        private Literal AddLiteral(Control parent, string text)
        {
            Literal literal = new Literal();
            literal.Text = Server.HtmlEncode(text);
            parent.Controls.Add(literal);
            return literal;
        }

        private HyperLink AddLink(Control parent, string text, string url)
        {
            HyperLink link = new HyperLink();
            link.Text = Server.HtmlEncode(text);
            link.NavigateUrl = url;
            parent.Controls.Add(link);
            return link;
        }

        protected override void AddStyleSheet()
        {
            base.AddStyleSheet();

            HtmlGenericControl style = (HtmlGenericControl) AddGeneric(Head, "style", null, @"
                @media screen {
                    body { 
                        margin: 0; 
                        font-family: arial;
                        font-size: small;
                    }

                    h1 { 
                        color: #FFF; 
                        font-size: large; 
                        padding: 0.5em;
                        background-color: #003366; 
                        margin-top: 0;
                    }

                    #Content {
                        margin: 1em;
                    }

                    dt {
                        margin-top: 0.5em;
                    }

                    dd {
                        margin-left: 2.5em;
                    }

                    .method {
                        font-size: small;
                        font-family: Monospace;
                    }

                    .method-name {
                        font-weight: bold;
                        color: #000080;
                    }

                    .method-param {
                        color: #404040;
                    }

                    .obsolete-message {
                        color: #FF0000;
                    }
                }");

            style.Attributes["type"] = "text/css";
        }
    }
}
