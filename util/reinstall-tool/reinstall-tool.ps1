$DebugPreference = "Continue"
$InformationPreference = "Continue"

[string]$toolProjectName = "p1exu5.testdbcontainer"

[string]$rootFolder = Join-Path $PSScriptRoot  -ChildPath "\..\.."
[string]$slnFile = Join-Path $rootFolder -ChildPath "p1eXu5.TestDbContainer.sln"

[string]$configuration = "Release"
[string]$verbosity = "minimal"
[string]$projFile = Join-Path $rootFolder -ChildPath "src\p1eXu5.TestDbContainer\p1eXu5.TestDbContainer.csproj"
[string]$nupkgFolder = Join-Path $rootFolder -ChildPath "nupkg"
[string]$nugetConfig = Join-Path $rootFolder -ChildPath "NuGet.Config"

dotnet tool uninstall --global $toolProjectName

dotnet restore $slnFile

dotnet build $slnFile `
    --configuration $configuration `
    --verbosity $verbosity


dotnet pack $projFile -c $configuration --no-build -o $nupkgFolder
 
dotnet tool install $toolProjectName --global --add-source $nupkgFolder
