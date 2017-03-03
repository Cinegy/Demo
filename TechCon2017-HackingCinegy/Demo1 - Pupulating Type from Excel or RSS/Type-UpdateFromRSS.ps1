# 
# Sample script to update Cinegy Type variables from RSS feeds
#

function RSS-GetItems([string]$url)
{
    #define the news items array
    $items = @()

    #webclient to grab RSS data from internet
    $web = New-Object Net.WebClient

    #download the data
    $serverResponse = [xml]$web.DownloadString($url)

    #parse server data
    if ($serverResponse)
    {
        #iterate through the all items
        foreach($node in $serverResponse.rss.channel.item)
        {
             #create new 'item' object
             $item = New-Object System.Object
             #assign object properties
             $item | Add-Member -type NoteProperty -name Title -value $node.Title.'#cdata-section'
             $item | Add-Member -type NoteProperty -name Description -value $node.Description
             #add 'item' object into 'items' array
             $items += $item
        }
    }

    return $items
}

#locate script root directory
$rootDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

#load helper functions
. ($rootDirectory + ".\Helpers-Excel.ps1")
. ($rootDirectory + ".\Helpers-Type.ps1")

#define Type template to be activated and Excel data to be loaded
$templatePath = $rootDirectory + ".\News.CinType"

#define Type template to be activated and Excel data to be loaded
$rssFeedsPath = $rootDirectory + ".\RSSFeeds.xml"
$newsToDispaly = 3

# import feeds from XML file
[xml]$rssFeedsXml = Get-Content $rssFeedsPath

# restart trigger - unique value to cause scene restart
$newsNumber = 0

#ensure template is visible
Type-ActivateTemplate $templatePath

# iterate through each rss feed
foreach($feed in $rssFeedsXml.rssfeeds.ChildNodes)
{
    #get feed items
    $items = RSS-GetItems $feed.url
    
    #push first 3 items to Type template
    for($i=0; $i -le $newsToDispaly; $i++)
    {
        #ensure restart trigger is acrtivated by passing new value - news number
        $newsNumber += 1
        #ensure value is cueued by avoiding duplicates as same value is ignored by Type
        if($newsNumber % 2 -eq 0) {$appender = " "} else {$appender=""}

        #push updates to Category and News variables
        Type-UpdateVariable 'Category' ($feed.name + $appender)
        Type-UpdateVariable 'News' ($items[$i].Title + $appender)
        Type-UpdateVariable 'RestartTrigger' $newsNumber

        #make sure values are applied by letting some time before next update
        Start-Sleep -m 10
    }
}