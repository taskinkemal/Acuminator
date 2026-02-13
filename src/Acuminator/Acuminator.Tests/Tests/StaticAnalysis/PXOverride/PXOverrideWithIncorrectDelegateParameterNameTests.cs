#nullable enable
using System.Collections.Immutable;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Analyzers.StaticAnalysis.PXOverride;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.PXOverride
{
	public class PXOverrideWithIncorrectDelegateParameterNameTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForIncorrectDelegateParameterNameTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new AddOrReplaceOrRenameBaseDelegateParameterFix();

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideOfPropertyFromBasePXGraphWithIncorrectDelegateParameterName.cs")]
		public Task PXOverride_OfProperty_WithIncorrect_BaseDelegateParameter_Name(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(15, 46));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideRefAndOutParametersWithIncorrectDelegateParameterName.cs")]
		public Task PXOverrides_WithRefAndOutParameters_And_Incorrect_BaseDelegateParameter_Name(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(15, 111),
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(24, 111),
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(33, 77));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideWithIncorrectDelegateParameterName.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegateParameter_Name(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(14, 96),
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(17, 132),
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(23, 152),
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.CreateFor(29, 58));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideWithIncorrectDelegateParameterName_Expected.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegateParameter_Name_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideRefAndOutParametersWithIncorrectDelegateParameterName_Expected.cs")]
		public Task PXOverrides_WithRefAndOutParameters_And_Incorrect_BaseDelegateParameter_Name_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideOfPropertyFromBasePXGraphWithIncorrectDelegateParameterName_Expected.cs")]
		public Task PXOverride_OfProperty_WithIncorrect_BaseDelegateParameter_Name_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideWithIncorrectDelegateParameterName.cs",
						  @"BaseDelegateParameter\InvalidParameterName\PXOverrideWithIncorrectDelegateParameterName_Expected.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegateParameter_Name_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideRefAndOutParametersWithIncorrectDelegateParameterName.cs",
						  @"BaseDelegateParameter\InvalidParameterName\PXOverrideRefAndOutParametersWithIncorrectDelegateParameterName_Expected.cs")]
		public Task PXOverrides_WithRefAndOutParameters_And_Incorrect_BaseDelegateParameter_Name_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\InvalidParameterName\PXOverrideOfPropertyFromBasePXGraphWithIncorrectDelegateParameterName.cs",
						  @"BaseDelegateParameter\InvalidParameterName\PXOverrideOfPropertyFromBasePXGraphWithIncorrectDelegateParameterName_Expected.cs")]
		public Task PXOverride_OfProperty_WithIncorrect_BaseDelegateParameter_Name_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		private sealed class PXOverrideAnalyzerForIncorrectDelegateParameterNameTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create
				(
					Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter
				);

			protected override void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext,
																			   IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodIsPublicNonVirtual(SymbolAnalysisContext context, PXContext pxContext, 
																		IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodForXmlDocComment(SymbolAnalysisContext context, PXContext pxContext, 
																	 PXOverrideInfo pxOverrideInfo)
			{ }
		}
	}
}