using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Analyzers.StaticAnalysis.PXOverride
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class AddOrReplaceOrRenameBaseDelegateParameterFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create
			(
				Descriptors.PX1079_PXOverrideWithoutDelegateParameter.Id,
				Descriptors.PX1101_PXOverrideWithInvalidDelegateParameter.Id,
				Descriptors.PX1102_PXOverrideInvalidNameOfDelegateParameter.Id
			);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.IsRegisteredForCodeFix())
				return Task.CompletedTask;

			var fixMode = GetFixMode(diagnostic);

			if (!fixMode.HasValue)
				return Task.CompletedTask;

			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.TryGetPropertyValue(PXOverrideDiagnosticProperties.PatchMethodName, out string? patchMethodName) ||
				patchMethodName.IsNullOrWhiteSpace())
			{
				patchMethodName = string.Empty;
			}

			context.CancellationToken.ThrowIfCancellationRequested();
			string? title = GetCodeFixTitle(fixMode.Value, patchMethodName);

			if (title == null)
				return Task.CompletedTask;

			var document = context.Document;
			var codeAction = CodeAction.Create(title,
											   cToken => FixBaseDelegateParameterAsync(document, context.Span, patchMethodName,
																					   fixMode.Value, cToken),
											   equivalenceKey: nameof(Resources.PX1079Fix));
			context.RegisterCodeFix(codeAction, diagnostic);
			return Task.CompletedTask;
		}

		private BaseDelegateParameterFixMode? GetFixMode(Diagnostic diagnostic) =>
			diagnostic.TryGetPropertyValue(PXOverrideDiagnosticProperties.DelegateParameterFixMode, out string? fixModeString) &&
			!fixModeString.IsNullOrWhiteSpace() && Enum.TryParse(fixModeString, out BaseDelegateParameterFixMode fixMode)
				? fixMode
				: null;

		private static string? GetCodeFixTitle(BaseDelegateParameterFixMode fixMode, string patchMethodName) =>
			fixMode switch
			{
				BaseDelegateParameterFixMode.AddDelegateParameter 	  => nameof(Resources.PX1079Fix).GetLocalized(patchMethodName).ToString(),
				BaseDelegateParameterFixMode.ReplaceDelegateParameter => nameof(Resources.PX1101Fix).GetLocalized().ToString(),
				BaseDelegateParameterFixMode.RenameDelegateParameter  => nameof(Resources.PX1102Fix).GetLocalized().ToString(),
				_ 													  => null
			};

		private static async Task<Solution> FixBaseDelegateParameterAsync(Document document, TextSpan span, string patchMethodName,
																		  BaseDelegateParameterFixMode fixMode, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			var root = await document.GetSyntaxRootAsync(cancellation).ConfigureAwait(false);
			var patchMethodNode = root?.FindNode(span)?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

			if (patchMethodNode == null)
				return document.Project.Solution;

			string baseDelegateParameterName = CreateBaseDelegateParameterName(patchMethodName, patchMethodNode);
			var newSolution = await FixBaseDelegateParameterAsync(document, root!, patchMethodNode, baseDelegateParameterName, fixMode, cancellation)
										.ConfigureAwait(false);
			return newSolution;
		}

		private static string CreateBaseDelegateParameterName(string patchMethodName, MethodDeclarationSyntax patchMethodNode)
		{
			patchMethodName = patchMethodName.NullIfWhiteSpace() ?? patchMethodNode.Identifier.Text;
			return !patchMethodName.IsNullOrWhiteSpace()
				? $"base_{patchMethodName}"
				: "baseDelegate";
		}

		private static Task<Solution> FixBaseDelegateParameterAsync(Document document, SyntaxNode root, MethodDeclarationSyntax patchMethodNode,
																	string baseDelegateParameterName, BaseDelegateParameterFixMode fixMode, 
																	CancellationToken cancellation)
		{
			switch (fixMode)
			{
				case BaseDelegateParameterFixMode.AddDelegateParameter:
					{
						var newSolution = FixBaseDelegateParameterType(document, root, patchMethodNode, baseDelegateParameterName,
																		replaceLastParameter: false);
						return Task.FromResult(newSolution);
					}
				case BaseDelegateParameterFixMode.ReplaceDelegateParameter:
					{
						var newSolution = FixBaseDelegateParameterType(document, root, patchMethodNode, baseDelegateParameterName,
																		replaceLastParameter: true);
						return Task.FromResult(newSolution);
					}
				case BaseDelegateParameterFixMode.RenameDelegateParameter:
					return RenameBaseDelegateParameterAsync(document, patchMethodNode, baseDelegateParameterName, cancellation);
				default:
					return Task.FromResult(document.Project.Solution);
			}
		}

		private static Solution FixBaseDelegateParameterType(Document document, SyntaxNode root, MethodDeclarationSyntax patchMethodNode,
															 string baseDelegateParameterName, bool replaceLastParameter)
		{
			var fixedMethodNode = GetMethodNodeWithFixedBaseDelegateParameterType(document, patchMethodNode, baseDelegateParameterName,
																				  replaceLastParameter);
			if (fixedMethodNode == null)
				return document.Project.Solution;

			var newRoot = root!.ReplaceNode(patchMethodNode, fixedMethodNode);
			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument.Project.Solution;
		}

		private static async Task<Solution> RenameBaseDelegateParameterAsync(Document document, MethodDeclarationSyntax patchMethodNode, 
																			 string baseDelegateParameterName, CancellationToken cancellation)
		{
			if (patchMethodNode.ParameterList.Parameters.Count == 0 || patchMethodNode.Identifier.Text == baseDelegateParameterName)
				return document.Project.Solution;

			var semanticModel = await document.GetSemanticModelAsync(cancellation).ConfigureAwait(false);
			
			if (semanticModel == null)
				return document.Project.Solution;

			var lastParameterNode = patchMethodNode.ParameterList.Parameters[^1];
			var lastParameterSymbol = semanticModel.GetDeclaredSymbol(lastParameterNode, cancellation);

			if (lastParameterSymbol == null)
				return document.Project.Solution;

			var renamedSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, lastParameterSymbol, baseDelegateParameterName,
																  document.Project.Solution.Options, cancellation)
											   .ConfigureAwait(false);
			return renamedSolution;
		}

		private static MethodDeclarationSyntax? GetMethodNodeWithFixedBaseDelegateParameterType(Document document, MethodDeclarationSyntax patchMethodNode,
																								string baseDelegateParameterName, bool replaceLastParameter)
		{
			var baseDelegateType = CreateBaseDelegateParameterType(patchMethodNode, replaceLastParameter);

			if (baseDelegateType == null)
				return null;

			var delegateParameter = Parameter(
										Identifier(baseDelegateParameterName))
									.WithType(baseDelegateType);

			if (replaceLastParameter)
			{
				var newParametersList = patchMethodNode.ParameterList.Parameters.RemoveAt(patchMethodNode.ParameterList.Parameters.Count - 1)
																				.Add(delegateParameter);
				return patchMethodNode.WithParameterList(
										ParameterList(newParametersList));
			}
			else
				return patchMethodNode.AddParameterListParameters(delegateParameter);
		}

		private static TypeSyntax? CreateBaseDelegateParameterType(MethodDeclarationSyntax patchMethodNode, bool replaceLastParameter)
		{
			const int maxFuncAndActionParametersCount = 16; // Maximum number of parameters supported by Func and Action delegates is 16
			var parameters = patchMethodNode.ParameterList.Parameters;
			int parametersCountToUse = replaceLastParameter
				? parameters.Count - 1
				: parameters.Count;

			if (parametersCountToUse > maxFuncAndActionParametersCount)
				return null;

			bool useActionDelegate = patchMethodNode.IsVoidMethod();
			string delegateTypeName = useActionDelegate
				? nameof(Action)
				: nameof(Func<object>);

			if (useActionDelegate && parametersCountToUse == 0)
				return IdentifierName(delegateTypeName);        // Case of non-generic action delegate with no parameters

			var baseDelegateTypeParameters = GetBaseDelegateTypeParameters(patchMethodNode, parametersCountToUse, useActionDelegate);

			if (baseDelegateTypeParameters?.Count is null or 0)
				return null;

			var delegateTypeNameNode = 
				GenericName(
					Identifier(delegateTypeName),
					TypeArgumentList(
						SeparatedList(baseDelegateTypeParameters)
						)
					);

			return delegateTypeNameNode;
		}

		private static List<TypeSyntax>? GetBaseDelegateTypeParameters(MethodDeclarationSyntax patchMethodNode, int parametersCountToUse,
																	   bool useActionDelegate)
		{
			var parameters = patchMethodNode.ParameterList.Parameters;
			int estimatedCapacity = useActionDelegate ? parametersCountToUse : (parametersCountToUse + 1);
			var baseDelegateTypeParameters = new List<TypeSyntax>(estimatedCapacity);
			
			for (int i = 0; i < parametersCountToUse; i++)
			{
				if (parameters[i].Type is not TypeSyntax parameterType)
					return null;

				baseDelegateTypeParameters.Add(parameterType);
			}

			if (!useActionDelegate)
				baseDelegateTypeParameters.Add(patchMethodNode.ReturnType); // Need to add return type for Func delegate

			return baseDelegateTypeParameters;
		}
	}
}
