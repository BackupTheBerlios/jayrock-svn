<%@ WebHandler Class="JayrockWeb.HelloWorld" Language="C#" %>

namespace JayrockWeb
{
    using System;
    using System.Web;
    using Jayrock.Json;
    using Jayrock.Json.Rpc;
    using Jayrock.Json.Rpc.Web;

    public class HelloWorld : JsonRpcHandler
    {
        [ JsonRpcMethod("greetings") ]
        public string Greetings()
        {
            return "Welcome to Jayrock!";
        }
    }
}

