using System;
using PX.Api;
using PX.Data;
using PX.Data.BQL;

namespace PX.Objects.HackathonDemo.DAC.InconsistentTypesOfDeclaredFieldAndReferencedDacFields
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class StringKeyAttribute : PXDBStringAttribute
	{
		public StringKeyAttribute() : base(25) { }
	}

	[Serializable]
	[PXCacheName("Foreign Keys Container")]
	public class DacWithPropertiesHavingForeignKeysAndTypeNotMatchingOneOfForeignProperty : IBqlTable
	{
		#region PaymentTermsListID
		/// <summary>
		/// Example initially found by commerce team. Field width should be 25.
		/// </summary>
		[PXDBString(1)]
		[PXUIField(DisplayName = "Payment Terms V")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID { get; set; }
		///<inheritdoc cref="PaymentTermsListID" />
		public abstract class paymentTermsListID : PX.Data.BQL.BqlString.Field<paymentTermsListID> { }
		#endregion

		#region PaymentTermsListID2
		/// <summary>
		/// Same as previous example, but with PXString. Should also trigger an error.
		/// </summary>
		[PXString(16)]
		[PXUIField(DisplayName = "Payment Terms Alt")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID2 { get; set; }
		///<inheritdoc cref="PaymentTermsListID" />
		public abstract class paymentTermsListID2 : PX.Data.BQL.BqlString.Field<paymentTermsListID2> { }
		#endregion

		#region PaymentTermsListID3
		/// <summary>
		/// Totally different type, should trigger an error
		/// </summary>
		[PXDBInt]
		[PXUIField(DisplayName = "Payment Terms Int")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual int? PaymentTermsListID3 { get; set; }
		///<inheritdoc cref="PaymentTermsListID" />
		public abstract class paymentTermsListID3 : PX.Data.BQL.BqlInt.Field<paymentTermsListID3> { }
		#endregion

		#region PaymentTermsListID4
		/// <summary>
		/// Derived type with correct length, triggers an error because of complexity of constructor analysis
		/// </summary>
		[StringKey]
		[PXUIField(DisplayName = "Payment Terms Str Key")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID4 { get; set; }
		///<inheritdoc cref="PaymentTermsListID" />
		public abstract class paymentTermsListID4 : PX.Data.BQL.BqlString.Field<paymentTermsListID4> { }
		#endregion

		#region ConnectViaSearch
		public abstract class connectViaSearch : BqlString.Field<connectViaSearch> { }

		[PXDBString(16)]
		[PXUIField(DisplayName = "Connect Via Search")]
		[PXSelector(typeof(Search<SYSubstitution.substitutionID>))]
		public virtual string ConnectViaSearch { get; set; }
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
