﻿using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace PX.Analyzers.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NonNullableTypeForBqlFieldAnalyzer : PXDiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(Descriptors.PX1014_NonNullableTypeForBqlField);

		internal override void AnalyzeCompilation(CompilationStartAnalysisContext compilationStartContext, PXContext pxContext)
		{
			compilationStartContext.RegisterSymbolAction(c => AnalyzeProperty(c, pxContext), SymbolKind.Property);
		}

		private static void AnalyzeProperty(SymbolAnalysisContext context, PXContext pxContext)
		{
			var property = (IPropertySymbol) context.Symbol;
			var parent = property.ContainingType;
			if (parent != null 
				&& (parent.ImplementsInterface(pxContext.IBqlTableType) || parent.InheritsFrom(pxContext.PXCacheExtensionType)))
			{
				var bqlField = parent.GetTypeMembers().FirstOrDefault(t => t.ImplementsInterface(pxContext.IBqlFieldType)
					&& String.Equals(t.Name, property.Name, StringComparison.OrdinalIgnoreCase));
				if (bqlField != null 
					&& property.Type.IsValueType && property.Type.SpecialType != SpecialType.System_Nullable_T)
				{
					context.ReportDiagnostic(Diagnostic.Create(Descriptors.PX1014_NonNullableTypeForBqlField, property.Locations.First()));
				}
			}
		}
	}
}