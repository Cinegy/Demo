#
# Helper functions to work with Type templates
#

#
# Extracts Type variables 
#
function Type-ExtractVariables([string]$templatePath)
{
    #create an empty array to hold variables
    $variables = @()

    #read Type template as XML
    [xml]$templateXml = Get-Content $templatePath

    #iterate through each variable in Titler->Public->Variables section

    #check if Titler node does exist
    if($templateXml.Titler -ne $null)
    {
        if($templateXml.Titler.Public -ne $null)
        {
            if($templateXml.Titler.Public.Variables -ne $null)
            {
                foreach($var in $templateXml.Titler.Public.Variables.ChildNodes)
                {
                    #create new 'variable' object
                    $variable = New-Object System.Object
                    #assign object properties
                    $variable | Add-Member -type NoteProperty -name Name -value $var.Name
                    $variable | Add-Member -type NoteProperty -name Value -value $var.Value
                    #add 'variable' object into 'variables' array
                    $variables += $variable
                }
            }
        }
    }
    return $variables
}

#
# Converts Variables array into XML
#
function Type-VariablesToXml($variables)
{
    $xmlDoc = New-Object System.Xml.XmlDocument;
    #create root 'variables' node
    $variablesXml = $xmlDoc.AppendChild($xmlDoc.CreateElement('variables'));
    #iterate through each variable in the array
    foreach($variable in $variables)
    {
        #create 'var' node
        $variableXml = $variablesXml.AppendChild($xmlDoc.CreateElement('var'));
        
        #set 'name' attribute
        $varName = $xmlDoc.CreateAttribute('name');
        $varName.Value = $variable.Name
        $variableXml.Attributes.Append($varName)

        #set 'value' attribute
        $varName = $xmlDoc.CreateAttribute('value');
        $varName.Value = $variable.Value
        $variableXml.Attributes.Append($varName)
    }

    return $xmlDoc
}

#
# Activate defined Type Template 
#
#
function Type-ActivateTemplate([string]$templatePath, [string]$server="localhost", [int]$instance=0, [int]$layer=0)
{
    $variables = Type-ExtractVariables $templatePath
    $variablesXml = Type-VariablesToXml $variables

    #make a Type PostRequest XML document using .Net XML document object
    $xmlDoc = New-Object System.Xml.XmlDocument;
    $decl = $xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", $null)

    #add root element for request - which is a 'PostRequest' element
    $xmlRootElem =  $xmlDoc.AppendChild($xmlDoc.CreateElement('Request'));
    $requestElement = $xmlDoc.InsertBefore($decl, $xmlDoc.DocumentElement)

    #create the first SetAttribute element (you can submit many in a request)
    $xmlSetValueElem = $xmlRootElem.AppendChild($xmlDoc.CreateElement('Event'));

    $xmlDevAttr = $xmlDoc.CreateAttribute('Device');
    $xmlDevAttr.Value = '*CG_' + $layer; 
    $xmlSetValueElem.Attributes.Append($xmlDevAttr);  

    $xmlCmdAttr = $xmlDoc.CreateAttribute('Cmd');
    $xmlCmdAttr.Value = 'SHOW'; 
    $xmlSetValueElem.Attributes.Append($xmlCmdAttr);  

    $xmlSetOp1Elem = $xmlDoc.CreateElement('Op1');
    $xmlSetOp1Elem.InnerText = $templatePath;
    $xmlSetValueElem.AppendChild($xmlSetOp1Elem);

    $xmlSetOp2Elem = $xmlDoc.CreateElement('Op2');
    $xmlSetOp2Elem.InnerText = $variablesXml.InnerXml
    $xmlSetValueElem.AppendChild($xmlSetOp2Elem);

    $xmlSetOp3Elem = $xmlDoc.CreateElement('Op3');
    $xmlSetOp3Elem.InnerText = "";
    $xmlSetValueElem.AppendChild($xmlSetOp3Elem);

    #create a .Net webclient which will be used to perform the HTTP POST
    $web = new-object net.webclient

    #Air requires that the data is in XML format and declared properly - so add the HTTP Header to state this
    $web.Headers.add("Content-Type", "text/xml; charset=utf-8")

    #perform the actual HTTP post to the IP and port (which is 5521 + instance number) of the XML data
    $url = "http://" + $server + ":" + (5521 + $instance) + "/video/command"
    $web.UploadString($url, $xmlDoc.OuterXml)
}

#
# Push variable update request to the specified instance of Cinegy Air Engine
# running on the defined host
#
function Type-UpdateVariable([string]$name, [string]$value, [string]$type = "Text", [string]$server="localhost", [int]$instance=0)
{
    #make a Type PostRequest XML document using .Net XML document object
    $xmlDoc = New-Object System.Xml.XmlDocument;

    #add root element for request - which is a 'PostRequest' element
    $xmlRootElem =  $xmlDoc.AppendChild($xmlDoc.CreateElement('PostRequest'));

    #create SetValue element and define variable Name, Type and Value
    $xmlSetValueElem = $xmlRootElem.AppendChild($xmlDoc.CreateElement('SetValue'));
    $xmlSetValueElem.SetAttribute("Name", $name);
    $xmlSetValueElem.SetAttribute("Type",$type);
    $xmlSetValueElem.SetAttribute("Value", $value);

    #create a .Net webclient which will be used to perform the HTTP POST
    $web = new-object net.webclient

    #Air requires that the data is in XML format and declared properly - so add the HTTP Header to state this
    $web.Headers.add("Content-Type", "text/xml; charset=utf-8")

    #perform the actual HTTP post to the IP and port (which is 5521 + instance number) of the XML data
    $url = "http://" + $server + ":" + (5521 + $instance) + "/postbox"
    $web.UploadString($url, $xmlDoc.OuterXml)
}
