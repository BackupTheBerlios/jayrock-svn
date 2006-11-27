namespace Jelly
{
    #region Imports

    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Web.Services.Protocols;
    using Jayrock.Json;
    using Jayrock.Json.Conversion.Export;
    using Jayrock.Json.Conversion.Import;

    #endregion

    public class JsonRpcClient : HttpWebClientProtocol
    {
        private int _id;

        public virtual object Invoke(string method, params object[] args)
        {
            WebRequest request = GetWebRequest(new Uri(Url));
            request.Method = "POST";
            
            using (Stream stream = request.GetRequestStream())
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                JsonObject call = new JsonObject();
                call["id"] = ++_id;
                call["method"] = method;
                call["params"] = args;
                call.Export(new JsonTextWriter(writer));
            }
            
            using (WebResponse response = GetWebResponse(request))
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                JsonObject answer = new JsonObject();
                answer.Import(new JsonTextReader(reader));

                object errorObject = answer["error"];
            
                if (errorObject != null)
                    OnError(errorObject);
            
                return answer["result"];
            }
        }

        protected virtual void OnError(object errorObject) 
        {
            JsonObject error = errorObject as JsonObject;
                        
            if (error != null)
                throw new Exception(error["message"] as string);
                        
            throw new Exception(errorObject as string);
        }
    }
}
