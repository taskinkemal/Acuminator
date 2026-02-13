using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class APReleaseProcessDatasourceExt : PXGraphExtension<MyGraph>
	{
		public delegate void DoSomethingDelegateType(ref MyDac doc, bool isPrebooking, List<MyDac> inDocs);

		[PXOverride]
		public void DoSomething(ref MyDac doc, bool isPrebooking, List<MyDac> inDocs, DoSomethingDelegateType base_DoSomething)
		{
			base_DoSomething(ref doc, isPrebooking, inDocs);
		}
	}

	public class MyGraph : PXGraph<MyGraph>
	{
		protected virtual void DoSomething(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs)
		{
			inDocs = new List<MyDac>();
		}
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}