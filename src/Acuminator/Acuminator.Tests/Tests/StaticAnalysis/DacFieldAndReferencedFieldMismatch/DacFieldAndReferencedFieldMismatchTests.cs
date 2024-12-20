using System.Threading.Tasks;
using Acuminator.Analyzers.StaticAnalysis.Dac;
using Acuminator.Analyzers.StaticAnalysis.DacFieldAndReferencedFieldMismatch;
using Acuminator.Tests.Helpers;
using Acuminator.Tests.Verification;
using Acuminator.Utilities;
using Microsoft.CodeAnalysis.Diagnostics;
using Xunit;
using static Acuminator.Analyzers.StaticAnalysis.Descriptors;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacFieldAndReferencedFieldMismatch
{
	public class DacFieldAndReferencedFieldMismatchTests : DiagnosticVerifier
	{
		protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => 
			new DacAnalyzersAggregator(
				CodeAnalysisSettings.Default.WithStaticAnalysisEnabled()
											.WithSuppressionMechanismDisabled(),
				new DacFieldAndReferencedFieldMismatchAnalyzer());

		[Theory]
		[EmbeddedFileData("MismatchedLengthDac.cs")]
		public Task PropertiesWithWrongLengthAreMarkedAsErroneous(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize.CreateFor(46, 4, "PaymentTermsListID", "SubstitutionID", "25"),
				PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize.CreateFor(59, 4, "PaymentTermsListID2", "SubstitutionID", "25"),
				PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize.CreateFor(72, 4, "PaymentTermsListID4", "SubstitutionID", "25"),
				PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize.CreateFor(84, 4, "ConnectViaSearch", "SubstitutionID", "25"),
				PX1078_TypesOfDacFieldAndReferencedFieldHaveDifferentSize.CreateFor(105, 4, "Aggregated16", "SubstitutionID", "25"));

		[Theory]
		[EmbeddedFileData("MismatchedTypeDac.cs")]
		public Task PropertyWithWrongTypeIsMarkedAsErroneous(string source) =>
			VerifyCSharpDiagnosticAsync(source,
				PX1078_TypesOfDacFieldAndReferencedFieldMismatch.CreateFor(22, 23, "PaymentTermsListID3", "SubstitutionID", "String"),
				PX1078_TypesOfDacFieldAndReferencedFieldMismatch.CreateFor(33, 23, "ConnectViaSearchWithFilter", "SubstitutionID", "String"));
	}
}
