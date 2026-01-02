$currentDir = Get-Location
$packages = @("api", "api.axios", "widgets.api", "widgets.react");

# Ensure all projects are built
npm run build

# Iterate over all of the packages and package them up
foreach($package in $packages)
{
	cd "$currentDir\packages\$package"
	npm pack
	xcopy *.tgz "..\..\dist\" /Y
	rm *.tgz
}

# Return to current directory
cd $currentDir

# Copy the widget react project over
xcopy ".\packages\widgets.react.app\build" ".\dist\@soundbite.widgets-react-app" /S /Q /R /I /Y
