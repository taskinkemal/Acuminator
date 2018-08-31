﻿using Acuminator.Analyzers.StaticAnalysis;
using Acuminator.Analyzers.StaticAnalysis.LongOperationDelegateClosures;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;

namespace Acuminator.Tests.Tests.StaticAnalysis.LongOperationDelegateClosures
{
    public class LongOperationDelegateClosuresTests : Verification.CodeFixVerifier
    {
        [Theory]
        [EmbeddedFileData("LongOperationDelegateClosures.cs")]
        public void TestDiagnostic(string actual)
        {
            VerifyCSharpDiagnostic(actual,
				new DiagnosticResult[]
				{
					CreatePX1008DiagnosticResult(line: 23, column: 3),
					CreatePX1008DiagnosticResult(line: 34, column: 3),
					CreatePX1008DiagnosticResult(line: 37, column: 3),
					CreatePX1008DiagnosticResult(line: 44, column: 3),
					CreatePX1008DiagnosticResult(line: 45, column: 3),
				});
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new LongOperationDelegateClosuresAnalyzer();
        }

		private DiagnosticResult CreatePX1008DiagnosticResult(int line, int column) =>
			new DiagnosticResult
			{
				Id = Descriptors.PX1008_LongOperationDelegateClosures.Id,
				Message = Descriptors.PX1008_LongOperationDelegateClosures.Title.ToString(),
				Severity = DiagnosticSeverity.Warning,
				Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
			};
	}
}
