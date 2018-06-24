# How to Contribute

There are several ways to contribute to this project. One can try things out, report bugs, propose improvements and new functionality, work on issues (for new contributors especially the issues marked with the labels **help wanted** and **Good First Issue**), and in general join in the conversations.

## Try Things Out

This project is still very much in its early stages, so the more eyes that take a look at the code, try out the analyzer on existing test etc. the better. So please try to use the analyzer with your NUnit test projects and provide feedback. The input is much appreciated.

## Report Bugs and Propose Improvements and New Functionality

The analyzer is still in its alpha release, so it is likely to contain a number of unknown bugs and/or missing some essential features. So try it out and provide feedback, such as bug reports or requests for improvements or new functionality.

### Bug Reports

For bug reports please provide as much information as possible:
* A short and clear title that describes the bug
* Version of the analyzer
* Include steps to reproduce the issue
* The expected and the actual behaviour (preferably including small code examples)

If the analyzer unexpectedly do not report a diagnostic, then try to compile the analyzer and run the analyzer as a VSIX extension as this will make thrown exceptions visible in Visual Studio, see the [Readme](README.md) for more information.

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

The project can be built from within **Visual Studio 2017** or using the **Cake** script in the root folder. The command `.\build.ps1` will restore the packages necessary to build the solution, build the projects (which require **Visual Studio 2017** and the **.NET Compiler Platform SDK**), and then run the tests. The script can also build the projects in **Release** mode using the option `-Configuration Release` or create NuGet Packages using the option `-Target Pack`.