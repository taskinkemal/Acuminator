using System.Collections.Generic;

using Acuminator.Utilities.Common;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.Semantic
{
	/// <summary>
	/// Helper that checks if method is Acumatica override.
	/// </summary>
	public static class AcumaticaOverridesHelper
	{
		private enum MethodsCompatibility
		{
			NotCompatible,
			ParametersMatch,
			ParametersMatchWithDelegate
		}

		/// <summary>
		/// Special signature check between a derived method with the PXOverride attribute and the base method.
		/// </summary>
		/// <param name="pxOverrideMethod">The method from the derived class, with the PXOverride attribute</param>
		/// <param name="baseMethod">The method from the base</param>
		/// <returns></returns>
		public static bool IsPXOverrideOf(this IMethodSymbol pxOverrideMethod, IMethodSymbol baseMethod)
		{
			pxOverrideMethod.ThrowOnNull();

			var baseMethodParameters = baseMethod.Parameters;
			var pxOverrideMethodParameters = pxOverrideMethod.Parameters;
			var methodsCompatibility = GetMethodsCompatibility(baseMethodParameters.Length, pxOverrideMethodParameters.Length);

			if (methodsCompatibility == MethodsCompatibility.NotCompatible ||
				!baseMethod.CanBeOverridden() || !baseMethod.IsAccessibleOutsideOfAssembly())
			{
				return false;
			}

			if (methodsCompatibility == MethodsCompatibility.ParametersMatch)
				return pxOverrideMethod.SignatureEquals(baseMethod);
			else if (methodsCompatibility == MethodsCompatibility.ParametersMatchWithDelegate)
			{
				if (pxOverrideMethod.IsGenericMethod != baseMethod.IsGenericMethod || pxOverrideMethod.RefKind != baseMethod.RefKind ||
					!SymbolEqualityComparer.Default.Equals(pxOverrideMethod.ReturnType, baseMethod.ReturnType))
				{
					return false;
				}

				if (pxOverrideMethodParameters[pxOverrideMethodParameters.Length - 1].Type is not INamedTypeSymbol @delegate ||
					@delegate.TypeKind != TypeKind.Delegate)
				{
					return false;
				}

				return baseMethodParameters.EqualsParameterRange(pxOverrideMethodParameters, rangeStart: 0, rangeEnd: baseMethodParameters.Length) &&
					   baseMethod.SignatureEquals(@delegate.DelegateInvokeMethod);
			}

			return false;
		}

		private static MethodsCompatibility GetMethodsCompatibility(int baseMethodParametersCount, int pxOverrideMethodParametersCount)
		{
			return baseMethodParametersCount == pxOverrideMethodParametersCount
				? MethodsCompatibility.ParametersMatch
				: (baseMethodParametersCount + 1) == pxOverrideMethodParametersCount
					? MethodsCompatibility.ParametersMatchWithDelegate
					: MethodsCompatibility.NotCompatible;
		}

		/// <summary>
		/// Check if <paramref name="methodWithBaseDelegate"/> has a valid base delegate as a last parameter.<br/>
		/// The valid base delegate should have the same return type and parameters as the method, except the last one.
		/// </summary>
		/// <param name="methodWithBaseDelegate">The method with the base delegate last parameter.</param>
		/// <returns>
		/// True if <paramref name="methodWithBaseDelegate"/> has a valid base delegate parameter as a last parameter, false if not.
		/// </returns>
		public static bool HasValidBaseDelegateParameter(this IMethodSymbol methodWithBaseDelegate)
		{
			if (methodWithBaseDelegate.CheckIfNull().Parameters.IsDefaultOrEmpty)
				return false;

			var delegateParameter = methodWithBaseDelegate.Parameters[^1];

			if (delegateParameter.Type is not INamedTypeSymbol delegateType || delegateType.TypeKind != TypeKind.Delegate ||
				delegateType.DelegateInvokeMethod == null)
			{
				return false;
			}

			IMethodSymbol baseDelegateMethod = delegateType.DelegateInvokeMethod;

			if (baseDelegateMethod.Parameters.Length != (methodWithBaseDelegate.Parameters.Length - 1) ||
				baseDelegateMethod.IsGenericMethod != methodWithBaseDelegate.IsGenericMethod ||
				baseDelegateMethod.RefKind != methodWithBaseDelegate.RefKind ||
			   !baseDelegateMethod.ReturnType.Equals(methodWithBaseDelegate.ReturnType, SymbolEqualityComparer.Default))
			{
				return false;
			}

			if (methodWithBaseDelegate.IsGenericMethod)
				return methodWithBaseDelegate.TypeParameters.Length == baseDelegateMethod.TypeParameters.Length;		// TODO no constraints check on type parameters currently

			return baseDelegateMethod.Parameters.EqualsParameterRange(methodWithBaseDelegate.Parameters, 
																	  rangeStart: 0, rangeEnd: baseDelegateMethod.Parameters.Length);
		}
	}
}
