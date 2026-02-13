using System;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseGraphExtension : PXGraphExtension<BaseGraph>
	{
		/// Overrides <seealso cref="BaseGraph.Test"/>
		[PXOverride]
		public void Test(PXAdapter adapter, Func<PXAdapter, IEnumerable> base_Test) => base_Test(adapter);
	}

	public class BaseGraph : PXGraph<BaseGraph>
	{
		public PXAction<SOOrder> test = null!;

		[PXButton]
		[PXUIField]
		public virtual IEnumerable Test(PXAdapter adapter) 
		{
			return adapter.Get();
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
