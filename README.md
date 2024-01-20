# NUnit Analyzers #

[![Build status](https://ci.appveyor.com/api/projects/status/rlx18p32vkh80p2f/branch/master?svg=true)](https://ci.appveyor.com/project/mikkelbu/nunit-analyzers/branch/master)
[![GitHub Actions build status](https://github.com/nunit/nunit.analyzers/actions/workflows/ci.yml/badge.svg)](https://github.com/nunit/nunit.analyzers/actions/workflows/ci.yml)
[![NuGet Version and Downloads count](https://buildstats.info/nuget/NUnit.Analyzers)](https://www.nuget.org/packages/NUnit.Analyzers)
[![MyGet Feed](https://img.shields.io/myget/nunit-analyzers/v/NUnit.Analyzers.svg)](https://www.myget.org/feed/nunit-analyzers/package/nuget/NUnit.Analyzers)

This is a suite of analyzers that target the NUnit testing framework. Right now, the code is separate from the NUnit framework, so if you want to try out the analyzers you'll need to download the analyzers separately as a nuget package. In the future the analyzers may be added as part of the NUnit framework package but that hasn't been done yet.

## Download ##

The latest stable release of the NUnit Analyzers is [available on NuGet](https://www.nuget.org/packages/NUnit.Analyzers/) or can be [downloaded from GitHub](https://github.com/nunit/nunit.analyzers/releases). Note that for Visual Studio 2017 one must use versions below 3.0 - note that these versions are no longer updated, so version 2.10.0 is the last version that works in Visual Studio 2017. Version 3.0 and upwards require Visual Studio 2019 (version 16.3) or newer, these versions also enables supression of compiler errors such as errors arising from nullable reference types.

Prerelease nuget packages can be found on [MyGet](https://www.myget.org/feed/nunit-analyzers/package/nuget/NUnit.Analyzers). Please try out the package and report bugs and feature requests.

## Analyzers ##

The full list of analyzers can be found in the [documentation](https://github.com/nunit/nunit.analyzers/blob/master/documentation/index.md).

Below we give two examples of analyzers. One will look for methods with the `[TestCase]` attribute and makes sure the argument values are correct for the types of the method parameters along with the `ExpectedResult` value if it is provided.

<img src="https://user-images.githubusercontent.com/1007631/44311794-269a7200-a3ee-11e8-86a0-1d290b355ac5.gif" alt="testcase analyzers" width="750"/>

The other analyzer looks for classic model assertions (e.g. `Assert.AreEqual()`, `Assert.IsTrue()`, etc.). This analyzer contains a fixer that can translate the classic model assertions into constraint model assertions (`Assert.That()`).

<img src="https://user-images.githubusercontent.com/1007631/44311791-213d2780-a3ee-11e8-90b8-6d144c0e3dbd.gif" alt="classic model assertions analyzers" width="1000"/>

## Which version works with Unity Test Framework ##

If your Unity project is made by Unity under 2021.2, then use NUnit.Analyzers v2.x.

If your Unity project is made by Unity 2021.2 or later, then use NUnit.Analyzers v3.3 (v3.4 or later of the analyzers does not work with Unity).

You should use an analyzer built with the same version of Microsoft.CodeAnalysis.CSharp as the one embedded in the Unity Editor.

## License ##

NUnit analyzers are Open Source software and released under the [MIT license](http://www.nunit.org/nuget/nunit3-license.txt), which allow the use of the analyzers in free and commercial applications and libraries without restrictions.

## Contributing ##

There are several ways to contribute to this project. One can try things out, report bugs, propose improvements and new functionality, work on issues (especially the issues marked with the labels [help wanted](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) and [Good First Issue](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22Good+First+Issue%22)), and in general join in the conversations. See [Contributing](CONTRIBUTING.md) for more information.

This project has adopted the Code of Conduct from the [Contributor Covenant](http://contributor-covenant.org), version 1.4, available at [http://contributor-covenant.org/version/1/4](http://contributor-covenant.org/version/1/4/). See the [Code of Conduct](CODE_OF_CONDUCT.md) for more information.

## Contributors ##

NUnit.Analyzers was created by [Jason Bock](https://www.github.com/jasonbock). A complete list of contributors can be found on the [GitHub contributors page](https://github.com/nunit/nunit.analyzers/graphs/contributors).
