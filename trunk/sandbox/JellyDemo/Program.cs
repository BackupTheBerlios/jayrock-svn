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