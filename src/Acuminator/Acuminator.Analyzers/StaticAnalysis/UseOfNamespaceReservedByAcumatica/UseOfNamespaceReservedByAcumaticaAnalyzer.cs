using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.DiagnosticSuppression;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.UseOfNamespaceReservedByAcumatica;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseOfNamespaceReservedByAcumaticaAnalyzer : PXDiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
		ImmutableArray.Create
		(
			Descriptors.PX1039_UseOfNamespaceReservedByAcumatica
		);

	public UseOfNamespaceReservedByAcumaticaAnalyzer() : base() { }

	/// <summary>
	/// Constructor accepting code analysis settings for tests.
	/// </summary>
	/// <param name="codeAnalysisSettings">The code analysis settings.</param>
	public UseOfNamespaceReservedByAcumaticaAnalyzer(CodeAnalysisSettings codeAnalysisSettings) : base(codeAnalysisSettings)
	{
	}

	protected override bool ShouldAnalyze(PXContext pxContext) => 
		base.ShouldAnalyze(pxContext) && pxContext.CodeAnalysisSettings.IsvSpecificAnalyzersEnabled;

	protected override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
	{
		var fileScopedNamespaceDeclaration = (SyntaxKind)SharedConstants.FileScopedNamespaceDeclarationKind;
		compilationStartContext.RegisterSyntaxNodeAction(syntaxContext => AnalyzeNamespaceDeclaration(syntaxContext, pxContext),
														 SyntaxKind.NamespaceDeclaration, fileScopedNamespaceDeclaration);
	}

	private void AnalyzeNamespaceDeclaration(SyntaxNodeAnalysisContext syntaxContext, PXContext pxContext)
	{
		syntaxContext.CancellationToken.ThrowIfCancellationRequested();
		
		var namespaceNameNode = GetNamespaceNameNodeToCheck(syntaxContext.Node);
		string? namespaceName = namespaceNameNode?.ToString().NullIfWhiteSpace();

		if (namespaceName.IsNullOrWhiteSpace())
			return;

		if (namespaceName.Equals(NamespaceNames.AcumaticaRootNamespace, StringComparison.OrdinalIgnoreCase) ||
			namespaceName.StartsWith(NamespaceNames.AcumaticaRootNamespaceWithDot, StringComparison.OrdinalIgnoreCase))
		{
			var identifier = syntaxContext.Node.ChildNodes().OfType<NameSyntax>().FirstOrDefault();
			var location = identifier?.GetLocation();

			syntaxContext.ReportDiagnosticWithSuppressionCheck(
				Diagnostic.Create(Descriptors.PX1039_UseOfNamespaceReservedByAcumatica, location),
				pxContext.CodeAnalysisSettings);
		}
	}

	private NameSyntax? GetNamespaceNameNodeToCheck(SyntaxNode nodeToCheck)
	{
		if (nodeToCheck is NamespaceDeclarationSyntax namespaceDeclaration)
		{
			var containingNamespaces = namespaceDeclaration.GetContainingNamespaces()
														   .ToList(capacity: 4);
			if (containingNamespaces.Count == 0)
				return namespaceDeclaration.Name;
			else
			{
				var outmostNamespace = containingNamespaces[^1];
				return outmostNamespace is NamespaceDeclarationSyntax outmostNamespaceDeclaration
					? outmostNamespaceDeclaration.Name
					: outmostNamespace.ChildNodes()
									  .OfType<NameSyntax>()
									  .FirstOrDefault();
			}
		}
		else if (nodeToCheck.RawKind == SharedConstants.FileScopedNamespaceDeclarationKind)
		{
			return nodeToCheck.ChildNodes()
							  .OfType<NameSyntax>()
							  .FirstOrDefault();
		}
		else
			return null;
	}
}
