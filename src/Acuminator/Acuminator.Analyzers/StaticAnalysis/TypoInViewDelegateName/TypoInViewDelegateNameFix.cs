﻿
using System;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

namespace Acuminator.Analyzers.StaticAnalysis.TypoInViewDelegateName
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class TypoInViewDelegateNameFix : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1005_TypoInViewDelegateName.Id);

		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

			var methodNode = root?.FindNode(context.Span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();
			if (methodNode == null)
				return;

			var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == Descriptors.PX1005_TypoInViewDelegateName.Id);
			if (diagnostic == null)
				return;

			if (!diagnostic.TryGetPropertyValue(TypoInViewDelegateNameAnalyzer.ViewFieldNameProperty, out string? fieldName) ||
				fieldName.IsNullOrWhiteSpace() || fieldName.Length <= 1)
			{
				return;
			}

			context.CancellationToken.ThrowIfCancellationRequested();

			string title = nameof(Resources.PX1005Fix).GetLocalized().ToString();
			var document = context.Document;
			var codeAction = CodeAction.Create(title, 
											   cToken => FixTypoInViewDelegateName(document, methodNode, fieldName, cToken), 
											   equivalenceKey: title);
			context.RegisterCodeFix(codeAction, context.Diagnostics);
		}

		private static async Task<Solution> FixTypoInViewDelegateName(Document document, MethodDeclarationSyntax methodNode, 
																	  string fieldName, CancellationToken cToken)
		{
			cToken.ThrowIfCancellationRequested();

			var semanticModel = await document.GetSemanticModelAsync(cToken).ConfigureAwait(false);
			var methodSymbol = semanticModel.GetDeclaredSymbol(methodNode);

			if (methodSymbol == null)
				return document.Project.Solution;

			string? newName = GenerateViewDelegateName(fieldName);

			if (newName == null)
				return document.Project.Solution;

			return await Renamer.RenameSymbolAsync(document.Project.Solution, methodSymbol, newName, document.Project.Solution.Options, cToken);
		}

		private static string? GenerateViewDelegateName(string viewName)
		{
			char firstChar = viewName[0];

			if (Char.IsUpper(firstChar))
				return viewName.FirstCharToLower();
			else if (Char.IsLower(firstChar))
				return viewName.ToPascalCase();
			else
				return null;
		}
	}
}
