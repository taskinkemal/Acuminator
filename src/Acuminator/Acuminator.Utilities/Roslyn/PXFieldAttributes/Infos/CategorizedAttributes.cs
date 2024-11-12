using System.Collections.Generic;
using Acuminator.Utilities.Roslyn.Semantic.Attribute;

namespace Acuminator.Utilities.Roslyn.PXFieldAttributes.Infos;

public record CategorizedAttributes(
	List<DacFieldAttributeInfo>? TypeAttributesOnDacProperty, 
	List<DacFieldAttributeInfo>? TypeAttributesWithDifferentDataTypesOnAggregator, 
	bool HasNonNullDataType);
