﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Objects.AR;

namespace PX.Objects.AR
{
	public class InvoiceNbrAttribute : PXEventSubscriberAttribute, IPXRowInsertedSubscriber, IPXRowUpdatedSubscriber, IPXRowPersistedSubscriber, IPXFieldVerifyingSubscriber
	{
		public virtual void RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			ARInvoiceNbr record = new ARInvoiceNbr();
			PXCache cache = sender.Graph.Caches[typeof(ARInvoiceNbr)];
			List<PXDataFieldAssign> field = new List<PXDataFieldAssign>();
			PXDatabase.Insert<ARInvoiceNbr>(fields.ToArray());
		}
	}
}