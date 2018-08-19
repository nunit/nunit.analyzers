# NUnit Analyzers #

[![Build status](https://ci.appveyor.com/api/projects/status/rlx18p32vkh80p2f/branch/master?svg=true)](https://ci.appveyor.com/project/mikkelbu/nunit-analyzers/branch/master)
[![MyGet Feed](https://img.shields.io/myget/nunit-analyzers/v/NUnit.Analyzers.svg)](https://www.myget.org/feed/nunit-analyzers/package/nuget/NUnit.Analyzers)

This is a suite of analyzers that target the NUnit testing framework. Right now, the code is separate from the NUnit framework, so if you want to try out the analyzers you'll need to download the code, build it and then use the generated NuGet package in your project. In the future the analyzers may be added as part of the NUnit framework package but that hasn't been done right now.

## Building ##

First, make sure you have the right tools and templates on your machine. You'll need **Visual Studio 2017** and the **.NET Compiler Platform SDK**. The **.NET Compiler Platform SDK** can be installed via the **Visual Studio Installer**. Either 
* check the **Visual Studio extension development** workload; open the **Visual Studio extension development** node in the summary tree to the right; and check the box for **.NET Compiler Platform SDK** (last under the optional components), or
* select the **Individual components** tab and check the box for **.NET Compiler Platform SDK** (at the top under the Compilers, build tools, and runtimes section).

The code can now be compiled using the cake script using `.\build.ps1` or from within Visual Studio. From Visual Studio one can also debug the analyzer by setting **nunit.analyzers.vsix** project as StartUp project and press F5 (Start Debugging). This will compile the analyzer as an extension and start a new (experimental) instance of Visual Studio with the extension.

One can also pack a NuGet package using the cake script via `.\build.ps1 -Target Pack`. This will create a NuGet package under `package\Debug\` (for a `Debug` build) and the file will be named `NUnit.Analyzers.***.nupkg` where `***` depends upon the build type (`Debug` vs. `Release`) and the version. The NuGet package can then be referenced from another project.

See [NuGet package vs. extension](https://docs.microsoft.com/en-us/visualstudio/code-quality/roslyn-analyzers-overview#nuget-package-vs-extension) for more information about the difference between installing a Roslyn analyzer as a NuGet package or as a Visual Studio extension.

## Analyzers ##

Right now there are two analyzers in the code base. One will look for methods with the `[TestCase]` attribute and makes sure the argument values are correct for the types of the method parameters along with the `ExpectedResult` value if it was provided. 
![testcase](https://user-images.githubusercontent.com/1007631/44311794-269a7200-a3ee-11e8-86a0-1d290b355ac5.gif)

The other analyzer looks for classic model assertions (e.g. `Assert.AreEqual()`, `Assert.IsTrue()`) and change them into constraint model assertions (`Assert.That()`).
![classicmodelassertions](https://user-images.githubusercontent.com/1007631/44311791-213d2780-a3ee-11e8-90b8-6d144c0e3dbd.gif)

## Download ##

Prerelease nuget packages can be found on [MyGet](https://www.myget.org/feed/nunit-analyzers/package/nuget/NUnit.Analyzers). Please try out the package and report bugs and feature requests.

## License ##

NUnit analyzers are Open Source software and released under the [MIT license](http://www.nunit.org/nuget/nunit3-license.txt), which allow the use of the analyzers in free and commercial applications and libraries without restrictions.

## Contributing ##

There are several ways to contribute to this project. One can try things out, report bugs, propose improvements and new functionality, work on issues (especially the issues marked with the labels [help wanted](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) and [Good First Issue](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22Good+First+Issue%22)), and in general join in the conversations. See [Contributing](CONTRIBUTING.md) for more information.

This project has adopted the Code of Conduct from the [Contributor Covenant](http://contributor-covenant.org), version 1.4, available at [http://contributor-covenant.org/version/1/4](http://contributor-covenant.org/version/1/4/). See the [Code of Conduct](CODE_OF_CONDUCT.md) for more information.

## Contributors ##

NUnit.Analyzers was created by [Jason Bock](https://www.github.com/jasonbock). A complete list of contributors can be found on the [GitHub contributors page](https://github.com/nunit/nunit.analyzers/graphs/contributors).
