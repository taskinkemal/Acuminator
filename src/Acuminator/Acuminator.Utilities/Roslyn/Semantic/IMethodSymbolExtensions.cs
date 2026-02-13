using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	public static class IMethodSymbolExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInstanceConstructor(this IMethodSymbol methodSymbol)
		{
			methodSymbol.ThrowOnNull();

			return !methodSymbol.IsStatic && methodSymbol.MethodKind == MethodKind.Constructor;
		}

		/// <summary>
		/// Check if the <paramref name="methodSymbol"/> is a nested method (local function or lambda).
		/// </summary>
		/// <param name="methodSymbol">The method to act on.</param>
		/// <returns>
		/// True if method is local function or lambda, false if not.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsNestedMethod([NotNullWhen(returnValue: true)] this IMethodSymbol? methodSymbol) =>
			methodSymbol?.MethodKind is MethodKind.LocalFunction or MethodKind.LambdaMethod;

		/// <summary>
		/// Gets the topmost non-local method containing the local function declaration. In case of a non-local method returns itself.
		/// </summary>
		/// <param name="localFunction">The method that can be local function.</param>
		/// <returns>
		/// The non-local method containing the local function.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMethodSymbol? GetContainingNonLocalMethod(this IMethodSymbol localFunction) =>
			GetStaticOrNonLocalContainingMethod(localFunction, stopOnStaticMethod: false);

		/// <summary>
		/// Gets the topmost static or non-local method containing the <paramref name="localFunction"/>. In case of a non-local method returns itself.
		/// </summary>
		/// <param name="localFunction">The method that can be local function.</param>
		/// <returns>
		/// the topmost static or non-local method containing the <paramref name="localFunction"/>.
		/// </returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IMethodSymbol? GetStaticOrNonLocalContainingMethod(this IMethodSymbol localFunction) =>
			GetStaticOrNonLocalContainingMethod(localFunction, stopOnStaticMethod: true);

		private static IMethodSymbol? GetStaticOrNonLocalContainingMethod(IMethodSymbol localFunctionOrLambda, bool stopOnStaticMethod)
		{
			localFunctionOrLambda.ThrowOnNull();

			if (!localFunctionOrLambda.IsNestedMethod())
				return localFunctionOrLambda;

			IMethodSymbol? current = localFunctionOrLambda;

			while (current.IsNestedMethod() && (!stopOnStaticMethod || !localFunctionOrLambda.IsStatic))
				current = current.ContainingSymbol as IMethodSymbol;

			return current;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<IMethodSymbol> GetContainingMethodsAndThis(this IMethodSymbol localFunction) =>
			localFunction.CheckIfNull().IsNestedMethod()
				? localFunction.GetContainingMethods(includeThis: true)
				: [localFunction];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<IMethodSymbol> GetContainingMethods(this IMethodSymbol localFunction) =>
			localFunction.CheckIfNull().IsNestedMethod()
				? localFunction.GetContainingMethods(includeThis: false)
				: [];

		private static IEnumerable<IMethodSymbol> GetContainingMethods(this IMethodSymbol localFunction, bool includeThis)
		{
			IMethodSymbol? current = includeThis
				? localFunction
				: localFunction.ContainingSymbol as IMethodSymbol;

			while (current != null)
			{
				yield return current;
				current = current.ContainingSymbol as IMethodSymbol;
			}
		}

		/// <summary>
		/// Gets all parameters available for local function or lambda including parameters from containing methods.
		/// </summary>
		/// <param name="localFunctionOrLambda">The method that can be a local function or lambda.</param>
		/// <param name="includeOwnParameters">True to include, false to exclude <paramref name="localFunctionOrLambda"/>'s own parameters.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// All parameters available for the local function or lambda including parameters from containing methods.
		/// </returns>
		public static ImmutableArray<IParameterSymbol> GetAllParametersAvailableForLocalFunctionOrLambda(this IMethodSymbol localFunctionOrLambda, 
																										 bool includeOwnParameters, 
																										 CancellationToken cancellation)
		{
			if (!localFunctionOrLambda.CheckIfNull().IsNestedMethod())
				return localFunctionOrLambda.Parameters;

			ImmutableArray<IParameterSymbol>.Builder parametersBuilder;

			if (localFunctionOrLambda.Parameters.IsDefaultOrEmpty || !includeOwnParameters)
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>();
			else
			{
				parametersBuilder = ImmutableArray.CreateBuilder<IParameterSymbol>(initialCapacity: localFunctionOrLambda.Parameters.Length);
				parametersBuilder.AddRange(localFunctionOrLambda.Parameters);
			}

			if (localFunctionOrLambda.IsStatic)
				return parametersBuilder.ToImmutable();

			IMethodSymbol? current = localFunctionOrLambda;

			do
			{
				cancellation.ThrowIfCancellationRequested();

				var containingMethod = current!.ContainingSymbol as IMethodSymbol;

				// For a non static nested local function we can add parameters from its containing local function even if it is static
				// But we must stop after that and won't take parameters from the methods containing static local function
				if (containingMethod != null && !containingMethod.Parameters.IsDefaultOrEmpty)
				{
					// If we do not include parameters from the local function then check if the outer parameters are redefined by the local function parameters.
					// Redefined parameters won't be available to the local function
					var notReassignedParameters = from parameter in containingMethod.Parameters
												  where !parametersBuilder.Contains(parameter) &&
														(includeOwnParameters || 
														 !localFunctionOrLambda.Parameters.Any(localParameter => localParameter.Name == parameter.Name))
												  select parameter;
					parametersBuilder.AddRange(notReassignedParameters);
				}

				current = containingMethod;
			}
			while (current.IsNestedMethod() && !current.IsStatic);

			return parametersBuilder.ToImmutable();
		}

		/// <summary>
		/// Check if  the parameter <paramref name="parameterName"/> from the non local method is redefined.
		/// </summary>
		/// <param name="localMethodOrLambda">The method that can be local function or lambda.</param>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>
		/// True if non local method parameter is redefined in a local method or lambda, false if not.
		/// </returns>
		public static bool IsNonLocalMethodParameterRedefined(this IMethodSymbol localMethodOrLambda, string parameterName)
		{
			localMethodOrLambda.ThrowOnNull();

			if (parameterName.IsNullOrWhiteSpace() || !localMethodOrLambda.IsNestedMethod())
				return false;

			IMethodSymbol? current = localMethodOrLambda;

			while (current.IsNestedMethod())
			{
				if ((!current.Parameters.IsDefaultOrEmpty && current.Parameters.Any(p => p.Name == parameterName)) ||
					current.IsStatic)
				{
					return true;
				}

				current = current.ContainingSymbol as IMethodSymbol;
			}

			return false;
		}

		/// <summary>
		/// Check if <paramref name="method"/> has either virtual, override or abstract signature.
		/// </summary>
		/// <param name="method">The method to act on.</param>
		/// <returns>True if <paramref name="method"/> </returns>
		public static bool CanBeOverridden(this IMethodSymbol method) =>
			method.CheckIfNull().IsVirtual || method.IsOverride || method.IsAbstract;

		/// <summary>
		/// Check if <paramref name="method"/> signature equals the signature of <paramref name="methodToCheck"/>.
		/// </summary>
		/// <param name="method">The method to act on.</param>
		/// <param name="methodToCheck">The method with signature to check.</param>
		/// <returns>
		/// True if signatures are equal.
		/// </returns>
		/// <remarks>
		/// This method does not check constraints on type parameters.
		/// </remarks>
		public static bool SignatureEquals(this IMethodSymbol method, [NotNullWhen(returnValue: true)] IMethodSymbol? methodToCheck)
		{
			if (!method.AreParametersEqual(methodToCheck) || method.IsGenericMethod != methodToCheck.IsGenericMethod ||
				 method.RefKind != methodToCheck.RefKind || !SymbolEqualityComparer.Default.Equals(method.ReturnType, methodToCheck.ReturnType))
			{
				return false;
			}

			if (method.IsGenericMethod)
				return method.TypeParameters.Length == methodToCheck.TypeParameters.Length;		// TODO no constraints check on type parameters currently

			return true;
		}

		/// <summary>
		/// Check if <paramref name="method"/> parameters are equal to the parameters of <paramref name="methodToCheck"/>.
		/// </summary>
		/// <param name="method">The method to act on.</param>
		/// <param name="methodToCheck">The method with parameters to check.</param>
		/// <returns>
		/// True if parameters are equal.
		/// </returns>
		public static bool AreParametersEqual(this IMethodSymbol method, [NotNullWhen(returnValue: true)] IMethodSymbol? methodToCheck)
		{
			method.ThrowOnNull();

			if (methodToCheck == null || method.Parameters.Length != methodToCheck.Parameters.Length)
				return false;

			return method.Parameters.EqualsParameterRange(methodToCheck.Parameters, rangeStart: 0, rangeEnd: method.Parameters.Length);
		}

		/// <summary>
		/// Check if parameters in the range from <paramref name="rangeStart"/> to <paramref name="rangeEnd"/> are equal.<br/>
		/// If one of the lists does not contain the entire range, then an exception will be thrown.
		/// </summary>
		/// <param name="sourceParameters">The source parameters to act on.</param>
		/// <param name="parametersToCheck">Parameters to check.</param>
		/// <param name="rangeStart">The range start. The range start is inclusive, all indexes greater than it will be taken.</param>
		/// <param name="rangeEnd">The range end. The range end is exclusive to the list of parameters, all indexes lower than it will be taken.</param>
		/// <returns>
		/// True if parameters in the specified range are equal, false if not.
		/// </returns>
		internal static bool EqualsParameterRange(this ImmutableArray<IParameterSymbol> sourceParameters, ImmutableArray<IParameterSymbol> parametersToCheck,
												  int rangeStart, int rangeEnd)
		{
			int minParamsCount = Math.Min(sourceParameters.Length, parametersToCheck.Length);

			if (rangeStart > rangeEnd || rangeStart < 0 || rangeEnd > minParamsCount)
				throw new ArgumentOutOfRangeException($"Invalid range - start: {rangeStart}, end: {rangeEnd}");
			else if (rangeStart == rangeEnd)
				return true;

			for (var i = rangeStart; i < rangeEnd; i++)
			{
				if (!AreParametersEqual(sourceParameters[i], parametersToCheck[i]))
					return false;
			}
			
			return true;

			//---------------------------------------Local Function-----------------------------------------------------------------
			static bool AreParametersEqual(IParameterSymbol paramX, IParameterSymbol paramY) =>
				paramX.RefKind == paramY.RefKind && paramX.IsOptional == paramY.IsOptional &&
				(paramX.Type.Equals(paramY.Type, SymbolEqualityComparer.Default) ||
				 paramX.OriginalDefinition.Type.Equals(paramY.OriginalDefinition.Type, SymbolEqualityComparer.Default));
		}

		/// <summary>
		/// Check if the <paramref name="method"/> has PXOverrideAttribute declared on the overrides chain.
		/// </summary>
		/// <param name="method">The method to act on.</param>
		/// <param name="pxContext">The context.</param>
		/// <returns>
		/// True the <paramref name="method"/> has PXOverrideAttribute declared on the overrides chain, false if not.
		/// </returns>
		public static bool HasPXOverrideAttribute(this IMethodSymbol method, PXContext pxContext) =>
			method.CheckIfNull().HasPXOverrideAttribute(pxContext.CheckIfNull().AttributeTypes.PXOverrideAttribute);

		/// <summary>
		/// Check if the <paramref name="method"/> has PXOverrideAttribute declared on the overrides chain.
		/// </summary>
		/// <param name="method">The method to act on.</param>
		/// <param name="pxContext">The context.</param>
		/// <returns>
		/// True the <paramref name="method"/> has PXOverrideAttribute declared on the overrides chain, false if not.
		/// </returns>
		internal static bool HasPXOverrideAttribute(this IMethodSymbol method, INamedTypeSymbol pxOverrideAttribute) =>
			method.HasAttribute(pxOverrideAttribute, checkOverrides: true, checkForDerivedAttributes: false);
	}
}
