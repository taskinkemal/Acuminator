#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace ExternalDependency.NoBqlFieldForDacFieldProperty
{
	[PXHidden]
	public class BaseDacWithoutBqlField : PXBqlTable, IBqlTable
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
