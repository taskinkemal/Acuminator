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

		/// Overrides <seealso cref="MyGraph.DoSomething1(ref MyDac, bool, out List{MyDac})"/>
		[PXOverride]
		public void DoSomething1(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs, DoSomething1DelegateType base_DoSomething1)
		{
			inDocs = new List<MyDac>();
			base_DoSomething1(ref doc, isPrebooking, out inDocs);
		}

		public delegate void DoSomething2DelegateType(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs);

		/// Overrides <seealso cref="MyGraph.DoSomething2(ref MyDac, bool, out List{MyDac})"/>
		[PXOverride]
		public void DoSomething2(ref MyDac doc, bool isPrebooking, out List<MyDac> inDocs, DoSomething2DelegateType base_DoSomething2)
		{
			inDocs = new List<MyDac>();
			base_DoSomething2(ref doc, isPrebooking, out inDocs);
		}

		public delegate ref readonly bool DoSomething3DelegateType(MyDac doc);

		/// Overrides <seealso cref="MyGraph.DoSomething3(MyDac)"/>
		[PXOverride]
		public ref readonly bool DoSomething3(MyDac doc, DoSomething3DelegateType base_DoSomething3) =>
			 ref base_DoSomething3(doc);
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