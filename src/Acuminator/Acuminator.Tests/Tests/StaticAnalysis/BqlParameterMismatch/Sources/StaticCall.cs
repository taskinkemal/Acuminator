﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;

namespace PX.Objects.HackathonDemo
{
	public class SOOrderTestEntry : PXGraph<SOOrderTestEntry>
	{
		public object MissingArgs_PassedAsArray()
		{
			var args = new int[] { 2 };

			var result =
                PXSelect<SOOrder, 
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>, 
					And<SOOrder.status, Equal<Required<SOOrder.status>>>>>
				.SelectSingleBound(this, pars: args, currents: null).ToArray();
			
            return this;
		}

		public object ExtraArgs_PassedAsArray()
		{
			var args = new int[] { 2, 3, 4 };

			var result =
				PXSelect<SOOrder,
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrder.status, Equal<Required<SOOrder.status>>>>>
				.SelectSingleBound(this, currents: null, pars: args)
				.ToArray();

			return this;
		}

		public object ExtraArgs_PassedAsParams()
		{
			object[] currents = null;

			var result =
				PXSelect<SOOrder,
				Where<SOOrder.orderType, Equal<Required<SOOrder.orderType>>,
					And<SOOrder.status, Equal<Required<SOOrder.status>>>>>
				.SelectSingleBound(this, currents, 2, 3, 4)
				.RowCast<SOOrder>()
				.ToList();

			return this;
		}

		public object OptionalArgs()
		{
			object[] currents = null;

			var result =
				PXSelect<SOOrder,
				Where<SOOrder.orderType, Equal<Optional<SOOrder.orderType>>,
					And<SOOrder.status, Equal<Required<SOOrder.status>>>>>
				.SelectSingleBound(this, currents, 2, 3, 4)
				.RowCast<SOOrder>()
				.ToList();

			return this;
		}
	}
}
