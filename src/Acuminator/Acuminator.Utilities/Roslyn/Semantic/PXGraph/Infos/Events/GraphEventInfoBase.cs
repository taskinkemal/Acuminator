﻿using System.Diagnostics;
using Acuminator.Utilities.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	/// <summary>
	/// A common non-generic graph event info DTO base class.
	/// </summary>
	public abstract class GraphEventInfoBase : GraphNodeSymbolItem<MethodDeclarationSyntax, IMethodSymbol>
	{
		public EventHandlerSignatureType SignatureType { get; }

		public EventType EventType { get; }

		public string DacName { get; }

		protected GraphEventInfoBase(MethodDeclarationSyntax node, IMethodSymbol symbol, int declarationOrder,
									 EventHandlerSignatureType signatureType, EventType eventType) :
								base(node, symbol, declarationOrder)
		{
			SignatureType = signatureType;
			EventType = eventType;
			DacName = GetDacName();
		}

		private string GetDacName()
		{
			switch (SignatureType)
			{
				case EventHandlerSignatureType.Default:
					var underscoreIndex = Symbol.Name.IndexOf('_');
					return underscoreIndex > 0
						? Symbol.Name.Substring(0, underscoreIndex)
						: string.Empty;

				case EventHandlerSignatureType.Generic:
					return GetDacNameFromGenericEvent();

				case EventHandlerSignatureType.None:
				default:
					return string.Empty;
			}
		}

		private string GetDacNameFromGenericEvent()
		{
			if (Symbol.Parameters.IsDefaultOrEmpty ||
					   !(Symbol.Parameters[0]?.Type is INamedTypeSymbol firstParameter) ||
					   firstParameter.TypeArguments.IsDefaultOrEmpty)
			{
				return string.Empty;
			}

			return firstParameter.TypeArguments[0].IsDAC()
				? firstParameter.TypeArguments[0].Name
				: string.Empty;
		}
	}
}
