using System;

using PX.Data;
using PX.Data.BQL;

namespace PX.Analyzers.Test.Sources
{
	/// <exclude/>
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	[PXHidden]
	public sealed class DacExtension : PXCacheExtension<BaseDac>        // PX1065 for ShipmentNbr should not be reported here because it is not declared in the extension
	{
		[PXString]
		[PXUIField(DisplayName = "Status")]
		public string? Status { get; set; }

		#region Selected
		[PXBool]
		[PXUIField(DisplayName = "Selected")]
		public bool? Selected
		{
			get;
			set;
		}
		#endregion

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
		public decimal? Amount              // PX1065 should be reported here because the property does not have a BQL field
		{
			get;
			set;
		}
		#endregion
	}
}