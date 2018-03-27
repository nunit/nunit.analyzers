# NUnit Analyzers #

[![Build status](https://ci.appveyor.com/api/projects/status/rlx18p32vkh80p2f/branch/master?svg=true)](https://ci.appveyor.com/project/mikkelbu/nunit-analyzers/branch/master)

This is a suite of analyzers that target the NUnit testing framework. Right now, the code is separate from the NUnit framework, so if you want to try out the analyzers you'll need to download the code, build it and then use the generated NuGet package in your project. In the future the analyzers may be added as part of the NUnit framework package but that hasn't been done right now.

## Building ##

First, make sure you have the right tools and templates on your machine. You'll need VS 2015 and a couple of CompilerAPI-related project templates installed. If you open the Extensibility node when you create a new project in VS 2015, you'll see the "Install Visual Studio Extensibility Tools" node in the window. Install the tools, then do the same step of creating a new project, except this time run the "Download the .NET Compiler Platform SDK" node.

Now you should be able to open the nunit.analyzers.sln file. When you build the nunit.analyzers project, the output folder should contains a .nupkg file, something like NUnit.Analyzers.1.0.0.0.nupkg. You can reference that NuGet package file in another project that uses the NUnit testing framework, or you can launch the VSIX project, which will install the analyzers into an experimental version of VS. The nunit.analyzers.integrationtests solution has some examples of NUnit-based code that should have analyzer issues.

## Analyzers ##

Right now there are two analyzers in the code base. One will look for classic model assertions (e.g. `Assert.AreEqual()`, `Assert.IsTrue()`) and change them into constraint model assertions (`Assert.That()`). The other analyzer looks for methods with the `[TestCase]` attribute and makes sure the argument values are correct for the types of the method parameters along with the `ExpectedResult` value if it was provided. 

## License ##

NUnit analyzers are Open Source software and released under the [MIT license](http://www.nunit.org/nuget/nunit3-license.txt), which allow the use of the analyzers in free and commercial applications and libraries without restrictions.

## Contributing ##

There are several ways to contribute to this project. One can try things out, report bugs, propose improvements and new functionality, work on issues (especially the issues marked with the labels [help wanted](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) and [Good First Issue](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22Good+First+Issue%22)), and in general join in the conversations.

This project has adopted the Code of Conduct from the [Contributor Covenant](http://contributor-covenant.org), version 1.4, available at [http://contributor-covenant.org/version/1/4](http://contributor-covenant.org/version/1/4/).

## Contributors ##

NUnit.Analyzers was created by [Jason Bock](https://www.github.com/jasonbock). A complete list of contributors can be found on the [GitHub contributors page](https://github.com/nunit/nunit.analyzers/graphs/contributors).
