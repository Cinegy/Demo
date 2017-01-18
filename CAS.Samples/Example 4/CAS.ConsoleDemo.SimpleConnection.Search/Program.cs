using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using CAS.ConsoleDemo.SimpleConnection.Search.CinegyArchiveService;

namespace CAS.ConsoleDemo.SimpleConnection.Search
{
    //This sample application runs as a console app and connects to a CAS instance
    //before providing an interactive user interface for performing search queries.

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
        /// methods in order to fulfill the final goal - in this case, running the main console input loop to perform a Cinegy search.
        /// </summary>
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green; //colour the console green, since it is springtime! #justforfun

            //create a client instance (in this case, the addition of a service reference auto-generated this 'proxy' class) to operate with.
            CinegyDataAccessServiceClient cinegyClient = new CinegyDataAccessServiceClient();

            //CAS requires a connection context to be provided with most calls, in order to assign the correct session to any individual call.
            //This context is linked to a user login, and will determine the security context in which results are returned.
            //Identical calls to the same method varying by context may yield different results, depending on the user permissions!
            ConnectContext clientConnectContext = ConnectToCas(cinegyClient);

            if (clientConnectContext == null)
            {
                while (Console.KeyAvailable == false) { }
                return;
            }
            
            Console.WriteLine("\nConnected to " + DbName + "\n");
            HandleConsoleInput(cinegyClient, clientConnectContext);
            DisconnectFromCas(cinegyClient, clientConnectContext);                      
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

                string applicationId = string.Format("{0}##{1}##{2}##{3}", ApplicationName, ApplicationGuid, MinimumCasVersion, VersionId);

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

        /// <summary>
        /// A method to explicitly disconnect from CAS, returning the used license and freeing this user account to be used again
        /// </summary>
        /// <param name="cinegyClient">A valid instance of a CAS proxy client</param>
        /// <param name="clientContext">The context to be returned to the pool and invalidated</param>
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
        /// The method used to allow interactive commanding of the search process. Will repeat
        /// inside a 'do' loop until the 'Q' key is pressed, whereupon it will quit the loop (ultimately
        /// leading to the application terminating).
        /// </summary>
        /// <param name="cinegyClient">The instance of the proxy class to use to connect with</param>
        /// <param name="clientContext">The active context to use for the query</param>
        private static void HandleConsoleInput(CinegyDataAccessServiceClient cinegyClient, ConnectContext clientContext)
        {
            ConsoleKeyInfo keyInfo = new ConsoleKeyInfo();

            //This is a List of node IDs for moving back and forward by the pageful in the result set. 
            //These IDs will be used as the _guid_start_id property of the SearchParameters object.
            IList<Guid> searchPageIDs = new List<Guid>();
            
            //Create an instance of the SearchWrapper class, used to command the search operation
            SearchWrapper searchWrapper = new SearchWrapper();
            SearchResult[] searchResults;
           
            //main 'do' loop for console input
            do
            {
                Console.WriteLine("\n--------------------------------------------------------");
                Console.WriteLine("Press the 's' key to start new search.");
                Console.WriteLine("Press the 'n' key to go to next search result page.");
                Console.WriteLine("Press the 'p' key to go to previous search result page.");
                Console.WriteLine("Press the 'q' key to quit\n");

                while (Console.KeyAvailable == false) { }
                    
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    //The user wants to start a new search. This will reset the wrapper, and set
                    //a new general string search paramter containing the entered text.
                    case ConsoleKey.S:
                        Console.Write("Keyword: ");
                        
                        //this list will be populated with IDs referring to every page of
                        //results that are returned.
                        searchPageIDs = new List<Guid>();
                        searchWrapper = new SearchWrapper();                                                
                        searchWrapper.Parameters._str_search = Console.ReadLine();
                        
                        Console.WriteLine("Searching...\n");
                        //Call to a local method to organise the values collected ready to run the actual search.
                        PrepareSearch(searchWrapper, searchPageIDs, SearchMode.NewSearch);

                        //Call to a local method to execute the prepared seach operation.
                        searchResults = Search(cinegyClient, clientContext, searchWrapper);

                        //Call to a local method to process the results and respond.
                        HandleSearchResults(searchResults, searchWrapper, searchPageIDs);                      
                        break;
                    
                    //Pages to the nex search page returned from CAS, so the screen is not overfilled
                    case ConsoleKey.N:                      
                        Console.WriteLine("Retrieving next Page for '" + searchWrapper.Parameters._str_search + "'....\n");
                        
                        //Again, calls to the PrepareSearch local method, but specifying the next page
                        PrepareSearch(searchWrapper, searchPageIDs, SearchMode.NextPage);
                        //And then re-executes the search method to get the filled results
                        searchResults = Search(cinegyClient, clientContext, searchWrapper);
                        //Before processing the display
                        HandleSearchResults(searchResults, searchWrapper, searchPageIDs);                      
                        break;

                    //Pages to the prvious search page returned from CAS.
                    case ConsoleKey.P:
                        Console.WriteLine("Retrieving previous Page for '" + searchWrapper.Parameters._str_search + "'....\n");
                        
                        //Same as next page, but now with a different mode (PreviousPage)
                        PrepareSearch(searchWrapper, searchPageIDs, SearchMode.PreviousPage);
                        searchResults = Search(cinegyClient, clientContext, searchWrapper);
                        HandleSearchResults(searchResults, searchWrapper, searchPageIDs);                      
                        break;

                    //Quit application.
                    case ConsoleKey.Q:
                        Console.WriteLine("Quitting...");
                        break;

                    default:
                        Console.WriteLine("Unknown Key Command");
                        break;
                }

            } while (keyInfo.Key != ConsoleKey.Q);

        }

        /// <summary>
        /// This method will set the Search Page number for search method and UI, and ensure paging is correct
        /// </summary>
        /// <param name="searchWrapper">The wrapper class containing all relevant parameters for the search.</param>
        /// <param name="searchPageIDs">The list of IDs of search pages</param>
        /// <param name="searchMode">Seach mode command detailing what action to take</param>
        private static void PrepareSearch(SearchWrapper searchWrapper, IList<Guid> searchPageIDs, SearchMode searchMode)
        {
            searchWrapper.Mode = searchMode;
            int index = 0;

            switch (searchMode)
            {
                case SearchMode.NewSearch:
                    //the start ID provides the clue to the search engine about which page is to be displayed.
                    //since the search wrapper and page ID list are reset before this call, it means
                    //that exactly 1 entry will exist, and it will be an emtpy GUID. This is required 
                    //for new searches.
                    searchPageIDs.Add(searchWrapper.Parameters._guid_start_id);      
                    break;
                case SearchMode.NextPage:
                    //This will safely increment the page index to the next item Guid.
                    //The current index of the last item of the current page should be located and incremented. 
                    index = searchPageIDs.IndexOf(searchWrapper.Parameters._guid_start_id);
                    index++;

                    //check we have not gone past the last item
                    if ((index <= 0) || (index >= searchPageIDs.Count - 1))
                    {
                        index = searchPageIDs.Count - 1;
                    }

                    //reset the ID to really be the ID of the first item of the next page
                    searchWrapper.Parameters._guid_start_id = searchPageIDs[index];
                    break;
                case SearchMode.PreviousPage:
                    //same as next, but doing the inverse
                    index = searchPageIDs.IndexOf(searchWrapper.Parameters._guid_start_id);
                    index--;

                    if ((index < 0) || (index >= searchPageIDs.Count))
                    {
                        index = 0;
                    }

                    searchWrapper.Parameters._guid_start_id = searchPageIDs[index];
                    break;
            }

            searchWrapper.CurrentPageNumber = index + 1;
        }

        /// <summary>
        /// This is the method for executing the actual search query on the server.
        /// </summary>
        /// <param name="cinegyClient">The instance of the proxy class to use to connect with</param>
        /// <param name="clientContext">The active context to use for the query</param>
        /// <param name="searchWrapper">The wrapper class containing all relevant parameters for the search.</param>
        /// <returns>An array of SearchResults populated with values for the page</returns>
        private static SearchResult[] Search(CinegyDataAccessServiceClient cinegyClient, ConnectContext clientContext, SearchWrapper searchWrapper)
        {
            try
            {
                string errorMsg;

                //Pepare the array for containing the result set.
                SearchResult[] searchResults;
                int returnCode;

                //Execute the actual search method against CAS, passing the prepared parameters. 
                cinegyClient.Search(clientContext, searchWrapper.Parameters, out searchResults, out returnCode, out errorMsg);
                Console.WriteLine("Return Code: " + returnCode);
                Console.WriteLine("Message: " + errorMsg);
                return searchResults;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Search failed. Exception: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// This is the method that renders the resulting array into the console view
        /// </summary>
        /// <param name="searchResults">An array of SearchResults populated with values for the page</param>
        /// <param name="searchWrapper">The wrapper class containing all relevant parameters for the search.</param>
        /// <param name="searchPageIDs">The list of IDs of search pages</param>
        private static void HandleSearchResults(SearchResult[] searchResults, SearchWrapper searchWrapper, IList<Guid> searchPageIDs)
        {
            if ((searchResults == null) || (searchResults.Length == 0))
            {
                Console.WriteLine("No Results");
                return;
            }

            switch (searchWrapper.Mode)
            {
                //This add the id of the last node in the current set to the list. Now the user can move from page 1 to page 2.
                case SearchMode.NewSearch:                    
                    searchPageIDs.Add(searchResults[searchResults.Length-1]._node._id._nodeid_id);
                    break;
                //If this is the last (highest) page in the list add the id of the last node in the current set to the list. Now the user can move from page n to page n+1.
                case SearchMode.NextPage:                    
                    if (!searchPageIDs.Contains(searchResults[searchResults.Length - 1]._node._id._nodeid_id))
                    {
                        searchPageIDs.Add(searchResults[searchResults.Length - 1]._node._id._nodeid_id);
                    }
                    break; 
            }
            
            Console.WriteLine("Page Number: " + searchWrapper.CurrentPageNumber + "\n");
            ShowSearchResults(searchResults, searchWrapper);
        }

        /// <summary>
        /// This is a method to show print node names of current result set to screen.
        /// </summary>
        /// <param name="searchResults">An array of SearchResults populated with values for the page</param>
        /// <param name="searchWrapper">The wrapper class containing all relevant parameters for the search.</param>
        private static void ShowSearchResults(SearchResult[] searchResults, SearchWrapper searchWrapper)
        {
            for (int index = 0; index < searchResults.Length; index++)
            {
                var searchResult = searchResults[index];
                int counter = (searchWrapper.CurrentPageNumber - 1)*searchWrapper.Parameters._nPageSize + index + 1;
                Console.WriteLine(counter + ": " + searchResult._node._type + ": " + searchResult._node.name);
            }
        }
    }

    /// <summary>
    /// This class is used to encapsulate all the parameters needed for executing a search.
    /// </summary>
    public class SearchWrapper
    {        
        public SearchParameters Parameters { get; set; }
        public int CurrentPageNumber { get; set; }
        public SearchMode Mode { get; set; }


        public SearchWrapper()
        {
            Mode = SearchMode.NewSearch;

            //SearchParamters object is needed by the CAS Search method
            Parameters = new SearchParameters()
                             {
                                 //This is the Node Guid from where search will start within the Cinegy explorer tree structure. 
                                 //If empty the whole tree will be searched. If set to a valid container-type node, only results within that 
                                 //container object will be returned.
                                 _guid_from_here = Guid.Empty,

                                 //This Node Guid indicates which result page the search method will return. It is the id of the last node id of a result page.
                                 _guid_start_id = Guid.Empty,
                                 
                                 _guidSession = new Guid(),
                                 _nIsSync = 0,

                                 //This controls how many nodes per page the search method will return. 
                                 //Larger values will increase the work done returning the paged items.
                                 _nPageSize = 5,
                                 
                                 _nTotal = 0,
                                 _nUseTotal = 0
                             };
        }
    }

    /// <summary>
    /// Simple command type used to direct the search operation to run with the appropriate mode.
    /// </summary>
    public enum SearchMode
    {
        NewSearch,
        NextPage,
        PreviousPage        
    }

}
