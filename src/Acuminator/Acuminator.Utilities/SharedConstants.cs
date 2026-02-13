using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator
{
	/// <summary>
	/// A constants shared by all Acuminator logic.
	/// </summary>
	public static class SharedConstants
	{
		/// <summary>
		/// The Acuminator package name.
		/// </summary>
		public const string PackageName = "Acuminator";

		/// <summary>
		/// The Acuminator diagnostic common prefix.
		/// </summary>
		public const string AcuminatorDiagnosticPrefix = "PX";


		/// <summary>
		/// Filename of the suppression file XML schema.
		/// </summary>
		public const string SuppressionFileXmlSchemaFileName = "SuppressionFileSchema.xsd";

		/// <summary>
		/// The file scoped namespace declaration syntax kind. The version of Roslyn used by Acuminator is too old to have this in the SyntaxKind enum.
		/// </summary>
		/// <remarks>
		/// TODO fix after upgrading to a newer Roslyn version. 
		/// </remarks>
		public const int FileScopedNamespaceDeclarationKind = 8845;
	}
}
