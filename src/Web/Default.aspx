<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd\">
<html>
    <head>
        <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
        <meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1">
        <meta name="ProgId" content="VisualStudio.HTML">
        <meta name="Originator" content="Microsoft Visual Studio .NET 7.1">
        <link rel="stylesheet" type="text/css" href="default.css">
        <title>JayRock Home Page</title>
    </head>
    <body>
        <div id="Content">
            <h1>Welcome to JayRock</h1>
            <p>
                Welcome to JayRock, a modest implementation of the <a href="http://www.json-rpc.org/">JSON-RPC</a>
                protocol for the <a href="http://msdn.microsoft.com/netframework/">Microsoft .NET 
                    Framework</a> and
                <a href="http://www.asp.net/">ASP.NET</a>.
            </p>
            <h1>ASP.NET Quick Start</h1>
            <p>
                To use JayRock in your ASP.NET project, add a reference to the principal 
                assembly,
                <code>JayRock.dll</code>.
                A JSON-RPC service is best exposed using JayRock by creating an
                <a href="http://msdn.microsoft.com/library/en-us/cpguide/html/cpconhttpruntimesupport.asp">
                    ASP.NET HTTP handler</a>.
                In this
                quick start, we will create a JSON-RPC service called <code>HelloWorld</code>. 
                Begin by creating
                a file called <code>HelloWorld.ashx</code> in the root your ASP.NET 
                application.
                Add the following code to the file:
            </p>
            <pre class="code">
&lt;%@ WebHandler Class="JayRockWeb.HelloWorld" %&gt;

namespace JayRockWeb
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
</pre>
            <p>
                There are a few interesting things to note about this code. First of all, <code>HelloWorld</code>
                inherits from the <code>JayRock.Json.Rpc.Web.JsonRpcHandler</code> class. This 
                is all that is
                needed to make your service callable using the standard JSON-RPC protocol. 
                Second, the
                <code>Greetings</code> method is decorated with the <code>JsonRpcMethod</code> attribute.
                This is required to tell JayRock that your method should be callable over 
                JSON-RPC. By default,
                public methods of your class are not exposed automatically. Next the first 
                parameter to the
                <code>JsonRpcMethod</code> attribute is the client-side name of the method, 
                which in this case
                happens to be <code>greetings</code>. This is optional, and if omitted, will 
                default to the
                actual method name as it appears in the source (that is, <code>Greetings</code> 
                with the capital letter G).
                Since
                <a href="http://en.wikipedia.org/wiki/Javascript">JavaScript</a> programs 
                usually adopt the
                <a href="http://en.wikipedia.org/wiki/Camel_Case">camel case</a> naming 
                convention,
                providing an alternate and client-side version of you method's internal name 
                via the
                <code>JsonRpcMethod</code> attribute is always a good idea. You are now almost 
                ready to
                test your service. The last item needed is the addition of a few sections in 
                the
                <code>web.config</code> of your ASP.NET application:
            </p>
            <pre class="code">
&lt;configsections&gt;
    ...
    &lt;sectiongroup name=&quot;json.rpc&quot;&gt;
        &lt;section name=&quot;features&quot; type=&quot;JayRock.Json.Rpc.Web.JsonRpcFeaturesSectionHandler, JayRock&quot; /&gt;        
    &lt;/sectiongroup&gt;
    ...
&lt;/configsections&gt;
...
&lt;json.rpc&gt;
    &lt;features&gt;
        &lt;add name=&quot;rpc&quot; type=&quot;JayRock.Json.Rpc.Web.JsonRpcExecutive, JayRock&quot; /&gt;
        &lt;add name=&quot;proxy&quot; type=&quot;JayRock.Json.Rpc.Web.JsonRpcProxyGenerator, JayRock&quot; /&gt;
        &lt;add name=&quot;help&quot; type=&quot;JayRock.Json.Rpc.Web.JsonRpcHelp, JayRock&quot; /&gt;
        &lt;add name=&quot;test&quot; type=&quot;JayRock.Json.Rpc.Web.JsonRpcTester, JayRock&quot; /&gt;
    &lt;/features&gt;
&lt;/json.rpc&gt;
...
</pre>
            <p>
                The above configuration lines enable various features on top of your service.
                These features are accessed by using the feature name for the query string to
                your handler's URL, as in <code>?<span class="em">feature</span></code> (very similar to 
                <a href="http://msdn.microsoft.com/library/en-us/vbcon/html/vbtskExploringWebService.asp">how you request the
                WSDL document for an ASP.NET Web Service</a>).
                . First and foremost, there is the <code>rpc</code> feature. It is
                responsible for actually making the JSON-RPC invocation on your service.
                Without this feature, your service is JSON-RPC ready but won't be callable by
                anyone. The <code>proxy</code> feature dynamically generates JavaScript
                code for the client-side proxy. This code will contain a class that you
                can instantiate and use to call the server methods either synchronously and
                asynchronously. In an HTML page, you can import the proxy by using your
                handler's URL as the script source and using <code>?proxy</code> as the query string:</p>
<pre class="code">
&lt;script 
    type=&quot;text/javascript&quot; 
    src=&quot;http://localhost/foobar/helloworld.ashx?proxy&quot;&gt;
&lt;/script&gt;</pre>                
            <p>
                The <code>help</code> feature provides a simple
                help page in HTML that provides a summary of your service and methods
                it exposes. Finally, the <code>test</code> feature provides a simple
                way to test the methods of your service from right within the browser.
                This is exactly what we are going
                to work with next. Open up a browser window and point it to the URL of your
                ASP.NET handler. For example, if your ASP.NET application is called
                <code>foobar</code> and is running on your local machine, then type
                <code><span class="fake-a">http://localhost/foobar/helloworld.ashx</span></code>.
                You should now see a page appear that lists the methods exposed by your 
                service:</p>
            <asp:image cssclass="figure" id="HelloWorldHelpImage" runat="server" imageurl="~/images/helloworldhelp.jpg" width="800" height="600" alternatetext="HelloWorld Help" />
            <p>
                Notice that there are two methods, namely <code>greetings</code> and 
                <code>system.listMethods</code>. The <code>system.listMethods</code> is always 
                there and inheirted by all services
                that inherit from the <code>JsonRpcHandler</code> class. It provides
                <a href="http://scripts.incutio.com/xmlrpc/introspection.html">introspection
                    similar to some XML-RPC implementations</a>. As this point, you
                are looking at the help page generated by the help feature.
                Notice, though, you did not have to specify the <code>?help</code> query
                string. That's because <code>JsonRpcHandler</code> defaults to the
                help feature when it sees a plain HTTP <code><a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">GET</a></code> 
                request for your JSON-RPC service (JayRock does not supports invocations over 
                HTTP <code><a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</a></code> right now).
                You should also be able to see a link to the test page from where you
                can invoke and test each individual method. Click on this link now,
                which should yield a page similar to the one shown here:
            </p>
            <asp:image cssclass="figure" id="Image1" runat="server" imageurl="~/images/helloworldtest.jpg" width="800" height="600" alternatetext="HelloWorld Test" />
            <p>
                To see if everything
                is working correctly, select the <code>greetings</code> method from
                the drop-down list and click the button labeled <code>Test</code>. 
                You should see the string <code>"Welcome to JayRock!"</code> returned in 
                the response box of the page.
            </p>
            <h1>Samples &amp; Demos</h1>
            <p>
                You can find a number JSON-RPC methods demonstrating various features in the
                <a href="demo.ashx">supplied sample <code>Demo</code> service</a>.
            </p>
        </div>
    </body>
</html>
