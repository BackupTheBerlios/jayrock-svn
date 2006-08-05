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

    internal sealed class JsonRpcHelp : JsonRpcPage, IRpcServiceFeature
    {
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
            
            string summary = JsonRpcHelpAttribute.GetText(ServiceClass.AttributeProvider);
            
            if (summary.Length > 0)
                AddPara(content, "service-help", summary);

            Control para = AddPara(content, "intro", null);
            AddLiteral(para, "The following ");
            AddLink(para, "JSON-RPC", "http://www.json-rpc.org/");
            AddLiteral(para, " methods are supported (try these using the ");
            AddLink(para, JsonRpcServices.GetServiceName(TargetService) + " test page", Request.FilePath + "?test");
            AddLiteral(para, "):");

            HtmlGenericControl methodList = new HtmlGenericControl("dl");
            content.Controls.Add(methodList);

            foreach (IRpcMethod method in SortedMethods)
                AddMethod(methodList, method);

            base.AddContent ();
        }

        private void AddMethod(Control parent, IRpcMethod method)
        {
            JsonRpcObsoleteAttribute obsoleteAttribute = JsonRpcObsoleteAttribute.Get(method.AttributeProvider);

            Control methodTerm = AddGeneric(parent, "dt", obsoleteAttribute == null ? "method" : "method obsolete-method");
            AddSpan(methodTerm, "method-name", method.Name);
            AddSignature(methodTerm, method);

            string summary = JsonRpcHelpAttribute.GetText(method.AttributeProvider);

            if (summary.Length > 0 || obsoleteAttribute != null)
            {
                AddGeneric(parent, "dd", "method-summary", summary);

                if (obsoleteAttribute != null)
                    AddSpan(parent, "obsolete-message", " This method has been obsoleted. " + obsoleteAttribute.Message);
            }
        }

        private static void AddSignature(Control parent, IRpcMethod method)
        {
            Control methodSignatureSpan = AddSpan(parent, "method-sig", null);
            AddSpan(methodSignatureSpan, "method-param-open", "(");
    
            IRpcParameter[] parameters = method.GetParameters();
            foreach (IRpcParameter parameter in parameters)
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
                body { 
                    margin: 0; 
                    font-family: verdana;
                }

                h1 { 
                    color: #ffffff; 
                    font-family: Tahoma; 
                    font-size: 150%; 
                    font-weight: normal; 
                    padding: 0.5em;
                    background-color: #003366; 
                    margin-top: 0;
                }

                #Content {
                    margin: 1em;
                    font-size: 0.7em;
                }

                dt {
                    margin-top: 0.5em;
                }

                dd {
                    margin-left: 2.5em;
                }

                .method {
                    font-size: 1.2em;
                    font-family: Courier New, Courier, Monospace;
                }

                .method-name {
                    font-weight: bold;
                    color: navy;
                }

                .method-param {
                    color: #808080;
                }

                .obsolete-message {
                    color: red;
                }");

            style.Attributes["type"] = "text/css";
        }
    }
}
