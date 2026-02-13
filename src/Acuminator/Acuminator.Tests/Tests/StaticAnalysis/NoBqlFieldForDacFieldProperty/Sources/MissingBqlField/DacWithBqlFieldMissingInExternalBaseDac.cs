using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class DerivedDac : ExternalDependency.NoBqlFieldForDacFieldProperty.BaseDacWithoutBqlField
	{
		[PXString]
		[PXUIField(DisplayName = "Order Number")]
		public virtual string OrderNbr { get; set; }

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