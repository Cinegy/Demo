#read city list from XML
[xml]$cities = Get-Content "CityList.xml"

#webclient to grab RSS data from internet
$client = New-Object Net.WebClient

while (1) {
ForEach ($city In $cities.CityList.ChildNodes) 
{   
    #make a Type PostRequest XML document using .Net XML document object
    $xmlDoc = New-Object System.Xml.XmlDocument;

    #add root element for request - which is a 'PostRequest' element
    $xmlRootElem =  $xmlDoc.AppendChild($xmlDoc.CreateElement('PostRequest'));

    $url = "http://weather.yahooapis.com/forecastrss?w=" + $city.LocID + "&u=c"
    $serverResponse = "";
    $serverResponse = [xml]$client.DownloadString($url)
    
    $saveFile = ".\\Saved\\" + $city.Abbreviation + ".xml"
    
    if ($serverResponse)
    {
        if($serverResponse.rss.channel.item) 
        {
            Out-File -filepath $saveFile -InputObject  $serverResponse.OuterXml

            [xml]$feed = Get-Content $saveFile

            #if we received the city information we update the template values
            if ($feed.rss.channel.Item.condition.temp)
            {
                #create the first SetAttribute element (you can submit many in a request)
                $xmlSetValueElem = $xmlRootElem.AppendChild($xmlDoc.CreateElement('SetValue'));

                #SetAttribute elements must have 3 attributes, the name, type and value
                $xmlSetValueElem.SetAttribute("Name", 'City');
                $xmlSetValueElem.SetAttribute("Type",'Text');
                $xmlSetValueElem.SetAttribute("Value", $city.Name);
	
                $xmlSetValueElem = $xmlRootElem.AppendChild($xmlDoc.CreateElement('SetValue'));
                $xmlSetValueElem.SetAttribute("Name",'Temp');
                $xmlSetValueElem.SetAttribute("Type",'Text');
                $xmlSetValueElem.SetAttribute("Value", $feed.rss.channel.Item.condition.temp.ToString()+' C');
	
                Out-File -filepath ".\\Saved\\PostXML-Log.xml" -InputObject  $xmlDoc.OuterXml
    
                #create a .Net webclient which will be used to perform the HTTP POST
                $web = new-object net.webclient

                #Air requires that the data is in XML format and declared properly - so add the HTTP Header to state this
                $web.Headers.add("Content-Type", "text/xml; charset=utf-8")

                #perform the actual HTTP post to the IP and port (which is 5521 + instance number) of the XML data
                $web.UploadString("http://localhost:5521/postbox", $xmlDoc.OuterXml)

				#wait five seconds before show the next city 
                sleep(5);
            }
            else
            {
                Write-Host ("*** Err: No information fooud: "+ $city.Name);
            }

       }
    }
 }
 }

