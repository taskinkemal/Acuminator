﻿
using System.Collections.Immutable;

using Acuminator.Analyzers.StaticAnalysis.PXGraph;
using Acuminator.Utilities;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Semantic.PXGraph;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Acuminator.Analyzers.StaticAnalysis.SavingChanges
{
    public class SavingChangesInGraphSemanticModelAnalyzer : PXGraphAggregatedAnalyzerBase
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(
                Descriptors.PX1058_PXGraphSavingChangesDuringInitialization,
                Descriptors.PX1083_SavingChangesInDataViewDelegate);

		public override void Analyze(SymbolAnalysisContext context, PXContext pxContext, PXGraphEventSemanticModel pxGraph)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            SaveChangesWalker walker = new SaveChangesWalker(context, pxContext, Descriptors.PX1058_PXGraphSavingChangesDuringInitialization);

            foreach (GraphInitializerInfo initializer in pxGraph.Initializers)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(initializer.Node);
            }

            walker = new SaveChangesWalker(context, pxContext, Descriptors.PX1083_SavingChangesInDataViewDelegate);

            foreach (DataViewDelegateInfo del in pxGraph.ViewDelegates)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                walker.Visit(del.Node);
            }
        }

        private class SaveChangesWalker : NestedInvocationWalker
        {
            private readonly SymbolAnalysisContext _context;
            private readonly DiagnosticDescriptor _descriptor;

            public SaveChangesWalker(SymbolAnalysisContext context, PXContext pxContext, DiagnosticDescriptor descriptor)
                : base(pxContext, context.CancellationToken)
            {
                _context = context;
                _descriptor = descriptor;
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                _context.CancellationToken.ThrowIfCancellationRequested();

                IMethodSymbol? symbol = GetSymbol<IMethodSymbol>(node);
                SemanticModel? semanticModel = GetSemanticModel(node.SyntaxTree);

                if (symbol != null && semanticModel != null && 
					SaveOperationHelper.GetSaveOperationKind(symbol, node, semanticModel, PxContext) != SaveOperationKind.None)
                {
                    ReportDiagnostic(_context.ReportDiagnostic, _descriptor, node);
                }
                else
                {
                    base.VisitInvocationExpression(node);
                }
            }
        }
    }
}
