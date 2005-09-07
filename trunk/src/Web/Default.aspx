<!doctype html public "-//w3c//dtd html 4.0 transitional//en" "http://www.w3.org/tr/html4/loose.dtd\">
<html>
    <head>
        <title>JayRock Home Page</title>
        <meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
        <meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1">
        <meta name="ProgId" content="VisualStudio.HTML">
        <meta name="Originator" content="Microsoft Visual Studio .NET 7.1">
        <link rel="stylesheet" type="text/css" href="Default.css">
    </head>
    <body>
        <div id="Content">
            <div id="Toc">
                <p id="TocTitle">Table of Contents</p>
                <ul>
                    <li><a href="#what-is">What is JayRock?</a></li>
                    <li><a href="#get-source">Downloading &amp; Compiling</a></li>
                    <li><a href="#contributing">Contributing to the Project</a></li>
                    <li><a href="#setup">Setting Up JayRock</a></li>
                    <li><a href="#quick-start">ASP.NET Quick Start</a></li>
                    <li><a href="#samples">Samples &amp; Demos</a></li>
                </ul>
            </div>
            <h1 class="h1-first"><a name="what-is">What is JayRock?</a></h1>
            <p>
                JayRock is an implementation of the 
                <a href="http://www.json-rpc.org/">JSON-RPC</a> protocol for the 
                <a href="http://msdn.microsoft.com/netframework/">Microsoft .NET
                    Framework</a> and <a href="http://www.asp.net/">ASP.NET</a>. </p>
            <h1><a name="get-source">Downloading &amp; Compiling</a></h1>
            <p>
                You can obtain the latest source of code of JayRock from the 
                <a href="http://subversion.tigris.org/">Subversion</a> repository 
                hosted at <a href="http://www.berlios.de">berliOS</a>.
                Needless to say, you will need <a href="http://subversion.tigris.org/project_packages.html">a Subversion 
                client</a> (see also <a href="http://tortoisesvn.tigris.org/">TortoiseSVN, a
                Windows Shell Extension for Subversion</a>) to access the repository.
                If you don't have a Subversion client handy and just wish to
                browse the source code, you can do so online using either
                <a href="http://svn.berlios.de/wsvn/jayrock">WebSVN</a> or 
                <a href="http://svn.berlios.de/viewcvs/jayrock">ViewCVS</a>.</p>
            <p>
                For anonymous access to the respository trunk, use
                <code>svn://svn.berlios.de/jayrock/trunk</code>. The commnad-line
                for the Subversion client would therefore be:</p>
            <p><code>svn checkout svn://svn.berlios.de/jayrock/trunk jayrock</code></p>
            <p> The third argument (<code>jayrock</code>) is the directory name where the local
                working copy will be downloaded so this can be another name if you like.</p>
            <h1><a name="contributing">Contributing to the Project</a></h1>
            <p>
                JayRock is provided as open source and free software (as per
                <a href="http://www.opensource.org/docs/definition.php">Open Source Definition</a>)
                for two principal reasons. First, an open source community provides a way for 
                individuals
                and companies to collaborate on projects that none could achieve on
                their own. Second, the open source model has the technical advantage of
                turning users into potential co-developers. With source code readily
                available, users will help you debug quickly and promote rapid code 
                enhancements.
                In short, <strong>you are absolutely encoraged to contribute!</strong>
            </p>
            <p>
                Please <a href="http://www.raboof.com/contact.aspx">contact Atif Aziz</a>
                (principal developer) if you are interested in contributing. You don't
                have to necessarily contribute in the form of code submissions only. 
                Contributions are appreciated and needed whether you can help diagnose problems,
                suggest fixes, improve the code or provide peer support on forums,
                mailing lists and newsgroups. For more information, see 
                <a href="http://developer.berlios.de/projects/jayrock/">JayRock on berliOS</a>.
            </p>
            <h1><a name="setup">Setting Up JayRock</a></h1>
            <ol>
                <li>Setup a virtual directory and application in IIS called <code>jayrock</code> that
                    points to the directory <code>src\Web</code> under your working copy of
                JayRock.
                <li>Open the Visual Studio .NET 2003 solution and compile all projects. There
                    is also a <a href="http://nant.sourceforge.net/">NAnt</a> 0.85 RC2 build
                script included, but this only
                builds the main JayRock assembly at the moment and not all other projects.
                <li>Open up a browser window (Internet Explorer and
                    FireFox tested) and navigate to the virtual root created in the first step
                    (most probably <code><span class="fake-a">http://localhost/jayrock/</span></code>).</li></ol>
            <h1><a name="quick-start">ASP.NET Quick Start</a></h1>
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
    &lt;sectiongroup name="json.rpc"&gt;
        &lt;section name="features" type="JayRock.Json.Rpc.Web.JsonRpcFeaturesSectionHandler, JayRock" /&gt;        
    &lt;/sectiongroup&gt;
    ...
&lt;/configsections&gt;
...
&lt;json.rpc&gt;
    &lt;features&gt;
        &lt;add name="rpc" type="JayRock.Json.Rpc.Web.JsonRpcExecutive, JayRock" /&gt;
        &lt;add name="proxy" type="JayRock.Json.Rpc.Web.JsonRpcProxyGenerator, JayRock" /&gt;
        &lt;add name="help" type="JayRock.Json.Rpc.Web.JsonRpcHelp, JayRock" /&gt;
        &lt;add name="test" type="JayRock.Json.Rpc.Web.JsonRpcTester, JayRock" /&gt;
    &lt;/features&gt;
&lt;/json.rpc&gt;
...
</pre>
            <p>
                The above configuration lines enable various features on top of your service.
                These features are accessed by using the feature name for the query string to
                your handler's URL, as in <code>?<span class="em">feature</span></code> (very
                similar to
                <a href="http://msdn.microsoft.com/library/en-us/vbcon/html/vbtskExploringWebService.asp">
                    how you request the
                    WSDL document for an ASP.NET Web Service</a>).
                . First and foremost, there is the <code>rpc</code> feature. It is
                responsible for actually making the JSON-RPC invocation on your service.
                Without this feature, your service is JSON-RPC ready but won't be callable by
                anyone. The <code>proxy</code> feature dynamically generates JavaScript
                code for the client-side proxy. This code will contain a class that you
                can instantiate and use to call the server methods either synchronously and
                asynchronously. In an HTML page, you can import the proxy by using your
                handler's URL as the script source and using <code>?proxy</code> as the query
                string:</p>
            <div>
                <pre class="code">
&lt;script 
    type="text/javascript" 
    src="http://localhost/foobar/helloworld.ashx?proxy"&gt;
&lt;/script&gt;</pre></div>
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
            <img class="figure" src="images/HelloWorldHelp.jpg" width="800" height="600" alt="HelloWorld Help">
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
                help feature when it sees a plain HTTP <code><a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.3">
                        GET</a></code>
                request for your JSON-RPC service (JayRock does not supports invocations over
                HTTP <code><a href="http://www.w3.org/Protocols/rfc2616/rfc2616-sec9.html#sec9.5">POST</a></code>
                right now).
                You should also be able to see a link to the test page from where you
                can invoke and test each individual method. Click on this link now,
                which should yield a page similar to the one shown here:
            </p>
            <img class="figure" src="images/HelloWorldTest.jpg" width="800" height="600" alt="HelloWorld Test">
            <p>
                To see if everything
                is working correctly, select the <code>greetings</code> method from
                the drop-down list and click the button labeled <code>Test</code>.
                You should see the string <code>"Welcome to JayRock!"</code> returned in
                the response box of the page.
            </p>
            <h1><a name="samples">Samples &amp; Demos</a></h1>
            <p>
                You can find a number JSON-RPC methods demonstrating various features in the
                supplied demo service. See <code><span class="fake-a">http://localhost/jayrock/demo.ashx</span></code>
                on your machine for a working copy of the demo.
            </p>
            <p>
                Note that some of the methods on the demo service that illustrate
                data access assume that you have a default instance of
                <a href="http://www.microsoft.com/sql/">Microsoft SQL Server 2000</a>
                running on your machihne with the Northwind database loaded.
            </p>
            <hr>
            <p>
                <a href="http://developer.berlios.de">
                    <img src="http://developer.berlios.de/bslogo.php?group_id=0" width="124" height="32" border="0" alt="BerliOS Logo"></a>
            </p>
        </div>
    </body>
</html>
