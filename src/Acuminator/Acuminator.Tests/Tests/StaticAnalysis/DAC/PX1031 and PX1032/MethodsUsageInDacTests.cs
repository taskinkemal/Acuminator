﻿using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using TestHelper;
using Microsoft.CodeAnalysis;
using Xunit;
using Acuminator.Tests.Helpers;

namespace Acuminator.Tests
{
    public class MethodsUsageInDacTests : DiagnosticVerifier
    {
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new DacMethodsUsageAnalyzer();

        private DiagnosticResult CreatePX1031DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1031_DacCannotContainInstanceMethods.Id,
                Message = Descriptors.PX1031_DacCannotContainInstanceMethods.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        private DiagnosticResult CreatePX1032DiagnosticResult(int line, int column)
        {
            return new DiagnosticResult
            {
                Id = Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.Id,
                Message = Descriptors.PX1032_DacPropertyCannotContainMethodInvocations.Title.ToString(),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        [Theory]
        [EmbeddedFileData(@"Dac\PX1031 and PX1032\Diagnostics\DacWithMethodsUsage.cs")]
        public void Test_Methods_Usage_In_Dac(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1032DiagnosticResult(23, 17),
                CreatePX1031DiagnosticResult(27, 9));
        }

        [Theory]
        [EmbeddedFileData(@"Dac\PX1031 and PX1032\Diagnostics\DacExtensionWithMethodsUsage.cs")]
        public void Test_Methods_Usage_In_Dac_Extension(string source)
        {
            VerifyCSharpDiagnostic(source,
                CreatePX1032DiagnosticResult(27, 17),
                CreatePX1031DiagnosticResult(31, 9));
        }
    }
}
