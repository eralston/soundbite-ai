# Script used to update the UI version from Azure pipelines.  Do not modify or move this file 
# without coordinating the appropriate Azure pipeline updates as well.

$content = Get-Content ".\Soundbite.Client\packages\Soundbite.Spa\public\index.html";
$m = $content | Select-String -Pattern ".*window.sbVerInfo = '';";
# Make sure that the line was found before proceeding
if($m -eq $null)
{ 
	throw "Failed to locate the Soundbite Version information line expected in the index.html file."; 
}            
$content = $content.replace($m.Line, ('        window.sbVerInfo = " - ' + (Get-Date -Format "yyyy.MM.dd") + '"'));
Set-Content ".\Soundbite.Client\packages\Soundbite.Spa\public\index.html" $content;