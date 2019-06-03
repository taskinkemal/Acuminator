﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acuminator.Utilities.Roslyn.Constants
{
	public class DelegateNames
	{
		#region PXDBFieldAttributeFields
		public static readonly string IsKey = "IsKey";
		#endregion

		#region PXUIFieldAttributeSymbols
		public static readonly string SetVisible = "SetVisible";
		public static readonly string SetVisibility = "SetVisibility";
		public static readonly string SetEnabled = "SetEnabled";
		public static readonly string SetRequired = "SetRequired";
		public static readonly string SetReadOnly = "SetReadOnly";
		public static readonly string SetDisplayName = "SetDisplayName";
		public static readonly string SetNeutralDisplayName = "SetNeutralDisplayName";
		#endregion
		
		#region PXContext
		public static readonly string Initialize = "Initialize";
		public static readonly string StartOperation = "StartOperation";
		#endregion

		#region ExceptionSymbols
		public static readonly string SetCaption = "SetCaption";
		public static readonly string SetTooltip = "SetTooltip";
		public static readonly string Press = "Press";
		#endregion

		#region PXCacheSymbols
		public static readonly string Insert = "Insert";
		public static readonly string Update = "Update";
		public static readonly string Delete = "Delete";
		public static readonly string RaiseExceptionHandling = "RaiseExceptionHandling";
		#endregion

		#region  PXDatabaseSymbols
		public static readonly string Select = "Select";
		public static readonly string ForceDelete = "ForceDelete";
		public static readonly string Ensure = "Ensure";
		#endregion

		#region PXGraphSymbols
		public static readonly string InstanceCreatedEvents = "PX.Data.PXGraph+InstanceCreatedEvents";
		public static readonly string InstanceCreatedEventsAddHabdler = "AddHandler";
		public static readonly string InitCacheMapping = "InitCacheMapping";
		public static readonly string CreateInstance = "CreateInstance";
		#endregion

		public static readonly string SetList = "SetList";

		public static readonly string SetParameters = "SetParametersDelegate";
		public static readonly string SetProcess = "SetProcessDelegate";

		public static readonly string View = "View";

		public static readonly string WhereAnd = "WhereAnd";
		public static readonly string WhereNew = "WhereNew";
		public static readonly string WhereOr = "WhereOr";
		public static readonly string Join = "Join";

		public static readonly string GetItem = "GetItem";

		public static readonly string AppendList = "AppendList";
		public static readonly string SetLocalizable = "SetLocalizable";

		#region PXSystemActionSymbols
		public static readonly string PXSave = "PX.Data.PXSave`1";
		public static readonly string PXCancel = "PX.Data.PXCancel`1";
		public static readonly string PXInsert = "PX.Data.PXInsert`1";
		public static readonly string PXDelete = "PX.Data.PXDelete`1";
		public static readonly string PXCopyPasteAction = "PX.Data.PXCopyPasteAction`1";
		public static readonly string PXFirst = "PX.Data.PXFirst`1";
		public static readonly string PXPrevious = "PX.Data.PXPrevious`1";
		public static readonly string PXNext = "PX.Data.PXNext`1";
		public static readonly string PXLast = "PX.Data.PXLast`1";
		public static readonly string PXChangeID = "PX.Data.PXChangeID`1";
		#endregion

		public static readonly string JoinNew = "JoinNew";
		public static readonly string StartRow = "StartRow";

		#region BqlModifyingMethods.cs
			#region BqlCommandInstance
			public static readonly string AggregateNew = "AggregateNew";
			public static readonly string OrderByNew = "OrderByNew";
			#endregion


			#region BqlCommandStatic
			public static readonly string Compose = "Compose";
			public static readonly string AddJoinConditions = "AddJoinConditions";
			public static readonly string AppendJoin = "AppendJoin";
			public static readonly string NewJoin = "NewJoin";
		#endregion
		#endregion
	}
}
