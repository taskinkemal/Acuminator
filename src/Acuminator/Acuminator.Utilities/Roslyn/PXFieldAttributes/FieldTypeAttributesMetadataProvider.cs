using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Semantic;

using Microsoft.CodeAnalysis;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes
{
	/// <summary>
	/// Provider of information about the Acumatica DAC field type attributes metadata.
	/// </summary>
	public class FieldTypeAttributesMetadataProvider
	{
		private readonly PXContext _pxContext;

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol?> UnboundDacFieldTypeAttributesWithFieldType { get; }

		public ImmutableDictionary<ITypeSymbol, ITypeSymbol?> BoundDacFieldTypeAttributesWithFieldType { get; }

		public ImmutableArray<INamedTypeSymbol> AttributesCalcedOnDbSide { get; }

		public ImmutableHashSet<ITypeSymbol> AllDacFieldTypeAttributes { get; }

		public ImmutableArray<MixedDbBoundnessAttributeInfo> SortedDacFieldTypeAttributesWithMixedDbBoundness { get; }

		public ImmutableArray<ITypeSymbol> WellKnownNonDataTypeAttributes { get; }

		public ImmutableDictionary<ITypeSymbol, DataTypeAttributeInfo> MetadataForWellKnownSpecialDataTypeAttributes { get; }

		private readonly INamedTypeSymbol _pxDBCalcedAttribute;
		private readonly INamedTypeSymbol _pxDBScalarAttribute;
		private readonly INamedTypeSymbol _pxDBFieldAttribute;

		public FieldTypeAttributesMetadataProvider(PXContext pxContext)
		{
			_pxContext = pxContext.CheckIfNull();
			_pxDBScalarAttribute = _pxContext.FieldAttributes.PXDBScalarAttribute;
			_pxDBCalcedAttribute = _pxContext.FieldAttributes.PXDBCalcedAttribute;
			_pxDBFieldAttribute = _pxContext.FieldAttributes.PXDBFieldAttribute;

			var attributesCalcedOnDbSide = new List<INamedTypeSymbol>(capacity: 2) { _pxDBScalarAttribute, _pxDBCalcedAttribute };
			AttributesCalcedOnDbSide = attributesCalcedOnDbSide.ToImmutableArray();

			UnboundDacFieldTypeAttributesWithFieldType = GetUnboundDacFieldTypeAttributesWithCorrespondingTypes(_pxContext)
															.ToImmutableDictionary(SymbolEqualityComparer.Default);
			BoundDacFieldTypeAttributesWithFieldType = GetBoundDacFieldTypeAttributesWithCorrespondingTypes(_pxContext)
															.ToImmutableDictionary(SymbolEqualityComparer.Default);

			AllDacFieldTypeAttributes = UnboundDacFieldTypeAttributesWithFieldType.Keys
										.Concat(BoundDacFieldTypeAttributesWithFieldType.Keys)
										.Concat(attributesCalcedOnDbSide)
										.ToImmutableHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

			SortedDacFieldTypeAttributesWithMixedDbBoundness = GetDacFieldTypeAttributesWithMixedDbBoundness(_pxContext)
																.OrderBy(typeWithValue => typeWithValue.AttributeType, TypeSymbolsByHierachyComparer.Instance)
																.ToImmutableArray();

			WellKnownNonDataTypeAttributes = GetWellKnownNonDataTypeAttributes(_pxContext);
			MetadataForWellKnownSpecialDataTypeAttributes = GetMetadataForWellKnownSpecialDataTypeAttributes(_pxContext);
		}

		public bool IsWellKnownNonDataTypeAttribute(ITypeSymbol attribute)
		{
			var attributeTypeHierarchy = attribute.GetBaseTypesAndThis();
			return attributeTypeHierarchy.Any(type => EnumerableExtensions.ImmutableArrayContains(WellKnownNonDataTypeAttributes, type, 
																								  SymbolEqualityComparer.Default));
		}

		public IReadOnlyCollection<DataTypeAttributeInfo> GetDacFieldTypeAttributeInfos(ITypeSymbol originalAttribute) =>
			GetDacFieldTypeAttributeInfos(originalAttribute, preparedFlattenedAttributes: null);

		internal IReadOnlyCollection<DataTypeAttributeInfo> GetDacFieldTypeAttributeInfos(ITypeSymbol? originalAttribute,
																						  ISet<ITypeSymbol>? preparedFlattenedAttributes)
		{
			if (originalAttribute == null)
				return [];

			if (IsWellKnownNonDataTypeAttribute(originalAttribute))
				return [];

			var flattenedAttributes = preparedFlattenedAttributes ?? originalAttribute.GetThisAndAllAggregatedAttributes(_pxContext, includeBaseTypes: true);
			return GetDacFieldTypeAttributeInfos_NoWellKnownNonDataTypeAttributesCheck(originalAttribute, flattenedAttributes);
		}

		internal IReadOnlyCollection<DataTypeAttributeInfo> GetDacFieldTypeAttributeInfos_NoWellKnownNonDataTypeAttributesCheck(ITypeSymbol originalAttribute,
																										ISet<ITypeSymbol> flattenedAttributes)
		{
			if (flattenedAttributes.Count == 0)
				return [];

			var metadataForWellKnownSpecialDataTypeAttribute = TryGetMetadataForWellKnownSpecialDataTypeAttribute(originalAttribute);
			
			if (metadataForWellKnownSpecialDataTypeAttribute != null)
				return [metadataForWellKnownSpecialDataTypeAttribute];

			var mixedDbBoundnessAttributeInfos = GetMixedDbBoundnessAttributeInfosInFlattenedSet(originalAttribute, flattenedAttributes);
			bool hasMixedBoundnessAttributes = mixedDbBoundnessAttributeInfos?.Count > 0;
			var typeAttributeInfos = hasMixedBoundnessAttributes
				? new List<DataTypeAttributeInfo>(mixedDbBoundnessAttributeInfos)
				: null;

			var pxDbFieldAttributeDirectlyUsedInfo = GetPXDbFieldAttributeInfoIfItIsUsedDirectly(originalAttribute);

			if (pxDbFieldAttributeDirectlyUsedInfo != null)
			{
				typeAttributeInfos ??= new List<DataTypeAttributeInfo>(capacity: 4);
				typeAttributeInfos.Add(pxDbFieldAttributeDirectlyUsedInfo);
			}

			var dacFieldTypeAttributesInFlattenedSet = AllDacFieldTypeAttributes.Intersect(flattenedAttributes);

			if (dacFieldTypeAttributesInFlattenedSet.Count == 0)
				return typeAttributeInfos as IReadOnlyCollection<DataTypeAttributeInfo> ?? [];

			foreach (INamedTypeSymbol dacFieldTypeAttribute in dacFieldTypeAttributesInFlattenedSet.OfType<INamedTypeSymbol>())
			{
				DataTypeAttributeInfo? attributeInfo = GetDacFieldTypeAttributeInfo(dacFieldTypeAttribute);

				if (attributeInfo == null || 
					(hasMixedBoundnessAttributes && 
					DacFieldTypeAttributeCapturedByMixedBoundnessAttributesTypeHierarchy(originalAttribute, dacFieldTypeAttribute, mixedDbBoundnessAttributeInfos!)))
				{
					continue;
				}
				
				typeAttributeInfos ??= new List<DataTypeAttributeInfo>(capacity: 4);
				typeAttributeInfos.Add(attributeInfo);
			}

			return typeAttributeInfos as IReadOnlyCollection<DataTypeAttributeInfo> ?? [];
		}

		private DataTypeAttributeInfo? TryGetMetadataForWellKnownSpecialDataTypeAttribute(ITypeSymbol attribute) 
		{
			if (MetadataForWellKnownSpecialDataTypeAttributes.Count == 0)
				return null;
			else if (MetadataForWellKnownSpecialDataTypeAttributes.TryGetValue(attribute, out var metadata))
				return metadata;

			var baseAttributeTypes = attribute.GetBaseTypes();

			foreach (ITypeSymbol baseAttributeType in baseAttributeTypes)
			{
				if (baseAttributeType.SpecialType == SpecialType.System_Object)
					return null;

				if (MetadataForWellKnownSpecialDataTypeAttributes.TryGetValue(baseAttributeType, out var metadata))
					return metadata;
			}

			return null;
		}

		/// <summary>
		/// Gets mixed database boundness attribute infos in the flattened set with consideration of type hierarchy.<br/>
		/// If there are multiple mixed boundness attributes in the type hierarchy then only the most derived one should be included into results <br/>
		/// unless base mixed attribute is aggregated directly.
		/// </summary>
		/// <param name="originalAttribute">The original attribute symbol.</param>
		/// <param name="flattenedAttributes">The flattened attributes.</param>
		/// <returns>
		/// The mixed database boundness attribute infos from the flattened set of attributes.
		/// </returns>
		private List<MixedDbBoundnessAttributeInfo>? GetMixedDbBoundnessAttributeInfosInFlattenedSet(ITypeSymbol originalAttribute,
																									 ISet<ITypeSymbol> flattenedAttributes)
		{
			List<MixedDbBoundnessAttributeInfo>? mixedDbBoundnessAttributeInfos = null;
			HashSet<ITypeSymbol>? checkedMixedAttributes = null;

			foreach (MixedDbBoundnessAttributeInfo mixedBoundnessAttribute in SortedDacFieldTypeAttributesWithMixedDbBoundness)
			{
				if (!flattenedAttributes.Contains(mixedBoundnessAttribute.AttributeType))
					continue;

				bool isPartOfAnotherMixedAttributeHierarchy = checkedMixedAttributes?.Contains(mixedBoundnessAttribute.AttributeType) == true;

				if (isPartOfAnotherMixedAttributeHierarchy)
				{
					if (!originalAttribute.EqualsOrAggregatesAttributeDirectly(mixedBoundnessAttribute.AttributeType, _pxContext))
						continue;
				}

				if (mixedDbBoundnessAttributeInfos == null)
				{
					mixedDbBoundnessAttributeInfos = new(capacity: 1) { mixedBoundnessAttribute };
					checkedMixedAttributes = mixedBoundnessAttribute.AcumaticaAttributesHierarchy
																	.ToHashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
				}
				else
				{
					mixedDbBoundnessAttributeInfos.Add(mixedBoundnessAttribute);
					checkedMixedAttributes!.AddRange(mixedBoundnessAttribute.AcumaticaAttributesHierarchy);
				}				
			}

			return mixedDbBoundnessAttributeInfos;
		}

		private bool DacFieldTypeAttributeCapturedByMixedBoundnessAttributesTypeHierarchy(ITypeSymbol originalAttribute, ITypeSymbol dacFieldTypeAttribute, 
																						  List<MixedDbBoundnessAttributeInfo> mixedDbBoundnessAttributeInfos)
		{
			bool presentInMixedAttributesHierarchy =
				mixedDbBoundnessAttributeInfos.Any(info => info.AcumaticaAttributesHierarchy.Contains(dacFieldTypeAttribute, 
																									  SymbolEqualityComparer.Default));

			// If data type attribute is present in the hierarchy of one of mixed boundness metadata attributes
			// then we need to check if dacFieldTypeAttribute is also directly aggregated on originalAttribute
			return presentInMixedAttributesHierarchy
				? !originalAttribute.EqualsOrAggregatesAttributeDirectly(dacFieldTypeAttribute, _pxContext)
				: false;
		}

		private DataTypeAttributeInfo? GetPXDbFieldAttributeInfoIfItIsUsedDirectly(ITypeSymbol attributeSymbol) =>
			attributeSymbol.EqualsOrAggregatesAttributeDirectly(_pxDBFieldAttribute, _pxContext)
				? new DataTypeAttributeInfo(FieldTypeAttributeKind.BoundTypeAttribute, _pxDBFieldAttribute, fieldType: null)
				: null;

		private DataTypeAttributeInfo? GetDacFieldTypeAttributeInfo(INamedTypeSymbol dacFieldTypeAttribute)
		{
			if (dacFieldTypeAttribute.Equals(_pxDBScalarAttribute, SymbolEqualityComparer.Default))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.PXDBScalarAttribute, _pxDBScalarAttribute, fieldType: null);
			else if (dacFieldTypeAttribute.Equals(_pxDBCalcedAttribute, SymbolEqualityComparer.Default))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.PXDBCalcedAttribute, _pxDBCalcedAttribute, fieldType: null);
			else if (BoundDacFieldTypeAttributesWithFieldType.TryGetValue(dacFieldTypeAttribute, out var boundFieldType))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.BoundTypeAttribute, dacFieldTypeAttribute, boundFieldType);
			else if (UnboundDacFieldTypeAttributesWithFieldType.TryGetValue(dacFieldTypeAttribute, out var unboundFieldType))
				return new DataTypeAttributeInfo(FieldTypeAttributeKind.UnboundTypeAttribute, dacFieldTypeAttribute, unboundFieldType);

			// TODO - some way of logging for Roslyn analyzers should be created with the support of out of process analysis
			return null;
		}

		private static Dictionary<ITypeSymbol, ITypeSymbol?> GetUnboundDacFieldTypeAttributesWithCorrespondingTypes(PXContext pxContext)
		{
			var types = new Dictionary<ITypeSymbol, ITypeSymbol?>(SymbolEqualityComparer.Default)
			{
				{ pxContext.FieldAttributes.PXLongAttribute,    pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXIntAttribute,     pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXShortAttribute,   pxContext.SystemTypes.Int16 },
				{ pxContext.FieldAttributes.PXStringAttribute,  pxContext.SystemTypes.String.Type },
				{ pxContext.FieldAttributes.PXByteAttribute,    pxContext.SystemTypes.Byte },
				{ pxContext.FieldAttributes.PXDecimalAttribute, pxContext.SystemTypes.Decimal },
				{ pxContext.FieldAttributes.PXDoubleAttribute,  pxContext.SystemTypes.Double },
				{ pxContext.FieldAttributes.PXFloatAttribute,   pxContext.SystemTypes.Float },
				{ pxContext.FieldAttributes.PXDateAttribute,    pxContext.SystemTypes.DateTime },
				{ pxContext.FieldAttributes.PXGuidAttribute,    pxContext.SystemTypes.Guid },
				{ pxContext.FieldAttributes.PXBoolAttribute,    pxContext.SystemTypes.Bool },
			};

			var pxVariantAttribute = pxContext.FieldAttributes.PXVariantAttribute;

			if (pxVariantAttribute != null)
				types.Add(pxVariantAttribute, pxContext.SystemTypes.ByteArray);

			return types;
		}

		private static Dictionary<ITypeSymbol, ITypeSymbol?> GetBoundDacFieldTypeAttributesWithCorrespondingTypes(PXContext pxContext)
		{
			var types = new Dictionary<ITypeSymbol, ITypeSymbol?>(SymbolEqualityComparer.Default)
			{
				{ pxContext.FieldAttributes.PXDBLongAttribute,         pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXDBIntAttribute,          pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXDBShortAttribute,        pxContext.SystemTypes.Int16 },
				{ pxContext.FieldAttributes.PXDBStringAttribute,       pxContext.SystemTypes.String.Type },
				{ pxContext.FieldAttributes.PXDBByteAttribute,         pxContext.SystemTypes.Byte },
				{ pxContext.FieldAttributes.PXDBDecimalAttribute,      pxContext.SystemTypes.Decimal },
				{ pxContext.FieldAttributes.PXDBDoubleAttribute,       pxContext.SystemTypes.Double },
				{ pxContext.FieldAttributes.PXDBFloatAttribute,        pxContext.SystemTypes.Float },
				{ pxContext.FieldAttributes.PXDBDateAttribute,         pxContext.SystemTypes.DateTime },
				{ pxContext.FieldAttributes.PXDBGuidAttribute,         pxContext.SystemTypes.Guid },
				{ pxContext.FieldAttributes.PXDBBoolAttribute,         pxContext.SystemTypes.Bool },
				{ pxContext.FieldAttributes.PXDBTimestampAttribute,    pxContext.SystemTypes.ByteArray },
				{ pxContext.FieldAttributes.PXDBIdentityAttribute,     pxContext.SystemTypes.Int32 },
				{ pxContext.FieldAttributes.PXDBLongIdentityAttribute, pxContext.SystemTypes.Int64 },
				{ pxContext.FieldAttributes.PXDBBinaryAttribute,       pxContext.SystemTypes.ByteArray },
				{ pxContext.FieldAttributes.PXDBUserPasswordAttribute, pxContext.SystemTypes.String.Type },
				{ pxContext.FieldAttributes.PXDBAttributeAttribute,    pxContext.SystemTypes.StringArray },
				{ pxContext.FieldAttributes.PXDBDataLengthAttribute,   pxContext.SystemTypes.Int64 },
			};

			var packedIntegerAttribute = pxContext.FieldAttributes.PXDBPackedIntegerArrayAttribute;

			if (packedIntegerAttribute != null)
			{
				types.Add(packedIntegerAttribute, pxContext.SystemTypes.UInt16Array);
			}

			return types;
		}


		/// <summary>
		/// Gets the DAC field type attributes with mixed DB boundness that can be put on both bound and unbound DAC field properties.
		/// </summary>
		/// <remarks>
		/// TODO: At this moment only the resolution of mixed DB boundness attribute is supported only for classic .Net inheritance. 
		/// The scenario where one mixed boundness attribute is aggregated on another mixed boundness attribute is not supported.
		/// </remarks>
		/// <param name="pxContext">The context.</param>
		/// <returns/>
		private static IEnumerable<MixedDbBoundnessAttributeInfo> GetDacFieldTypeAttributesWithMixedDbBoundness(PXContext pxContext) =>
			new List<MixedDbBoundnessAttributeInfo?>()
			{
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.PeriodIDAttribute,					   pxContext.SystemTypes.String.Type, isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.UnboundAccountAttribute,				   pxContext.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.UnboundCashAccountAttribute,			   pxContext.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.APTranRecognizedInventoryItemAttribute, pxContext.SystemTypes.Int32, isDbBoundByDefault: false),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.AcctSubAttribute,					   null,						isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.PXEntityAttribute,					   null,						isDbBoundByDefault: true),
				MixedDbBoundnessAttributeInfo.Create(pxContext.FieldAttributes.PXDBLocalizableStringAttribute,		   pxContext.SystemTypes.String.Type, isDbBoundByDefault: true),
			}
			.Where(attributeTypeWithIsDbFieldValue => attributeTypeWithIsDbFieldValue != null)!;

		private static ImmutableArray<ITypeSymbol> GetWellKnownNonDataTypeAttributes(PXContext pxContext)
		{
			var wellKnownNonDataTypeAttributes = ImmutableArray.CreateBuilder<ITypeSymbol>(initialCapacity: 9);

			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXUIFieldAttribute.Type);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXDefaultAttribute);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXStringListAttribute.Type);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXIntListAttribute.Type);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXSelectorAttribute.Type);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXForeignReferenceAttribute);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXParentAttribute);
			wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.PXDBDefaultAttribute);

			if (pxContext.AttributeTypes.AutoNumberAttribute.IsDefined)
				wellKnownNonDataTypeAttributes.Add(pxContext.AttributeTypes.AutoNumberAttribute.Type!);

			return wellKnownNonDataTypeAttributes.ToImmutable();
		}

		private static ImmutableDictionary<ITypeSymbol, DataTypeAttributeInfo> GetMetadataForWellKnownSpecialDataTypeAttributes(PXContext pxContext)
		{
			ImmutableDictionary<ITypeSymbol, DataTypeAttributeInfo>? metadataForWellKnownSpecialDataTypeAttributes = null;
			var packedIntegerAttribute = pxContext.FieldAttributes.PXDBPackedIntegerArrayAttribute;

			if (packedIntegerAttribute != null)
			{
				// PXDBPackedIntegerArrayAttribute is a special attribute written in a very hacky way.
				// It derives from PXDBBinaryAttribute which works with byte[] but in reality PXDBPackedIntegerArrayAttribute works with ushort[].
				// This breaks Acumatica design principle where derived attribute work on properties with the same property type as their base attribute.
				// Thus, the attribute needs special handling in Acuminator.
				var packedIntegerMetadata = new DataTypeAttributeInfo(FieldTypeAttributeKind.BoundTypeAttribute, packedIntegerAttribute,
																	  pxContext.SystemTypes.UInt16Array);
				metadataForWellKnownSpecialDataTypeAttributes = 
					ImmutableDictionary.CreateRange(SymbolEqualityComparer.Default,
													new[] { KeyValuePair.Create(packedIntegerAttribute as ITypeSymbol, packedIntegerMetadata) });
			}

			return metadataForWellKnownSpecialDataTypeAttributes ?? ImmutableDictionary<ITypeSymbol, DataTypeAttributeInfo>.Empty;
		}
	}
}