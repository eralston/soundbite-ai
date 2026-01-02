
# Masticore.CodeGen
For converting C# into analogous Typescript for .Net web apps

## Example
When consuming this library, you simply include the various [CodeGen____] attributes on your code and run the library. This will convert valid C# into analogous Typescript code.

The most basic approach is generating Typescript models that are 1:1 to your response objects. For instance, the UserNotifications settings class in Masticre.Models:

```CSharp
[CodeGenModel]
public class UserNotifications : IUserNotifications
{
    public bool AllowNews { get; set; }
    public bool AllowMarketing { get; set; }
    public bool AllowEmail { get; set; }
    public bool AllowSms { get; set; }
}
```

Running the tool's capabilities over the code then creates the following Typescript:

```Typescript
export interface UserNotifications extends IUserNotifications {

  allowEmail: boolean;
  allowMarketing: boolean;
  allowNews: boolean;
  allowSms: boolean;
}
```

Notice it includes a reference to the IUserNotifications, which will have also been generated due to walking type dependencies. This also works generating POST/GET/PUT/DELETE calls from a controller to make an Axios client.

## Getting Started
To use this library, add a new CLI-style executable project to your solution that includes `Masticore.CodeGen` with a basic stub to initiate:

```CSharp
namespace Masticore.CodeGen
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Runner.ParseAndProcess(args);
        }
    }
}
```

Add a post-build action or run the app via the CLI to run the tool. In addition, you likely want it to delete the previous generated to start fresh each time and run a formatting script at the end to ensure the new Typescript fits your expectations:

```
if $(ConfigurationName) == DebugWithCodeGen (
  echo "Deleting API Typescript"
  rmdir /s /q "$(SolutionDir)Soundbite.Client\packages\api\src\generated"
  echo "Generating API Typescript..."
  $(ProjectDir)/bin/DebugWithCodeGen/net6.0/Soundbite.CodeGen.exe -s CodeGen-PostBuild.json
  echo "Formatting API Typescript..."
  cd $(SolutionDir)/Soundbite.Client
  call npm run format
  echo "API Typescript Generation Successful"
)
```

The settings (passed with the `-s` command-line argument in the example above) provide locations for key resources needed to understand what code is being analyzed and converted. in the example above, the CodeGen exe is being built with a novel solution configuration named `DebugWithCodeGen`, then the exe is running from the `bin` directory of that settings. It reads in the content of `CodeGen-PostBuild.json`, which is as follows:

```Javascript
{
  "AssemblyPaths": [
    "bin\\Debug\\net6.0\\Masticore.DirectorySync.Aad.dll",
    "bin\\Debug\\net6.0\\Masticore.DirectorySync.dll",
    "bin\\Debug\\net6.0\\Masticore.DirectorySync.Okta.dll",
    "bin\\Debug\\net6.0\\Masticore.dll",
    "bin\\Debug\\net6.0\\Masticore.Entity.dll",
    "bin\\Debug\\net6.0\\Soundbite.Api.dll",
    "bin\\Debug\\net6.0\\Soundbite.dll"
  ],
  "OutputRootDir": "..\\Soundbite.Client\\packages\\api\\src\\generated",
  "ControllerOutputDir": "services",
  "ModelOutputDir": "models",
  "EnumOutputDir": "enums",
  "ControllerBaseClass": "Service",
  "XmlDocFile": "..\\Soundbite.Api\\bin\\DebugWithCodeGen\\net6.0\\Soundbite.Api.xml"
}
```

Which reflects the unique structure of the Soundbite project. Using all of the related relative paths, most critically the list of assemblies to scan to determine relevant types, Masticore.CodeGen can discover the controllers, classes, and interfaces required for the web client and generate them as needed.

It is highly recommended you:
- Make a separate solution configuration for running the CLI executable built on Masticore.CodeGen
- Make a separate project for your executable that references Masticore.CodeGen and your related projects for which you wish to generate code
- Be sure to also add the projects to your config JSON file
- Do NOT hand-modify code resulting from Masticore.CodeGen runs
- Make it obvious in the structure of the recieving Typescript project that the code is generated and should NOT be modified