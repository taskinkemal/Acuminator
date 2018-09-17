﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Xunit;
using static Acuminator.Tests.Verification.VerificationHelper;

namespace Acuminator.Tests.Verification
{
	/// <summary>
	/// Superclass of all Unit tests made for diagnostics with codefixes.
	/// Contains methods used to verify correctness of codefixes
	/// </summary>
	public abstract class CodeFixVerifier : DiagnosticVerifier
	{
		/// <summary>
		/// Returns the codefix being tested (C#) - to be implemented in non-abstract class
		/// </summary>
		/// <returns>The CodeFixProvider to be used for CSharp code</returns>
		protected virtual CodeFixProvider GetCSharpCodeFixProvider()
		{
			return null;
		}

		/// <summary>
		/// Returns the codefix being tested (VB) - to be implemented in non-abstract class
		/// </summary>
		/// <returns>The CodeFixProvider to be used for VisualBasic code</returns>
		protected virtual CodeFixProvider GetBasicCodeFixProvider()
		{
			return null;
		}


		/// <summary>
		/// Called to test a C# codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		protected Task VerifyCSharpFixAsync(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
		{
			return VerifyFixAsync(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
		}

		/// <summary>
		/// Called to test a VB codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		protected Task VerifyBasicFixAsync(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
		{
			return VerifyFixAsync(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics);
		}

		/// <summary>
		/// Called to test a C# codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		protected void VerifyCSharpFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
		{
			VerifyFixAsync(LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), GetCSharpCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics).Wait();
		}

		/// <summary>
		/// Called to test a VB codefix when applied on the inputted string as a source
		/// </summary>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		protected void VerifyBasicFix(string oldSource, string newSource, int? codeFixIndex = null, bool allowNewCompilerDiagnostics = false)
		{
			VerifyFixAsync(LanguageNames.VisualBasic, GetBasicDiagnosticAnalyzer(), GetBasicCodeFixProvider(), oldSource, newSource, codeFixIndex, allowNewCompilerDiagnostics).Wait();
		}

		/// <summary>
		/// General verifier for codefixes.
		/// Creates a Document from the source string, then gets diagnostics on it and applies the relevant codefixes.
		/// Then gets the string after the codefix is applied and compares it with the expected result.
		/// Note: If any codefix causes new diagnostics to show up, the test fails unless allowNewCompilerDiagnostics is set to true.
		/// </summary>
		/// <param name="language">The language the source code is in</param>
		/// <param name="analyzer">The analyzer to be applied to the source code</param>
		/// <param name="codeFixProvider">The codefix to be applied to the code wherever the relevant Diagnostic is found</param>
		/// <param name="oldSource">A class in the form of a string before the CodeFix was applied to it</param>
		/// <param name="newSource">A class in the form of a string after the CodeFix was applied to it</param>
		/// <param name="codeFixIndex">Index determining which codefix to apply if there are multiple</param>
		/// <param name="allowNewCompilerDiagnostics">A bool controlling whether or not the test will fail if the CodeFix introduces other warnings after being applied</param>
		private async Task VerifyFixAsync(string language, DiagnosticAnalyzer analyzer, CodeFixProvider codeFixProvider, string oldSource, string newSource, int? codeFixIndex, bool allowNewCompilerDiagnostics)
		{
			var document = CreateDocument(oldSource, language);
			var analyzerDiagnostics = await GetSortedDiagnosticsFromDocumentsAsync(analyzer, new[] { document }).ConfigureAwait(false);
			var compilerDiagnostics = await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false);
			var attempts = analyzerDiagnostics.Length;

			for (int i = 0; i < attempts; ++i)
			{
				var actions = new List<CodeAction>();
				var context = new CodeFixContext(document, analyzerDiagnostics[0], (a, d) => actions.Add(a), CancellationToken.None);
				await codeFixProvider.RegisterCodeFixesAsync(context).ConfigureAwait(false);

				if (!actions.Any())
				{
					break;
				}

				if (codeFixIndex != null)
				{
					document = await ApplyCodeActionAsync(document, actions.ElementAt((int)codeFixIndex)).ConfigureAwait(false);
					break;
				}

				document = await ApplyCodeActionAsync(document, actions.ElementAt(0)).ConfigureAwait(false);
				analyzerDiagnostics = await GetSortedDiagnosticsFromDocumentsAsync(analyzer, new[] { document }).ConfigureAwait(false);

				var newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false));

				//check if applying the code fix introduced any new compiler diagnostics
				if (!allowNewCompilerDiagnostics && newCompilerDiagnostics.Any())
				{
					// Format and get the compiler diagnostics again so that the locations make sense in the output
					document = document.WithSyntaxRoot(Formatter.Format(await document.GetSyntaxRootAsync().ConfigureAwait(false), 
						Formatter.Annotation, document.Project.Solution.Workspace));
					newCompilerDiagnostics = GetNewDiagnostics(compilerDiagnostics, await GetCompilerDiagnosticsAsync(document).ConfigureAwait(false));

					Assert.True(false,
						string.Format("Fix introduced new compiler diagnostics:\r\n{0}\r\n\r\nNew document:\r\n{1}\r\n",
							string.Join("\r\n", newCompilerDiagnostics.Select(d => d.ToString())),
							(await document.GetSyntaxRootAsync().ConfigureAwait(false)).ToFullString()));
				}

				//check if there are analyzer diagnostics left after the code fix
				if (!analyzerDiagnostics.Any())
				{
					break;
				}
			}

			//after applying all of the code fixes, compare the resulting string to the inputted one
			var actual = await GetStringFromDocumentAsync(document).ConfigureAwait(false);
			Assert.Equal(newSource, actual);
		}
	}
}