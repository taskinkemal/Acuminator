﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Acuminator.Utilities.Roslyn.Constants;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	public class DacPrimaryKeyDeclarationAnalyzer : DacKeyDeclarationAnalyzerBase
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create
			(
				Descriptors.PX1033_MissingDacPrimaryKeyDeclaration,
				Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac,
				Descriptors.PX1036_WrongDacPrimaryKeyName
			);

		protected override bool IsKeySymbolDefined(PXContext context) => context.ReferentialIntegritySymbols.IPrimaryKey != null;

		protected override bool ShouldAnalyzeDac(PXContext context, DacSemanticModel dac) =>
			 base.ShouldAnalyzeDac(context, dac) && dac.DacProperties.Count(property => property.IsKey) > 0;

		public override void Analyze(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();

			var keyDeclarations = GetPrimaryKeyDeclarations(context, dac).ToList(capacity: 1);

			switch (keyDeclarations.Count)
			{
				case 0:
					ReportNoPrimaryKeyDeclarationsInDac(symbolContext, context, dac);
					return;
				case 1:
					AnalyzePrimaryKeyDeclaration(symbolContext, context, keyDeclarations[0]);
					return;
				default:
					ReportMultiplePrimaryKeyDeclarationsInDac(symbolContext, context, dac, keyDeclarations);
					return;
			}
		}

		private IEnumerable<INamedTypeSymbol> GetPrimaryKeyDeclarations(PXContext context, DacSemanticModel dac)
		{
			var nestedTypes = dac.Symbol.GetTypeMembers();
			return nestedTypes.IsDefaultOrEmpty
				? Enumerable.Empty<INamedTypeSymbol>()
				: nestedTypes.Where(type => type.ImplementsInterface(context.ReferentialIntegritySymbols.IPrimaryKey));
		}

		private void ReportNoPrimaryKeyDeclarationsInDac(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac)
		{
			Location location = dac.Node.Identifier.GetLocation() ?? dac.Node.GetLocation();

			if (location != null)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1033_MissingDacPrimaryKeyDeclaration, location),
					context.CodeAnalysisSettings);
			} 
		}

		private void ReportMultiplePrimaryKeyDeclarationsInDac(SymbolAnalysisContext symbolContext, PXContext context, DacSemanticModel dac, 
															   List<INamedTypeSymbol> keyDeclarations)
		{
			symbolContext.CancellationToken.ThrowIfCancellationRequested();
			var locations = keyDeclarations.Select(declaration => declaration.GetSyntax(symbolContext.CancellationToken))
										   .OfType<ClassDeclarationSyntax>()
										   .Select(keyClassDeclaration => keyClassDeclaration.Identifier.GetLocation() ??
																		  keyClassDeclaration.GetLocation())
										   .Where(location => location != null);

			foreach (var location in locations)
			{
				symbolContext.ReportDiagnosticWithSuppressionCheck(
					Diagnostic.Create(Descriptors.PX1035_MultiplePrimaryKeyDeclarationsInDac, location),
					context.CodeAnalysisSettings);
			}
		}

		private void AnalyzePrimaryKeyDeclaration(SymbolAnalysisContext symbolContext, PXContext context, INamedTypeSymbol keyDeclaration)
		{
			if (keyDeclaration.Name == TypeNames.PrimaryKeyClassName)
				return;

			var keyDeclarationNode = keyDeclaration.GetSyntax(symbolContext.CancellationToken);
			Location location = (keyDeclarationNode as ClassDeclarationSyntax)?.Identifier.GetLocation() ?? keyDeclarationNode?.GetLocation();

			if (location == null)
				return;

			symbolContext.ReportDiagnosticWithSuppressionCheck(
										Diagnostic.Create(Descriptors.PX1036_WrongDacPrimaryKeyName, location),
										context.CodeAnalysisSettings);
		}
	}
}