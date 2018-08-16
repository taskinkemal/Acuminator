﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Acuminator.Analyzers;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities
{
	/// <summary>
	/// Information about the Acumatica field attributes.
	/// </summary>
	public class AttributeInformation
	{
		private readonly PXContext _context;


		public AttributeInformation(PXContext pxContext)
		{
			pxContext.ThrowOnNull(nameof(pxContext));

			_context = pxContext;
		}

		public bool ContainsBaseType(ITypeSymbol attributeSymbol, ITypeSymbol type)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));
			type.ThrowOnNull(nameof(type));

			List<ITypeSymbol> attributeTypeHierarchy = attributeSymbol.GetBaseTypesAndThis().ToList();
			if (attributeTypeHierarchy.Contains(type))
				return true;
			return false;
		}

		public bool AttributeDerivedFromClass(ITypeSymbol attributeSymbol, ITypeSymbol type, int depth = 10)
		{
			attributeSymbol.ThrowOnNull(nameof(attributeSymbol));
			type.ThrowOnNull(nameof(type));
			if (depth <= 0 || depth > 100)
				return false;
			if (ContainsBaseType(attributeSymbol, type))
				return true;

			//get symbols from PXContext
			//PX.Data.PXAggregateAttribute and PX.Data.PXDynamicAggregateAttribute
			var aggregateAttribute = _context.PXAggregateAttribute;
			var dynamicAggregateAttribute = _context.PXDynamicAggregateAttribute;

			if (ContainsBaseType(attributeSymbol, aggregateAttribute) || ContainsBaseType(attributeSymbol, dynamicAggregateAttribute))
			{
				var allAttributes = attributeSymbol.GetAllAttributesDefinedOnThisAndBaseTypes().ToList();
				foreach (var attribute in allAttributes)
				{
					//go in recursuion
					var result = AttributeDerivedFromClass(attribute, type, --depth);
					if (depth <= 0 || depth > 100)
						return false;
					if (result)
						return result;
				}
			}
			return false;
		}

		public bool IsBoundAttribute(ITypeSymbol attributeSymbol)
		{
			//Get this symbols from PXContext
			//PX.Data.PXDBFieldAttribute
			var dbFieldAttribute = _context.FieldAttributes.PXDBFieldAttribute;
			return AttributeDerivedFromClass(attributeSymbol, dbFieldAttribute);
		}


		// hm... Is it nesessary to realize this method? What type of arguments must be here? SyntaxNode, PropertyDeclaratioinSyntax or I...Symbol?
		// [attribute1]
		// [attribute2]
		// int SomeField;
		public bool IsBoundField()
		{
			throw new NotImplementedException();
		}

		public bool AreBoundAttributes(IEnumerable<ITypeSymbol> attributesSymbols)
		{
			foreach (var attribute in attributesSymbols)
			{
				if (IsBoundAttribute(attribute))
					return true;
			}
			return false;
		}

	}
}