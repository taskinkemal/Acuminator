﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acuminator.Utilities;
using Acuminator.Utils.RoslynExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ConnectionScopeInRowSelectingAnalyzer : PXDiagnosticAnalyzer
	{
		private class Walker : NestedInvocationWalker
		{
			private class PXConnectionScopeVisitor : CSharpSyntaxVisitor<bool>
			{
				private readonly PXContext _pxContext;
				private readonly SemanticModel _semanticModel;

				public PXConnectionScopeVisitor(PXContext pxContext, SemanticModel semanticModel)
				{
					pxContext.ThrowOnNull(nameof(pxContext));
					semanticModel.ThrowOnNull(nameof(semanticModel));
					
					_pxContext = pxContext;
					_semanticModel = semanticModel;
				}

				public override bool VisitUsingStatement(UsingStatementSyntax node)
				{
					return (node.Declaration?.Accept(this) ?? false) || (node.Expression?.Accept(this) ?? false);
				}

				public override bool VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
				{
					var symbolInfo = _semanticModel.GetSymbolInfo(node.Type);
					return symbolInfo.Symbol?.OriginalDefinition != null 
						&& symbolInfo.Symbol.OriginalDefinition.Equals(_pxContext.PXConnectionScope);
				}
			}

			private static readonly IEnumerable<string> MethodPrefixes = new[] { "Select", "Search", "Update", "Delete" };

			private SymbolAnalysisContext _context;
			private readonly PXContext _pxContext;
			private bool _insideConnectionScope;
			private readonly PXConnectionScopeVisitor _connectionScopeVisitor;

			public Walker(SymbolAnalysisContext context, PXContext pxContext, SemanticModel semanticModel)
				: base(semanticModel, context.CancellationToken)
			{
				pxContext.ThrowOnNull(nameof (pxContext));

				_context = context;
				_pxContext = pxContext;

				_connectionScopeVisitor = new PXConnectionScopeVisitor(pxContext, semanticModel);
			}

			public override void VisitUsingStatement(UsingStatementSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				if (_insideConnectionScope)
				{
					base.VisitUsingStatement(node);
				}
				else
				{
					_insideConnectionScope = node.Accept(_connectionScopeVisitor);
					base.VisitUsingStatement(node);
					_insideConnectionScope = false;
				}
			}

			public override void VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				_context.CancellationToken.ThrowIfCancellationRequested();

				if (_insideConnectionScope)
					return;
				
				var methodSymbol = GetSymbol<IMethodSymbol>(node);

				if (methodSymbol != null && IsDatabaseCall(methodSymbol))
				{
					ReportDiagnostic(OriginalNode ?? node);
				}
				else
				{
					base.VisitInvocationExpression(node);
				}
			}

			private bool IsDatabaseCall(IMethodSymbol candidate)
			{
				var containingType = candidate.ContainingType?.OriginalDefinition;
				return MethodPrefixes.Any(p => candidate.Name.StartsWith(p, StringComparison.Ordinal))
				       && containingType != null
				       && (containingType.IsBqlCommand(_pxContext)
				           || containingType.InheritsFromOrEquals(_pxContext.PXViewType)
				           || containingType.InheritsFromOrEquals(_pxContext.PXSelectorAttribute)
				           || containingType.Equals(_pxContext.PXDatabase));
			}

			private void ReportDiagnostic(SyntaxNode node)
			{
				_context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1042_ConnectionScopeInRowSelecting,
					node.GetLocation()));
			}
		}

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1042_ConnectionScopeInRowSelecting);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeMethod(c, pxContext), SymbolKind.Method);
		}

		private void AnalyzeMethod(SymbolAnalysisContext context, PXContext pxContext)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var methodSymbol = (IMethodSymbol) context.Symbol;
			
			if (methodSymbol != null && IsRowSelectingMethod(methodSymbol, pxContext))
			{
				var methodSyntax = methodSymbol.GetSyntax(context.CancellationToken) as MethodDeclarationSyntax;
				if (methodSyntax != null)
				{
					var semanticModel = context.Compilation.GetSemanticModel(methodSyntax.SyntaxTree);
					methodSyntax.Accept(new Walker(context, pxContext, semanticModel));
				}
			}
		}

		private bool IsRowSelectingMethod(IMethodSymbol symbol, PXContext pxContext)
		{
			if (symbol.ReturnsVoid && symbol.TypeParameters.IsEmpty && !symbol.Parameters.IsEmpty)
			{
				// Loosely check method signature because sometimes business logic from event handler calls is extracted to a separate method

				// New generic event syntax
				if (symbol.Parameters[0].Type.OriginalDefinition.Equals(pxContext.Events.RowSelecting))
					return true;

				// Old syntax
				if (symbol.Parameters.Length >= 2
				    && symbol.Parameters[0].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.PXCacheType)
				    && symbol.Parameters[1].Type.OriginalDefinition.InheritsFromOrEquals(pxContext.Events.PXRowSelectingEventArgs))
					return true;
			}

			
			return false;
		}
	}
}
