using System;
using System.Collections;
using System.Collections.Generic;

using PX.Data;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidPXActionSignature.Sources
{
	public class APRegister : PXBqlTable, IBqlTable
	{ }

	class PXBaseCuryAttribute : PXDecimalAttribute
	{
	}

	public abstract class APDataEntryGraph<TGraph, TPrimary> : PXGraph<TGraph, TPrimary>
		where TGraph : PXGraph
		where TPrimary : APRegister, new()
	{
		public PXAction<TPrimary> action = null!;

		protected virtual IEnumerable Action(PXAdapter adapter,
			[PXString()]
			string ActionName
			)
		{
			return adapter.Get();
		}

		public PXAction<TPrimary> report = null!;
		[PXUIField(DisplayName = "Reports", MapEnableRights = PXCacheRights.Select)]
		[PXButton(SpecialType = PXSpecialButtonType.Report)]
		protected virtual IEnumerable Report(PXAdapter adapter,
			[PXString(8)]
			[PXStringList(new string[] { "AP610500", "AP622000", "AP622500"}, new string[] { "AP Edit", "AP Register Detailed", "AP Payment Register" })]
			string reportID,
			[PXBool]
			bool refresh
			)
		{
			return adapter.Get();
		}

		public PXAction<TPrimary> action1 = null!;
		[PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Select)]
		[PXButton]
		protected virtual IEnumerable Action1(PXAdapter adapter,
		[PXInt]
		[PXIntList(new int[] { 1, 2 }, new string[] { "Persist", "Update" })]
		int? actionID,
		[PXBool]
		bool refresh,
		[PXString]
		string actionName)
		{
			return adapter.Get();
		}


		public PXAction<TPrimary> runReversal = null!;
		[PXUIField(DisplayName = "we", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXProcessButton]
		// TODO: Split parameters between appropriate actions (waiting for Andrew Boulanov)
		public virtual IEnumerable RunReversal(PXAdapter adapter,
			[PXDate]
			DateTime? disposalDate,
			[PXString]
			string disposalPeriodID,
			[PXBaseCury]
			decimal? disposalCost,
			[PXInt]
			int? disposalMethID,
			[PXInt]
			int? disposalAcctID,
			[PXInt]
			int? disposalSubID,
			[PXString]
			string dispAmtMode,
			[PXBool]
			bool? deprBeforeDisposal,
			[PXString]
			string reason,
			[PXInt]
			int? assetID
			)
		{
			return adapter.Get();
		}

		public PXAction<TPrimary> UpdateIN = null!;
		[PXUIField(DisplayName = "Update IN", Visible = false, MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		private IEnumerable updateIN(PXAdapter adapter, List<TPrimary>? shipmentList = null)
		{
			return adapter.Get();
		}
	}
}
