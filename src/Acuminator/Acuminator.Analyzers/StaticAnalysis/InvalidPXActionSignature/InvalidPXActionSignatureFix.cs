using System;
using System.Collections;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace Acuminator.Analyzers.StaticAnalysis.InvalidPXActionSignature
{
	[ExportCodeFixProvider(LanguageNames.CSharp), Shared]
	public class InvalidPXActionSignatureFix : PXCodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } =
			ImmutableArray.Create(Descriptors.PX1000_InvalidPXActionHandlerSignature.Id);

		protected override Task RegisterCodeFixesForDiagnosticAsync(CodeFixContext context, Diagnostic diagnostic)
		{
			context.CancellationToken.ThrowIfCancellationRequested();

			if (!diagnostic.IsRegisteredForCodeFix())
				return Task.CompletedTask;

			string codeActionName = nameof(Resources.PX1000Fix).GetLocalized().ToString();
			context.RegisterCodeFix(
				new ChangeSignatureAction(codeActionName, context.Document, context.Span),
				diagnostic);

			return Task.CompletedTask;
		}

		//-------------------------------------Code Action for Fix---------------------------------------------------------------------------
		internal class ChangeSignatureAction : CodeAction
		{
			private const string AdapterParameterName = "adapter";
			private const string AdapterGetMethodName = "Get";

			private readonly string _title;
			private readonly Document _document;
			private readonly TextSpan _span;

			public override string Title => _title;
			public override string EquivalenceKey => _title;

			public ChangeSignatureAction(string title, Document document, TextSpan span)
			{
				_title 	  = title.CheckIfNullOrWhiteSpace();
				_document = document.CheckIfNull();
				_span 	  = span;
			}

			protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var (semanticModel,root) = await _document.GetSemanticModelAndRootAsync(cancellationToken)
														  .ConfigureAwait(false);
				if (semanticModel == null || root == null)
					return _document;

				var diagnosticNode = root.FindNode(_span);
				var method = diagnosticNode?.FirstAncestorOrSelf<MethodDeclarationSyntax>();

				if (method == null || (method.Body == null && method.ExpressionBody == null))
					return _document;

				cancellationToken.ThrowIfCancellationRequested();

				var pxContext = new PXContext(semanticModel.Compilation, codeAnalysisSettings: null);
				var generator = SyntaxGenerator.GetGenerator(_document);
				var newReturnType = GetNewReturnType(generator, semanticModel);
				var newParametersList = GetNewParametersList(generator, method, pxContext, semanticModel, cancellationToken);

				if (newReturnType == null || newParametersList == null)
					return _document;

				cancellationToken.ThrowIfCancellationRequested();
				var newMethod = method.WithReturnType(newReturnType);

				if (!ReferenceEquals(method.ParameterList, newParametersList))
				{
					newMethod = newMethod.WithParameterList(newParametersList);
				}

				if (method.Body != null)
				{
					ControlFlowAnalysis? controlFlow = semanticModel.AnalyzeControlFlow(method.Body);

					if (controlFlow != null && controlFlow.Succeeded && controlFlow.ReturnStatements.IsEmpty)
					{
						newMethod = AddReturnStatement(newMethod, generator);
					}
				}
				else if (method.ExpressionBody != null)
				{
					newMethod = ConvertToBlockBodyAndAddReturnStatement(newMethod, generator);
				}
				else
					return _document;

				cancellationToken.ThrowIfCancellationRequested();

				var newRoot = root.ReplaceNode(method, newMethod);
				newRoot = AddCollectionsUsing(newRoot, generator);

				return _document.WithSyntaxRoot(newRoot);
			}

			private TypeSyntax? GetNewReturnType(SyntaxGenerator generator, SemanticModel semanticModel)
			{
				var ienumerableType = semanticModel.Compilation.GetSpecialType(SpecialType.System_Collections_IEnumerable);
				var ienumerableTypeNode = generator.TypeExpression(ienumerableType) as TypeSyntax;
				return ienumerableTypeNode;
			}

			private ParameterListSyntax? GetNewParametersList(SyntaxGenerator generator, MethodDeclarationSyntax method, PXContext pxContext,
															  SemanticModel semanticModel, CancellationToken cancellation)
			{
				var oldParameters = method.ParameterList.Parameters;

				if (oldParameters.Count > 0)
				{
					var firstParameter = oldParameters[0];
					var parameterSymbol = semanticModel.GetDeclaredSymbol(firstParameter, cancellation);

					if (pxContext.PXAdapterType.Equals(parameterSymbol?.Type, SymbolEqualityComparer.Default))
						return method.ParameterList;
				}

				var pxAdapterTypeNode = generator.TypeExpression(pxContext.PXAdapterType);
				var adapterPar		  = generator.ParameterDeclaration(AdapterParameterName, pxAdapterTypeNode) as ParameterSyntax;

				if (adapterPar == null)
					return null;

				var newParameters = oldParameters.Insert(0, adapterPar);
				return method.ParameterList.WithParameters(newParameters);
			}

			private MethodDeclarationSyntax AddReturnStatement(MethodDeclarationSyntax method, SyntaxGenerator generator)
			{
				var returnStatement = GetReturnStatement(generator);
				return returnStatement != null 
					? method.AddBodyStatements(returnStatement)
					: method;
			}

			private StatementSyntax? GetReturnStatement(SyntaxGenerator generator)
			{
				var getMethodInvocation =
					generator.InvocationExpression(
						generator.MemberAccessExpression(generator.IdentifierName(AdapterParameterName),
														 AdapterGetMethodName));

				return generator.ReturnStatement(getMethodInvocation)
							   ?.WithAdditionalAnnotations(Formatter.Annotation) as StatementSyntax;
			}

			private MethodDeclarationSyntax ConvertToBlockBodyAndAddReturnStatement(MethodDeclarationSyntax	method, SyntaxGenerator generator)
			{
				var oldBodyStatement = SyntaxFactory.ExpressionStatement(method.ExpressionBody!.Expression);
				var returnStatement  = GetReturnStatement(generator);

				if (returnStatement == null)
					return method;

				var body = SyntaxFactory.Block(oldBodyStatement, returnStatement);
				return method.WithExpressionBody(null)
							 .WithSemicolonToken(default)
							 .WithBody(body);
			}

			private SyntaxNode AddCollectionsUsing(SyntaxNode root, SyntaxGenerator generator)
			{
				if (root is not CompilationUnitSyntax compilationUnit)
					return root;

				var oldUsings = compilationUnit.Usings;
				var usingCollectionsNamespace = generator.NamespaceImportDeclaration(typeof(IEnumerable).Namespace) as UsingDirectiveSyntax;

				if (usingCollectionsNamespace == null)
					return root;

				bool usingExists = oldUsings.Any(usingDir => SyntaxFactory.AreEquivalent(usingDir, usingCollectionsNamespace));

				if (usingExists)
					return root;

				string usingCollectionsNsName = usingCollectionsNamespace.Name.ToString();
				int indexToInsert = oldUsings.IndexOf(usingDirective =>
														String.CompareOrdinal(usingDirective.Name.ToString(), usingCollectionsNsName) > 0);
				var newUsings = indexToInsert >= 0 
					? oldUsings.Insert(indexToInsert, usingCollectionsNamespace)
					: oldUsings.Add(usingCollectionsNamespace);
		
				return compilationUnit.WithUsings(newUsings);
			}
		}
	}
}