﻿using System;

namespace Acuminator.Tests.Sources
{
	public class BaseClass : PX.Data.PXGraphExtension<int>
	{
		public virtual object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class ExtClass : BaseClass
	{
		[PX.Data.PXOverride]
		public override object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}
}

namespace PX.Data
{
	public class PXOverrideAttribute : Attribute
	{
	}

	public abstract class PXGraphExtension<Graph>
	{
		internal Graph _Base;
	}

	public abstract class PXGraphExtension<Extension1, Graph>
	{
		internal Extension1 MyExt;

		internal Graph MyGraph;
	}
}