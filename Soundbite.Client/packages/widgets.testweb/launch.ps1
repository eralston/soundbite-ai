$npmProjects = @(
	"..\Soundbite.npm.api\",
	"..\Soundbite.npm.api.axios\",
	"..\Soundbite.npm.react.widgets\",
	"..\Soundbite.npm.react.widgets.app\",
	"..\Soundbite.npm.widgets.api\");

cls
Write-Host =======================================================================
Write-Host Launching Node Applications
Write-Host =======================================================================

foreach($npmProject in $npmProjects)
{
	$path = $npmProject + "dist";
	
	echo "  Building $npmProject"
	if(Test-Path $path) 
	{
		Remove-Item -Recurse -Force $path
	}
	
	# Change to the npm project directory
	cd $npmProject
	
	# Compile the project
	tsc	
	
	# Change back to the partner web
	cd ..\Partner.Web.Node
}

$widgetApp = start powershell 'cd ..\Soundbite.npm.react.widgets.app\;npm start' -PassThru
$widgetAppId = $widgetApp.Id;
$partnerApp = start powershell 'npm start' -PassThru
$partnerAppId = $partnerApp.Id;

Write-Host
Write-Host "Press Enter to Kill Processes ..."
Read-Host

$partnerApp = Get-Process -Id $partnerAppId
$partnerApp.CloseMainWindow()
$widgetApp = Get-Process -Id $widgetAppId
$widgetApp.CloseMainWindow()