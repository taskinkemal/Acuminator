﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Verification
{
	/// <summary>
	/// Superclass of all Unit Tests for DiagnosticAnalyzers
	/// </summary>
	public abstract class DiagnosticVerifier
	{
		#region To be implemented by Test classes
		/// <summary>
		/// Get the CSharp analyzer being tested - to be implemented in non-abstract class
		/// </summary>
		protected abstract DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer();
		#endregion

		#region Verifier wrappers
		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
		/// Note: input a DiagnosticResult for each Diagnostic expected
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on</param>
		/// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
		protected Task VerifyCSharpDiagnosticAsync(string source, params DiagnosticResult[] expected) => 
			VerifyDiagnosticsAsync(new[] { source }, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument: true);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source.</param>
		protected Task VerifyCSharpDiagnosticAsync(string source, bool checkOnlyFirstDocument, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(new[] { source }, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
		/// Note: input a DiagnosticResult for each Diagnostic expected
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on</param>
		/// <param name="additionalSource">Additional source document to run the analyzer on</param>
		/// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
		protected Task VerifyCSharpDiagnosticAsync(string source, string additionalSource, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(new[] { source, additionalSource }, 
				LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument: true);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on.</param>
		/// <param name="additionalSource">Additional source document to run the analyzer on.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source.</param>
		protected Task VerifyCSharpDiagnosticAsync(string source, string additionalSource, bool checkOnlyFirstDocument, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(new[] { source, additionalSource },
				LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source
		/// Note: input a DiagnosticResult for each Diagnostic expected
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on</param>
		/// <param name="additionalSource1">Additional source document to run the analyzer on</param>
		/// <param name="additionalSource2">Additional source document to run the analyzer on</param>
		/// <param name="expected"> DiagnosticResults that should appear after the analyzer is run on the source</param>
		protected Task VerifyCSharpDiagnosticAsync(string source, string additionalSource1, string additionalSource2, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(new[] { source, additionalSource1, additionalSource2 }, 
								  LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument: true);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on.</param>
		/// <param name="additionalSource1">Additional source document to run the analyzer on.</param>
		/// <param name="additionalSource2">Additional source document to run the analyzer on.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source.</param>
		protected Task VerifyCSharpDiagnosticAsync(string source, string additionalSource1, string additionalSource2, 
														bool checkOnlyFirstDocument, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(new[] { source, additionalSource1, additionalSource2 },
								  LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source.  
		/// Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="sources">An array of strings to create source documents from to run the analyzers on.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources.</param>
		protected Task VerifyCSharpDiagnosticAsync(string[] sources, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument: true);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source.  
		/// Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="sources">An array of strings to create source documents from to run the analyzers on.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources.</param>
		protected Task VerifyCSharpDiagnosticAsync(string[] sources, bool checkOnlyFirstDocument, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument);

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source.</param>
		protected void VerifyCSharpDiagnostic(string source, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(new[] { source }, LanguageNames.CSharp, 
									GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument: true).Wait();

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the single inputted string as a source Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="source">A class in the form of a string to run the analyzer on.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the source.</param>
		protected void VerifyCSharpDiagnostic(string source, bool checkOnlyFirstDocument, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(new[] { source }, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument).Wait();

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="sources">An array of strings to create source documents from to run the analyzers on.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources.</param>
		protected void VerifyCSharpDiagnostic(string[] sources, params DiagnosticResult[] expected) =>
			VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument: true).Wait();

		/// <summary>
		/// Called to test a C# DiagnosticAnalyzer when applied on the inputted strings as a source Note: input a DiagnosticResult for each Diagnostic expected.
		/// </summary>
		/// <param name="sources">An array of strings to create source documents from to run the analyzers on.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources.</param>
		protected void VerifyCSharpDiagnostic(string[] sources, bool checkOnlyFirstDocument, params DiagnosticResult[] expected) => 
			VerifyDiagnosticsAsync(sources, LanguageNames.CSharp, GetCSharpDiagnosticAnalyzer(), expected, checkOnlyFirstDocument).Wait();

		/// <summary>
		/// General method that gets a collection of actual diagnostics found in the source after the analyzer is run, then verifies each of them.
		/// </summary>
		/// <param name="sources">An array of strings to create source documents from to run the analyzers on.</param>
		/// <param name="language">The language of the classes represented by the source strings.</param>
		/// <param name="analyzer">The analyzer to be run on the source code.</param>
		/// <param name="expected">DiagnosticResults that should appear after the analyzer is run on the sources.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <returns>
		/// An asynchronous result.
		/// </returns>
		private async Task VerifyDiagnosticsAsync(string[] sources, string language, DiagnosticAnalyzer analyzer, 
												  DiagnosticResult[] expected, bool checkOnlyFirstDocument)
		{
			try
			{
				var diagnostics = await GetSortedDiagnosticsAsync(sources, language, analyzer, checkOnlyFirstDocument).ConfigureAwait(false);
				VerifyDiagnosticResults(diagnostics, analyzer, expected);
			}
			catch (AggregateException aggregateException)
			{
				var innerExceptions = aggregateException.Flatten().InnerExceptions;

				switch (innerExceptions.Count)
				{
					case 0:
						throw;
					case 1:
						throw innerExceptions[0];
					default:
						string errorMsg = string.Join(Environment.NewLine + Environment.NewLine, innerExceptions);
						throw new Exception(errorMsg);
				}
			}	
		}

		#endregion

		#region Actual comparisons and verifications
		/// <summary>
		/// Checks each of the actual Diagnostics found and compares them with the corresponding DiagnosticResult in the array of expected results.
		/// Diagnostics are considered equal only if the DiagnosticResultLocation, Id, Severity, and Message of the DiagnosticResult match the actual diagnostic.
		/// </summary>
		/// <param name="actualResults">The Diagnostics found by the compiler after running the analyzer on the source code</param>
		/// <param name="analyzer">The analyzer that was being run on the sources</param>
		/// <param name="expectedResults">Diagnostic Results that should have appeared in the code</param>
		private static void VerifyDiagnosticResults(IEnumerable<Diagnostic> actualResults, DiagnosticAnalyzer analyzer, params DiagnosticResult[] expectedResults)
		{
			int expectedCount = expectedResults.Count();
			int actualCount = actualResults.Count();

			if (expectedCount != actualCount)
			{
				string diagnosticsOutput = actualResults.Any() ? FormatDiagnostics(analyzer, actualResults.ToArray()) : "    NONE.";
                
				Assert.True(false,
					string.Format("Mismatch between number of diagnostics returned, expected \"{0}\" actual \"{1}\"\r\n\r\nDiagnostics:\r\n{2}\r\n", expectedCount, actualCount, diagnosticsOutput));
			}

			for (int i = 0; i < expectedResults.Length; i++)
			{
				var actual = actualResults.ElementAt(i);
				var expected = expectedResults[i];

				if (expected.Line == -1 && expected.Column == -1)
				{
					if (actual.Location != Location.None)
					{
						Assert.True(false,
							string.Format("Expected:\nA project diagnostic with No location\nActual:\n{0}",
							FormatDiagnostics(analyzer, actual)));
					}
				}
				else
				{
					VerifyDiagnosticLocation(analyzer, actual, actual.Location, expected.Locations.First());
					var additionalLocations = actual.AdditionalLocations.ToArray();

					if (additionalLocations.Length != expected.Locations.Length - 1)
					{
						Assert.True(false,
							string.Format("Expected {0} additional locations but got {1} for Diagnostic:\r\n    {2}\r\n",
								expected.Locations.Length - 1, additionalLocations.Length,
								FormatDiagnostics(analyzer, actual)));
					}

					for (int j = 0; j < additionalLocations.Length; ++j)
					{
						VerifyDiagnosticLocation(analyzer, actual, additionalLocations[j], expected.Locations[j + 1]);
					}
				}

				if (actual.Id != expected.Id)
				{
					Assert.True(false,
						string.Format("Expected diagnostic id to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
							expected.Id, actual.Id, FormatDiagnostics(analyzer, actual)));
				}

				if (actual.Severity != expected.Severity)
				{
					Assert.True(false,
						string.Format("Expected diagnostic severity to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
							expected.Severity, actual.Severity, FormatDiagnostics(analyzer, actual)));
				}

				if (actual.GetMessage() != expected.Message)
				{
					Assert.True(false,
						string.Format("Expected diagnostic message to be \"{0}\" was \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
							expected.Message, actual.GetMessage(), FormatDiagnostics(analyzer, actual)));
				}
			}
		}

		/// <summary>
		/// Helper method to VerifyDiagnosticResult that checks the location of a diagnostic and compares it with the location in the expected DiagnosticResult.
		/// </summary>
		/// <param name="analyzer">The analyzer that was being run on the sources</param>
		/// <param name="diagnostic">The diagnostic that was found in the code</param>
		/// <param name="actual">The Location of the Diagnostic found in the code</param>
		/// <param name="expected">The DiagnosticResultLocation that should have been found</param>
		private static void VerifyDiagnosticLocation(DiagnosticAnalyzer analyzer, Diagnostic diagnostic, Location actual, DiagnosticResultLocation expected)
		{
			var actualSpan = actual.GetLineSpan();

			Assert.True(actualSpan.Path == expected.Path || (actualSpan.Path != null && actualSpan.Path.Contains("Test0.") && expected.Path.Contains("Test.")),
				string.Format("Expected diagnostic to be in file \"{0}\" was actually in file \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
					expected.Path, actualSpan.Path, FormatDiagnostics(analyzer, diagnostic)));

			var actualLinePosition = actualSpan.StartLinePosition;

			// Only check line position if there is an actual line in the real diagnostic
			if (actualLinePosition.Line > 0)
			{
				if (actualLinePosition.Line + 1 != expected.Line)
				{
					Assert.True(false,
						string.Format("Expected diagnostic to be on line \"{0}\" was actually on line \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
							expected.Line, actualLinePosition.Line + 1, FormatDiagnostics(analyzer, diagnostic)));
				}
			}

			// Only check column position if there is an actual column position in the real diagnostic
			if (actualLinePosition.Character > 0)
			{
				if (actualLinePosition.Character + 1 != expected.Column)
				{
					Assert.True(false,
						string.Format("Expected diagnostic to start at column \"{0}\" was actually at column \"{1}\"\r\n\r\nDiagnostic:\r\n    {2}\r\n",
							expected.Column, actualLinePosition.Character + 1, FormatDiagnostics(analyzer, diagnostic)));
				}
			}
		}
		#endregion

		#region Formatting Diagnostics
		/// <summary>
		/// Helper method to format a Diagnostic into an easily readable string
		/// </summary>
		/// <param name="analyzer">The analyzer that this verifier tests</param>
		/// <param name="diagnostics">The Diagnostics to be formatted</param>
		/// <returns>The Diagnostics formatted as a string</returns>
		private static string FormatDiagnostics(DiagnosticAnalyzer analyzer, params Diagnostic[] diagnostics)
		{
			var builder = new StringBuilder();
			for (int i = 0; i < diagnostics.Length; ++i)
			{
				builder.AppendLine("// " + diagnostics[i].ToString());

				var analyzerType = analyzer.GetType();
				var rules = analyzer.SupportedDiagnostics;

				foreach (var rule in rules)
				{
					if (rule != null && rule.Id == diagnostics[i].Id)
					{
						var location = diagnostics[i].Location;
						if (location == Location.None)
						{
							builder.AppendFormat("GetGlobalResult({0}.{1})", analyzerType.Name, rule.Id);
						}
						else
						{
							Assert.True(location.IsInSource,
								$"Test base does not currently handle diagnostics in metadata locations. Diagnostic in metadata: {diagnostics[i]}\r\n");

							string resultMethodName = diagnostics[i].Location.SourceTree.FilePath.EndsWith(".cs") ? "GetCSharpResultAt" : "GetBasicResultAt";
							var linePosition = diagnostics[i].Location.GetLineSpan().StartLinePosition;

							builder.AppendFormat("{0}({1}, {2}, {3}.{4})",
								resultMethodName,
								linePosition.Line + 1,
								linePosition.Character + 1,
								analyzerType.Name,
								rule.Id);
						}

						if (i != diagnostics.Length - 1)
						{
							builder.Append(',');
						}

						builder.AppendLine();
						break;
					}
				}
			}
			return builder.ToString();
		}
		#endregion

		#region Static helpers
		/// <summary>
		/// Given classes in the form of strings, their language, and an IDiagnosticAnlayzer to apply to it, return the diagnostics found in the string after converting it to a document.
		/// </summary>
		/// <param name="sources">Classes in the form of strings.</param>
		/// <param name="language">The language the source classes are in.</param>
		/// <param name="analyzer">The analyzer to be run on the sources.</param>
		/// <param name="checkOnlyFirstDocument">True to check only first document.</param>
		/// <returns>
		/// An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location.
		/// </returns>
		protected static Task<Diagnostic[]> GetSortedDiagnosticsAsync(string[] sources, string language, 
																	  DiagnosticAnalyzer analyzer, bool checkOnlyFirstDocument)
		{
			return GetSortedDiagnosticsFromDocumentsAsync(analyzer, VerificationHelper.GetDocuments(sources, language), checkOnlyFirstDocument);
		}

		/// <summary>
		/// Given an analyzer and a document to apply it to, run the analyzer and gather an array of diagnostics found in it.
		/// The returned diagnostics are then ordered by location in the source document.
		/// </summary>
		/// <param name="analyzer">The analyzer to run on the documents</param>
		/// <param name="documents">The Documents that the analyzer will be run on</param>
		/// <returns>An IEnumerable of Diagnostics that surfaced in the source code, sorted by Location</returns>
		protected static async Task<Diagnostic[]> GetSortedDiagnosticsFromDocumentsAsync(DiagnosticAnalyzer analyzer, Document[] documents, 
																						 bool checkOnlyFirstDocument)
		{
			var projects = new HashSet<Project>();

			foreach (var document in documents)
			{
				projects.Add(document.Project);
			}

			var allDiagnostics = new List<Diagnostic>();
			var compilationWithAnalyzerOptions = CreateCompilationWithAnalyzersOptions();
			var analyzers = ImmutableArray.Create(analyzer);

			foreach (var project in projects)
			{
				var compilation = await project.GetCompilationAsync().ConfigureAwait(false);
				var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers, compilationWithAnalyzerOptions);
				var projectDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);

				foreach (var diag in projectDiagnostics)
				{
					if (diag.Location == Location.None || diag.Location.IsInMetadata)
					{
						allDiagnostics.Add(diag);
					}
					else
					{
						int documentsToCheckCount = checkOnlyFirstDocument
							? Math.Min(1, documents.Length)
							: documents.Length;

						for (int i = 0; i < documentsToCheckCount; i++)
						{
							var document = documents[i];
							string documentPath = document.FilePath ?? document.Name;

							if (documentPath == diag.Location.SourceTree.FilePath)
							{
								allDiagnostics.Add(diag);
							}
						}
					}
				}
			}

			var results = SortDiagnostics(allDiagnostics);
			allDiagnostics.Clear();
			return results;
		}

		/// <summary>
		/// Creates compilation with analyzers options with default values + do not use concurrent analysis during the debugging of unit tests.
		/// </summary>
		/// <returns>
		/// The new compilation with analyzers options with default values.
		/// </returns>
		private static CompilationWithAnalyzersOptions CreateCompilationWithAnalyzersOptions()
		{
			bool isUnderDebug = Debugger.IsAttached;
			return new CompilationWithAnalyzersOptions(options: null, onAnalyzerException: null,
													   concurrentAnalysis: !isUnderDebug, logAnalyzerExecutionTime: true,
													   reportSuppressedDiagnostics: false, analyzerExceptionFilter: null);
		}

		/// <summary>
		/// Sort diagnostics by location in source document
		/// </summary>
		/// <param name="diagnostics">The list of Diagnostics to be sorted</param>
		/// <returns>An IEnumerable containing the Diagnostics in order of Location</returns>
		private static Diagnostic[] SortDiagnostics(IEnumerable<Diagnostic> diagnostics)
		{
			return diagnostics.OrderBy(d => d.Location.SourceSpan.Start).ToArray();
		}
		#endregion
	}
}
