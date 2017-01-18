using System;
using System.ServiceModel;
using CAS.ConsoleDemo.SimpleConnection.CinegyArchiveService;

namespace CAS.ConsoleDemo.SimpleConnection
{
    //This sample application runs as a console app to simply connect to a CAS instance
    //and then determine the root node name (likley to be 'Root').

    //This is the simplest functional operation, and is implemented with minimal error handling
    //and as a console application to avoid any confusing GUI elements or programming patterns.

    class Program
    {
        //Below are the constant values used to specify how to connect to the Cinegy Archvie Service instance.
        //As an example, they are set to values on our internet-facing testing system.

        //if you try to connect to this service - you might get a response - but it is possible it will
        //fail at the license check stage. It is recommended to connect to a local CAS service for 
        //a proper testing experience!

        private const string CasUrl = "http://localhost:8082/ICinegyDataAccessService";
        private const string UserName = "DevDBUser";
        private const string UserPassword = "C1negySDK";
        private const string DbName = "DevDB";
        private const string DomainName = "Demo2";
        private const string DbServer = ".";    //a '.' (dot) is shorthand in SQL for 'localhost' - connecting to an instance would look like '.\sqlexpress' or '.\desktopexpress'
        private const string ApplicationName = "SDK Sample Dev App";
        private const string ApplicationGuid = "{705EADF7-EAAD-4f7c-8141-862C2C511A61}"; //this is the public developer connection string for R&D
        private const string VersionId = "1.0";
        private const string MinimumCasVersion = "1411301"; //you can define here a minimum version of CAS to support - this is the shipping version number of 10.3 CAS
        private const ConnectionType UserIdConnectionType = ConnectionType.RemoteTrusted; //use RemoteTrusted if specifying a windows user on the server, or SQL if using a DB local user

        /// <summary>
        /// The Main method - the standard entry function for a .Net console application. This method will execute other
        /// methods in order to fulfill the final goal - in this case, printing the name of the Root node in the Archive instance.
        /// </summary>
        static void Main()
        {
            //create a client instance (in this case, the addition of a service reference auto-generated this 'proxy' class) to operate with.
            CinegyDataAccessServiceClient cinegyClient = new CinegyDataAccessServiceClient();

            //CAS requires a connection context to be provided with most calls, in order to assign the correct session to any individual call.
            //This context is linked to a user login, and will determine the security context in which results are returned.
            //Identical calls to the same method varying by context may yield different results, depending on the user permissions!
            ConnectContext clientConnectContext = ConnectToCas(cinegyClient);

            //The CAS proxy object implements a number of concrete types - but any type that exists in the explorer tree structure
            //inherit from Node. The Root Node is a special object which represents the very start of the explorer tree.
            //If you wanted to draw the tree out, you would start by getting the details of the root node.
            Node rootNode = GetRootNode(cinegyClient, clientConnectContext);

            //Little bit of error checking, in case the connection has failed and the root node is null
            if(rootNode!=null)
                Console.WriteLine("Connected to CAS OK, Root Node Name: " + rootNode.name);

            //CAS will keep a connection alive for a time (the default is 5 minutes, unless altered in configuration). 
            //A heartbeat can be transmitted to let the service know the connection is still alive, and absence of this 
            //light-weight heartbeat (or other calls) will make the service terminate the connection (and invalidate the context).
            //However, if you know you are disconnecting, call the explict disconnection and then the service will 
            //return the license for that connection and invalidate the context immediately. This is very useful when configured
            //to block secondary logins from one user if another is active (just like the Desktop client).
            DisconnectFromCas(cinegyClient, clientConnectContext);

            Console.WriteLine("\n\nPress Enter to Quit.");

            Console.ReadLine();
        }

        /// <summary>
        /// This is the worker method for carrying out the actual connection to the CAS service.
        /// If the connection fails, errors are logged to the console (this should be structured exceptions in production code)
        /// </summary>
        /// <param name="cinegyClient">The instance of the proxy class to use to connect with</param>
        /// <returns>If successful, a valid ConnectContext object to be used for other stateful calls to methods</returns>
        private static ConnectContext ConnectToCas(CinegyDataAccessServiceClient cinegyClient)
        {
            ConnectContext clientContext;

            try
            {
                string errorMsg;
                int retCode;

                //All connections to CAS should be identified, allowing easier administration.
                //The applcation ID here matches a pattern for the server to parse, which includes a 
                //free-text (human friendly) applcation name, a Guid (which is used to retrieve the correct
                //license from the CAS license pool), the minimum CAS version required by the client, and a 
                //version ID string identifying the version of the client.
                string applicationId = string.Format("{0}##{1}##{2}##{3}", ApplicationName, ApplicationGuid,MinimumCasVersion, VersionId);

                //The use of an endpoint address here allows the configuration in auto-generated WCF proxies to be overriden.
                //This is very useful when the client application may need to connect to multiple endpoints based on user selection.
                cinegyClient.Endpoint.Address = new EndpointAddress(CasUrl);

                //This is the call to the actual method (via the proxy client) that will return the context.
                //Tracing through this method in debug mode will reveal other code generated by WCF that deals with making this call over HTTP
                clientContext = cinegyClient.Connect(DbServer, 0, DbName, UserIdConnectionType, UserName,
                                                     UserPassword, applicationId, DomainName, WrapperType.None,
                                                     out retCode, out errorMsg);

                //If things have failed, the return code from any CAS method will be non-zero
                if (retCode != 0)
                {
                    Console.WriteLine("Error " + retCode + " while connecting: " + errorMsg);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connection attempt threw exception: " + ex.Message);
                return null;
            }

            return clientContext;
        }

        /// <summary>
        /// A method to explicitly disconnect from CAS, returning the used license and freeing this user account to be used again
        /// </summary>
        /// <param name="cinegyClient">A valid instance of a CAS proxy client</param>
        /// <param name="clientContext">The context to be returned and invalidated</param>
        private static void DisconnectFromCas(CinegyDataAccessServiceClient cinegyClient, ConnectContext clientContext)
        {
            try
            {
                string errorMsg;
                cinegyClient.Disconnect(ref clientContext, out errorMsg);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to disconnect cleanly from CAS - don't worry, the service will timeout the connection within a few minutes anyway. Exception: " + ex.Message);
            }
        }
    
        /// <summary>
        /// The method that locates and returns the special-case 'root node'. From this node, all other nodes can be located.
        /// </summary>
        /// <param name="cinegyClient">A valid instance of a CAS proxy client</param>
        /// <param name="clientContext">The context to be returned and invalidated</param>
        /// <returns>A generic CAS Node object containing the root node details for the database the supplied context was generated from.</returns>
        private static Node GetRootNode(CinegyDataAccessServiceClient cinegyClient, ConnectContext clientContext)
        {
            string errorMsg;
            int retCode;

            //the actual root node is cast into a CAS Node class from the client special method 'GetRoot' - the main call in this wrapper method.
            Node rootNode = cinegyClient.GetRoot(clientContext, out retCode, out errorMsg);

            if (rootNode == null)
            {
                Console.WriteLine("Error " + retCode + " while trying to retrieve root node: " + errorMsg);

                return null;
            }

            return rootNode;
        }
    
    }
}
