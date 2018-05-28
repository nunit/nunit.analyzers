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
var SRC_DIR = PROJECT_DIR + "src/";

var PLAYGROUND_OUTPUT_DIR = SRC_DIR + "nunit.analyzers.playground/bin/";
var ANALYZERS_TESTS_OUTPUT_DIR = SRC_DIR + "nunit.analyzers.tests/bin/";
var ANALYZERS_OUTPUT_DIR = SRC_DIR + "nunit.analyzers/bin/";

// Solution
var SOLUTION_FILE = PROJECT_DIR + "src/nunit.analyzers.sln";

// Test Assembly
var TEST_FILE = ANALYZERS_TESTS_OUTPUT_DIR + configuration + "/net461/nunit.analyzers.tests.dll";

// Package sources for nuget restore
var PACKAGE_SOURCE = new string[]
{
    "https://www.nuget.org/api/v2",
};

//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(PLAYGROUND_OUTPUT_DIR);
        CleanDirectory(ANALYZERS_TESTS_OUTPUT_DIR);
        CleanDirectory(ANALYZERS_OUTPUT_DIR);
    });


//////////////////////////////////////////////////////////////////////
// RESTORE NUGET PACKAGES
//////////////////////////////////////////////////////////////////////

Task("RestorePackages")
    .Does(() =>
    {
        NuGetRestore(SOLUTION_FILE, new NuGetRestoreSettings
        {
            Source = PACKAGE_SOURCE,
        });
    });

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
    .IsDependentOn("RestorePackages")
    .Does(() =>
    {
        MSBuild(SOLUTION_FILE, new MSBuildSettings()
            .SetConfiguration(configuration)
            .SetVerbosity(Verbosity.Minimal)
            .SetNodeReuse(false)
            .SetPlatformTarget(PlatformTarget.MSIL)
            .SetMSBuildPlatform(MSBuildPlatform.x86)
        );
    });

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        NUnit3(TEST_FILE);
    });


//////////////////////////////////////////////////////////////////////
// Pack
//////////////////////////////////////////////////////////////////////

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
    {
        NuGetPack("./src/nunit.analyzers/nunit.analyzers.nuspec", new NuGetPackSettings());
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Rebuild")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

Task("Default")
    .IsDependentOn("Build")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
