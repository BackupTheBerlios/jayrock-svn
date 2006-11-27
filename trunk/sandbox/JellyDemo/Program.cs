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

namespace JellyDemo
{
    #region Imports

    using System;
    using System.Collections;
    using System.Net;
    using Jelly;

    #endregion

    internal sealed class Program
    {
        [ STAThread ]
        static void Main()
        {
            JsonRpcClient client = new JsonRpcClient();
            client.Url = "http://www.raboof.com/projects/jayrock/demo.ashx";
            Console.WriteLine(client.Invoke("system.about"));
            Console.WriteLine(client.Invoke("system.version"));
            Console.WriteLine(string.Join(Environment.NewLine, (string[]) (new ArrayList((ICollection) client.Invoke("system.listMethods"))).ToArray(typeof(string))));
            Console.WriteLine(client.Invoke("now"));
            Console.WriteLine(client.Invoke("sum", 123, 456));
            Console.WriteLine(client.Invoke("total", new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
            client.CookieContainer = new CookieContainer();
            Console.WriteLine(client.Invoke("counter"));
            Console.WriteLine(client.Invoke("counter"));
        }
    }
}