using System;
using System.Linq;

using Microsoft.CodeAnalysis;

using AttributeFlags = (bool IsDbInterceptorAttribute, bool IsProjection, bool IsPXCacheName, bool IsPXHidden, bool IsPXAccumulator);

namespace Acuminator.Utilities.Roslyn.Semantic.Attribute
{
	/// <summary>
	/// Information about an attribute of a DAC or a DAC extension.
	/// </summary>
	public class DacAttributeInfo : AttributeInfoBase
	{
		public override AttributePlacement Placement => AttributePlacement.Dac;

		/// <summary>
		/// An indicator of whether the attribute configures the default navigation.
		/// </summary>
		public bool IsDefaultNavigation { get; }

		/// <summary>
		/// An indicator of whether the attribute configures a projection DAC.
		/// </summary>
		public bool IsPXProjection { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXCacheName attribute.
		/// </summary>
		public bool IsPXCacheName { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXHidden attribute.
		/// </summary>
		public bool IsPXHidden { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXAccumulatorAttribute attribute.
		/// </summary>
		public bool IsPXAccumulatorAttribute { get; }

		/// <summary>
		/// An indicator of whether the attribute is a PXDBInterceptorAttribute attribute.
		/// </summary>
		public bool IsDbInterceptorAttribute { get; }

		public DacAttributeInfo(PXContext pxContext, AttributeData attributeData, int declarationOrder) : base(attributeData, declarationOrder)
		{
			if (AttributeType != null)
			{
				IsDefaultNavigation = AttributeType.IsDefaultNavigation(pxContext);
				(IsDbInterceptorAttribute, IsPXProjection, IsPXCacheName, IsPXHidden, IsPXAccumulatorAttribute) = 
					GetAttributeFlags(AttributeType, pxContext);
			}
		}

		private static AttributeFlags GetAttributeFlags(INamedTypeSymbol attributeType, PXContext pxContext)
		{
			var dbInterceptorAttribute = pxContext.AttributeTypes.PXDBInterceptorAttribute;
			var projectionAttribute	   = pxContext.AttributeTypes.PXProjectionAttribute;
			var pxCacheNameAttribute   = pxContext.AttributeTypes.PXCacheNameAttribute;
			var pxHiddenAttribute 	   = pxContext.AttributeTypes.PXHiddenAttribute;
			var pxAccumulatorAttribute = pxContext.AttributeTypes.PXAccumulatorAttribute;

			// This is a hot path optimization to not get base attributes if possible by manually performing the first iteration
			// PXProjection and PXAccumulator are already DB interceptors, and PXDBInterceptorAttribute is an abstract attribute.
			// So, most likely, it won't be declared directly.
			bool isPXProjection  		  = attributeType.Equals(projectionAttribute, SymbolEqualityComparer.Default);
			bool isPXAccumulator 		  = !isPXProjection && attributeType.Equals(pxAccumulatorAttribute, SymbolEqualityComparer.Default);
			bool isDbInterceptorAttribute = isPXProjection || isPXAccumulator || 
											attributeType.Equals(dbInterceptorAttribute, SymbolEqualityComparer.Default);

			bool isPXCacheName = !isDbInterceptorAttribute && attributeType.Equals(pxCacheNameAttribute, SymbolEqualityComparer.Default);
			bool isPXHidden	   = !isDbInterceptorAttribute && attributeType.Equals(pxHiddenAttribute, SymbolEqualityComparer.Default);

			if (isDbInterceptorAttribute || isPXCacheName || isPXHidden)
				return (isDbInterceptorAttribute, isPXProjection, isPXCacheName, isPXHidden, isPXAccumulator);

			// Get base attributes and check them
			var attributeBaseTypes = attributeType.GetBaseTypes();

			foreach (var baseType in attributeBaseTypes)
			{
				if (baseType.SpecialType == SpecialType.System_Object)
					break;

				isPXProjection 	= isPXProjection  || baseType.Equals(projectionAttribute, SymbolEqualityComparer.Default);
				isPXAccumulator = isPXAccumulator || (!isPXProjection && baseType.Equals(pxAccumulatorAttribute, SymbolEqualityComparer.Default));
				isDbInterceptorAttribute = isDbInterceptorAttribute || isPXProjection || isPXAccumulator || 
										   baseType.Equals(dbInterceptorAttribute, SymbolEqualityComparer.Default);
				
				if (isDbInterceptorAttribute)
				{
					isPXCacheName = false;
					isPXHidden	  = false;
				}
				else
				{
					isPXCacheName = isPXCacheName || baseType.Equals(pxCacheNameAttribute, SymbolEqualityComparer.Default);
					isPXHidden	  = isPXHidden || baseType.Equals(pxHiddenAttribute, SymbolEqualityComparer.Default);
				}

				if (isDbInterceptorAttribute || isPXCacheName || isPXHidden)
					return (isDbInterceptorAttribute, isPXProjection, isPXCacheName, isPXHidden, isPXAccumulator);
			}

			return (isDbInterceptorAttribute, isPXProjection, isPXCacheName, isPXHidden, isPXAccumulator);
		}
	}
}