﻿using System;
using System.Collections.Generic;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Acuminator.Utilities.Roslyn.CodeGeneration
{
	/// <summary>
	/// Roslyn utils for code generation.
	/// </summary>
	public static class CodeGeneration
	{
		/// <summary>
		/// Adds a missing using directive for namespace with name <paramref name="namespaceName"/> at the end of the using directives list.
		/// If the namespace is present does not return anything.
		/// </summary>
		/// <param name="root">The root node.</param>
		/// <param name="namespaceName">Name of the namespace.</param>
		/// <returns>
		/// The root node with added using directive.
		/// </returns>
		public static CompilationUnitSyntax AddMissingUsingDirectiveForNamespace(this CompilationUnitSyntax root, string namespaceName)
		{
			root.ThrowOnNull();
			namespaceName.ThrowOnNullOrWhiteSpace();

			bool alreadyHasUsing = root.Usings.Any(usingDirective => namespaceName == usingDirective.Name?.ToString());

			if (alreadyHasUsing)
				return root;

			return root.AddUsings(
							UsingDirective(
								ParseName(namespaceName)));
		}

		public static ClassDeclarationSyntax? GenerateBqlField(PropertyDeclarationSyntax? property, string propertyTypeName, string bqlFieldName,
															   bool isFirstField)
		{
			var bqlFieldBaseTypeNode = BaseTypeForBqlField(propertyTypeName, bqlFieldName);

			if (bqlFieldBaseTypeNode == null)
				return null;

			var baseTypesListNode = BaseList(
										SingletonSeparatedList<BaseTypeSyntax>(bqlFieldBaseTypeNode));
			var bqlFieldNode = ClassDeclaration(
									attributeLists: default,
									TokenList(
										Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AbstractKeyword)),
									Identifier(bqlFieldName), typeParameterList: null, baseTypesListNode,
									constraintClauses: default, members: default)
								.WithOpenBraceToken(
									Token(leading: TriviaList(), SyntaxKind.OpenBraceToken, TriviaList()));

			var clostBracketToken = isFirstField
				? Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken, 
						TriviaList(CarriageReturn, LineFeed, CarriageReturn, LineFeed))
				: Token(leading: TriviaList(Space), SyntaxKind.CloseBraceToken, TriviaList(CarriageReturn, LineFeed));

			bqlFieldNode = bqlFieldNode.WithCloseBraceToken(clostBracketToken);

			if (property != null)
				bqlFieldNode = CopyRegionsFromProperty(bqlFieldNode, property);

			return bqlFieldNode;
		}

		public static SimpleBaseTypeSyntax? BaseTypeForBqlField(string propertyTypeName, string bqlFieldName)
		{
			if (propertyTypeName.IsNullOrWhiteSpace() || bqlFieldName.IsNullOrWhiteSpace())
				return null;

			var bqlTypeName = PropertyTypeToBqlFieldTypeMapping.GetBqlFieldType(propertyTypeName).NullIfWhiteSpace();

			if (bqlTypeName == null)
				return null;

			GenericNameSyntax fieldTypeNode =
				GenericName(Identifier("Field"))
					.WithTypeArgumentList(
						TypeArgumentList(
							SingletonSeparatedList<TypeSyntax>(IdentifierName(bqlFieldName)))
						.WithGreaterThanToken(
							Token(leading: TriviaList(), SyntaxKind.GreaterThanToken, TriviaList(Space))));

			var newBaseType =
				SimpleBaseType(
					QualifiedName(
						QualifiedName(
							QualifiedName(
								QualifiedName(
									IdentifierName("PX"),
									IdentifierName("Data")),
									IdentifierName("BQL")),
									IdentifierName(bqlTypeName)),
						fieldTypeNode));

			return newBaseType;
		}

		private static ClassDeclarationSyntax CopyRegionsFromProperty(ClassDeclarationSyntax bqlFieldNode, PropertyDeclarationSyntax property)
		{
			var leadingTrivia = property.GetLeadingTrivia();

			if (leadingTrivia.Count == 0)
				return bqlFieldNode;

			var regionTrivias = leadingTrivia.GetRegionDirectiveLinesFromTrivia();

			if (regionTrivias.Count == 0)
				return bqlFieldNode;

			var regionsTrivia = TriviaList(regionTrivias);
			var bqlFieldNodeLeadingTrivia = bqlFieldNode.GetLeadingTrivia();
			var newBqlFieldNodeTrivia = bqlFieldNodeLeadingTrivia.AddRange(regionsTrivia);

			return bqlFieldNode.WithLeadingTrivia(newBqlFieldNodeTrivia);
		}
	}
}
