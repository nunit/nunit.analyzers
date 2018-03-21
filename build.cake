#tool nuget:?package=NUnit.ConsoleRunner&version=3.8.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var TEST_BIN_DIR = PROJECT_DIR + "src/nunit.analyzers.tests/bin/" + configuration + "/";

// Solution
var SOLUTION_FILE = PROJECT_DIR + "src/" + "nunit.analyzers.sln";

// Test Assembly
var TEST_FILE = TEST_BIN_DIR + "net461/nunit.analyzers.tests.dll3";

// Package sources for nuget restore
var PACKAGE_SOURCE = new string[]
{
    "https://www.nuget.org/api/v2",
};

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

//Task("Clean")
//    .Does(() =>
//{
//    CleanDirectory(BIN_DIR);
// });


//////////////////////////////////////////////////////////////////////
// RESTORE NUGET PACKAGES
//////////////////////////////////////////////////////////////////////

Task("RestorePackages")
    .Does(() =>
{
    NuGetRestore(SOLUTION_FILE, new NuGetRestoreSettings
    {
        Source = PACKAGE_SOURCE,
        Verbosity = NuGetVerbosity.Detailed
    });
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
//    .IsDependentOn("Clean")
    .IsDependentOn("RestorePackages")
    .Does(() =>
    {
        // Use MSBuild
        MSBuild(SOLUTION_FILE, new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetVerbosity(Verbosity.Minimal)
            .SetNodeReuse(false)
            .SetPlatformTarget(PlatformTarget.MSIL)
        );
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        NUnit3(TEST_FILE, new NUnit3Settings());
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
