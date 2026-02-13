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
	public class PXOverrideWithoutBaseDelegateParameterTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForNoDelegateParameterTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new AddOrReplaceOrRenameBaseDelegateParameterFix();

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideRefAndOutParametersWithoutBaseDelegateParameter.cs")]
		public Task PXOverrides_WithRefAndOutParameters_Without_BaseDelegateParameter(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(15, 15),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(21, 15),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(27, 28));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter.cs")]
		public Task PXOverrides_Without_BaseDelegateParameter(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(12, 16),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(15, 15),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(20, 17),
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.CreateFor(26, 15));

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithCustomBaseDelegateParameter.cs")]
		public Task PXOverrides_With_BaseDelegateParameter_WithCustomDelegateTypes(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_Without_BaseDelegateParameter_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter.cs",
						  @"BaseDelegateParameter\WithoutParameter\PXOverrideWithoutBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_Without_BaseDelegateParameter_CodeFix(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);

		// This test checks that code fix in fact is not registered and applied for the cases when method signature has non-trivial ref kinds.
		[Theory]
		[EmbeddedFileData(@"BaseDelegateParameter\WithoutParameter\PXOverrideRefAndOutParametersWithoutBaseDelegateParameter.cs",
						  @"BaseDelegateParameter\WithoutParameter\PXOverrideRefAndOutParametersWithoutBaseDelegateParameter_Expected.cs")]
		public Task PXOverrides_WithRefAndOutParameters_Without_BaseDelegateParameter_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		private sealed class PXOverrideAnalyzerForNoDelegateParameterTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create
				(
					Descriptors.PX1079_PXOverrideWithoutDelegateParameter
				);

			protected override void CheckPatchMethodIsPublicNonVirtual(SymbolAnalysisContext context, PXContext pxContext,
																	   IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext,
																				IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodForXmlDocComment(SymbolAnalysisContext context, PXContext pxContext,
																	 PXOverrideInfo pxOverrideInfo)
			{ }
		}
	}
}