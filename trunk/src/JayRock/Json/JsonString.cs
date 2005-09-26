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

namespace Jayrock.Json
{
    #region Imports

    using System;
    using System.Globalization;
    using System.Text;

    #endregion

    internal sealed class JsonString
    {
        /// <summary>
        /// Produces a string in double quotes with backslash sequences in all
        /// the right places.
        /// </summary>
        /// <returns>A correctly formatted string for insertion in a JSON
        /// message.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Public Domain 2002 JSON.org, ported to C# by Are Bjolseth
        /// (teleplan.no) and re-adapted by Atif Aziz (www.raboof.com)</para>
        /// </remarks>
    
        public static string Enquote(string s)
        {
            if (s == null || s.Length == 0)
                return "\"\"";

            int length = s.Length;
            StringBuilder sb = new StringBuilder(length + 4);

            sb.Append('"');

            for (int index = 0; index < length; index++)
            {
                char ch = s[index];

                if ((ch == '\\') || (ch == '"') || (ch == '>'))
                {
                    sb.Append('\\');
                    sb.Append(ch);
                }
                else if (ch == '\b')
                    sb.Append("\\b");
                else if (ch == '\t')
                    sb.Append("\\t");
                else if (ch == '\n')
                    sb.Append("\\n");
                else if (ch == '\f')
                    sb.Append("\\f");
                else if (ch == '\r')
                    sb.Append("\\r");
                else
                {
                    if (ch < ' ')
                    {
                        //t = "000" + Integer.toHexString(c);
                        //string tmp = new string(ch, 1);
                        string t = "000" + ((int) ch).ToString(CultureInfo.InvariantCulture);
                        sb.Append("\\u" + t.Substring(t.Length - 4));
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                }
            }

            sb.Append('"');
            return sb.ToString();
        }

        private JsonString()
        {
            throw new NotSupportedException();
        }
    }
}