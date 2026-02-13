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
	public class PXOverrideWithoutXmlCommentTests : CodeFixVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new PXGraphAnalyzer(
				CodeAnalysisSettings.Default
									.WithRecursiveAnalysisEnabled()
									.WithStaticAnalysisEnabled()
									.WithSuppressionMechanismDisabled(),
				new PXOverrideAnalyzerForMissingXmlCommentTests());

		protected override CodeFixProvider GetCSharpCodeFixProvider() => new AddXmlDocCommentWithReferenceToBaseMethodFix();

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideRefAndOutParametersWithoutXmlComment.cs")]
		public Task PXOverride_Methods_WithRefAndOutParameters_WithoutXmlDocComment(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(15, 15),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(24, 15),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(33, 28));

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideWithoutXmlComment.cs")]
		public Task PXOverrides_WithoutXmlDocComment(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(23, 17),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(29, 15),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(35, 27),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(43, 15),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(50, 14),
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(62, 15));

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideOfPropertyFromBasePXGraphWithoutXmlComment.cs")]
		public Task PXOverride_Property_WithoutXmlDocComment(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment.CreateFor(14, 15));

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideWithoutXmlComment_Expected.cs")]
		public Task PXOverrides_WithoutXmlDocComment_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideOfPropertyFromBasePXGraphWithoutXmlComment_Expected.cs")]
		public Task PXOverride_Property_WithoutXmlDocComment_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideRefAndOutParametersWithoutXmlComment_Expected.cs")]
		public Task PXOverride_Methods_WithRefAndOutParameters_WithoutXmlDocComment_AfterCodeFix(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideWithoutXmlComment.cs",
						  @"XmlComment\PXOverrideWithoutXmlComment_Expected.cs")]
		public Task PXOverride_Methods_WithoutXmlDocComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideOfPropertyFromBasePXGraphWithoutXmlComment.cs",
						  @"XmlComment\PXOverrideOfPropertyFromBasePXGraphWithoutXmlComment_Expected.cs")]
		public Task PXOverride_Property_WithoutXmlDocComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData(@"XmlComment\PXOverrideRefAndOutParametersWithoutXmlComment.cs",
						  @"XmlComment\PXOverrideRefAndOutParametersWithoutXmlComment_Expected.cs")]
		public Task PXOverride_Methods_WithRefAndOutParameters_WithoutXmlDocComment_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);

		private sealed class PXOverrideAnalyzerForMissingXmlCommentTests : PXOverrideAnalyzer
		{
			public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
				ImmutableArray.Create
				(
					Descriptors.PX1098_PXOverrideMethodWithoutXmlDocComment
				);

			protected override void ReportPatchMethodWithIncompatibleSignature(SymbolAnalysisContext context, PXContext pxContext,
																			   IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodIsPublicNonVirtual(SymbolAnalysisContext context, PXContext pxContext,
																		IMethodSymbol patchMethodWithPXOverride)
			{ }

			protected override void CheckPatchMethodBaseDelegateParameter(SymbolAnalysisContext context, PXContext pxContext,
																			PXOverrideInfo pxOverrideInfo)
			{ }
		}
	}
}