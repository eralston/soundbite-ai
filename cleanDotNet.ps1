$currentPath = Split-Path $MyInvocation.MyCommand.Path -Parent
Write-Host $currentPath
$folders = Get-ChildItem | ?{ $_.PSIsContainer }

foreach($folder in $folders)
{
	if((test-path "$currentPath\$folder\bin") -eq $true)
	{
		Remove-Item -Recurse -Force "$currentPath\$folder\bin"
		Write-Host "Removing $currentPath\$folder\bin"
	}
	
	if((test-path "$currentPath\$folder\obj") -eq $true)
	{
		Remove-Item -Recurse -Force "$currentPath\$folder\obj"
		Write-Host "Removing $currentPath\$folder\obj"
	}
}


