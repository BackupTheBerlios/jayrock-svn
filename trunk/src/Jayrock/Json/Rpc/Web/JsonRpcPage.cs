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
    using System.Collections;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;

    #endregion

    internal class JsonRpcPage : Page, IRpcServiceFeature
    {
        private IRpcService _targetService;
        private IRpcServiceDescriptor _serviceDescriptor;
        private IRpcMethodDescriptor[] _methods;
        private bool _serviceDescriptorInitialized;
        protected HtmlGenericControl _head;
        private HtmlGenericControl _body;

        public IRpcService TargetService
        {
            get { return _targetService; }
        }

        void IRpcServiceFeature.Initialize(IRpcService targetService)
        {
            if (_targetService != null)
                throw new InvalidOperationException();

            if (targetService == null)
                throw new ArgumentNullException("targetService");

            _targetService = targetService;
        }

        protected IRpcServiceDescriptor ServiceDescriptor
        {
            get
            {
                if (!_serviceDescriptorInitialized)
                {
                    _serviceDescriptorInitialized = true;
                    _serviceDescriptor = TargetService.GetDescriptor();
                }

                return _serviceDescriptor;
            }
        }

        protected IRpcMethodDescriptor[] SortedMethods
        {
            get
            {
                if (_methods == null)
                {
                    IRpcMethodDescriptor[] methods = ServiceDescriptor.GetMethods();
                    Array.Sort(methods, new MethodNameComparer());
                    _methods = methods;
                }

                return _methods;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            LiteralControl docType = new LiteralControl("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">");
            Controls.Add(docType);

            HtmlGenericControl html = new HtmlGenericControl("html");
            Controls.Add(html);

            _head = new HtmlGenericControl("head");
            html.Controls.Add(_head);

            _body = new HtmlGenericControl("body");
            html.Controls.Add(_body);

            HtmlGenericControl title = new HtmlGenericControl("title");
            title.InnerText = Title;
            _head.Controls.Add(title);

            base.OnInit(e);
        }

        protected Control Head
        {
            get { return _head; }
        }

        protected Control Body
        {
            get { return _body; }
        }

        protected virtual string Title
        {
            get { return ServiceDescriptor.Name; }
        }

        private sealed class MethodNameComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                IRpcMethodDescriptor methodX = (IRpcMethodDescriptor) x;
                IRpcMethodDescriptor methodY = (IRpcMethodDescriptor) y;
                return string.Compare(methodX.Name, methodY.Name, false, CultureInfo.InvariantCulture);
            }
        }
    }
}