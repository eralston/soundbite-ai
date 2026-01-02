$dirs = 
	".\", 
	".\packages\api\",
	".\packages\api.axios\",
	".\packages\Soundbite.Spa\",
	".\packages\Soundbite.Teams\",
	".\packages\widgets.react\",
	".\packages\widgets.react.app\",
	".\packages\widgets.react.injectable\",
	".\packages\widgets.sharepoint.app\",
	".\packages\widgets.testweb\";

foreach($dir in $dirs)
{
	Write-Host ("Processing {0}" -f $dir)
	rm ("{0}node_modules\" -f $dir) -Recurse -ErrorAction SilentlyContinue
	rm ("{0}package-lock.json" -f $dir) -ErrorAction SilentlyContinue
}

# rm .\node_modules\ -Recurse
# rm .\package-lock.json