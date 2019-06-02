# NUnit Analyzers #

[![Build status](https://ci.appveyor.com/api/projects/status/rlx18p32vkh80p2f/branch/master?svg=true)](https://ci.appveyor.com/project/mikkelbu/nunit-analyzers/branch/master)
[![MyGet Feed](https://img.shields.io/myget/nunit-analyzers/v/NUnit.Analyzers.svg)](https://www.myget.org/feed/nunit-analyzers/package/nuget/NUnit.Analyzers)

This is a suite of analyzers that target the NUnit testing framework. Right now, the code is separate from the NUnit framework, so if you want to try out the analyzers you'll need to download the analyzers separately as a nuget package. In the future the analyzers may be added as part of the NUnit framework package but that hasn't been done yet.

## Download ##

Prerelease nuget packages can be found on [MyGet](https://www.myget.org/feed/nunit-analyzers/package/nuget/NUnit.Analyzers). Please try out the package and report bugs and feature requests.

## Analyzers ##

| Id       | Title
| :--      | :--
| [NUnit1001]()| Find Incorrect TestCaseAttribute Usage
| [NUnit1002]()| Find TestCaseSource(StringConstant) Usage
| [NUnit1003]()| Find Incorrect TestCaseAttribute Usage
| [NUnit1004]()| Find Incorrect TestCaseAttribute Usage
| [NUnit1005]()| Find Incorrect TestAttribute or TestCaseAttribute Usage
| [NUnit1006]()| Find Incorrect TestAttribute or TestCaseAttribute Usage
| [NUnit1007]()| Find Incorrect TestAttribute or TestCaseAttribute Usage
| [NUnit1008]()| Find Incorrect ParallelizableAttribute Usage
| [NUnit1009]()| Find Incorrect ParallelizableAttribute Usage
| [NUnit1010]()| Find Incorrect ParallelizableAttribute Usage
| [NUnit1011]()| TestCaseSource argument does not specify an existing member.
| [NUnit1012]()| Find Incorrect TestAttribute or TestCaseAttribute Usage
| [NUnit1013]()| Find Incorrect TestAttribute or TestCaseAttribute Usage
| [NUnit1014]()| Find Incorrect TestAttribute or TestCaseAttribute Usage
| [NUnit2001]()| Find Classic Assertion Usage
| [NUnit2002]()| Find Classic Assertion Usage
| [NUnit2003]()| Find Classic Assertion Usage
| [NUnit2004]()| Find Classic Assertion Usage
| [NUnit2005]()| Find Classic Assertion Usage
| [NUnit2006]()| Find Classic Assertion Usage
| [NUnit2007]()| Actual value should not be constant
| [NUnit2008]()| Find incorrect IgnoreCase usage
| [NUnit2009]()| Find same value provided as actual and expected argument

Below we give two examples of analyzers. One will look for methods with the `[TestCase]` attribute and makes sure the argument values are correct for the types of the method parameters along with the `ExpectedResult` value if it is provided. 
<!-- ![testcase](https://user-images.githubusercontent.com/1007631/44311794-269a7200-a3ee-11e8-86a0-1d290b355ac5.gif) -->
<img src="https://user-images.githubusercontent.com/1007631/44311794-269a7200-a3ee-11e8-86a0-1d290b355ac5.gif" alt="testcase analyzers" width="750"/>

The other analyzer looks for classic model assertions (e.g. `Assert.AreEqual()`, `Assert.IsTrue()`, etc.). This analyzer contains a fixer that can translate the classic model assertions into constraint model assertions (`Assert.That()`).
<img src="https://user-images.githubusercontent.com/1007631/44311791-213d2780-a3ee-11e8-90b8-6d144c0e3dbd.gif" alt="classic model assertions analyzers" width="1000"/>

## License ##

NUnit analyzers are Open Source software and released under the [MIT license](http://www.nunit.org/nuget/nunit3-license.txt), which allow the use of the analyzers in free and commercial applications and libraries without restrictions.

## Contributing ##

There are several ways to contribute to this project. One can try things out, report bugs, propose improvements and new functionality, work on issues (especially the issues marked with the labels [help wanted](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22help+wanted%22) and [Good First Issue](https://github.com/nunit/nunit.analyzers/issues?q=is%3Aissue+is%3Aopen+label%3A%22Good+First+Issue%22)), and in general join in the conversations. See [Contributing](CONTRIBUTING.md) for more information.

This project has adopted the Code of Conduct from the [Contributor Covenant](http://contributor-covenant.org), version 1.4, available at [http://contributor-covenant.org/version/1/4](http://contributor-covenant.org/version/1/4/). See the [Code of Conduct](CODE_OF_CONDUCT.md) for more information.

## Contributors ##

NUnit.Analyzers was created by [Jason Bock](https://www.github.com/jasonbock). A complete list of contributors can be found on the [GitHub contributors page](https://github.com/nunit/nunit.analyzers/graphs/contributors).
