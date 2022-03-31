# How to Contribute

There are several ways to contribute to this project. One can try things out, report bugs, propose improvements and new functionality, work on issues (for new contributors especially the issues marked with the labels **help wanted** and **Good First Issue**), and in general join in the conversations.

## Try Things Out

This project is still very much in its early stages, so the more eyes that take a look at the code, try out the analyzers on existing test etc. the better. So please try to use the analyzers with your NUnit test projects and provide feedback. The input is much appreciated.

## Report Bugs and Propose Improvements and New Functionality

The analyzers are still in its alpha release, so it is likely to contain a number of unknown bugs and/or missing some essential features. So try it out and provide feedback, such as bug reports or requests for improvements or new functionality.

### Bug Reports

For bug reports please provide as much information as possible:
* A short and clear title that describes the bug
* Version of the package
* Include steps to reproduce the issue
* The expected and the actual behaviour (preferably including small code examples)

If the analyzers unexpectedly do not report a diagnostic, then try to compile the analyzers and run the analyzers as a VSIX extension as this will make them throw exceptions visible in Visual Studio, see the section **Building using Visual Studio** below for more information.

### Requests for Improvements and New Functionality

For requests for improvements and new functionality please provide:
* A short and clear title that describes the feature
* A more thorough description of the feature (preferably including small code examples)
* Possibly how the feature relates to existing functionality

## Working on Issues

Please provide pull requests for issues, but before starting on a pull request comment in the issue such that we avoid duplicated work - also comment if you stop working on the issue. The issues marked with the labels **help wanted** and **Good First Issue** should be most accessible, and be a good starting ground for new contributors.

The coding standards is specified in the [.editorconfig](.editorconfig), and otherwise try to follow the existing codebase. Please also supply tests for changes - if the change is possible to test.

Note that by contributing to NUnit.Analyzers, you assert that:

* The contribution is your own original work.
* You have the right to assign the copyright for the work (it is not owned by your employer, or you have been given copyright assignment in writing).
* You [license](license.txt) the contribution under the terms applied to the rest of the NUnit.Analyzers project.
* You agree to follow the [code of conduct](CODE_OF_CONDUCT.md).

## Building the Project

First, make sure you have the right tools and templates on your machine. You'll need **Visual Studio 2017** or **Visual Studio 2019** and the **.NET Compiler Platform SDK**. The **.NET Compiler Platform SDK** can be installed via the **Visual Studio Installer**. Either 
* check the **Visual Studio extension development** workload; open the **Visual Studio extension development** node in the summary tree to the right; and check the box for **.NET Compiler Platform SDK** (last under the optional components), or
* select the **Individual components** tab and check the box for **.NET Compiler Platform SDK** (at the top under the Compilers, build tools, and runtimes section).

The project can now be built from within **Visual Studio 2017** or **Visual Studio 2019**, or by using the **Cake** script in the root folder.

### Building using Visual Studio

From Visual Studio one can debug the analyzers by setting the **nunit.analyzers.vsix** project as the StartUp project and pressing F5 (Start Debugging). This will compile the analyzers as an extension and start a new (experimental) instance of Visual Studio with the extension.

### Building using Cake

The command `.\build.ps1` will restore the packages necessary to build the solution, build the projects, and then run the tests. The script can also build the projects in **Release** mode using the option `--configuration=Release` or create NuGet Packages using the option `--target=Pack`. This will create a NuGet package under `package\Debug\` (for a `Debug` build) and the file will be named `NUnit.Analyzers.***.nupkg` where `***` depends upon the build type (`Debug` vs. `Release`) and the version. The NuGet package can then be referenced from another project.
You need to use the `--targetFramework=netstandard1.6` option to build a analyzer version for `netstandard1.6` (VS 2017), this version will not include the new DiagnosticSuppressor rules (NUnit3001-) as these require `netstandard2.0`.

See [NuGet package vs. extension](https://docs.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview#nuget-package-vs-extension) for more information about the difference between installing a Roslyn analyzer as a NuGet package or as a Visual Studio extension.

