﻿using System;
using System.Linq;

using Acuminator.Analyzers.StaticAnalysis.AnalyzersAggregator;
using Acuminator.Analyzers.StaticAnalysis.AutoNumberAttribute;
using Acuminator.Analyzers.StaticAnalysis.ConstructorInDac;
using Acuminator.Analyzers.StaticAnalysis.DacExtensionDefaultAttribute;
using Acuminator.Analyzers.StaticAnalysis.DacFieldAndReferencedFieldMismatch;
using Acuminator.Analyzers.StaticAnalysis.DacKeyFieldDeclaration;
using Acuminator.Analyzers.StaticAnalysis.DacNonAbstractFieldType;
using Acuminator.Analyzers.StaticAnalysis.DacPropertyAttributes;
using Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity;
using Acuminator.Analyzers.StaticAnalysis.DacUiAttributes;
using Acuminator.Analyzers.StaticAnalysis.ForbiddenFieldsInDac;
using Acuminator.Analyzers.StaticAnalysis.InheritanceFromPXCacheExtension;
using Acuminator.Analyzers.StaticAnalysis.LegacyBqlField;
using Acuminator.Analyzers.StaticAnalysis.MethodsUsageInDac;
using Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived;
using Acuminator.Analyzers.StaticAnalysis.MissingTypeListAttribute;
using Acuminator.Analyzers.StaticAnalysis.NoBqlFieldForDacFieldProperty;
using Acuminator.Analyzers.StaticAnalysis.NoIsActiveMethodForExtension;
using Acuminator.Analyzers.StaticAnalysis.NonNullableTypeForBqlField;
using Acuminator.Analyzers.StaticAnalysis.NonPublicGraphsDacsAndExtensions;
using Acuminator.Analyzers.StaticAnalysis.PropertyAndBqlFieldTypesMismatch;
using Acuminator.Analyzers.StaticAnalysis.PXGraphCreationInGraphInWrongPlaces;
using Acuminator.Analyzers.StaticAnalysis.PXGraphUsageInDac;
using Acuminator.Analyzers.StaticAnalysis.UnderscoresInDac;
using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.Dac
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DacAnalyzersAggregator : SymbolAnalyzersAggregator<IDacAnalyzer>
    {
        protected override SymbolKind SymbolKind => SymbolKind.NamedType;

        public DacAnalyzersAggregator() : this(null,
			new DacPropertyAttributesAnalyzer(),
			new DacAutoNumberAttributeAnalyzer(),
			new DacNonAbstractFieldTypeAnalyzer(),
			new ConstructorInDacAnalyzer(),
			new UnderscoresInDacAnalyzer(),
			new NonPublicGraphAndDacAndExtensionsAnalyzer(),
			new ForbiddenFieldsInDacAnalyzer(),
			new DacUiAttributesAnalyzer(),
			new InheritanceFromPXCacheExtensionAnalyzer(),
			new NoBqlFieldForDacFieldPropertyAnalyzer(),
			new MissingBqlFieldRedeclarationInDerivedDacAnalyzer(),
			new PropertyAndBqlFieldTypesMismatchAnalyzer(),
			new LegacyBqlFieldAnalyzer(),
			new MethodsUsageInDacAnalyzer(),
			new KeyFieldDeclarationAnalyzer(),
			new DacPrimaryAndUniqueKeyDeclarationAnalyzer(),
			new DacForeignKeyDeclarationAnalyzer(),
			new DacExtensionDefaultAttributeAnalyzer(),
			new NonNullableTypeForBqlFieldAnalyzer(),
			new MissingTypeListAttributeAnalyzer(),
			new PXGraphUsageInDacAnalyzer(),
			new NoIsActiveMethodForExtensionAnalyzer(),
			new PXGraphCreationInGraphInWrongPlacesDacAnalyzer(),
			new DacFieldAndReferencedFieldMismatchAnalyzer())
		{
        }

        /// <summary>
        /// Constructor for the unit tests.
        /// </summary>
        public DacAnalyzersAggregator(CodeAnalysisSettings? settings, params IDacAnalyzer[] innerAnalyzers) : base(settings, innerAnalyzers)
        {
        }

		protected override void AnalyzeSymbol(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (context.Symbol is not INamedTypeSymbol type)
				return;

			var inferredDacModel = DacSemanticModel.InferModel(pxContext, type, cancellation: context.CancellationToken);

			if (inferredDacModel == null)
				return;

			context.CancellationToken.ThrowIfCancellationRequested();
			var effectiveDacAnalyzers = _innerAnalyzers.Where(analyzer => analyzer.ShouldAnalyze(pxContext, inferredDacModel))
													   .ToList(capacity: _innerAnalyzers.Length);

			RunAggregatedAnalyzersInParallel(effectiveDacAnalyzers, context, analyzerIndex =>
			{
				context.CancellationToken.ThrowIfCancellationRequested();

				var aggregatedAnalyzer = effectiveDacAnalyzers[analyzerIndex];
				aggregatedAnalyzer.Analyze(context, pxContext, inferredDacModel);
			});
		}
    }
}
