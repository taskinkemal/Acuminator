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
	public class PXOverrideWithIncorrectBaseDelegateParameterTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForIncorrectBaseDelegateParameterTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new AddOrReplaceOrRenameBaseDelegateParameterFix();

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideRefAndOutParametersMismatch.cs")]
		public Task PXOverrides_With_BaseDelegateParameter_WithIncorrect_RefModifiers(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(15, 86),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(24, 86),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(33, 52));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegateParameter(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(14, 63),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(18, 9),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(24, 10),
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.CreateFor(30, 27));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_With_Incorrect_BaseDelegateParameter_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter.cs",
						  @"BaseDelegateParameter\IncorrectParameter\PXOverrideWithIncorrectBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_With_BaseDelegateParameters_WithSignatureMismatch_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		// This test checks that code fix in fact is not registered and applied for the cases when method signature has non-trivial ref kinds.
		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\IncorrectParameter\PXOverrideRefAndOutParametersMismatch.cs",
						  @"BaseDelegateParameter\IncorrectParameter\PXOverrideRefAndOutParametersMismatch_Expected.cs")]
		public Task PXOverrides_WithRefAndOutParameters_And_BaseDelegateParameters_WithSignatureMismatch_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);


		private sealed class PXOverrideAnalyzerForIncorrectBaseDelegateParameterTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create
				(
					Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter
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