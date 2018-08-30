﻿using System;
using System.Collections.Generic;
using System.Linq;
using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.Base;
using Acuminator.Utilities.Roslyn.Semantic;
using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PrimaryDacFinder.PrimaryDacRules.GraphRules
{
	/// <summary>
	/// A rule to add scores to the pair of views with special names in graph.
	/// </summary>
	public class PairOfViewsWithSpecialNamesGraphRule : GraphRuleBase
	{
		public sealed override bool IsAbsolute => false;

		private readonly string firstName, secondName;

		public PairOfViewsWithSpecialNamesGraphRule(string aFirstName, string aSecondName, double? customWeight = null) : base(customWeight)
		{
			if (aFirstName.IsNullOrWhiteSpace())
				throw new ArgumentNullException(nameof(aFirstName));
			else if (aSecondName.IsNullOrWhiteSpace())
				throw new ArgumentNullException(nameof(aSecondName));

			firstName = aFirstName;
			secondName = aSecondName;
		}

		public override IEnumerable<ITypeSymbol> GetCandidatesFromGraphRule(PrimaryDacFinder dacFinder)
		{
			if (dacFinder?.Graph == null || dacFinder.CancellationToken.IsCancellationRequested || dacFinder.GraphViewSymbolsWithTypes.Length == 0)
				return Enumerable.Empty<ITypeSymbol>();

			bool firstNameFound = false, secondNameFound = false;
			ITypeSymbol firstDacCandidate = null, secondDacCandidate = null;

			foreach (var (view, viewType) in dacFinder.GraphViewSymbolsWithTypes)
			{
				if (dacFinder.CancellationToken.IsCancellationRequested)
					return Enumerable.Empty<ITypeSymbol>();

				if (view.Name == firstName)
				{
					firstNameFound = true;
					firstDacCandidate = viewType.GetDacFromView(dacFinder.PxContext);
					continue;
				}

				if (view.Name == secondName)
				{
					secondNameFound = true;
					secondDacCandidate = viewType.GetDacFromView(dacFinder.PxContext);
					continue;
				}

				if (firstNameFound && secondNameFound)
					break;
			}

			var dacCandidate = ChooseDacCandidate(firstDacCandidate, secondDacCandidate);
			return dacCandidate?.ToEnumerable() ?? Enumerable.Empty<ITypeSymbol>();
		}

		private static ITypeSymbol ChooseDacCandidate(ITypeSymbol firstDacCandidate, ITypeSymbol secondDacCandidate)
		{
			if (firstDacCandidate == null && secondDacCandidate == null)
				return null;
			else if (firstDacCandidate != null && secondDacCandidate != null)
			{
				return firstDacCandidate.Equals(secondDacCandidate)
					? firstDacCandidate
					: null;
			}
			else
			{
				return firstDacCandidate != null
					? firstDacCandidate
					: secondDacCandidate;
			}
		}
	}
}