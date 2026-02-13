using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Acuminator.Utilities.Common;
using Acuminator.Utilities.Roslyn.Constants;
using Acuminator.Utilities.Roslyn.PXFieldAttributes;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;
using Acuminator.Utilities.Roslyn.Semantic.Shared;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer;
using Acuminator.Utilities.Roslyn.Semantic.Shared.Infer.Dac;
using Acuminator.Utilities.Roslyn.Syntax;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Acuminator.Utilities.Roslyn.Semantic.Dac
{
	public class DacSemanticModel : ISemanticModel
	{
		private readonly CancellationToken _cancellation;

		public PXContext PXContext { get; }

		public DacType DacType { get; }

		public DacOrDacExtInfoBase DacOrDacExtInfo { get; }

		public string Name => DacOrDacExtInfo.Name;

		[MemberNotNullWhen(returnValue: false, nameof(Node))]
		public bool IsInMetadata => DacOrDacExtInfo.IsInMetadata;

		[MemberNotNullWhen(returnValue: true, nameof(Node))]
		public bool IsInSource => DacOrDacExtInfo.IsInSource;

		public ClassDeclarationSyntax? Node => DacOrDacExtInfo.Node;

		public int DeclarationOrder => DacOrDacExtInfo.DeclarationOrder;

		public ITypeSymbol Symbol => DacOrDacExtInfo.Symbol;

		/// <summary>
		/// The DAC symbol. For the DAC, the value is the same as <see cref="Symbol"/>. 
		/// For DAC extensions, the value is the symbol of the extension's base DAC.
		/// </summary>
		public ITypeSymbol? DacSymbol { get; }

		/// <summary>
		/// An indicator of whether the DAC is a mapping DAC derived from the PXMappedCacheExtension class.
		/// </summary>
		public bool IsMappedCacheExtension { get; }

		/// <summary>
		/// An indicator of whether the DAC is fully unbound.
		/// </summary>
		public bool IsFullyUnbound { get; }

		/// <summary>
		/// An indicator of whether the DAC is a projection DAC.
		/// </summary>
		public bool IsProjectionDac { get; }

		public ImmutableDictionary<string, DacPropertyInfo> PropertiesByNames { get; }
		public IEnumerable<DacPropertyInfo> Properties => PropertiesByNames.Values;

		public IEnumerable<DacPropertyInfo> DacFieldPropertiesWithBqlFields => Properties.Where(p => p.HasBqlFieldEffective);

		public IEnumerable<DacPropertyInfo> DacFieldPropertiesWithAcumaticaAttributes => 
			Properties.Where(p => p.HasAcumaticaAttributesEffective);

		public IEnumerable<DacPropertyInfo> AllDeclaredProperties => Properties.Where(p => p.Symbol.IsDeclaredInType(Symbol));

		public IEnumerable<DacPropertyInfo> DeclaredDacFieldPropertiesWithBqlFields => 
			Properties.Where(p => p.HasBqlFieldEffective && p.Symbol.IsDeclaredInType(Symbol));

		public IEnumerable<DacPropertyInfo> DeclaredDacFieldPropertiesWithAcumaticaAttributes =>
			Properties.Where(p => p.HasAcumaticaAttributesEffective && p.Symbol.IsDeclaredInType(Symbol));

		public ImmutableDictionary<string, DacBqlFieldInfo> BqlFieldsByNames { get; }
		public IEnumerable<DacBqlFieldInfo> BqlFields => BqlFieldsByNames.Values;

		public IEnumerable<DacBqlFieldInfo> DeclaredBqlFields => BqlFields.Where(f => f.Symbol.IsDeclaredInType(Symbol));

		public ImmutableDictionary<string, DacFieldInfo> DacFieldsByNames { get; }

		public IEnumerable<DacFieldInfo> DacFields => DacFieldsByNames.Values;

		public IEnumerable<DacFieldInfo> DeclaredDacFields => DacFields.Where(f => f.IsDeclaredInType(Symbol));

		/// <summary>
		/// Information about the IsActive method of the DAC extensions. 
		/// The value can be <c>null</c>. The value is always <c>null</c> for DACs.
		/// <value>
		/// Information about the IsActive method.
		/// </value>
		public IsActiveMethodInfo? IsActiveMethodInfo { get; }

		/// <summary>
		/// The attributes declared on a DAC or a DAC extension.
		/// </summary>
		public ImmutableArray<DacAttributeInfo> Attributes { get; }

		/// <summary>
		/// The PXDBInterceptorAttribute attributes declared on a DAC or a DAC extension.
		/// </summary>
		public ImmutableArray<DacAttributeInfo> DBInterceptorAttributes { get; }

		/// <summary>
		/// The PXAccumulator-derived attribute if there is any declared on a DAC.
		/// </summary>
		public DacAttributeInfo? AccumulatorAttribute { get; }

		[MemberNotNullWhen(returnValue: true, nameof(AccumulatorAttribute))]
		public bool HasAccumulatorAttribute => AccumulatorAttribute != null;

		protected DacSemanticModel(PXContext pxContext, DacOrDacExtInfoBase dacOrDacExtInfo, CancellationToken cancellation)
		{
			cancellation.ThrowIfCancellationRequested();

			PXContext 		= pxContext.CheckIfNull();
			DacOrDacExtInfo = dacOrDacExtInfo.CheckIfNull();

			(DacType, DacSymbol) = dacOrDacExtInfo switch
			{
				DacInfo dacInfo				=> (DacType.Dac, dacInfo.Symbol),
				DacExtensionInfo dacExtInfo => (DacType.DacExtension, dacExtInfo.Dac?.Symbol),
				_							=> throw new ArgumentOutOfRangeException(nameof(dacOrDacExtInfo),
													$"The \"{nameof(dacOrDacExtInfo)}\" parameter must be either {nameof(DacInfo)} or {nameof(DacExtensionInfo)}.")
			};

			_cancellation = cancellation;
			IsMappedCacheExtension = Symbol.InheritsFromOrEquals(PXContext.PXMappedCacheExtensionType);

			Attributes		  		= GetDacAttributes();
			DBInterceptorAttributes = Attributes.Where(attr => attr.IsDbInterceptorAttribute).ToImmutableArray();
			BqlFieldsByNames  		= GetDacBqlFields();
			PropertiesByNames 		= GetDacProperties(BqlFieldsByNames);
			DacFieldsByNames  		= DacFieldsCollector.CollectDacFieldsFromDacPropertiesAndBqlFields(DacOrDacExtInfo, PXContext,
																									   BqlFieldsByNames, PropertiesByNames);
			IsActiveMethodInfo = GetIsActiveMethodInfo();

			IsProjectionDac = CheckIfDacIsProjection();
			AccumulatorAttribute = GetPXAccumulatorAttribute();
			IsFullyUnbound = IsFullyUnboundDac();
		}

		/// <summary>
		/// Returns the semantic model of DAC or DAC extension which is inferred from <paramref name="dacOrDacExtTypeSymbol"/>.
		/// </summary>
		/// <param name="pxContext">Context instance.</param>
		/// <param name="dacOrDacExtTypeSymbol">The DAC or DAC extension type symbol.</param>
		/// <param name="customDeclarationOrder">(Optional) The custom declaration order.</param>
		/// <param name="cancellation">(Optional)Cancellation token.</param>
		/// <returns>
		/// A semantic model for a given DAC or DAC extension type <paramref name="dacOrDacExtTypeSymbol"/>.
		/// </returns>
		public static DacSemanticModel? InferModel(PXContext pxContext, ITypeSymbol dacOrDacExtTypeSymbol, int? customDeclarationOrder = null,
												   CancellationToken cancellation = default)
		{
			cancellation.ThrowIfCancellationRequested();

			var inferredInfo = DacAndDacExtInfoBuilder.Instance.InferTypeInfo(dacOrDacExtTypeSymbol, pxContext, customDeclarationOrder, cancellation);

			if (inferredInfo?.GetResultKind() != InferResultKind.Success || inferredInfo.InferredInfo is not DacOrDacExtInfoBase dacExtInfoBase)
				return null;

			return InferModel(pxContext, dacExtInfoBase, cancellation);
		}

		/// <summary>
		/// Returns the semantic model of DAC or DAC extension which is inferred from <paramref name="dacOrDacExtInfo"/>.
		/// </summary>
		/// <param name="pxContext">Context instance.</param>
		/// <param name="dacOrDacExtInfo">The DAC or DAC extension inferred information obtained from resolving a hierarchy of chained DAC extensions and base types.</param>
		/// <param name="cancellation">Cancellation token.</param>
		/// <returns>
		/// A semantic model for a given DAC or DAC extension <paramref name="dacOrDacExtInfo"/>.<br/>
		/// If <paramref name="dacOrDacExtInfo"/> is not DAC or DAC extension, then returns <see langword="null"/>.
		/// </returns>
		public static DacSemanticModel? InferModel(PXContext pxContext, DacOrDacExtInfoBase dacOrDacExtInfo, CancellationToken cancellation)
		{		
			cancellation.ThrowIfCancellationRequested();

			if (dacOrDacExtInfo is not (DacInfo or DacExtensionInfo))
				return null;

			return new DacSemanticModel(pxContext, dacOrDacExtInfo, cancellation);
		}

		/// <summary>
		/// Gets the member nodes of the specified type from the declaration of a DAC or a DAC extension.
		/// The method does not perform boxing of <see cref="SyntaxList{TNode}"/> <see cref="DacNode.Members"/> which is good for performance.
		/// </summary>
		/// <typeparam name="TMemberNode">Type of the member node</typeparam>
		/// <returns/>
		public IEnumerable<TMemberNode> GetMemberNodes<TMemberNode>()
		where TMemberNode : MemberDeclarationSyntax
		{
			if (IsInMetadata)
				yield break;

			var memberList = Node.Members;

			for (int i = 0; i < memberList.Count; i++)
			{
				if (memberList[i] is TMemberNode memberNode)
					yield return memberNode;
			}
		}

		protected ImmutableArray<DacAttributeInfo> GetDacAttributes()
		{
			var attributes = Symbol.GetAttributes();

			if (attributes.IsDefaultOrEmpty)
				return ImmutableArray<DacAttributeInfo>.Empty;

			var attributeInfos = attributes.Select((attributeData, relativeOrder) => new DacAttributeInfo(PXContext, attributeData, relativeOrder));
			var builder = ImmutableArray.CreateBuilder<DacAttributeInfo>(attributes.Length);
			builder.AddRange(attributeInfos);

			return builder.ToImmutable();
		}

		protected ImmutableDictionary<string, DacBqlFieldInfo> GetDacBqlFields() =>
			DacOrDacExtInfo.GetDacBqlFieldInfos(PXContext, _cancellation)
						   .ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);

		protected ImmutableDictionary<string, DacPropertyInfo> GetDacProperties(IDictionary<string, DacBqlFieldInfo> dacBqlFields) =>
			DacOrDacExtInfo.GetPropertyInfos(PXContext, dacBqlFields, _cancellation)
						   .ToImmutableDictionary(keyComparer: StringComparer.OrdinalIgnoreCase);

		protected IsActiveMethodInfo? GetIsActiveMethodInfo()
		{
			if (DacType != DacType.DacExtension)
				return null;

			_cancellation.ThrowIfCancellationRequested();
			return IsActiveMethodInfo.GetIsActiveMethodInfo(Symbol, _cancellation);
		}

		protected bool CheckIfDacIsProjection()
		{
			if (DacType != DacType.Dac || DBInterceptorAttributes.IsDefaultOrEmpty)
				return false;

			return DBInterceptorAttributes.Any(attrInfo => attrInfo.IsPXProjection);
		}

		protected DacAttributeInfo? GetPXAccumulatorAttribute()
		{
			if (DacType != DacType.Dac || DBInterceptorAttributes.IsDefaultOrEmpty)
				return null;

			return DBInterceptorAttributes.FirstOrDefault(attr => attr.IsPXAccumulatorAttribute);
		}

		protected bool IsFullyUnboundDac()
		{
			if (DacFieldsByNames.Count == 0)
				return true;

			// Heuristic - DACs with PXDBInterceptorAttribute should be DB bound.
			if (!DBInterceptorAttributes.IsDefaultOrEmpty)
				return false;

			if (!Attributes.IsDefaultOrEmpty)
			{
				var pxVirtualAttribute = PXContext.AttributeTypes.PXVirtualAttribute;

				if (Attributes.Any(aInfo => pxVirtualAttribute.Equals(aInfo.AttributeType, SymbolEqualityComparer.Default)))
					return true;
			}

			int unboundFieldsCount = 0;
			int boundFieldsCount = 0;

			foreach (DacPropertyInfo property in DacFieldPropertiesWithAcumaticaAttributes)
			{
				if (property.EffectiveDbBoundness is DbBoundnessType.Unbound or DbBoundnessType.NotDefined)
					unboundFieldsCount++;
				else
					boundFieldsCount++;
			}

			switch (boundFieldsCount)
			{
				case 0:
					return true;
				case 1:
					// Heuristic for NoteID - if NoteID is the only bound field and there is more than one field, then consider the DAC as fully unbound.
					// This is required because there is no good way to configure an unbound NoteID field.
					if (unboundFieldsCount == 0 || !PropertiesByNames.TryGetValue(DacFieldNames.System.NoteID, out var noteIdProperty))
						return false;

					return noteIdProperty.EffectiveDbBoundness == DbBoundnessType.DbBound;
				default:
					return false;
			}
		}
	}
}
