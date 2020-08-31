﻿using System;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;

namespace Acuminator.Tests.Tests.StaticAnalysis.DacReferentialIntegrity.Sources
{
	[PXCacheName("INUnit")]
	public partial class INUnitMultipleUniqueKeysNoPKNoSuitableUK : IBqlTable  //There is a suggestion to add PK because there is no UK suitable to be a PK
	{
		public static class UK
		{
			public class ByGlobal : PrimaryKeyOf<INUnitMultipleUniqueKeysNoPKNoSuitableUK>.By<unitType, fromUnit, toUnit>
			{
				public static INUnitMultipleUniqueKeysNoPKNoSuitableUK Find(PXGraph graph, string fromUnit, string toUnit) => FindBy(graph, INUnitType.Global, fromUnit, toUnit);			
			}

			public class ByInventory : PrimaryKeyOf<INUnitMultipleUniqueKeysNoPKNoSuitableUK>.By<unitType, inventoryID, fromUnit>
			{
				public static INUnitMultipleUniqueKeysNoPKNoSuitableUK Find(PXGraph graph, int? inventoryID, string fromUnit) => FindBy(graph, INUnitType.InventoryItem, inventoryID, fromUnit);		
			}

			public class ByItemClass : PrimaryKeyOf<INUnitMultipleUniqueKeysNoPKNoSuitableUK>.By<unitType, itemClassID, fromUnit>
			{
				public static INUnitMultipleUniqueKeysNoPKNoSuitableUK Find(PXGraph graph, int? itemClassID, string fromUnit) => FindBy(graph, INUnitType.ItemClass, itemClassID, fromUnit);
			}
		}

		public static class FK
		{
			public class ItemClass : INItemClass.PK.ForeignKeyOf<INUnitMultipleUniqueKeysNoPKNoSuitableUK>.By<itemClassID> { }

			public class Inventory : InventoryItem.PK.ForeignKeyOf<INUnitMultipleUniqueKeysNoPKNoSuitableUK>.By<inventoryID> { }
		}

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlLong.Field<recordID> { }

		[PXDBLongIdentity]
		public virtual long? RecordID { get; set; }
		#endregion

		#region UnitType
		public abstract class unitType : PX.Data.BQL.BqlShort.Field<unitType> { }

		[PXDBShort(IsKey = true)]
		[PXDefault((short)3)]
		[PXUIField(DisplayName = "Unit Type", Visibility = PXUIVisibility.Invisible, Visible = false)]
		[PXIntList(new int[] { 1, 2, 3 }, new string[] { "Inventory Item", "Item Class", "Global" })]
		public virtual short? UnitType { get; set; }
		#endregion

		#region FromUnit
		public abstract class fromUnit : PX.Data.BQL.BqlString.Field<fromUnit> { }

		[PXDefault]
		[INUnit(IsKey = true, DisplayName = "From Unit", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual string FromUnit { get; set; }
		#endregion

		#region ToUnit
		public abstract class toUnit : PX.Data.BQL.BqlString.Field<toUnit> { }

		[PXDefault]
		[INUnit(IsKey = true, DisplayName = "To Unit", Visibility = PXUIVisibility.Visible)]
		public virtual string ToUnit { get; set; }
		#endregion

		#region InventoryID
		public abstract class inventoryID : PX.Data.BQL.BqlInt.Field<inventoryID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Inventory ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual int? InventoryID { get; set; }
		#endregion

		#region ItemClassID
		public abstract class itemClassID : PX.Data.BQL.BqlInt.Field<itemClassID> { }

		[PXDBInt(IsKey = true)]
		[PXDefault(0)]
		[PXUIField(DisplayName = "Item Class ID", Visibility = PXUIVisibility.Invisible, Visible = false)]
		public virtual int? ItemClassID { get; set; }
		#endregion
	}
}
