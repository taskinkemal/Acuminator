using System.Collections;
using PX.Data;
using PX.Data.BQL;

namespace Acuminator.Tests.Tests.StaticAnalysis.InvalidPXActionSignature.Sources
{
	// Acuminator disable once PX1069 MissingMandatoryDacFields [Justification]
	[PXHidden]
	public class SOOrder : PXBqlTable, IBqlTable
	{
		public abstract class orderType : BqlString.Field<orderType> { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		public string? OrderType { get; set; }

		public abstract class orderNbr : BqlString.Field<orderNbr> { }

		[PXDBString(IsKey = true, InputMask = "")]
		[PXDefault]
		public string? OrderNbr { get; set; }
	}

	public class SOEntry : PXGraph<SOEntry>
	{
		public PXSelect<SOOrder> Documents = null!;
	}

	// Acuminator disable once PX1016 ExtensionDoesNotDeclareIsActiveMethod extension should be constantly active
	public class SOEntryExt : PXGraphExtension<SOEntry>
	{
		public PXAction<SOOrder> NewOrder = null!;

		[PXUIField(DisplayName = "New Order", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.DataEntry)]
		public void newOrder(PXAdapter adapter)
		{

		}
	}
}
