using System;
using System.Collections.Generic;
using System.Linq;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class APReleaseProcessDatasourceExt : PXGraphExtension<MyGraph>
	{
		public delegate void DoSomething1DelegateType(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs);

		[PXOverride]
		public void DoSomething1(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs, DoSomething1DelegateType baseMethod)
		{
			inDocs = new List<MyDac>();
			baseMethod(ref doc, isPrebooking, out inDocs);
		}

		public delegate void DoSomething2DelegateType(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs);

		[PXOverride]
		public void DoSomething2(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs, DoSomething2DelegateType action)
		{
			inDocs = new List<MyDac>();
			action(ref doc, isPrebooking, out inDocs);
		}

		public delegate ref readonly bool DoSomething3DelegateType(MyDac doc);

		[PXOverride]
		public ref readonly bool DoSomething3(MyDac doc, DoSomething3DelegateType func) =>
			 ref func(doc);
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