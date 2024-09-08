﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.CodeGeneration;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.MissingBqlFieldRedeclarationInDerived
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class MissingBqlFieldRedeclarationInDerivedDacFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1067_MissingBqlFieldRedeclarationInDerivedDac.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.DacName, out string? dacName) ||
				!diagnostic.TryGetPropertyValue(DiagnosticProperty.DacFieldName, out string? bqlFieldName) ||
				dacName.IsNullOrWhiteSpace() || bqlFieldName.IsNullOrWhiteSpace())
			{
				return Task.CompletedTask;
			}

			if (!diagnostic.TryGetPropertyValue(DiagnosticProperty.BqlFieldType, out string? bqlFieldTypeName))
				return Task.CompletedTask;

			bqlFieldTypeName = bqlFieldTypeName.NullIfWhiteSpace();

			// equivalence key should not contain format arguments to allow mass code fixes
			string equivalenceKey = nameof(Resources.PX1067FixFormat).GetLocalized().ToString();
			string codeActionName = nameof(Resources.PX1067FixFormat).GetLocalized(bqlFieldName, dacName).ToString();
			var codeAction = CodeAction.Create(codeActionName,
											   cToken => RedeclareBqlFieldAsync(context.Document, bqlFieldName, bqlFieldTypeName,
																				context.Span, cToken),
											   equivalenceKey);
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private async Task<Document> RedeclareBqlFieldAsync(Document document, string bqlFieldName, string? bqlFieldTypeName,
															TextSpan span, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			SyntaxNode? nodeWithDiagnostic = root?.FindNode(span);

			if (nodeWithDiagnostic == null)
				return document;

			var propertyWithoutBqlFieldNode = (nodeWithDiagnostic as PropertyDeclarationSyntax) ??
											   nodeWithDiagnostic?.Parent<PropertyDeclarationSyntax>();
			cancellationToken.ThrowIfCancellationRequested();

			var dacNode = propertyWithoutBqlFieldNode != null
				? propertyWithoutBqlFieldNode.Parent<ClassDeclarationSyntax>()
				: nodeWithDiagnostic as ClassDeclarationSyntax;

			if (dacNode == null)
				return document;

			BqlFieldTypeName? strongBqlFieldTypeName = !bqlFieldTypeName.IsNullOrWhiteSpace()
				? new BqlFieldTypeName(bqlFieldTypeName)
				: null;
			var newDacMembers = CreateMembersListWithBqlField(dacNode, propertyWithoutBqlFieldNode, bqlFieldName, strongBqlFieldTypeName);

			if (newDacMembers == null)
				return document;

			var newDacNode = dacNode.WithMembers(newDacMembers.Value);
			var newRoot = root!.ReplaceNode(dacNode, newDacNode);

			return document.WithSyntaxRoot(newRoot);
		}

		private SyntaxList<MemberDeclarationSyntax>? CreateMembersListWithBqlField(ClassDeclarationSyntax dacNode,
																				   PropertyDeclarationSyntax? propertyWithoutBqlFieldNode,
																				   string bqlFieldName, BqlFieldTypeName? bqlFieldTypeName)
		{
			var members = dacNode.Members;

			if (members.Count == 0)
			{
				var newSingleBqlFieldNode = CreateBqlFieldClassNode(propertyWithoutBqlFieldNode, bqlFieldName, isFirstField: true,
																	bqlFieldTypeName);
				return newSingleBqlFieldNode != null
					? SingletonList<MemberDeclarationSyntax>(newSingleBqlFieldNode)
					: null;
			}

			int indexToInsertBqlField = GetIndexToInsertBqlFieldRedeclaration(dacNode, propertyWithoutBqlFieldNode, bqlFieldName);

			if (indexToInsertBqlField < 0)
				indexToInsertBqlField = 0;

			var newBqlFieldNode = CreateBqlFieldClassNode(propertyWithoutBqlFieldNode, bqlFieldName, isFirstField: indexToInsertBqlField == 0,
														  bqlFieldTypeName);
			if (newBqlFieldNode == null)
				return null;

			var newMembers = members;

			if (propertyWithoutBqlFieldNode != null)
			{
				var propertyWithoutRegions = CodeGeneration.RemoveRegionsFromPropertyLeadingTrivia(propertyWithoutBqlFieldNode);
				newMembers = members.Replace(propertyWithoutBqlFieldNode, propertyWithoutRegions);
			}
			
			newMembers = newMembers.Insert(indexToInsertBqlField, newBqlFieldNode);
			return newMembers;
		}

		private ClassDeclarationSyntax? CreateBqlFieldClassNode(PropertyDeclarationSyntax? propertyWithoutBqlFieldNode, string bqlFieldName,
																bool isFirstField, BqlFieldTypeName? bqlFieldTypeName)
		{
			if (bqlFieldTypeName.HasValue)
			{
				var stronglyTypedBqlFieldNode = BqlFieldGeneration.GenerateTypedBqlField(bqlFieldTypeName.Value, bqlFieldName, isFirstField,
																						 isRedeclaration: true, propertyWithoutBqlFieldNode);
				return stronglyTypedBqlFieldNode;
			}
			else
			{
				var weaklyTypedBqlFieldNode = BqlFieldGeneration.GenerateWeaklyTypedBqlField(bqlFieldName, isFirstField, isRedeclaration: true,
																							 propertyWithoutBqlFieldNode);
				return weaklyTypedBqlFieldNode;
			}
		}

		private int GetIndexToInsertBqlFieldRedeclaration(ClassDeclarationSyntax dacNode, PropertyDeclarationSyntax? propertyWithoutBqlFieldNode,
														  string bqlFieldName)
		{
			var dacMembers = dacNode.Members;

			if (dacMembers.Count == 0)
				return 0;

			if (propertyWithoutBqlFieldNode != null)
			{
				int propertyMemberIndex = dacMembers.IndexOf(propertyWithoutBqlFieldNode);
				return propertyMemberIndex < 0 
					? 0 
					: propertyMemberIndex;
			}
			else
			{
				// insert BQL field redeclaration at the end of the DAC
				return dacMembers.Count;
			}
		}
	}
}