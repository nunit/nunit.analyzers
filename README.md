# NUnit Analyzers #

This is a suite of analyzers that target the NUnit testing framework. Right now, the code is separate from the NUnit framework, so if you want to try out the analyzers you'll need to download the code, build it and then use the generated NuGet package in your project. In the future the analyzers may be added as part of the NUnit framework package but that hasn't been done right now.

## Building ##

First, make sure you have the right tools and templates on your machine. You'll need VS 2015 and a couple of CompilerAPI-related project templates installed. If you open the Extensibility node when you create a new project in VS 2015, you'll see the "Install Visual Studio Extensibility Tools" node in the window:

{TODO: insert image}

Install the tools, then do the same step of creating a new project, except this time run the "Download the .NET Compiler Platform SDK" node:

{TODO: insert image}

Now you should be able to open the nunit.analyzers.sln file. When you build the nunit.analyzers project, the output folder should contains a .nupkg file, something like NUnit.Analyzers.1.0.0.0.nupkg. You can reference that NuGet package file in another project that uses the NUnit testing framework, or you can launch the VSIX project, which will install the analyzers into an experimental version of VS. The nunit.analyzers.integrationtests solution has some examples of NUnit-based code that should have analyzer issues.

## Analyzers ##

Right now there are two analyzers in the code base. One will look for classic model assertions (e.g. `Assert.AreEqual()`, `Assert.IsTrue()`) and change them into constraint model assertions (`Assert.That()`). The other analyzer looks for methods with the `[TestCase]` attribute and makes sure the argument values are correct for the types of the method parameters along with the `ExpectedResult` value if it was provided. 

## License ##

NUnit analyzers are Open Source software and released under the [MIT license](http://www.nunit.org/nuget/nunit3-license.txt), which allow the use of the analyzers in free and commercial applications and libraries without restrictions.

## Contributors ##

Current contributors to the analyzers are [Jason Bock] (https://www.github.com/jasonbock).
