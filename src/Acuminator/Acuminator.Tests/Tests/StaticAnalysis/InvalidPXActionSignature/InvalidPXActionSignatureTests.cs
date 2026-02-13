using System;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature;
using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidPXActionSignature
{
	public class InvalidPXActionSignatureTests : CodeFixVerifier
	{
		protected override CodeFixProvider GetCSharpCodeFixProvider() => new InvalidPXActionSignatureFix();

		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new PXGraphAnalyzer(CodeAnalysisSettings.Default, 
				new InvalidPXActionSignatureAnalyzer());

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature.cs")]
		public Task Invalid_ActionSignature_InPXGraph(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, Descriptors.PX1000_InvalidPXActionHandlerSignature.CreateFor(line: 17, column: 15));

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignatureGraphExtension.cs")]
		public Task Invalid_ActionSignature_InPXGraphExtension(string actual) =>
			VerifyCSharpDiagnosticAsync(actual, 
				Descriptors.PX1000_InvalidPXActionHandlerSignature.CreateFor(line: 36, column: 15));

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature_WithPXOverride.cs")]
		public Task Invalid_ActionSignature_WithPXOverride(string actual) =>
			VerifyCSharpDiagnosticAsync(actual,
				Descriptors.PX1000_InvalidPXActionHandlerSignature.CreateFor(line: 20, column: 22),
				Descriptors.PX1000_InvalidPXActionHandlerSignature.CreateFor(line: 24, column: 15));

		[Theory]
		[EmbeddedFileData("ValidPXActionSignature_WithParameters.cs")]
		public Task Valid_ActionSignature_WithParameters_NoDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("ValidPXActionSignature_WithPXOverride.cs")]
		public Task Valid_ActionSignature_WithPXOverride_NoDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature_Expected.cs")]
		public Task Invalid_ActionSignature_InPXGraph_AfterCodeFix_NoDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignatureGraphExtension_Expected.cs")]
		public Task Invalid_ActionSignature_InPXGraphExtension_AfterCodeFix_NoDiagnostic(string actual) =>
			VerifyCSharpDiagnosticAsync(actual);

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature.cs",
						  "InvalidPXActionSignature_Expected.cs")]
		public Task Invalid_ActionSignature_InPXGraph_CodeFix(string actual, string expected) => 
			VerifyCSharpFixAsync(actual, expected);

		[Theory]
		[EmbeddedFileData("InvalidPXActionSignatureGraphExtension.cs",
						  "InvalidPXActionSignatureGraphExtension_Expected.cs")]
		public Task Invalid_ActionSignature_InPXGraphExtension_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);


		// Check that no code fix is registered for action delegates with PXOverrides
		[Theory]
		[EmbeddedFileData("InvalidPXActionSignature_WithPXOverride.cs",
						  "InvalidPXActionSignature_WithPXOverride_Expected.cs")]
		public Task Invalid_ActionSignature_WithPXOverride_CodeFix(string actual, string expected) =>
			VerifyCSharpFixAsync(actual, expected);
	}
}
