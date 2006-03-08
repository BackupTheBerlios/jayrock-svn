<%@ WebHandler Class="JayRockWeb.TemplateService" %>

namespace JayRockWeb
{
    #region Imports
    
    using System;
    using System.Web;
    using Jayrock.Json;
    using Jayrock.Json.Rpc;
    using Jayrock.Json.Rpc.Web;
    
    #endregion
    
    /// <summary>
    /// A template to be used as the starting point for your own service.
    /// </summary>

    public class TemplateService : JsonRpcHandler
    {
        /*
        [ JsonRpcMethod("greetings") ] 
        public string Greetings()
        {
            return "Welcome!";
        }
        */
    }
}
