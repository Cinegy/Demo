using System;
using System.ServiceModel;
using CAS.ConsoleDemo.SimpleConnection.CinegyArchiveService;

namespace CAS.ConsoleDemo.SimpleConnection
{
    //This sample application runs as a console app and connects to a CAS instance
    //before returning a few common metadata fields.

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
        /// methods in order to fulfill the final goal - in this case, printing the details of all the children of the specified node, along with any metadata fields set.
        /// </summary>
        static void Main(string[] args)
        {
            CinegyDataAccessServiceClient cinegyClient = new CinegyDataAccessServiceClient();
            
            ConnectContext clientConnectContext = ConnectToCas(cinegyClient);


            if ((args == null) || (args.Length == 0))
            {
                Console.WriteLine("No Node ID provided - please provide a GUID as a command line argument to this application.");

                //The root node cannot currently be looked up with the generic call to GetNodeByID (in the future, this will be improved) 
                //So don't try and get metadata from the root node with this sample!
                Console.WriteLine("Please note, this GUID provided must NOT be the Root Node GUID!");
                Console.WriteLine();
            }
            else
            {
                Guid nodeId;

                if (Guid.TryParse(args[0], out nodeId))
                {
                    Node node = GetNode(nodeId, cinegyClient, clientConnectContext);

                    if (node != null)
                    {
                        Console.WriteLine("[" + node.name + "]");
                        Console.WriteLine("\t" + "Type: " + node._type.ToString());
                        Console.WriteLine("\t" + "ID: " + node._id._nodeid_id.ToString());
                        Console.WriteLine("\t" + "Owner: " + node.owner);
                        Console.WriteLine();

                        //The node metadataSet is the group of metadata objects, which includes values, assigned to the node.
                        //An object with no metadata values will not have a metadata set, since empty values are not 
                        //included in the results. 
                        if (node.metadataSet != null)
                        {
                            Console.WriteLine("*** Metadata ***" );
                            Console.WriteLine();

                            //The node metadatSet has a property 'metadata' which is an array of metadata objects
                            foreach (var metadata in node.metadataSet.metadata)
                            {
                                //print out the interesting properties of the metadata object
                                Console.WriteLine("[" + metadata.internalName + "]");
                                Console.WriteLine("\t" + "Descriptor ID: " + metadata.descriptorID._nodeid_id.ToString());
                                Console.WriteLine("\t" + "Type: " + metadata.type);
                                Console.WriteLine("\t" + "Value: " + metadata.value);
                                Console.WriteLine();
                            }

                        }
                    }
                }
                else
                {
                    Console.WriteLine("Format of Node ID is not valid");
                }
            }

            //Remember, disconnect when you will not use this context again
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
           
        private static Node GetNode(Guid nodeId, CinegyDataAccessServiceClient cinegyClient, ConnectContext clientContext)
        {
            string errorMsg;
            int retCode;
            NODEID id = new NODEID();
            id._nodeid_id = nodeId;

            NodeBase nodeBase = new NodeBase()
            {
                _id = id
            };

            Node node = cinegyClient.GetNode(clientContext, nodeBase, GET_NODE_REQUEST_TYPE.NotDeleted, out retCode, out errorMsg);

            if (node == null)
            {
                Console.WriteLine("Error " + retCode + " while trying to retrieve Node: " + errorMsg);

                return null;
            }

            return node;
        }
    
    }
}
