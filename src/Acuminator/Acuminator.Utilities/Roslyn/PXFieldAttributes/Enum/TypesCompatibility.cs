namespace Acuminator.Utilities.Roslyn.PXFieldAttributes.Enum;

/// <summary>
/// Relation between CLR type of property and data type annotations.
/// </summary>
public enum TypesCompatibility
{
	/// <summary>
	/// DAC property is missing data type attributes
	/// </summary>
	MissingTypeAnnotation = 0,

	/// <summary>
	/// CLR type of DAC property is incompatible with data type attributes
	/// </summary>
	IncompatibleTypes,

	/// <summary>
	/// CLR type of DAC property is compatible with data type attributes
	/// </summary>
	CompatibleTypes
}
