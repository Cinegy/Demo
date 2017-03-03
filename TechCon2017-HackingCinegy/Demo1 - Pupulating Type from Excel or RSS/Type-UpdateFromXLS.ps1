# 
# Sample script to update Cinegy Type variables from Excel file
#

#locate script root directory
$rootDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent

#load helper functions
. ($rootDirectory + ".\Helpers-Excel.ps1")
. ($rootDirectory + ".\Helpers-Type.ps1")

#define Type template to be activated and Excel data to be loaded
$templatePath = $rootDirectory + ".\News.CinType"
$excelDataPath = $rootDirectory + ".\SampleNewsStories.xlsx"

# import data from Excel file
$newsData = Excel-ImportData $excelDataPath

# restart trigger - unique value to cause scene restart
$newsNumber = 0

#ensure template is visible
Type-ActivateTemplate $templatePath

# iterate through each news line and post updates to the variables
foreach($news in $newsData)
{
    #ensure restart trigger is acrtivated by passing new value - news number
    $newsNumber += 1
    #ensure value is cueued by avoiding duplicates as same value is ignored by Type
    if($newsNumber % 2 -eq 0) {$appender = " "} else {$appender=""}

    #push updates to Category and News variables
    Type-UpdateVariable 'Category' ($news.Category + $appender)
    Type-UpdateVariable 'News' ($news.News + $appender)
    Type-UpdateVariable 'RestartTrigger' $newsNumber

    #make sure values are applied by letting some time before next update
    Start-Sleep -m 10
}