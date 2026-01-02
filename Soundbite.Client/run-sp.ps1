
$startingDir = (Get-Location).Path;

try
{
	cd ("{0}\packages\widgets.sharepoint.app\" -f $startingDir)
	gulp serve
}
catch
{
	Write-Host ======================================================================
	Write-Host "An error occured" -f Red
	Write-Host ======================================================================
	Write-Host $_
	Write-Host ======================================================================
}
	
cd $startingDir
