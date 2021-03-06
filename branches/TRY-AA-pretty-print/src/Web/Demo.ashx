<%@ WebHandler Class="JayrockWeb.DemoService" Language="C#" %>

namespace JayrockWeb
{
    using System;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Collections;
    using System.Web;
    using System.Web.SessionState;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Jayrock.Json;
    using Jayrock.Json.Rpc;
    using Jayrock.Json.Rpc.Web;

    [ JsonRpcHelp("This is JSON-RPC service that demonstrates the basic features of the Jayrock library.") ]    
    public class DemoService : JsonRpcHandler, IRequiresSessionState 
    {
        [ JsonRpcMethod("echo")]
        [ JsonRpcHelp("Echoes back the text sent as input.") ]
        public string Echo(string text)
        {
            return text;
        }

        [ JsonRpcMethod("echoObject")]
        [ JsonRpcHelp("Echoes back the object sent as input.") ]
        public object EchoOject(object o)
        {
            return o;
        }

        [ JsonRpcMethod("echoArgs")]
        [ JsonRpcHelp("Echoes back the arguments sent as input. This method demonstrates variable number of arguments.") ]
        public object EchoArgs([ JsonRpcParams ] object[] args)
        {
            return args;
        }

        [ JsonRpcMethod("echoAsStrings")]
        [ JsonRpcHelp("Echoes back the arguments as an array of strings. This method demonstrates working with variable number of arguments.") ]
        public object EchoAsStrings([ JsonRpcParams ] object[] args)
        {
            string[] strings = new string[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] != null)
                    strings[i] = args[i].ToString();
            }
                
            return strings;
        }

        [ JsonRpcMethod("sum")]
        [ JsonRpcHelp("Return the sum of two integers.") ]
        [ JsonRpcObsolete("Use the total method instead.") ]
        public int Sum(int a, int b)
        {
            return a + b;
        }
        
        [ JsonRpcMethod("getStringArray")]
        [ JsonRpcHelp("Returns an array of city names. Demonstrates returning a strongly-typed array.") ]
        public string[] GetCities()
        {
            return new string[] { "London", "Zurich", "Paris", "New York" };
        }
        
        [ JsonRpcMethod("now")]
        [ JsonRpcHelp("Returns the local time on the server. Demonstrates how DateTime is returned simply as a string using the ISO 8601 format.") ]
        public DateTime Now()
        {
            return DateTime.Now;
        }

        [ JsonRpcMethod("newGuid")]
        [ JsonRpcHelp("Generates and returns a GUID as a string.") ]
        public Guid NewGuid()
        {
            return Guid.NewGuid();
        }

        [ JsonRpcMethod("cookies")]
        [ JsonRpcHelp("Returns the cookie names seen by the server.") ]
        public HttpCookieCollection Cookies()
        {
            return Request.Cookies;
        }
        
        [ JsonRpcMethod("getAuthor")]
        [ JsonRpcHelp("Returns information about the author. Demonstrates how a Hashtable from the server is automatically converted into an object on the client-side.") ]
        public IDictionary GetAuthor()
        {
            Hashtable author = new Hashtable();
            author["FirstName"] = "Atif";
            author["LastName"] = "Aziz";
            return author;
        }
        
        [ JsonRpcMethod("getDataSet")]
        [ JsonRpcHelp("Returns the Northwind employees as a DataSet.") ]
        public DataSet GetEmployeeSet()
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationSettings.AppSettings["NorthwindConnectionString"]))
            {
                SqlDataAdapter a = new SqlDataAdapter();
                a.SelectCommand = new SqlCommand("SELECT EmployeeID, LastName, FirstName, Title, TitleOfCourtesy, BirthDate, HireDate, Address, City, Region, PostalCode, Country, HomePhone, Extension, Notes, ReportsTo, PhotoPath FROM Employees", connection);
                DataSet ds = new DataSet();
                a.Fill(ds, "Employee");
                return ds;
            }
        }
        
        [ JsonRpcMethod("getDataTable")]
        [ JsonRpcHelp("Returns the Northwind employees as a DataTable.") ]
        public DataTable GetEmployeeTable()
        {
            return GetEmployeeSet().Tables[0];
        }

        [ JsonRpcMethod("getRowArray")]
        [ JsonRpcHelp("Returns the Northwind employees as an array of DataRow objects.") ]
        public DataRow[] GetEmployeeRowArray()
        {
            return GetEmployeeSet().Tables[0].Select();
        }

        [ JsonRpcMethod("getRowCollection")]
        [ JsonRpcHelp("Returns the Northwind employees as a DataRowCollection.") ]
        public DataRowCollection GetEmployeeRows()
        {
            return GetEmployeeSet().Tables[0].Rows;
        }

        [ JsonRpcMethod("getDataView")]
        [ JsonRpcHelp("Returns the Northwind employees as a DataView object.") ]
        public DataView GetEmployeeView()
        {
            return GetEmployeeSet().Tables[0].DefaultView;
        }

        [ JsonRpcMethod("getFirstDataRow")]
        [ JsonRpcHelp("Returns the first Northwind employee as a DataRow object.") ]
        public DataRow GetFirstEmployeeRow()
        {
            return GetEmployeeSet().Tables[0].Rows[0];
        }

        [ JsonRpcMethod("getFirstDataRowView")]
        [ JsonRpcHelp("Returns the first Northwind employee as a DataRowView object.") ]
        public DataRowView GetFirstEmployeeRowView()
        {
            return GetEmployeeSet().Tables[0].DefaultView[0];
        }

        [ JsonRpcMethod("getDropDown")]
        [ JsonRpcHelp("Returns a data-bound DropDownList to the client as HTML.") ]
        public Control EmployeeDropDown()
        {
            DropDownList ddl = new DropDownList();
            DataSet ds = GetEmployeeSet();
            ds.Tables[0].Columns.Add("FullName", typeof(string), "FirstName + ' ' + LastName");
            ddl.DataSource = ds;
            ddl.DataMember = "Employee";
            ddl.DataTextField = "FullName";
            ddl.DataValueField = "EmployeeID";
            ddl.DataBind();
            return ddl;
        }
        
        [ JsonRpcMethod("getDataGrid")]
        [ JsonRpcHelp("Returns a data-bound DataGrid to the client as HTML.") ]
        public Control EmployeeDataGrid()
        {
            DataGrid grid = new DataGrid();
            grid.DataSource = GetEmployeeSet();
            grid.DataBind();
            return grid;
        }

        [ JsonRpcMethod("total")]
        [ JsonRpcHelp("Returns the total of all integers sent in an array.") ]
        public int Total(JArray values)
        {
            int total = 0;
            
            foreach (object value in values)
                total += Convert.ToInt32(value);
            
            return total;
        }
        
        [ JsonRpcMethod("sleep") ]
        [ JsonRpcHelp("Blocks the request for the specified number of milliseconds (maximum 7 seconds).") ]
        public void Sleep(int milliseconds)
        {
            System.Threading.Thread.Sleep(Math.Min(7000, milliseconds));
        }
        
        [ JsonRpcMethod("throwError")]
        [ JsonRpcHelp("Throws an error if you try to call this method.") ]
        public void ThrowError()
        {
            throw new ApplicationException();
        }

        [ JsonRpcMethod("format")]
        [ JsonRpcHelp("Formats placeholders in a format specification with supplied replacements. This method demonstrates fixed and variable arguments.") ]
        public string Format(string format, [ JsonRpcParams ] object[] args)
        {
            return string.Format(format, args);
        }
        
        [ JsonRpcMethod("counter")]
        [ JsonRpcHelp("Increments a counter and returns its new value. Demonstrates use of session state.") ]
        public int SessionCounter()
        {
            int counter = 0;
            object counterObject = Session["Counter"];
            if (counterObject != null)
                counter = (int) counterObject;
            Session["Counter"] = ++counter;
            return counter;
        }
    }
}
