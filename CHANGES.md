﻿# NUnit Analyzers 4.11.1 - October 30, 2025

This release of the NUnit Analyzers fixes a regression related to the analysis of nullable enum parameters in
`NUnit1001` and `NUnit1031`. The release also contains a new analyzer and codefix for unnecessary usage of lambda
expressions in asserts.

The release contains contributions from the following users (in alphabetical order):
* @MaceWindu
* @maksim-sovkov
* @manfred-brands

Issues Resolved

Features and Enhancements
* #927 Add new rule for unnecessary use of lambda expressions

Bugs
* #939 NUnit1001 - false positive for arguments with nullable enum parameters
* #938 [NUnit1031] False positive for nullable structs


# NUnit Analyzers 4.11 - October 28, 2025

This release of the NUnit Analyzers includes improvements and fixes related to `IDisposable` handling and initialization 
in `SetUp` and `OneTimeSetUp` methods. It also introduces the ability to configure additional methods that should be 
treated as `SetUp` and `TearDown` methods by the analyzers. This can be done in the `.editorconfig`, and there are four 
configurations for this:

* `dotnet_diagnostic.NUnit.additional_setup_methods`
* `dotnet_diagnostic.NUnit.additional_teardown_methods`
* `dotnet_diagnostic.NUnit.additional_one_time_setup_methods`
* `dotnet_diagnostic.NUnit.additional_one_time_teardown_methods`

Each configuration accepts a list of method names, separated by commas, semicolons, or spaces. For example:

```ini
dotnet_diagnostic.NUnit.additional_setup_methods = CustomSetup, MyInit
```

As in recent releases, a major part of this work was contributed by @manfred-brands.

The release contains contributions from the following users (in alphabetical order):
* @AlisonAMorrison
* @BodrickLight
* @cbersch
* @manfred-brands
* @mikkelbu
* @PiotrKlecha
* @sbe-schleupen

Issues Resolved

Features and Enhancements
* #921 NUnit1032 - disposals in overridden methods not detected
* #919 NUnit1032/NUnit3002 - local functions not analyzed
* #918 NUnit2045 - false positive for inline usings
* #911 NUnit3002 doesn't recognize the using statement.
* #910 using declarations not recognized by NUnit2045

Bugs
* #922 NUnit1001 - false positive for arguments with generic parameters
* #914 Wrong position of NUnit1001 diagnostic for TestCase with four parameters or more

Tooling, Process, and Documentation
* #926 chore: Bump NUnit3TestAdapter
* #908 chore: bump version
* #885 Bump to NUnit version 4.4 when this is released


# NUnit Analyzers 4.10 - August 9, 2025

This release of the NUnit Analyzers contains some minor improvements to NUnit2050, NUnit2056, and NUnit2007 as well
as some improvements to existing tests. Once again, @manfred-brands was responsible for the majority of the work.

The release contains contributions from the following users (in alphabetical order):
* @dfev77
* @manfred-brands
* @mikebro
* @mikkelbu

Issues Resolved

Bugs
* #901 False positive on NUnit2050
* #899 NUnit2056 analyzer's code fix removes comments and empty lines above it.
* #896 NUnit2007 shouldn't trigger for generic types e.g. typeof(T)

Tooling, Process, and Documentation
* #905 NUnit4.4 alpha -> beta changes
* #897 chore: Correct typo in NUnit2045.md
* #894 Improve tests by adding ↓ to tests were it is missing in the source
* #887 chore: bump version


# NUnit Analyzers 4.9.2 - June 17, 2025

This release of the NUnit Analyzers extends the `NUnit3001` nullability suppressor
to also work in the context of `Assert.EnterMultipleScope` and other using statements.

The release contains contributions from the following users (in alphabetical order):
* @artificialWave
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #892 Assert.EnterMultipleScope not detecting nullability properly 


# NUnit Analyzers 4.9.1 - June 12, 2025

This release of the NUnit Analyzers fixes a problem with the code-fix for translating
`Assert.Multiple`/`Assert.MultipleAsync` into the new `Assert.EnterMultipleScope` format
when the test method already is asynchronous and have a return type different from `Task`.

The release contains contributions from the following users (in alphabetical order):
* @MaceWindu
* @manfred-brands
* @mikkelbu

Issues Resolved

Bugs
* #888 Assert.MultipleAsync -> EnterMultipleScope autofix produce invalid code


# NUnit Analyzers 4.9 - June 11, 2025

This release of the NUnit Analyzers adds several new analyzers. For the `RangeAttribute`,
the analyzers now warn about potential issues at runtime.

It also introduces an analyzer and code fix for translating
`Assert.Multiple`/`Assert.MultipleAsync` into the new `Assert.EnterMultipleScope` format, as well
as for converting `is T` checks into `Is.InstanceOf<T>()` constraints.

For `NUnit2021`, the analyzer now respects `UsingPropertiesComparer`, including enhancements
that will be available in NUnit 4.4.

Finally, this release includes improvements to `NUnit2007`, `NUnit2045`, and `NUnit4002`.
See the list of resolved issues below for more details.

The release contains contributions from the following users (in alphabetical order):
* @MaceWindu
* @manfred-brands
* @mikkelbu
* @OsirisTerje
* @stevenaw

Issues Resolved

Features and Enhancements
* #880 NUnit2007 could flag typeof() as a constant first parameter
* #866 When Assert.Multiple is found, should suggest to convert to Assert.EnterMultipleScope
* #865 NUnit 2045 suggest Assert.Multiple, but should suggest Assert.EnterMultipleScope
* #857 NUnit2021 Should not raise for UsingPropertiesComparer
* #765 Add Rule for converting is T into Is.InstanceOf<T>()
* #89 Test the correct usage of the Range attribute

Bugs
* #879 `NUnit4002` shouldn't trigger for `T` vs `nullable<T>` struct types

Tooling, Process, and Documentation
* #868 chore: bump version


# NUnit Analyzers 4.8.1 - May 29, 2025

This release of the NUnit Analyzers fixes a problem with `NUnit4002` when applied to comparisons between non-number
types - e.g. strings.

The release contains contributions from the following users (in alphabetical order):
* @adrianbanks
* @manfred-brands
* @mikkelbu

Issues Resolved

Bugs
* #870 Compilation error caused by exception in an analyzer after updating to v4.8.0


# NUnit Analyzers 4.8 - May 22, 2025

This release of the NUnit Analyzers adds a new diagnostic `NUnit1034` that checks
whether base TestFixtures are declared as `abstract`. When a base class is not `abstract` 
it will also be run as a standalone test which is most times not the intention.

The release also contains some fixes to `NUnit4002` and `Nunit2045`.

The release contains contributions from the following users (in alphabetical order):
* @Bartleby2718
* @CharliePoole
* @MaceWindu
* @manfred-brands
* @mikkelbu
* @Rabadash8820

Issues Resolved

Features and Enhancements
* #840 Detect incorrect or questionable use of TestFixture inheritance.

Bugs
* #862 NUnit.Analyzers doesn't recognize the version of NUnit in use
* #856 NUnit4002 shouldn't trigger for unknown types

Tooling, Process, and Documentation
* #861 Add missing backticks in NUnit4002.md
* #855 chore: bump version 


# NUnit Analyzers 4.7 - April 1, 2025

The release primarily add another diagnostic `NUnit4002` - and associated codefix - to help simplify
`EqualTo` constraints when the expected value is a simple constant - e.g. `true`, `false`, `0`, or 
`default`. The release also removes some false positives for `Nunit1029`.

As tooling contributions the analyzers now build using .NET8.0 and also analyzers and codefixes are
now split into separate projects as only editors should load codefixes.

The release contains contributions from the following users (in alphabetical order):
* @cbersch
* @Dreamescaper
* @manfred-brands
* @mikkelbu
* @seanblue
* @zlepper

Issues Resolved

Features and Enhancements
* #828 Replace Is.EqualTo(default) with Is.Default
* #826 Suggest to use Is.Null instead of Is.EqualTo(null)
* #824 Use Is.False / Is.True instead of Is.EqualTo

Bugs
* #832 False positive for Nunit1029 when only a type argument is use

Tooling, Process, and Documentation
* #853 chore: Add NUnit4002.md solution file
* #846 chore: Bump NUnit3TestAdapter to version 5 
* #843 chore(deps): Bump Microsoft.NET.Test.Sdk and Microsoft.NETFramework.ReferenceAssemblies
* #838 chore: bump version 
* #677 Build using .NET8.0 SDK


# NUnit Analyzers 4.6 - January 9, 2025

This release contains two improvements: Allowing `NUnit1001` to understand `DateOnly` and `TimeOnly` parameters in
`TestCaseUsage` and making `NUnit2045` support `Assert.EnterMultipleScope` (introduced in NUnit version 4.2). 

The release contains contributions from the following users (in alphabetical order):
* @Dreamescaper
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #820 NUnit1001 compains about DateOnly parameters
* #769 Recognized the new Assert.EnterMultipleScope() concept.

Tooling, Process, and Documentation
* #829 chore: Bump year to 2025 in copyrights
* #823 chore: Bump cake.tool to version 4
* #822 chore: Bump NUnit to 4.3.2
* #818 chore: Replace "buildstats.info" with "img.shields.io"
* #815 chore: bump version


# NUnit Analyzers 4.5 - December 22, 2024

The release primarily fixes a problem with the NUnit Analyzers when used with NUnit 4.3.1 - see #811 for more
information. In additional, we have also added some smaller improvements and bug fixes.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @mikkelbu
* @RenderMichael
* @zlepper

Issues Resolved

Features and Enhancements
* #811 NUnit2021 ignores cast operation since NUnit 4.3.1
* #801 Allow NUnit2005 to recognize Is.Empty

Bugs
* #794 AD0001: Occasional InvalidOperationException error in analyzer in IDE

Tooling, Process, and Documentation
* #808 chore: Correct typo
* #806 Add information about dotnet_diagnostic.NUnit1032.additional_dispose_methods to the docs
* #795 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.11.1 to 17.12.0
* #793 chore: bump version


# NUnit Analyzers 4.4 - November 13, 2024

This release of the NUnit Analyzers adds handling of `Assert.IsAssignableFrom`/`Assert.IsNotAssignableFrom` and 
`Assert.Positive`/`Assert.Negative`. Also insertion of trivia for code fixes for `NUnit2007` and `NUnit2046`
have been improved.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @MCPtz
* @mikkelbu
* @verdie-g

Issues Resolved

Features and Enhancements
* #790 NUnit.Analyzers 4.3.0 missed an Assert.IsAssignableFrom that caused a build error after upgrading to latest NUnit 4.2.2
* #789 NUnit.Analyzers 4.3.0 missed an Assert.Positive that caused a build error after upgrading to latest NUnit 4.2.2

Bugs
* #784 Fix trivia for NUnit2046
* #783 NUnit2007 doesn't apply trivia correctly

Tooling, Process, and Documentation
* #788 chore: Bump NUnit to version 4.2.2 
* #785 chore(deps): Bump NUnit3TestAdapter from 4.5.0 to 4.6.0
* #780 chore(deps): Bump CSharpIsNullAnalyzer from 0.1.495 to 0.1.593
* #778 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.10.0 to 17.11.1
* #776 chore: bump version 
* #775 chore: Update release notes 


# NUnit Analyzers 4.3 - August 9, 2024

This release of the NUnit Analyzers contains some bug fixes to existing analyzers and code fixes - among other
improvements to trivia when using the code fix of NUnit2049.

For new features we now warn against using `TestContext.Write` as this will be obsolete in NUnit at some point;
NUnit1001 now recognises and check generic TestCase attributes; and we have added a new analyzer and code fix for
simplifying usages of `ValuesAttribute`.

The release contains contributions from the following users (in alphabetical order):
* @andrewimcclement
* @Bartleby2718
* @DrPepperBianco
* @KaiBNET
* @maettu-this
* @manfred-brands
* @mikkelbu
* @RenderMichael
* @SeanKilleen
* @trampster

Issues Resolved

Features and Enhancements
* #770 Add rule to detect calls to TestContext.Write methods and CodeFix to replace usages with Out.Write
* #767 Augment NUnit1001 to recognized and check generic TestCase attributes
* #755 New diagnostic: The Values attribute can be simplified.

Bugs
* #766 Error when TearDown method is defined in partial test classes - Syntax node is not within syntax tree
* #743 NUnit1032 (missing Dispose), if dispose is wrapped in "(… as IDisposable)?.Dispose()"
* #739 Null suppression does not work when Assert is fully qualified
* #713 Code fix for NUnit2049 places the comma at a wrong place and messes up indentation bug

Tooling, Process, and Documentation
* #764 Update the solution file
* #761 Update nunit.analyzers.nuspec to specify that NUnit.Analyzers v4 is intended to be used with NUnit 4. 
* #756 error NUnit1032 is incorrect when InstancePerTestCase and constructor is used to initialize IDisposible
* #741 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.9.0 to 17.10.0 in /src 
* #737 Update NUnit2013.md to fix code block 
* #736 chore: bump version 
* #734 Why dropping composite message formatting support also for Assert.Pass/Fail/... where there are no actual and constraint parameters?


# NUnit Analyzers 4.2 - April 24, 2024

This release of the Analyzers extends NUnit2050 to also cover `Assume` and Nunit1032 to consider dispose of a type 
having explicit interface implementation. Furthermore, named parameters are now handled correctly codefixes for 
classical asserts, and NUnit2010 improves the logic for determining `Equals` methods.

The release contains contributions from the following users (in alphabetical order):
* @Bartleby2718
* @hazzik
* @maettu-this
* @manfred-brands
* @matode
* @mikkelbu

Issues Resolved

Features and Enhancements
* #731 Add test for wrapping conditional expression in parenthesis for NUnit2050
* #720 Replace UpdateStringFormatToFormattableString with String.Format
* #719 NUnit2050 should cover Assume
* #710 Nunit1032 - consider dispose of a type having explicit interface implementation

Bugs
* #728 NUnit2010 - do not consider Equals call if it doesn't override Object.Equals
* #712 [bug] Code fix for NUnit2005 does not correctly fix Assert.AreEqual if named parameters are used in unexpected order

Tooling, Process, and Documentation
* #733 Missing full stops added to NUnit2050
* #722 Use Markdown for CHANGES
* #708 chore: bump-version


# NUnit Analyzers 4.1 - March 16, 2024

This release of the Analyzers extends the suppression of nullable warnings to also respect assumptions - 
expressed via `Assume.That`. Also nullable warnings are suppressed even in the context of the 
null-forgiving operator `!`, and NUnit2010 is extended to also cover `is` pattern expressions - e.g.
`is null` and more general integer patterns as `is < 0 or >= 1`.

The release contains contributions from the following users (in alphabetical order):
* @lahma
* @manfred-brands
* @mikkelbu
* @RenderMichael
* @TheBigNeo
* @verdie-g

Issues Resolved

Features and Enhancements
* #693 Possibly Null Reference Warning should be suppressed for Assume
* #691 Extent rule NUnit2010 to detect 'is null'
* #679 Null suppression doesn't work when the body has a null suppression

Bugs
* #700 CodeFix for Assert with null message causes ambiguous code.
* #689 Incorrect constraint model transformation for named parameters

Tooling, Process, and Documentation
* #697 chore: Bump NUnit to version 4.1.0 
* #694 Switch to using license expression 
* #690 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.8.0 to 17.9.0
* #687 chore: Update release notes


# NUnit Analyzers 4.0.1 - February 1, 2024

Small release that fixes a problem with the 4.0 release when combining `TestCaseSource` and `CancelAfter`.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @mikkelbu
* @richardissimo

Issues Resolved

Bugs
* #685 error NUnit1029: The TestCaseSource provides '1' parameter(s), but the Test method expects '1' parameter(s)

Tooling, Process, and Documentation
* #684 chore: bump-version 


# NUnit Analyzers 4.0 - January 27, 2024

Version 4 of the NUnit analyzers drops the support for older releases of Visual Studio. I.e.
releases of Visual Studio that are older than Visual Studio 2019 16.3. In addition, this release
contains various bug fixes to existing analyzers, support of the CancelAfterAttribute
that was introduced in NUnit 4, and extends the codefix for NUnit2007 to also work when
.Within is used.

The release contains contributions from the following users (in alphabetical order):
* @Abrynos
* @gfoidl
* @Laniusexcubitor
* @MaceWindu
* @manfred-brands
* @mikkelbu
* @RenderMichael
* @SeanKilleen

Issues Resolved

Features and Enhancements
* #669 NUnit2007 does not provide codefix when .Within is used
* #609 Drop support for VS before 2019

Bugs
* #663 NUnit1027 fired when CancellationToken and [CancelAfter] is given
* #659 NUnit1032 throws an exception in a specific configuration
* #656 NUnit1028 warns about overridden methods
* #635 ArgumentException in DisposeFieldsAndPropertiesInTearDownAnalyzer

Tooling, Process, and Documentation
* #676 chore: markdownlint-cli2-config is removed use flag instead 
* #668 chore(deps): Bump StyleCop.Analyzers.Unstable from 1.2.0.507 to 1.2.0.556
* #662 chore: Bump NUnit 4 to version 4.0.1
* #660 chore: Bump to NUnit 4
* #654 chore: Bump version
* #630 Update NUnit Analyzer docs to respect 120-character docs rule documentation
* #508 Restore "File version" and "Product version" in analyzer dlls


# NUnit Analyzers 3.10 (and 2.10) - November 27, 2023

This release adds a couple of improvements to the analyzers: 
* Check that users don't accidentally specify CallerArgumentExpression parameters
* Relax analyzers for added support for IAsyncEnumerable on *Source attributes

These improvements extend the functionality in the beta that added support for NUnit 4 and
for migrating to NUnit 4.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @mikkelbu
* @stevenaw

Issues Resolved

Features and Enhancements
* #639 Rule to check users don't accidentally specify CallerArgumentExpression parameters
* #634 Relax analyzers for added support for IAsyncEnumerable on *Source attributes

Tooling, Process, and Documentation
* #648 chore: Skip branch builds on PRs
* #644 chore: Update release notes for 3.10 beta
* #429 Drop the VSIX project


# NUnit Analyzers 3.10-beta1 (and 2.10-beta1) - November 17, 2023

This beta adds support for the upcoming NUnit 4 - see pull request #612 - which solves the following issues
* #620 Make Classic Conversion rule for CollectionAssert improvement
* #618 Make Classic Conversion rule for StringAssert
* #617 Update .Within makes no sense rule
* #610 Ensure Test Code works with NUnit4
* #606 Support for NUnit 4 legacy asserts
* #562 Warn use of params for assertion messages

The primary change is the handling of the movement of classic asserts into a new namespace
`NUnit.Framework.Legacy` and of the improved assert result messages - for more information see
https://docs.nunit.org/articles/nunit/Towards-NUnit4.html. The analyzers can help updating the
classic assert and fix the assert messages.

The release contains contributions from the following users (in alphabetical order):
* @CollinAlpert
* @manfred-brands
* @mikkelbu
* @OsirisTerje

Issues Resolved

Features and Enhancements
* #620 Make Classic Conversion rule for CollectionAssert improvement
* #618 Make Classic Conversion rule for StringAssert
* #617 Update .Within makes no sense rule
* #615 Add support for Assert.MultipleAsync
* #610 Ensure Test Code works with NUnit4
* #606 Support for NUnit 4 legacy asserts
* #562 Warn use of params for assertion messages

Bugs
* #632 NUnit1031 doesn't seem to work with Generic parameters
* #631 NUnit1001/NUnit1031 don't observe null forgiveness operator
* #621 NUnit2025 fires unnecessarily

Tooling, Process, and Documentation
* #633 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.7.2 to 17.8.0


# NUnit Analyzers 3.9 (and 2.9) - October 27, 2023

This release contains bugfixes to NUnit2045 - Use Assert.Multiple - and NUnit1032 - 
An IDisposable field/property should be Disposed in a TearDown method - and corrects
a StackOverflowException when tests calls methods recursively.

The release contains contributions from the following users (in alphabetical order):
* @andrewimcclement
* @manfred-brands
* @mikkelbu
* @NottsColin
* @RenderMichael

Issues Resolved

Bugs
* #614 NUnit2045 does not respect lambda captures
* #607 NUnit1032 false positive when test class is static
* #602 Bug: StackOverflowException when test calls recursive method in 3.7.


# NUnit Analyzers 3.8 (and 2.8) - September 25, 2023

This release contains a fix to the WithinUsageAnalyzer and handling of false duplicates when
combining NUnit1032 - An IDisposable field/property should be Disposed in a TearDown method -
with LifeCycle.InstancePerTestCase.

The release contains contributions from the following users (in alphabetical order):
* @andrewimcclement
* @fredjeronimo
* @HenryZhang-ZHY
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #595 NUnit1032 false positive in LifeCycle.InstancePerTestCase test fixture using constructor

Bugs
* #596 WithinUsageAnalyzer threw an exception of type 'System.IndexOutOfRangeException'

Tooling, Process, and Documentation
* #598 Update NUnit1032.md to fix typo Dipose -> Dispose.


# NUnit Analyzers 3.7 (and 2.7) - September 16, 2023

This release contains a lot of improvements and corrections to the supression of non-nullable fields and properties
that are assigned in setup methods (and not in constructors). We have also added suppression of:
* CA1812 - Avoid uninstantiated internal classes - on test classes
* CA1001 - Types that own disposable fields should be disposable - when disposed is called in TearDown methods

The release also contain some improvements to the performance of the analyzers by avoid repeated calls to 
GetTypeByMetadataName. Also ValuesAttribute is now also handled by the analyzer in a similar manner as TestCaseAttribute.
Most of the work done in this release have either been driven by or made by @manfred-brands.

The release contains contributions from the following users (in alphabetical order):
* @333fred
* @Corniel
* @andrewimcclement
* @IlIlIllIllI
* @jhinder
* @MaceWindu
* @manfred-brands
* @mikkelbu
* @RenderMichael
* @SeanKilleen
* @stevenaw

Issues Resolved

Features and Enhancements
* #585 NonNullableFieldOrPropertyIsUninitializedSuppressor doesn't check async methods called from SetUp
* #582 NonNullableFieldOrPropertyIsUninitializedSuppressor does not detect assignments in try/finally blocks
* #569 Added a suppressor when CA1812 fires on NUnit Test classes.
* #568 Feature request: suppress CA1001 when Dispose is called in the TearDown method
* #561 NUnit2021 error shown when comparing Uri and string
* #548 Use RegisterCompilationStartAction to avoid repeated calls to GetTypeByMetadataName 
* #542 Allow the *Source Analyzers to permit Task<IEnumerable>
* #462 DiagnosticsSuppress does not suppress CS8634
* #344 Add a rule informing that .Within is not valid for non-numeric types.
* #52 Reuse TestCaseAttribute logic for ValuesAttribute improvement

Bugs
* #587 Buggy interaction between the Assert.Multiple fixer and null reference suppression
* #580 False positive for WithinUsageAnalyzer
* #559 FP NUnit1001: CustomTypeConverters could convert from anything
* #549 Code Fix for NUnit2010 on Ref Structs Creates CS0306
* #541 [NUnit2045] Incorrect refactoring
* #535 DiagnosticSuppressor doesn't suppress values passed as arguments
* #534 QuickFix for Assert.Multiple looses white space before and comments after bug

Tooling, Process, and Documentation
* #579 chore: Bump Microsoft.NET.Test.Sdk
* #578 chore(deps): Bump Microsoft.CodeAnalysis.NetAnalyzers
* #573 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.7.0 to 17.7.1
* #571 chore(deps): Bump System.Collections.Immutable from 6.0.0 to 7.0.0
* #567 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.6.3 to 17.7.0
* #566 Update CONTRIBUTING.md to fix link to MS documentation.
* #560 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.6.2 to 17.6.3
* #558 chore(deps): Bump StyleCop.Analyzers.Unstable from 1.2.0.435 to 1.2.0.507
* #557 chore(deps): Bump Microsoft.CodeAnalysis.NetAnalyzers from 7.0.1 to 7.0.3
* #556 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.6.1 to 17.6.2
* #553 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.6.0 to 17.6.1
* #551 chore(deps): Bump NUnit3TestAdapter from 4.4.2 to 4.5.0
* #547 chore(deps): Bump CSharpIsNullAnalyzer from 0.1.300 to 0.1.495
* #543 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.5.0 to 17.6.0
* #531 Markdown fixes


# NUnit Analyzers 3.6.1 (and 2.6.1) - March 10, 2023

This release of the NUnit Analyzers contain a single bug fix and some updates of dependencies.
The bug fix removes a false positive from NUnit1030 - "The type of parameter provided by the TestCaseSource 
does not match the type of the parameter in the Test method" - when using TestCaseParameters.

The release contains contributions from the following users (in alphabetical order):
* @ehonda
* @manfred-brands
* @mikkelbu

Issues Resolved

Bugs
* #523 False positive for NUnit1030 with TestCaseParameters bug

Tooling, Process, and Documentation
* #528 chore(deps): Bump NUnit3TestAdapter from 4.4.0 to 4.4.2
* #527 chore(deps): Bump NUnit3TestAdapter from 4.3.1 to 4.4.0
* #522 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.4.1 to 17.5.0


# NUnit Analyzers 3.6 (and 2.6) - February 21, 2023

This release of the NUnit Analyzers improves on the analysis of nullable reference types - in particular
in case of null coalescing operator and tuple deconstruction assignments.

The release also adds two new diagnostics for the TestCaseSource attribute. The diagnostics examines the test data returned
from the TestCaseSource:
* NUnit1029 - The number of parameters provided by the TestCaseSource does not match the number of parameters in the Test method
* NUnit1030 - The type of parameter provided by the TestCaseSource does not match the type of the parameter in the Test method

In addition, the release also contains some bug fixes to existing analyzers, and we have added a GitHub Actions workflow
and status badge.

The release contains contributions from the following users (in alphabetical order):
* @get-me-power
* @Hounsvad
* @JasonBock
* @manfred-brands
* @mikkelbu
* @nowsprinting
* @oskar
* @SeanKilleen
* @yaakov-h

The majority of the code contributions were provided by @manfred-brands.

Issues Resolved

Features and Enhancements
* #503 DiagnosticsSuppressor doesn't suppress null coalescing operator
* #499 DiagnosticSuppressor doesn't detect tuple deconstruction assignments
* #442 Analyzer for TestCaseSource does not check Test method parameters

Bugs
* #509 Giving TestCaseAttribute an explicit decimal for a parameter that is a decimal gives a Nunit 1001 error
* #496 NUnit2045 code-fix does not correctly lift asynchronous assertions into Assert.Multiple
* #475 Code Fix for NUnit2010 on Ref Structs Creates CS1503

Tooling, Process, and Documentation
* #519 chore: Update year to 2023 
* #518 fix: Correct MSDN link on new pages
* #516 Replace link to ruleset docs 
* #507 chore: Add Github Action build status badge 
* #500 Fix path to global.json 
* #497 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.3.2 to 17.4.1
* #489 Add note about works with unity3d 
* #487 chore(deps): Bump NUnit3TestAdapter from 4.2.1 to 4.3.0
* #486 chore: Bump version to 3.6 (2.6)
* #476 [add] GitHub Actions workflow for running test 


# NUnit Analyzers 3.5 (and 2.5) - October 23, 2022

This release of the NUnit Analyzers only fixes the casing in a file name in the nuget file as nuget upload validation failed.

The release contains contributions from the following users (in alphabetical order):
* @mikkelbu

Issues Resolved

Tooling, Process, and Documentation
* #484 chore: Bump version to 3.5 (2.5)
* #483 chore: NuGet.org validation is case sensitive


# NUnit Analyzers 3.4 (and 2.4) - October 23, 2022

This release of the NUnit Analyzers improves on false positives in the existing analyzers for:
* NUnit2021 - "Incompatible types for EqualTo constraint"
* NUnit2046 - "Use CollectionConstraint for better assertion messages in case of failure"

The release also improves on NUnit3002 - "Field/Property is initialized in SetUp or OneTimeSetUp method" to
also consider initialization in overridden SetUp methods, and adds support for classic StringAssert constraints
in NUnit2007 - "The actual value should not be a constant" -  analyzer and code fix.

In addition, the release also contains some bug fixes to existing analyzers.
Moreover, several dependencies have been bumped in this release and the build process has been improved.

The release contains contributions from the following users (in alphabetical order):
* @eatdrinksleepcode
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #465 False positive for NUnit2021 with Throws/EqualTo contraint
* #459 False positives for NUnit2046 on non-array-like types
* #453 Support classic StringAssert in ConstActualValueUsage analyzer and code fix
* #448 Analyzer doesn't detect non-nullable field set in overridden SetUp method

Bugs
* #440 Analyzer conversion for class with Count property can be incorrect
* #436 Using Has.Count in Assert.Multiple re-raises nullable reference warning
* #420 NUnit2045 raises IndexOutOfRangeException on CodeAnalysis

Tooling, Process, and Documentation
* #479 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.3.1 to 17.3.2 in /src
* #470 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.2.0 to 17.3.1 in /src
* #467 Build Issues
* #463 chore(deps): Bump Nullable from 1.3.0 to 1.3.1 in /src
* #461 chore(deps): Bump Gu.Roslyn.Asserts from 4.2.1 to 4.3.0 in /src
* #456 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.1.0 to 17.2.0 in /src
* #455 chore(deps): Bump Gu.Roslyn.Asserts from 4.2.0 to 4.2.1 in /src
* #454 Ignore Rider IDE files
* #452 chore(deps): Bump StyleCop.Analyzers.Unstable from 1.2.0.406 to 1.2.0.435 in /src
* #451 Mark build.sh as executable
* #446 Move to latest Gu.Roslyn.Assert nuget packages
* #441 chore(deps): Bump NUnit from 3.13.2 to 3.13.3 in /src
* #440 chore(deps): Bump Microsoft.NET.Test.Sdk from 17.0.0 to 17.1.0 in /src 
* #437 chore(deps): Bump NUnit3TestAdapter from 4.1.0 to 4.2.1 in /src
* #428 Move to use cake as a dotnet tool
* #419 chore: Bump version to 3.4 (2.4)
* #418 Add README.md to nuget file
* #272 Enforce coding standard more strictly


# NUnit Analyzers 3.3 (and 2.3) - January 8, 2022

This release of the NUnit Analyzers adds a DiagnosticSuppressor for Nullable<T>, an analyzer and a code fix 
for rewriting independent asserts into Assert.Multiple, and an analyzer and a code fix for replacing calls to
.Length or .Count() in the actual expression with inbuilt assertion functionality. The release also contains 
some bug fixes to existing analyzers.

Moreover, several dependencies have been bumped in this release.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @mikkelbu
* @rprouse

Issues Resolved

Features and Enhancements
* #414 Add DiagnosticSuppressor for Nullable<T>
* #389 Idea: Suggest Assert.Multiple in tests where Assert statements are called back to back
* #330 Analyzer to warn against calling .Length or .Count() in the actual expression

Bugs
* #410 NUnit2044 false-positive on explicit delegate type
* #407 NUnit2022 false-positive on generic Dictionary type bug

Tooling, Process, and Documentation
* #401 chore(deps): Bump NUnit3TestAdapter from 4.0.0 to 4.1.0 in /src
* #398 Ensure all NunitFrameworkConstants are tested
* #397 Allow using C# 9
* #396 chore(deps): Bump Microsoft.NET.Test.Sdk from 16.11.0 to 17.0.0 in /src
* #394 chore(deps): Bump Microsoft.CodeAnalysis.Analyzers from 3.3.2 to 3.3.3 in /src
* #392 Replace netcoreapp2.1 for test with netcoreapp3.1
* #388 chore: Bump version to 3.3 (2.3)
* #381 Update Code of Conduct


# NUnit Analyzers 3.2 (and 2.2) - August 28, 2021

This release of the NUnit Analyzers contains an improvement to the supression of 
'Non-nullable field must contain a non-null value when exiting constructor (CS8618)'
when the field/property is initialized by a method called from a SetUp/OneTimeSetUp method.

Moreover, several dependencies have been bumped in this release.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #385 Supress (even more) 'Non-nullable field must contain a non-null value when exiting constructor (CS8618)'

Tooling, Process, and Documentation
* #384 chore(deps): Bump Microsoft.NET.Test.Sdk from 16.10.0 to 16.11.0 in /src
* #379 chore(deps): Bump StyleCop.Analyzers.Unstable from 1.2.0.333 to 1.2.0.354 in /src
* #376 chore(deps): Bump NUnit3TestAdapter from 3.17.0 to 4.0.0 in /src
* #375 chore(deps): Bump Microsoft.NET.Test.Sdk from 16.9.4 to 16.10.0 in /src
* #374 chore(deps): Bump NUnit from 3.13.1 to 3.13.2 in /src
* #373 chore: Bump version to 3.2 (2.2) 
* #367 chore(deps): Bump Microsoft.CodeAnalysis.CSharp.CodeStyle from 3.8.0 to 3.9.0 in /src


# NUnit Analyzers 3.1 (and 2.1) - April 4, 2021

This release of the NUnit Analyzers primarily contains improvements to the analysis of nullable reference types
to handle even more cases.

The release also contain improvements when passing a non-lambda to Throws assertions and when non-delegate actual
value is used with DelayedConstraint.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #368 Extend NonNullableFieldIsUninitialized to also work for properties
* #363 NonNullableFieldIsUninitializedSuppressor fails when there is a constructor
* #360 NonNullableFieldIsUnintializedSuppressor doesn't recognize ExpressionBody
* #357 Warns about passing a non-lambda to Throws assertions
* #353 Replace deprecated FxCopAnalyzer with NetAnalyzer
* #345 Nullable warnings should be suppressed for Assert.Catch()
* #342 Nullable suppression for Assert.Throws does not work in presence of a Assert.Multiple block
* #149 Warning when non-delegate actual value is used with DelayedConstraint

Tooling, Process, and Documentation
* #371 chore(deps): Bump Microsoft.NET.Test.Sdk from 16.9.1 to 16.9.4 in /src
* #370 chore(deps): Bump StyleCop.Analyzers.Unstable from 1.2.0.321 to 1.2.0.333 in /src
* #365 chore(deps): Bump Microsoft.NET.Test.Sdk from 16.8.3 to 16.9.1 in /src
* #358 Bump Gu.Roslyn.Asserts from 3.3.1 to 3.3.2 in /src
* #356 chore: Bump cake bootstrap to latest to make it compatible with Cake 1.0
* #352 Bump NUnit3TestAdapter from 3.15.1 to 3.17.0 in /src
* #351 Bump NUnit from 3.13 to 3.13.1 in /src
* #350 Bump StyleCop.Analyzers.Unstable from 1.2.0.261 to 1.2.0.321 in /src
* #349 Bump Microsoft.NET.Test.Sdk from 16.8.0 to 16.8.3 in /src
* #348 Bump Nullable from 1.2.1 to 1.3.0 in /src
* #347 Update dependabot.yml
* #346 Create dependabot.yml
* #341 chore: Bump version to 3.1 (2.1)


# NUnit Analyzers 3.0 (and 2.0) - January 18, 2021

This release of the NUnit Analyzers adds the possibility to suppress compiler errors based on context.
Initially, we support the suppression of errors arising from nullable reference types (many thanks to
Manfred Brands for this major contribution). This functionality depends on a newer version of Roslyn
which is not supported in Visual Studio 2017 (only in Visual Studio 2019).

So we have decided to release two versions of the analyzers: versions starting with 2.x can be used in Visual
Studio 2017 and versions starting with 3.x can be used in Visual Studio 2019 (the major version number matches
the major version number of Roslyn). Most features will be available in both the 2.x versions and the 3.x versions,
unless they require Roslyn functionality that is only available in the 3.x versions.

The release also contains some bugfixes to existing diagnostics.

The release contains contributions from the following users (in alphabetical order):
* @Dreamescaper
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #157 Suppress nullability warnings after Is.Not.Null constraint
* #329 Analyzer to warn against comparison operators in actual expression
* #333 Add nullable suppression for Assert.Throws

Bugs
* #320 Incorrect NUnit1011 when TestCaseSource references a base class member
* #322 NUnit2021 false positive for IEquatable<T>
* #332 NUnit2026 shouldn't fire when CustomEqualityComparer is used

Tooling, Process, and Documentation
* #319 chore: Bump version to 0.7
* #323 Update to latest GuOrg.Roslyn.Asserts
* #326 fix: Correct missing change `AnalyzerAssert` => `RoslynAssert`
* #328 fix(docs): Fix linting issue
* #335 Move Constant code closer to actual rule
* #336 Create a .NETStandard2.0 release


# NUnit Analyzers 0.6 - November 24, 2020

This release of the NUnit Analyzers corrects several false positives from the analyzers.

We have also added a dependency on the Microsoft.CodeAnalysis.Analyzers package to help
us follow Roslyn best practices. The analyzer project is now multi-targeting to allow
us to better use Nullable Reference Types in the codebase.

The release contains contributions from the following users (in alphabetical order):
* @Dreamescaper
* @manfred-brands
* @mikkelbu

Issues Resolved

Bugs
* #300 NUnit2041 false positives
* #302 False positive NUnit1004 for test functions that receive array parameters
* #304 Unable to compile test project when classlib with TestCaseSources is referenced
* #306 NUnit2022 rule has false positive on hierarchies when using And
* #308 NUnit1028 false positives
* #311 NUnit1028 vs. IDisposable
* #312 NUNIT1003 false positive

Tooling, Process, and Documentation
* #299 chore: Bump version to 0.6
* #301 fix(documentation): Linting and spelling mistakes
* #314 Add Microsoft.CodeAnalysis.Analyzers


# NUnit Analyzers 0.5 - September 30, 2020

This release of the NUnit Analyzers adds three new diagnostics. One for ensuring that only test methods
are public in a test class, one disallowing the SameAs constraint on non-reference types, and one enforcing
that only compatible types are used in a comparison constraint. We have also extended several of the existing
diagnostics so that they apply to more cases.

Furthermore, we have significantly improved the performance of many of the existing diagnostics - inspired
by work done on the xunit.analyzers.

Finally, the documentation has been improved with additional information in the index and information about
configuration of the diagnostics using .editorconfig, and we have added StyleCop and FxCop analyzers
to enforce the coding standard.

The release contains contributions from the following users (in alphabetical order):
* @Dreamescaper
* @jnm2
* @manfred-brands
* @mikkelbu

Issues Resolved

Features and Enhancements
* #147 Analyzer for non-test methods to be non-public
* #197 Handle custom type converters
* #270 Get inspiration from the performance improvements made for the xunit.analyzers
* #273 NUnit2003: Add code fix with less explicit overload for testing if something is true
* #276 SameAs should warn if used with value types
* #279 Augment SameAsIncompatibleTypes and EqualToIncompatibleTypes to apply for more cases
* #293 Make diagnostic's severity consistent

Bugs
* #274 NUnit2021 fires on delegates
* #280 Consider overload resolution when transforming to comparison constraints
* #290 Not treating Func<T> and Action properly for type compatibility.

Tooling, Process, and Documentation
* #20 Add StyleCop.Analyzers & fix warnings
* #191 Make it possible to run tests both in .NET framework and .NET Core
* #210 index.md in the documentation should also contain information about Severity
* #267 chore: Bump version to 0.5
* #268 Correct typo in NUnit1027 documentation
* #271 Documentation: Add section for each analyzer on how to configure severity using an .editorconfig file
* #288 Add FxCop Roslyn analyzer
* #296 fix: Make code compile in both targets


# NUnit Analyzers 0.4 - July 25, 2020

This release of the NUnit Analyzers adds 13 new diagnostics and codefixes for asserts in the classical model -
e.g. Assert.Greater, Assert.IsNotEmpty, Assert.IsNotInstanceOf etc. We have also improved the handling of
asserts against constants and variables of type Task.

In addition, we now properly handle ValueSourceAttribute and test methods decorated with both a
TestAttribute and a TestCaseSourceAttribute/TestCaseAttribute. We have also added a diagnostic and codefix
to ensure that test methods are public.

The release contains contributions from the following users (in alphabetical order):
* @manfred-brands
* @mikkelbu

Issues Resolved

Structure, TestCase, and TestCaseSource
* #93 Analyzer to ensure that the "target" of ValueSourceAttribute exists and is static
* #212 Nunit1007 - ignore [Test] attribute when [TestCaseSource] is present
* #215 Expected a warning when no arguments are specified for a parameterized test method
* #245 Ensure that Test Methods are Public

Assertions
* #233 Code fix switching Has.Length to Has.Count for NUnit2022
* #256 NUnit2007 should not fire if both sides are constant
* #257 Need more rules for Classic Model with codefix to transform to Constraint model
* #258 NUnit2023 fires on variables of type Task

Process, documentation, and tooling
* #244 chore: Bump version to 0.4
* #253 Correct markdown linting errors in the analyzers documentation
* #261 Update .editorconfig to match overall code-style used


# NUnit Analyzers 0.3 - May 20, 2020

This release of the NUnit Analyzers improves the documentation of all the diagnostics.
Furthermore, we have added the following analyzers and diagnostics:
* an analyzer for proper usage of string constraints,
* an analyzer for proper usage of ContainsConstraint,
* an analyzer for proper usage of Does.Contain and Contains.Item,
* six new diagnostics to handle the full scope of the TestCaseSource attribute.

The release contains contributions from the following users (in alphabetical order):
 * @Dreamescaper
 * @mikkelbu
 * @SeanKilleen

Issues Resolved

Structure, TestCase, and TestCaseSource
* #204 Extend TestCaseSourceUsesStringAnalyzer to handle all constructors of TestCaseSourceAttribute
* #209 NUnit1001 and NUnit1005 complains about string to integer conversion
* #214 Add diagnostics and tests for all cases of TestCaseSourceUsesStringAnalyzer
* #227 TestCaseSource: Fix codefix for nameof so that it also works when the source is in another class

Assertions
* #219 Add warning when string analyzers are used against non-string actual argument
* #221 Add warning when ContainsConstraint is used against incompatible argument type
* #229 Fix EqualToIncompatibleTypesAnalyzer false positive
* #230 False positive NUnit2020 for Has.None.SameAs
* #235 Fix StringConstraintAnalyzer false positive for delegate/task
* #240 Add SomeItemsIncompatibleTypesAnalyzer

Process, documentation, and tooling
* #201 chore: Bump version to 0.3
* #202 chore: Add nuget badge and update download section
* #205 Document all the analyzers
* #208 Analyzers List: Add icon for "enabled by default"
* #216 refac: Replace Tuples with ValueTuples
* #222 Use nullable reference types
* #225 chore: Add missing documentation files to solution
* #226 Nullable fixes
* #228 Tests fixes
* #234 Improve every text: titles, messages, and descriptions of the analyzers and code-fixes


# NUnit Analyzers 0.2 - April 13, 2020

This is the initial release of the NUnit Analyzers. The release consists of analyzers
and code fixes for:
* proper usage of the TestCaseAttribute,
* proper usage of ParallelScopeAttribute,
* translation of assertions written in the classic model into the constraint model,
* proper usage of some of the most used assertions (Is.EqualTo, Is.SameAs, Has.Count,
  Has.Property(...), Is.Null).

The full list of analyzers can be found at https://github.com/nunit/nunit.analyzers/blob/master/documentation/index.md.

The release contains contributions from the following users (in alphabetical order):

 * @304NotModified
 * @aolszowka
 * @Dreamescaper
 * @JasonBock
 * @jcurl
 * @JohanLarsson
 * @MaximRouiller
 * @mgyongyosi
 * @mikkelbu
 * @stevenaw

Issues Resolved

Structure and TestCase
 * #1 TestCase analyser doesn't handle nullables
 * #7 Feature request: Look for async void tests
 * #8 TestCaseUsageAnalyzer throws NullReferenceException for non-existent attribute
 * #11 NRE in AnalyzePositionalArgumentsAndParameters
 * #14 NRE in AttributeArgumentSyntaxExtensions.CanAssignTo
 * #28 NRE when writing an attribute before adding a reference to NUnit
 * #41 Only examine TestCaseAttribute if reference to NUnit exists
 * #42 Reorder statements for performance reasons
 * #48 Async test method must have non-generic Task return type when no result is expected
 * #49 Message: Async test method must have Task<T> return type when a result is expected
 * #50 Remove false positive from ExpectedResult when used in connection with async tests
 * #54 Analyzer for TestCaseAttribute should also work for generic TestCases
 * #55 Analyzer for TestCaseAttribute should also work for nullable types
 * #56 Analyzer for TestCaseAttribute should work when passed 1 to 3 arguments
 * #57 Make conversions work in netcoreapp2.0
 * #64 Add an Analyzer to Verify ParallelScope Usage
 * #75 Add error if tests/testcases have a return value, but no ExpectedResult
 * #76 Add initial class for test method analyzer
 * #79 NUNIT_7 does not cater for Task and ValueTask
 * #80 Suggestion: Analyzer Should Fix TestCaseSource(string) usage to use nameof()
 * #86 Simplify TestCaseUsage
 * #92 Analyzer to ensure that the "target" of TestCaseSourceAttribute exists and is static
 * #106 Add fixer for the TestCaseSource(StringConstant) analyzer
 * #118 Async Tests display warning about missing Expected
 * #123 Generalise TestMethodUsageAnalyzer to support custom awaitables
 * #148 Analyzer update needed for string->datetime offset conversion in testcaseattribute
 * #150 Analyzers and refactorings to use string constraints instead of methods
 * #162 NUnit1001: False positive
 * #165 AD0001 Crash: TestCase contains int instead of enum
 * #169 NUnit1001 False Positive with params in the list
 * #186 False error when Uri is parameter and string is argument
 * #196 Avoid loading new assemblies into the VS process

Assertions
 * #12 NRE in ClassicModelAssertUsageAnalyzer.AnalyzeInvocation
 * #13 "Sequence contains no elements" in ClassicModelAssertUsageAnalyzer.AnalyzeInvocation
 * #39 Warning if actual type does not match expected type
 * #40 Warning if literal or const value is provided as actual value
 * #83 NUNIT_7 Throws Unexpectedly On Integer to Decimal Conversions
 * #90 Check that checks against null is done on reference types
 * #117 Code fix for ConstActualValueUsageAnalyzer
 * #124 Warning if IgnoreCase is used for non-string constraint argument
 * #128 Add analyzer to capture missing properties
 * #129 Warning if actual value is same as expected
 * #145 Make analyzers suggesting refactoring to the constraint model default disabled?
 * #146 Add analyzer and code fix for Is.EqualTo usage
 * #152 Fix CodeFix for logical not expression
 * #153 Constraint model suggestion for Assert.AreSame
 * #154 Warning when SameAs compares expressions of incompatible types
 * #158 Add collection contains analyzer and codefix
 * #160 Do not replace line breaks in code fixes
 * #163 Feature: Assert.IsNull / Assert.IsNotNull Classic to Fluent
 * #168 Add Analyzers for Assert.[Is][Not]Null(expr)
 * #174 Introduce ConstraintExpression
 * #175 EqualToIncompatibleTypesAnalyzer should not warn if constraint expression has conditional parts
 * #178 Fix EqualToIncompatibleTypesAnalyzer boxing false positive
 * #179 EqualToIncompatibleTypes should warn if two different enums compared
 * #184 Fix Property analyzer for inherited interface
 * #187 Fix EqualTo diagnostic when errors present

Process and tooling
 * #16 Make the extension work with VS 2017
 * #17 Add CI support for master and PRs
 * #19 New project format and netstandard for analyzer project
 * #21 Remove dependency on NUnit for analyzer project
 * #22 Add editorconfig
 * #23 Use get-only properties for FixableDiagnosticIds
 * #25 Add initial cake script
 * #27 Update README.md
 * #30 Refine cake configuration
 * #32 Update readme
 * #33 Add information to CONTRIBUTING.md
 * #36 Make Pack work
 * #37 Add myget feed to tooling
 * #38 Make all cs files conform to editorconfig
 * #43 Create nuget package on AppVeyor
 * #44 Set package version
 * #46 Add badge to MyGet Feed
 * #47 Examine concurrent execution af analyzers
 * #61 Update Download section
 * #63 Make Appveyor run tests before creating nuget package
 * #68 Add images to README.md
 * #70 Simplify test in this project
 * #71 Small cleanup
 * #72 Fixing repository url metadata
 * #74 Add Installation element
 * #84 Partition the analyzers into categories and create identifier ranges for each category
 * #85 Give all analyzers unique identifiers
 * #87 Simplify TestMethodUsageAnalyzerTests and TestCaseSourceUsesStringAnalyzerTests
 * #88 Upload test results to AppVeyor
 * #96 Document implemented analyzers and fixers
 * #98 Replace tuples with named tuples
 * #101 Rewrite tests of CodeFixes with Gu.Roslyn.Asserts
 * #102 Remove nunit.analyzers.integrationtests and nunit.analyzers.playground projects
 * #103 Transfer ClassicModelAssertUsage tests to Gu.Roslyn.Asserts
 * #104 Remove unused methods
 * #105 Move test data into code
 * #116 Fix licenseUrl element in nuspec, will be deprecated
 * #126 Update README and move content to CONTRIBUTING
 * #131 Bump versions of nunit, nunit-console, and NUnit3TestAdapter
 * #132 Update nunit.analyzers.csproj
 * #134 Constraint analyzers have a lot of repeating code
 * #135 Pre-release nuget packages should be created with unique assembly versions
 * #139 Tests checking documentation
 * #140 Add helplinks pointing to analyzer docs
 * #141 Improve documentation of analyzers and fixers
 * #144 feat: Add motivation and examples for classical assertions
 * #182 Remove warning from "Pack"
 * #183 fix: Make version numbers consistent
 * #185 chore: Bump version
 * #192 Add missing information to nuget package
 * #194 fix: Replace invalid tokens in suffix
