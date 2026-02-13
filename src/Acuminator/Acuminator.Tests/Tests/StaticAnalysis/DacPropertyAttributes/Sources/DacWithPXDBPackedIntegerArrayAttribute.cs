using System;
using System.Collections.Generic;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	// Acuminator disable once PX1069 MissingMandatoryDacFields No need in unit test
	[PXHidden]
	public class SomeDac : PXBqlTable, IBqlTable
	{
		#region DacID
		public abstract class dacID : PX.Data.BQL.BqlInt.Field<dacID> { }

		[PXDBInt]
		public virtual int? DacID
		{
			get;
			set;
		}
		#endregion

		#region DiscountsAppliedToLine
		public abstract class discountsAppliedToLine : IBqlField { }

		[PXDBPackedIntegerArray]
		public virtual ushort[]? DiscountsAppliedToLine { get; set; }
		#endregion
	}
}