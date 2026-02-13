using System;
using System.Collections;
using System.Linq;
using PX.Common;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidPXActionSignature.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseGraphExtension : PXGraphExtension<BaseGraph>
	{

		/// Overrides <seealso cref="BaseGraph.TestTest1"/>
		[PXOverride]
		public void TestTest1(Action base_TestTest1) { }

		/// Overrides <seealso cref="BaseGraph.TestTest2"/>
		[PXOverride]
		public IEnumerable TestTest2(PXAdapter adapter, Func<PXAdapter, IEnumerable> base_TestTest2) => base_TestTest2(adapter);
	}

	public class BaseGraph : PXGraph<BaseGraph>
	{
		public PXAction<SOOrder> testTest1 = null!;

		[PXButton]
		[PXUIField]
		public virtual void TestTest1() { }

		public PXAction<SOOrder> testTest2 = null!;

		[PXButton]
		[PXUIField]
		public virtual IEnumerable TestTest2(PXAdapter adapter) 
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
