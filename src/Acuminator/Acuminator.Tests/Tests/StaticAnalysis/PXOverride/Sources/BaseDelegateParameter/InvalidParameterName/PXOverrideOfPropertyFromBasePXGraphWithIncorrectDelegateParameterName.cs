using System;
using System.Diagnostics.CodeAnalysis;

using PX.Data;

namespace Acuminator.Tests.Sources
{
	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class BaseExtension : PXGraphExtension<MyGraph>
	{

		/// Overrides <seealso cref="PXGraph.PrimaryItemType"/>
		[PXOverride]
		[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Test sources")]
		public Type get_PrimaryItemType(Func<Type> baseProperty) => typeof(MyDac);
	}

	public class MyGraph : PXGraph<MyGraph>
	{
	}

	[PXHidden]
	public class MyDac : PXBqlTable, IBqlTable
	{
	}
}