using System;
using PX.Api;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.HackathonDemo.DAC.InconsistentTypesOfDeclaredFieldAndReferencedDacFields
{
	[Serializable]
	[PXCacheName("Foreign Keys Container")]
	public class DacWithForeignKeys : IBqlTable
	{
		#region PaymentTermsListID3
		/// <summary>
		/// Totally different type, should trigger an error
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Payment Terms Int")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? PaymentTermsListID3 { get; set; }
		///<inheritdoc cref="PaymentTermsListID3" />
		public abstract class paymentTermsListID3 : PX.Data.BQL.BqlInt.Field<paymentTermsListID3> { }
		#endregion

		#region ConnectViaSearchWithFilter
		public abstract class connectViaSearchWithFilter : BqlString.Field<connectViaSearchWithFilter> { }

		[PXInt]
		[PXUIField(DisplayName = "Connect Via Search With Filter")]
		[PXSelector(typeof(Search<SYSubstitution.substitutionID, Where<SYSubstitution.fieldName, NotEqual<Null>>>))]
		public virtual int? ConnectViaSearchWithFilter { get; set; }
		#endregion

	}
}
