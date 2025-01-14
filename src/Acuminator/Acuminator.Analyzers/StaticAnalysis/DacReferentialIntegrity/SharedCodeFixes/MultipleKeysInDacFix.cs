﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Analyzers.StaticAnalysis.DacReferentialIntegrity
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class DuplicateKeysInDacFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = 
			ImmutableArray.Create(Descriptors.PX1035_MultipleKeyDeclarationsInDacWithSameFields.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

            if (diagnostic.AdditionalLocations.Count == 0)
                return Task.CompletedTask;

			var codeActionTitle = nameof(Resources.PX1035Fix).GetLocalized().ToString();
			var codeAction = CodeAction.Create(codeActionTitle,
											   cancellation => DeleteOtherPrimaryKeyDeclarationsFromDacAsync(context.Document, diagnostic.AdditionalLocations, cancellation),
											   equivalenceKey: codeActionTitle);

			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private async Task<Document> DeleteOtherPrimaryKeyDeclarationsFromDacAsync(Document document, IReadOnlyList<Location> locationsToRemove,
																				   CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			SyntaxNode? root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);

			if (root == null)
				return document;

			var nodesToRemove = locationsToRemove.Select(location => root.FindNode(location.SourceSpan))
												 .OfType<ClassDeclarationSyntax>();

			var newRoot = root.RemoveNodes(nodesToRemove, SyntaxRemoveOptions.KeepNoTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives)!;
			var newDocument = document.WithSyntaxRoot(newRoot);

			cancellation.ThrowIfCancellationRequested();
			return newDocument;			
		}
    }
}
