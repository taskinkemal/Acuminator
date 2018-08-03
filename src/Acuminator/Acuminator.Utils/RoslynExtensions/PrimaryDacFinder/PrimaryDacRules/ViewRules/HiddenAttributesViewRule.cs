﻿using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Acuminator.Analyzers;


namespace Acuminator.Utilities.PrimaryDAC
{
	/// <summary>
	/// An excluding rule for view with PXHiddenAttribute or PXCopyPasteHiddenViewAttribute attributes.
	/// </summary>
	public class HiddenAttributesViewRule : ViewRuleBase
	{
		public sealed override bool IsAbsolute => false;

		public HiddenAttributesViewRule(double? weight = null) : base(weight)
		{
		}

		public override bool SatisfyRule(PrimaryDacFinder dacFinder, ISymbol view, INamedTypeSymbol viewType)
		{
			if (view == null || dacFinder == null || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			ImmutableArray<AttributeData> attributes = view.GetAttributes();

			if (attributes.Length == 0)
				return false;

			INamedTypeSymbol hiddenAttribute = dacFinder.PxContext.PXHiddenAttribute;
			bool hasHiddenAttribute = attributes.Any(a => a.AttributeClass.Equals(hiddenAttribute));

			if (hasHiddenAttribute)
				return true;
			else if (dacFinder.GraphViewSymbolsWithTypes.Length <= 1 || dacFinder.CancellationToken.IsCancellationRequested)
				return false;

			INamedTypeSymbol copyPasteHiddenViewAttribute = dacFinder.PxContext.PXCopyPasteHiddenViewAttribute;
			return attributes.Any(a => a.AttributeClass.InheritsFromOrEquals(copyPasteHiddenViewAttribute));
		}
	}
}