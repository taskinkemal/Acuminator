﻿using System;

namespace Acuminator.Tests.Sources
{
	public class SuperBaseClass : PX.Data.PXGraphExtension<int, int>
	{
		public object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class BaseClass : PX.Data.PXGraphExtension<SuperBaseClass>
	{
		[PX.Data.PXOverride]
		public object TestMethod(int x, bool drilldown, double y)
		{
			return new object();
		}
	}

	public class ExtClass : PX.Data.PXGraphExtension<BaseClass, int>
	{
		public delegate object MyDelegate(int x, bool drilldown, double y);

		[PX.Data.PXOverride]
		public object TestMethod(int x, bool drilldown, double y, MyDelegate del)
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