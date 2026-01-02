$packages = @('api','api.axios','widgets.api','widgets.react','widgets.react.app');
$startingDir = (Get-Location).Path;
	foreach($package in $packages)
	{
		Write-Host "Processing $package"
		cd ("{0}\packages\{1}" -f $startingDir, $package)
		tsc	
	}
cd $startingDir