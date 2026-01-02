$npmProjects = @(
	"..\api\",
	"..\api.axios\",
	"..\widgets.api\",
	"..\widgets.react\",
	"..\widgets.react.app\");

#using invoke-expression with full npm string for dependencies if they help
	
$npmDependencies = @(
	"",
	"@soundbite/api"
	"@soundbite/api @soundbite/api-axios"
	"@soundbite/api @soundbite/api-axios @soundbite/react-widgets",
	"");
	

$index = 0;
foreach($npmProject in $npmProjects)
{
	$path = $npmProject + "dist";
	
	echo "Processing $npmProject"
	if(Test-Path $path) 
	{
		Remove-Item -Recurse -Force $path
	}
	
	# Change to the npm project directory
	cd $npmProject
	
	# Compile the project
	tsc	
	
	# Increment the index
	$index++;
	
	# Change back to the partner web
	cd ..\widgets.testweb
}

