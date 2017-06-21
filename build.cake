#tool "GitVersion.CommandLine"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var nugetApiKey = Argument("nugetapikey", EnvironmentVariable("NUGET_API_KEY"));

//////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
//////////////////////////////////////////////////////////////////////
var version = "1.0.0";
var versionNumber = "1.0.0";

var artifacts = Directory("./artifacts");
var solution = File("./src/Deploy.sln");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////
Task("Clean")
    .Does(() => 
{
    CleanDirectories("./src/**/bin");
    CleanDirectories("./src/**/obj");

    if (DirectoryExists(artifacts))
        DeleteDirectory(artifacts, true);
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() => 
{
    DotNetCoreRestore(solution);
});

Task("Versioning")
    .IsDependentOn("Clean")
    .Does(() => 
{
    if (!BuildSystem.IsLocalBuild)
    {
        GitVersion(new GitVersionSettings
        {
            OutputType = GitVersionOutput.BuildServer
        });
    }

    var result = GitVersion(new GitVersionSettings
    {
        OutputType = GitVersionOutput.Json
    });

    version = result.NuGetVersion;
    versionNumber = result.MajorMinorPatch;
});

Task("Build")
    .IsDependentOn("Versioning")
    .IsDependentOn("Restore")
    .Does(() => 
{
    CreateDirectory(artifacts);

    DotNetCoreBuild(solution, new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        ArgumentCustomization = x => x
            .Append("/p:Version={0}", version)
            .Append("/p:AssemblyVersion={0}", versionNumber)
            .Append("/p:FileVersion={0}", versionNumber)
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() => 
{
    var projects = GetFiles("./src/**/*.Tests.csproj");

    foreach (var project in projects)
    {
        DotNetCoreTest(project.FullPath);
    }
});

Task("Package")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .Does(() => 
{
    var projects = GetFiles("./src/**/*.csproj", x => !x.Path.FullPath.EndsWith("Tests"));

    foreach (var project in projects)
    {
        DotNetCorePack(project.FullPath, new DotNetCorePackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifacts,
            NoBuild = true,
            ArgumentCustomization = x => x
                .Append("/p:Version={0}", version)
        });
    }
});

Task("Publish")
    .IsDependentOn("Package")
    .Does(() =>
{
    var package = "./artifacts/Deploy." + version + ".nupkg";

    NuGetPush(package, new NuGetPushSettings
    {
        ApiKey = nugetApiKey
    });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////
Task("Default")
    .IsDependentOn("Package");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////
RunTarget(target);
