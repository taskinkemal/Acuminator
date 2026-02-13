using System;

using PX.Data;

using ExternalDependency.NoBqlFieldForDacFieldProperty;

namespace PX.Analyzers.Test.Sources
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXHidden]
	public sealed class DacExtensionOnExternalDac : PXCacheExtension<BaseDacWithoutBqlField>    // No PX1065 should be reported here because ShipmentNbr is not declared in the extension
	{
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string? Status { get; set; }

		#region Amount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Customized Amount")]
		public decimal? Amount                  // PX1065 should be reported here because the base property without a BQL field is declared in the extension 
		{
			get;
			set;
		}
		#endregion
	}
}