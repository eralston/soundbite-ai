cls
$env = @("dev","test","preview","usw");
for($i=0;$i-lt$env.length;$i++)
{
	$manifest = "manifest." + $env[$i] + ".json";
	$app = "Teams.App." + $env[$i] + ".zip";
	Write-Host $manifest
	Copy-Item -Path $manifest -Destination "Application\manifest.json";
	Compress-Archive -Path .\Application\*.* -DestinationPath $app -Force
}



##Compress-Archive -Path .\Application\*.* -DestinationPath App.zip -Force