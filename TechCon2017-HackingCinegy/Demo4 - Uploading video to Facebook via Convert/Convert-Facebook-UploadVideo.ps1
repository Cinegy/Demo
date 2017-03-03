param([Convert.Common.Media.Linker.Sinks.PostScriptProvider]$provider)

$logger = [Cinegy.Logging.ILogger]$provider.Logger
$logger.Info("# Executing Post-Processing script")

#locate root directory
$rootDirectory = (get-location).Path

#
# !!! PLEASE CHANGE THIS TO PROPER PATH !!!
#
#load Facebook API from <CinegyConvert_Installation_Folder>\FacebookSDK\Facebook.dll
$facebookSdkPath = $rootDirectory + "\FacebookSDK\Facebook.dll"
$logger.Info("# Loading Facebook API: " + $facebookSdkPath)
[Reflection.Assembly]::LoadFile($facebookSdkPath)

#
# !!! PLEASE CHANGE THIS TO PROPER PATH !!!
#
#load access token data from file 
$accessToken = Get-Content "C:\<YourFolderPath>\Convert-Facebook-AccessToken.txt"

#create Facebook client and set access token
$fb = New-Object Facebook.FacebookClient
$fb.AccessToken = $accessToken

#get current Facebook user info
$currentUser = $fb.Get("/me")["name"];
$logger.Info("# Logged in as: " + $currentUser)

#get first output file metdadata
$outputFile = $provider.QueryFiles()[0];
$filePath = $outputFile["dst.location"] + "\" + $outputFile["dst.name"] + $outputFile["dst.extension"]

#open file as FileStream
$logger.Info("# Uploading file: " + $filePath)
$fileStream = New-Object IO.FileStream $filePath, 'Open',  'Read', 'ReadWrite'

#initialize FacebookMediaStream for upload
$fbMedia = New-Object Facebook.FacebookMediaStream
$fbMedia.ContentType = "video/mp4"
$fbMedia.FileName = $filePath
$fbMedia.SetValue($fileStream)

#fill Facebook upload parameters
$parameters = New-Object 'system.collections.generic.dictionary[string,object]'
$parameters.Add("title", $provider.Metadata['job.name'])
$parameters.Add("description", $provider.Metadata['src.name'] + " This video is brought to you by Cinegy Convert!")
$parameters.Add("video", $fbMedia)

#upload video via API call
$response = $fb.Post("/me/videos", $parameters)
$logger.Info("# Upload completed: " + $response["id"])