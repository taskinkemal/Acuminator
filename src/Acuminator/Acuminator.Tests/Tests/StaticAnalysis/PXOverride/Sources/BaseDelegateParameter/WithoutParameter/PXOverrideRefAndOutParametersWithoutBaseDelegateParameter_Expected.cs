using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class APReleaseProcessDatasourceExt : PXGraphExtension<MyGraph>
	{
		private readonly bool _field = true;

		[PXOverride]
		public void DoSomething1(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs)
		{
			inDocs = new List<MyDac>();
		}

		[PXOverride]
		public void DoSomething2(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs)
		{
			inDocs = new List<MyDac>();
		}

		[PXOverride]
		public ref readonly bool DoSomething3(MyDac doc) => ref _field;
	}

	public class MyGraph : PXGraph<MyGraph>
	{
		private readonly bool _field = true;

		protected virtual void DoSomething1(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs)
		{
			inDocs = new List<MyDac>();
		}

		protected virtual void DoSomething2(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs)
		{
			inDocs = new List<MyDac>();
		}

		protected virtual ref readonly bool DoSomething3(MyDac doc)
		{
			return ref _field;
		}
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}