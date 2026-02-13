using System;
using System.Collections;
using System.Linq;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidPXActionSignature.Sources
{
	public class SOEntry : PXGraph<SOEntry>
	{
		public PXSelect<SOOrder> Documents = null!;

		public PXAction<SOOrder> Release = null!;

		public void release(PXAdapter adapter)
		{
			string s = "blabla";
		}
	}

	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class SOOrder : PXBqlTable, IBqlTable
	{
		public abstract class orderType : BqlString.Field<orderType> { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		public string? OrderType { get; set; }

		public abstract class orderNbr : BqlString.Field<orderNbr> { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		public string? OrderNbr { get; set; }
	}
}
