param([Cinegy.Media.Linker.Strategies.ScriptOptimizationProvider]$provider)

$logger = [Cinegy.Logging.ILogger]$provider.Logger
$logger.Info("Executing Pre-Processing script")

#check if TrafficProgrammId is already defined in metadata via CinegyLink file
if($provider.Metadata.ContainsKey("src.meta.TrafficProgrammId"))
{
    $logger.Info("TrafficProgrammId is already defined {src.meta.TrafficProgrammId}: " + $provider.Metadata['src.meta.TrafficProgrammId'])
} else
{
	#initialize TrafficProgrammId with source file name
	$TrafficProgrammId = $provider.Metadata['src.shortname']
	$logger.Info("TrafficProgrammId is set to {src.shortname}: " + $TrafficProgrammId)

	#remove NRHD postfix from the TrafficProgrammId if any
	$postfixToRemove = "NRHD"
	if($TrafficProgrammId.ToUpper().EndsWith($postfixToRemove))
	{
		$TrafficProgrammId = $TrafficProgrammId.SubString(0, $TrafficProgrammId.Length-$postfixToRemove.Length)
		$logger.Info("NRHD is removed from TrafficProgrammId: " + $TrafficProgrammId)
	}

	#register TrafficProgrammId value in source metadata
	$provider.AddMetadata('src.meta.TrafficProgrammId', $TrafficProgrammId)	
	$logger.Info("TrafficProgrammId is added as {src.meta.TrafficProgrammId}: " + $provider.Metadata['src.meta.TrafficProgrammId'])
}





