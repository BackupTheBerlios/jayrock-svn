<%@ WebHandler Class="JamAgainWeb.HelloWorld" %>

namespace JamAgainWeb
{
    using System;
    using System.Web;
    using JayRock.Json;
    using JayRock.Json.Rpc;
    using JayRock.Json.Rpc.Web;

    public class HelloWorld : JsonRpcHandler
    {
        [ JsonRpcMethod("greetings") ]
        public string Greetings()
        {
            return "Welcome to JayRock!";
        }
    }
}

