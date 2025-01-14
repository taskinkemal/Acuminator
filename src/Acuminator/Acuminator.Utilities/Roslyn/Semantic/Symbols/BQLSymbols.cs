﻿#nullable enable

using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using Acuminator.Utilities.Roslyn.Constants;
using System.Collections.Generic;

namespace Acuminator.Utilities.Roslyn.Semantic.Symbols
{
	/// <summary>
	/// BQL Symbols are stored in separate file.
	/// </summary>
	public class BQLSymbols : SymbolsSetBase
	{
		#region PXSetup
		public ImmutableArray<INamedTypeSymbol> PXSetupTypes { get; }

		public INamedTypeSymbol PXSetup => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetup1)!;

		public INamedTypeSymbol PXSetupWhere => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetup2)!;

		public INamedTypeSymbol PXSetupJoin => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetup3)!;

		public INamedTypeSymbol PXSetupSelect => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetupSelect)!;

		public INamedTypeSymbol? PXSetupOptional => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetupOptional1);

		public INamedTypeSymbol? PXSetupOptionalWhere => Compilation.GetTypeByMetadataName(TypeFullNames.PXSetupOptional2);
		#endregion

		#region CustomDelegates
		public INamedTypeSymbol CustomPredicate => Compilation.GetTypeByMetadataName(TypeFullNames.CustomPredicate)!;

		public INamedTypeSymbol AreSame => Compilation.GetTypeByMetadataName(TypeFullNames.AreSame2)!;

		public INamedTypeSymbol AreDistinct => Compilation.GetTypeByMetadataName(TypeFullNames.AreDistinct2)!;
		#endregion

		public INamedTypeSymbol Required => Compilation.GetTypeByMetadataName(TypeFullNames.Required1)!;

		public INamedTypeSymbol Argument => Compilation.GetTypeByMetadataName(TypeFullNames.Argument1)!;

		public INamedTypeSymbol Optional => Compilation.GetTypeByMetadataName(TypeFullNames.Optional1)!;
		public INamedTypeSymbol Optional2 => Compilation.GetTypeByMetadataName(TypeFullNames.Optional2)!;

		public INamedTypeSymbol BqlCommand => Compilation.GetTypeByMetadataName(TypeFullNames.BqlCommand)!;

		public INamedTypeSymbol IBqlParameter => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlParameter)!;

		public INamedTypeSymbol BqlParameter => Compilation.GetTypeByMetadataName(TypeFullNames.BqlParameter)!;

		public INamedTypeSymbol PXSelectBaseGenericType => Compilation.GetTypeByMetadataName(TypeFullNames.PXSelectBase1)!;

		public INamedTypeSymbol PXFilter => Compilation.GetTypeByMetadataName(TypeFullNames.PXFilter1)!;

		public INamedTypeSymbol IPXNonUpdateable => Compilation.GetTypeByMetadataName(TypeFullNames.IPXNonUpdateable)!;

		public INamedTypeSymbol FbqlCommand => Compilation.GetTypeByMetadataName(TypeFullNames.FbqlCommand)!;

		public INamedTypeSymbol? PXViewOf => Compilation.GetTypeByMetadataName(TypeFullNames.PXViewOf);

		public INamedTypeSymbol? PXViewOf_BasedOn => Compilation.GetTypeByMetadataName(TypeFullNames.PXViewOfBasedOn);

		public INamedTypeSymbol IBqlSearch => Compilation.GetTypeByMetadataName(TypeFullNames.IBqlSearch)!;

		internal BQLSymbols(Compilation compilation) : base(compilation)
		{
			PXSetupTypes = GetSetupSymbols();
		}

		private ImmutableArray<INamedTypeSymbol> GetSetupSymbols()
		{
			var setupTypes = ImmutableArray.CreateBuilder<INamedTypeSymbol>(initialCapacity: 6);

			setupTypes.Add(PXSetup);
			setupTypes.Add(PXSetupWhere);
			setupTypes.Add(PXSetupJoin);
			setupTypes.Add(PXSetupSelect);

			if (PXSetupOptional is { } pxSetupOptional)
				setupTypes.Add(pxSetupOptional);

			if (PXSetupOptionalWhere is { } pxSetupOptionalWhere)
				setupTypes.Add(pxSetupOptionalWhere);

			return setupTypes.ToImmutable();
		}
	}
}
