using System;

using PX.Data;

namespace PX.Analyzers.Test.Sources
{
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class DerivedDac : BaseDac             // PX1065 for ShipmentNbr should not be reported here because it is not declared in the extension
	{
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string? Status { get; set; }

		#region Amount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Customized Amount")]
		public override decimal? Amount                  // PX1065 should be reported here because the base property without a BQL field is declared in the extension 
		{
			get;
			set;
		}
		#endregion
	}

	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class BaseDac : PXBqlTable, IBqlTable
	{
		#region Status
		public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
		#endregion

		[PXInt]
		public virtual int? ShipmentNbr { get; set; }

		public virtual string? ExtraData { get; set; }

		#region Amount
		[PXDBDecimal]
		[PXUIField(DisplayName = "Amount")]
		public virtual decimal? Amount              // PX1065 should be reported here because the property does not have a BQL field
		{
			get;
			set;
		}
		#endregion
	}
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
}