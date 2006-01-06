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
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    #endregion

    internal sealed class JsonRpcTester : JsonRpcPage
    {
        protected override string Title
        {
            get { return "Test " + base.Title; }
        }

        protected override void OnLoad(EventArgs e)
        {
            AddStyles();

            Control banner = AddDiv(Body, null);
            banner.ID = "banner";

            AddGeneric(banner, "h1", null, Title);

            Control content = AddDiv(Body, null);
            content.ID = "content";
            
            string summary = JsonRpcHelpAttribute.GetText(ServiceDescriptor.AttributeProvider);

            if (summary.Length > 0)
                AddGeneric(content, "span", "service-help", summary);

            Control form = AddGeneric(content, "form");
            form.ID = "TestForm";
    
            Control selectionPara = AddPara(form, null, "Select method to test: ");
    
            HtmlSelect methodSelector = new HtmlSelect();
            methodSelector.ID = "Method";
            methodSelector.Attributes.Add("onchange", "return Method_onchange(this)");

            foreach (IRpcMethodDescriptor method in SortedMethods)
                methodSelector.Items.Add(method.Name);
    
            selectionPara.Controls.Add(methodSelector);

            HtmlInputButton testButton = new HtmlInputButton();
            testButton.ID = "Test";
            testButton.Value = "Test";
            testButton.Attributes["onclick"] = "return Test_onclick(this)";
            testButton.Attributes["accesskey"] = "T";
            selectionPara.Controls.Add(new LiteralControl(" "));
            selectionPara.Controls.Add(testButton);

            selectionPara.Controls.Add(new LiteralControl(" "));

            HyperLink helpLink = new HyperLink();
            helpLink.Text = "Help";
            helpLink.NavigateUrl = Request.FilePath + "?help";
            selectionPara.Controls.Add(helpLink);

            Control requestPara = AddPara(form, null, "Request: ");

            HtmlTextArea requestArea = new HtmlTextArea();
            requestArea.ID = "Request";
            requestArea.Rows = 10;
            requestArea.Attributes.Add("title", "Enter the array of parameters (in JSON) to send in the RPC request.");
            requestPara.Controls.Add(requestArea);
        
            Control responsePara = AddPara(form, null, "Response: ");

            HtmlTextArea responseArea = new HtmlTextArea();
            responseArea.ID = "Response";
            responseArea.Rows = 10;
            responseArea.Attributes.Add("readonly", "readonly");
            responseArea.Attributes.Add("title", "The result or error object (in JSON) from the last RPC response.");
            responsePara.Controls.Add(responseArea);

            AddScriptInclude((Request.ApplicationPath.Equals("/") ? 
                string.Empty : Request.ApplicationPath) + "/json.js");

            AddScriptBlock(@"
                var callTemplates = " + BuildCallTemplatesObject() + @";
                var theForm = null;
                var nextRequestId = 0;

                window.onload = function() 
                { 
                    theForm = document.forms[0]; 
                    Method_onchange(theForm.Method); 
                }

                function Method_onchange(sender)
                {
                    theForm.Request.value = callTemplates[sender.options[sender.selectedIndex].value];
                }

                function Test_onclick(sender)
                {
                    var form = theForm;

                    try
                    {
                        var request = { 
                            id : ++nextRequestId, 
                            method : form.Method.value, 
                            params : JSON.parse(theForm.Request.value) };
                        
                        form.Response.value = '';
                        form.Response.className = '';
                        var response = callSync(request);
                        form.Response.value = JSON.stringify(response);
                    }
                    catch (e)
                    {
                        form.Response.className = 'error';
                        form.Response.value = JSON.stringify(e);
                        alert(e.message);
                    }
                }

                function callSync(request)
                {
                    var http = window.ActiveXObject ? 
                        new ActiveXObject('Microsoft.XMLHTTP') :
                        new XMLHttpRequest();
                    http.open('POST', '" + Request.FilePath + @"', false);
                    http.setRequestHeader('Content-Type', 'text/plain; charset=utf-8');
                    http.setRequestHeader('X-JSON-RPC', request.method);
                    http.send(JSON.stringify(request));
                    if (http.status != 200)
                        throw { message : http.status + ' ' + http.statusText, toString : function() { return message; } };
                    var response = JSON.parse(http.responseText);
                    if (response.error != null) throw response.error;
                    return response.result;
                }
                ");

            base.OnLoad(e);
        }

        private JObject BuildCallTemplatesObject()
        {
            JObject info = new JObject();
            StringBuilder sb = new StringBuilder();
    
            foreach (IRpcMethodDescriptor method in ServiceDescriptor.GetMethods())
            {
                sb.Length = 0;
                sb.Append("[ ");

                IRpcParameterDescriptor[] parameters = method.GetParameters();
                
                if (parameters.Length == 0)
                {
                    sb.Append("/* void */");
                }
                else
                {
                    foreach (IRpcParameterDescriptor parameter in parameters)
                    {
                        if (parameter.Position > 0) 
                            sb.Append(", ");

                        sb.Append("/* ").Append(parameter.Name).Append(" = */ ?");
                    }
                }

                sb.Append(" ]");
                info.Put(method.Name, sb.ToString());
            }
            return info;
        }

        private static Control AddGeneric(Control parent, string tagName)
        {
            return AddGeneric(parent, tagName, null);
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

        private static Control AddDiv(Control parent, string className)
        {
            return AddGeneric(parent, "div", className);
        }

        private void AddScriptBlock(string script)
        {
            Head.Controls.Add(new LiteralControl("<script type='text/javascript'>" + script + "</script>"));
        }

        private void AddScriptInclude(string url)
        {
            Head.Controls.Add(new LiteralControl("<script type='text/javascript' src='" + url + "'></script>"));
        }

        private void AddStyles()
        {
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
                }

                #content {
                    margin: 1em;
                    font-size: 0.7em;
                }

                #Request, #Response {
                    display: block;
                    margin-top: 0.5em;
                    width: 90%;
                }

                #Response.error {
                    color: red;
                }
                ");

            style.Attributes["type"] = "text/css";
        }
    }
}