using System;
using System.Linq;
using System.Runtime.CompilerServices;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.Semantic.Dac;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic.PXGraph
{
	public static class GraphSymbolUtils
	{
		/// <summary>
		/// Gets the graph type from graph extension type.
		/// </summary>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The graph from graph extension.
		/// </returns>
		public static ITypeSymbol? GetGraphFromGraphExtension(this ITypeSymbol? graphExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			if (graphExtension == null || !graphExtension.InheritsFrom(pxContext.PXGraphExtension.Type))
				return null;

			var baseGraphExtensionType = graphExtension.GetBaseTypesAndThis()
													   .OfType<INamedTypeSymbol>()
													   .FirstOrDefault(type => type.IsGraphExtensionBaseType());
			if (baseGraphExtensionType == null)
				return null;

			var graphExtTypeArgs = baseGraphExtensionType.TypeArguments;

			if (graphExtTypeArgs.Length == 0)
				return null;

			ITypeSymbol firstTypeArg = graphExtTypeArgs.Last();
			return firstTypeArg.IsPXGraph(pxContext)
				? firstTypeArg
				: null;
		}

		public static bool IsDelegateForViewInPXGraph(this IMethodSymbol method, PXContext pxContext)
		{
			if (method == null || method.ReturnType.SpecialType != SpecialType.System_Collections_IEnumerable)
				return false;

			INamedTypeSymbol containingType = method.ContainingType;

			if (containingType == null || !containingType.IsPXGraphOrExtension(pxContext))
				return false;

			return containingType.GetMembers()
								 .OfType<IFieldSymbol>()
								 .Where(field => field.Type.InheritsFrom(pxContext.PXSelectBase.Type))
								 .Any(field => string.Equals(field.Name, method.Name, StringComparison.OrdinalIgnoreCase));
		}

		public static bool IsValidActionHandler(this IMethodSymbol method, PXContext pxContext)
		{
			method.ThrowOnNull();
			pxContext.ThrowOnNull();

			if (method.Parameters.Length == 0)
				return method.ReturnsVoid;
			else
			{
				return method.Parameters[0].Type.InheritsFromOrEquals(pxContext.PXAdapterType) &&
					   method.ReturnType.InheritsFromOrEquals(pxContext.SystemTypes.IEnumerable, includeInterfaces: true);
			}
		}

		public static bool IsValidViewDelegate(this IMethodSymbol method, PXContext pxContext)
		{
			method.ThrowOnNull();
			pxContext.ThrowOnNull();

			if (!method.ReturnType.Equals(pxContext.SystemTypes.IEnumerable, SymbolEqualityComparer.Default))
				return false;

			var parameters = method.Parameters;

			// Check that all view delegate parameters are either regular or passed by ref with a ref keyword (the second, very rare case of view delegates)
			return parameters.All(p => p.RefKind == RefKind.None) || parameters.All(p => p.RefKind == RefKind.Ref);
		}

		public static bool IsValidInitializeMethod(this IMethodSymbol method) =>
			method.CheckIfNull().ReturnsVoid && !method.IsStatic && method.Parameters.IsDefaultOrEmpty;

		/// <summary>
		/// Get declared primary DAC from graph or graph extension.
		/// </summary>
		/// <param name="graphOrExtension">The graph or graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// The declared primary DAC from graph or graph extension.
		/// </returns>
		public static ITypeSymbol? GetDeclaredPrimaryDacFromGraphOrGraphExtension(this ITypeSymbol? graphOrExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			if (graphOrExtension == null)
				return null;

			bool isGraph = graphOrExtension.InheritsFrom(pxContext.PXGraph.Type);

			if (!isGraph && !graphOrExtension.InheritsFrom(pxContext.PXGraphExtension.Type))
				return null;

			ITypeSymbol? graph = isGraph
				? graphOrExtension
				: graphOrExtension.GetGraphFromGraphExtension(pxContext);

			var baseGraphType = graph?.GetBaseTypesAndThis()
									  .OfType<INamedTypeSymbol>()
									  .FirstOrDefault(type => IsGraphWithPrimaryDacBaseGenericType(type));

			if (baseGraphType == null || baseGraphType.TypeArguments.Length < 2)
				return null;

			ITypeSymbol primaryDacType = baseGraphType.TypeArguments[1];
			return primaryDacType.IsDAC() ? primaryDacType : null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static bool IsGraphWithPrimaryDacBaseGenericType(INamedTypeSymbol type) =>
			type.TypeArguments.Length >= 2 && type.Name == TypeNames.PXGraph;

		internal static IMethodSymbol? GetConfigureMethodFromBaseGraphOrGraphExtension(this INamedTypeSymbol pxGraphOrPXGraphExtension, PXContext pxContext)
		{
			var pxScreenConfiguration = pxContext?.PXScreenConfiguration;

			if (pxScreenConfiguration == null)
				return null;

			var configureMethods = pxGraphOrPXGraphExtension!.GetMethods(DelegateNames.Workflow.Configure);
			return configureMethods.FirstOrDefault(method => method.ReturnsVoid && method.IsVirtual && method.DeclaredAccessibility == Accessibility.Public &&
															 method.Parameters.Length == 1 && pxScreenConfiguration.Equals(method.Parameters[0].Type,
																														   SymbolEqualityComparer.Default));
		}

		/// <summary>
		/// Check whether <paramref name="graphExtension"/> is a terminal graph extension.
		/// </summary>
		/// <remarks>
		/// A <b>terminal graph extension</b> is a graph extension that will be instantiated by Acumatica Framework at runtime during the initialization of the corresponding graph.<br/>
		///<br/>
		/// Currently, there are two types of terminal extensions:
		/// <list type="number">
		/// <item>
		/// <b>Non-abstract and non-generic graph extension</b>: A concrete graph extension without generic type parameters.
		/// </item>
		/// <item>
		/// <b>Abstract graph extension with <c>PXProtectedAccessAttribute</c></b>: An abstract graph extension decorated with the <c>PXProtectedAccessAttribute</c> attribute.
		/// </item>
		/// </list>
		/// </remarks>
		/// <param name="graphExtension">The graph extension to act on.</param>
		/// <param name="pxContext">Context.</param>
		/// <returns>
		/// True if terminal graph extension, false if not.
		/// </returns>
		public static bool IsTerminalGraphExtension(this INamedTypeSymbol graphExtension, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			if (!graphExtension.CheckIfNull().TypeParameters.IsDefaultOrEmpty)
				return false;
			else if (!graphExtension.IsAbstract)
				return true;

			var pxProtectedAccessAttribute = pxContext.AttributeTypes.PXProtectedAccessAttribute;
			return pxProtectedAccessAttribute != null &&
				   graphExtension.HasAttribute(pxProtectedAccessAttribute, checkOverrides: false);
		}

		/// <summary>
		/// Returns true if the data view is a processing view
		/// </summary>
		/// <param name="view">The type symbol of a data view</param>
		/// <param name="pxContext">The context</param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsProcessingView(this ITypeSymbol view, PXContext pxContext)
		{
			pxContext.ThrowOnNull();

			return view.CheckIfNull().InheritsFromOrEqualsGeneric(pxContext.PXProcessingBase.Type);
		}
	}
}