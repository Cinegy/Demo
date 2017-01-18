using System;
using System.ServiceModel;
using CAS.ConsoleDemo.SimpleConnection.CinegyArchiveService;

namespace CAS.ConsoleDemo.SimpleConnection
{
    //This sample application runs as a console app to connect to a CAS instance
    //and then determine the children of the node specified by GUID on the command line.

    //This is the simplest functional operation, and is implemented with minimal error handling
    //and as a console application to avoid any confusing GUI elements or programming patterns.

    //Only when new code features are introduced are extra detailed comments added - please see earlier
    //sample programs first for more details.

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
        /// methods in order to fulfill the final goal - in this case, printing the details of all the children of the specified node.
        /// If no node ID GUID is passed as a command line argument, the details of the root node will be displayed.
        /// </summary>
        static void Main(string[] args)
        {
            CinegyDataAccessServiceClient cinegyClient = new CinegyDataAccessServiceClient();
            
            ConnectContext clientConnectContext = ConnectToCas(cinegyClient);

            //if no arguments have been passed in, provide the name and DB ID of the root node
            if ((args == null) || (args.Length == 0))
            {
                Node rootNode = GetRootNode(cinegyClient, clientConnectContext);

                if (rootNode != null)
                {
                    Console.WriteLine("Connected to CAS OK, Root Node Name: " + rootNode.name + ", Root Node ID: " + rootNode._id._nodeid_id.ToString());
                }
            }
            else
            {
                Guid parentId;

                //attempt to turn the first command line argument to a valid GUID
                
                if (Guid.TryParse(args[0], out parentId))
                {
                    //Get an array of Node objects representing the 'children' contained by this specified 'parent' node.
                    Node[] children = GetChildren(parentId, cinegyClient, clientConnectContext);
                  
                    if (children != null)
                    {
                        if (children.GetLength(0) == 0)
                        {
                            Console.WriteLine("No children nodes found - is the supplied node empty or missing?");
                            Console.WriteLine();
                        }
                        //All items that exist in the tree can be cast as Nodes, no matter what they
                        //really are (folders, clips, bins) - they all inherit from Node.
                        //This makes it simple to get key properties that are shared by 
                        //these different types.
                        foreach (Node child in children)
                        {
                            //All nodes have names - this is what is displayed to the user, for example in the explorer tree
                            Console.WriteLine("[" + child.name + "]");
                            //A node type is a concrete type which defines key behaviours and properties. Sub-types are loosely coupled, and configurable.
                            //For example, Portfolios, Programmes and Folders are all of master type 'Folder', and differ by Sub-type (which allows
                            //these items to have different names and icons).
                            Console.WriteLine("\t" + "Type: " + child._type.ToString());
                            //A node's _id value is the identity that it has in the database, and is unique. Nodes can be looked up
                            //using other methods in the client by their node _id value.
                            Console.WriteLine("\t" + "ID: " + child._id._nodeid_id.ToString());

                            Console.WriteLine();
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Format of Node ID is not valid");
                }
            }

            //Don't forget to disconnect!
            DisconnectFromCas(cinegyClient, clientConnectContext);

            Console.WriteLine("\n\nPress Enter to Quit.");

            Console.ReadLine();
        }

        private static ConnectContext ConnectToCas(CinegyDataAccessServiceClient cinegyClient)
        {
            ConnectContext clientContext;

            try
            {
                string errorMsg;
                int retCode;

                string applicationId = string.Format("{0}##{1}##{2}##{3}", ApplicationName, ApplicationGuid,MinimumCasVersion, VersionId);

                cinegyClient.Endpoint.Address = new EndpointAddress(CasUrl);
                BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);

                //in this example, we are updating various WCF parameters to allow more data to flow back and forward.
                //the defaults set by the automatic generation of the proxy are a bit low.
                //they can be altered by hand in the app.config file, or by code (shown below).
                //watch out for problems with these values being too small if you generate a fresh project and reference!
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.ReaderQuotas.MaxStringContentLength = 2147483647;
                binding.ReaderQuotas.MaxArrayLength = 2147483647;
                cinegyClient.Endpoint.Binding = binding;

                clientContext = cinegyClient.Connect(DbServer, 0, DbName, UserIdConnectionType, UserName,
                                                     UserPassword, applicationId, DomainName, WrapperType.None,
                                                     out retCode, out errorMsg);

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
    

        private static Node GetRootNode(CinegyDataAccessServiceClient cinegyClient, ConnectContext clientContext)
        {
            string errorMsg;
            int retCode;

            Node rootNode = cinegyClient.GetRoot(clientContext, out retCode, out errorMsg);

            if (rootNode == null)
            {
                Console.WriteLine("Error " + retCode + " while trying to retrieve root node: " + errorMsg);

                return null;
            }

            return rootNode;
        }

        /// <summary>
        /// This method, first introduced in this sample application, looks up a specified node and returns an array of all of it's child nodes.
        /// </summary>
        /// <param name="parentId">The database GUID of the node to look up and return the child nodes from.</param>
        /// <param name="cinegyClient">A valid instance of a CAS proxy client</param>
        /// <param name="clientContext">The context to be returned and invalidated</param>
        /// <returns>An array of child nodes, belonging to the parent node specifed.</returns>
        private static Node[] GetChildren(Guid parentId, CinegyDataAccessServiceClient cinegyClient, ConnectContext clientContext)
        {
            string errorMsg;
            int retCode;
            
            //While CinegyAS node id's are often exchanged as simple .NET Guids, they need to be cast into this 
            //special NODEID object when actually used with the proxy client methods. This object contains the 
            //actual GUID as a field.
            NODEID id = new NODEID();
            id._nodeid_id = parentId;

            //When calling the method to GetChildrenNodes, a real instance of the parent node does not need to be 
            //created - a local copy of NodeBase (from which Node inherits) can be used instead, with the crucial
            //ID value allocated. Doing this save a call just to get a real copy of the 'Node' first.
            NodeBase parentNode = new NodeBase()
            {
                _id = id
            };

            //The first interesting new method of this sample - the call to GetChildrenNode.
            //This method requires a reference to the NodeBase (which can be a previously returned instance of a Node, or a local variable of NodeBase),
            //which will be used as the parent to query.
            //The GET_NODE_REQUEST_TYPE is interesting, since it allows you to filter the results to show all, deleted or not deleted nodes. Not Deleted will
            //be the most useful in many cases.
            Node[] children = cinegyClient.GetChildrenNodes(clientContext, parentNode, GET_NODE_REQUEST_TYPE.NotDeleted, out retCode, out errorMsg);

            if (retCode != 0)
            {
                Console.WriteLine("Error " + retCode + " while trying to retrieve Children: " + errorMsg);

                return null;
            }

            return children;
        }
    
    }
}
