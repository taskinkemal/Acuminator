﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Acuminator.Tests.Helpers;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using static Acuminator.Tests.Verification.VerificationHelper;

namespace Acuminator.Tests.Tests.Utilities.AttributeInformation
{
	
	public class AttributeInformationTests : Verification.DiagnosticVerifier
	{
		/* 
		 *  Tests attribute derived 
		 * */
		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		public async void TestAttributeSimpleInformation(string source) =>
			await TestAttributeInformationAsync(source, new List<bool> { false, true, false, false, true, false });

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public async void TestAggregateAttributeAsync(string source) =>
			await TestAttributeInformationAsync(source, new List<bool> { true, true });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public async void TestAggregateRegursiveAttributeAsync(string source) =>
			await TestAttributeInformationAsync(source, new List<bool> { true, false });
		
		private async Task TestAttributeInformationAsync(string source, List<bool> expected)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = await document.GetSemanticModelAsync();
			var syntaxRoot = document.GetSyntaxRootAsync().Result;

			List<bool> actual = new List<bool>();
			var pxContext = new PXContext(semanticModel.Compilation);

			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();
				foreach (var attribute in attributes)
				{
					var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);
					var defaultAttribute = pxContext.AttributeTypes.PXDefaultAttribute;
					actual.Add(attributeInformation.AttributeDerivedFromClass(attribute.AttributeClass, defaultAttribute));
				}
			}
			Assert.Equal(expected, actual);
		}

		/*
		 * Tests IsBoundAttribute 
		 */

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		public void TestAreBoundAttributes(string source) =>
			_testIsBoundAttribute(source, new List<bool> { false, false, false ,true, false, false });

		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		public void TestAreBoundAggregateAttributes(string source) =>
			_testIsBoundAttribute(source, new List<bool> { true, false });

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		public void TestAreBoundAggregateRecursiveAttribute(string source) =>
			_testIsBoundAttribute(source, new List<bool> { false, true });

		private void _testIsBoundAttribute(string source, List<bool> expected)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			var syntaxRoot = document.GetSyntaxRootAsync().Result;

			List<bool> actual = new List<bool>();
			var pxContext = new PXContext(semanticModel.Compilation);

			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();

			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();
				foreach (var attribute in attributes)
				{
					var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);
					actual.Add(attributeInformation.IsBoundAttribute(attribute.AttributeClass));
				}
			}
			Assert.Equal(expected, actual);
		}

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private void TestListOfParentsSimple(string source)
		{
			_testListOfParents(source, 
								new List<List<string>> {
									new List<string>{ "PX.Data.PXBoolAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute" },
									new List<string>{ "PX.Data.PXUIFieldAttribute" },
									new List<string>{ "PX.Data.PXDBCalcedAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute"},
									new List<string>{ "PX.Data.PXUIFieldAttribute"}
								});
		}


		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private void TestListOfParentsAggregate(string source)
		{
			_testListOfParents(source,
								new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo.PXAccountAttribute",
										"PX.Objects.HackathonDemo.PXCustomDefaultAttribute"
									}
								});

		}

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		private void TestListOfParentsAggregateRecursive(string source)
		{
			_testListOfParents(source, new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo._NonNullableIntListAttribute",
										"PX.Objects.HackathonDemo._NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXIntListAttribute"
									}
								});
		}

		[Theory]
		[EmbeddedFileData(@"AttributeInformationSimpleDac.cs")]
		private void TestListOfParentsSimpleExpanded(string source)
		{
			_testListOfParents(source,
								new List<List<string>> {
									new List<string>{ "PX.Data.PXBoolAttribute" },
									new List<string>{ "PX.Data.PXDefaultAttribute" },
									new List<string>{ "PX.Data.PXUIFieldAttribute" },
									new List<string>{ "PX.Data.PXDBCalcedAttribute"},
									new List<string>{ "PX.Data.PXDefaultAttribute"},
									new List<string>{ "PX.Data.PXUIFieldAttribute"}
								},
								true);
		}


		[Theory]
		[EmbeddedFileData(@"AggregateAttributeInformation.cs")]
		private void TestListOfParentsAggregateExpanded(string source)
		{
			_testListOfParents(source,
								new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXDBFieldAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo.PXAccountAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo.PXCustomDefaultAttribute",
										"PX.Data.PXDefaultAttribute"
									}
								},
								true);

		}

		[Theory]
		[EmbeddedFileData(@"AggregateRecursiveAttributeInformation.cs")]
		private void TestListOfParentsAggregateRecursiveExpanded(string source)
		{
			_testListOfParents(source, new List<List<string>> {
									new List<string>
									{
										"PX.Objects.HackathonDemo.NonNullableIntListAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo.NonNullableIntAttribute",
										"PX.Data.PXDefaultAttribute",
										"PX.Data.PXIntListAttribute"
									},
									new List<string>
									{
										"PX.Objects.HackathonDemo._NonNullableIntListAttribute",
										"PX.Data.PXAggregateAttribute",
										"PX.Objects.HackathonDemo._NonNullableIntAttribute",
										"PX.Data.PXDBIntAttribute",
										"PX.Data.PXDBFieldAttribute",
										"PX.Data.PXIntListAttribute"
									}
								},
								true);
		}


		private void _testListOfParents(string source, List<List<string>> expected, bool expand = false)
		{
			Document document = CreateDocument(source);
			SemanticModel semanticModel = document.GetSemanticModelAsync().Result;
			var syntaxRoot = document.GetSyntaxRootAsync().Result;

			var pxContext = new PXContext(semanticModel.Compilation);

			var expectedSymbols = _convertStringsToITypeSymbols(expected, semanticModel);
		
			var properties = syntaxRoot.DescendantNodes().OfType<PropertyDeclarationSyntax>();
			
			List<HashSet<ITypeSymbol>> result = new List<HashSet<ITypeSymbol>>();
			
			foreach (var property in properties)
			{
				var typeSymbol = semanticModel.GetDeclaredSymbol(property);
				var attributes = typeSymbol.GetAttributes();

				foreach (var attribute in attributes)
				{
					var attributeInformation = new Acuminator.Utilities.Roslyn.PXFieldAttributes.AttributeInformation(pxContext);
					result.Add(attributeInformation.AttributesListDerivedFromClass(attribute.AttributeClass,expand).ToHashSet());
				}
			}
			Assert.Equal(expectedSymbols, result);
		}

		private List<HashSet<ITypeSymbol>> _convertStringsToITypeSymbols(List<List<string>> expected,SemanticModel semanticModel)
		{
			var expectedSymbols = new List<HashSet<ITypeSymbol>>();
			foreach(var symbolsArray in expected)
			{
				HashSet<ITypeSymbol> attributesHashSet = new HashSet<ITypeSymbol>();
				foreach (var symbol in symbolsArray)
				{
					attributesHashSet.Add(semanticModel.Compilation.GetTypeByMetadataName(symbol));
				}
				expectedSymbols.Add(attributesHashSet);
			}
			return expectedSymbols;
		}

	}
}