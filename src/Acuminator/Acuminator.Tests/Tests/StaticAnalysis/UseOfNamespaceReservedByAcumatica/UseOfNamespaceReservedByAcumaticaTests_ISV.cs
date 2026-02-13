using System;
using System.Threading.Tasks;

using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.UseOfNamespaceReservedByAcumatica;
using Acuminator.Utilities;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.UseOfNamespaceReservedByAcumatica
{
	public class UseOfNamespaceReservedByAcumaticaTests_ISV : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() =>
			new UseOfNamespaceReservedByAcumaticaAnalyzer(
							CodeAnalysisSettings.Default
												.WithStaticAnalysisEnabled()
												.WithSuppressionMechanismDisabled()
												.WithIsvSpecificAnalyzersEnabled());

		[Theory]
		[EmbeddedFileData("InvalidFileScopedNamespaceDeclaration.cs")] 
		public virtual Task Invalid_FileScoped_Namespace(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1039_UseOfNamespaceReservedByAcumatica.CreateFor(5, 11));

		[Theory]
		[EmbeddedFileData("InvalidNamespaceDeclarations.cs")]
		public virtual Task Invalid_NestedNamespaces(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				Descriptors.PX1039_UseOfNamespaceReservedByAcumatica.CreateFor(5, 11),
				Descriptors.PX1039_UseOfNamespaceReservedByAcumatica.CreateFor(13, 12),
				Descriptors.PX1039_UseOfNamespaceReservedByAcumatica.CreateFor(23, 11));

		[Theory]
		[EmbeddedFileData("CorrectFileScopedNamespaceDeclaration.cs")]
		public virtual Task Correct_FileScoped_Namespace(string source) =>
			VerifyCSharpDiagnosticAsync(source);

		[Theory]
		[EmbeddedFileData("CorrectNamespaceDeclarations.cs")]
		public virtual Task Correct_NestedNamespaces(string source) =>
			VerifyCSharpDiagnosticAsync(source);
	}
}
