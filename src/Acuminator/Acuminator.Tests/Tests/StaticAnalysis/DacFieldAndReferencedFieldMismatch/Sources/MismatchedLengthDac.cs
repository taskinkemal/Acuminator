using System;
using System.ComponentModel;
using PX.Api;
using PX.Data;

namespace PX.Objects.HackathonDemo.DAC.InconsistentTypesOfDeclaredFieldAndReferencedDacFields
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class StringKeyAttribute : PXDBStringAttribute
	{
		public StringKeyAttribute() : base(25) { }
	}

	[PXDBString(25)]
	[PXString(12)]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class AggregatedInconsistentStringKeyAttribute : PXAggregateAttribute
	{

	}

	[PXDBString(25)]
	[PXString(25)]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class AggregatedLen25StringKeyAttribute : PXAggregateAttribute
	{

	}

	[PXDBString(16)]
	[PXString(16)]
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class AggregatedLen16StringKeyAttribute : PXAggregateAttribute
	{

	}

	[Serializable]
	[PXCacheName("Foreign Keys Container")]
	public class DacWithForeignKeys : IBqlTable
	{
		#region PaymentTermsListID
		/// <summary>
		/// Example initially found by commerce team. Field width should be 25.
		/// </summary>
		[PXDBString(16)]
		[PXUIField(DisplayName = "Payment Terms")]
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
		[PXUIField(DisplayName = "Payment Terms Altn")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID2 { get; set; }
		///<inheritdoc cref="PaymentTermsListID2" />
		public abstract class paymentTermsListID2 : PX.Data.BQL.BqlString.Field<paymentTermsListID2> { }
		#endregion

		#region PaymentTermsListID4
		/// <summary>
		/// Derived type with correct length, should trigger an error - analysis is complicated
		/// </summary>
		[StringKey]
		[PXUIField(DisplayName = "Payment Terms Str Key")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		[PXDefault(PersistingCheck = PXPersistingCheck.Nothing)]
		public virtual string PaymentTermsListID4 { get; set; }
		///<inheritdoc cref="PaymentTermsListID4" />
		public abstract class paymentTermsListID4 : PX.Data.BQL.BqlString.Field<paymentTermsListID4> { }
		#endregion

		#region ConnectViaSearch
		public abstract class connectViaSearch : BqlString.Field<connectViaSearch> { }

		[PXDBString(16)]
		[PXUIField(DisplayName = "Connect Via Search")]
		[PXSelector(typeof(Search<SYSubstitution.substitutionID>))]
		public virtual string ConnectViaSearch { get; set; }
		#endregion

		#region Aggregated Inconsistent

		// length mismatch on aggregated attribute, should skip
		[AggregatedInconsistentStringKeyAttribute]
		[PXUIField(DisplayName = "Aggregated Inconsistent")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		public virtual string AggregatedInconsistent { get; set; }

		public abstract class aggregatedInconsistent : BqlString.Field<aggregatedInconsistent> { }

		#endregion

		#region Aggregated

		// this should trigger the diagnostic, as all data types have same incorrect length
		[AggregatedLen16StringKey]
		[PXUIField(DisplayName = "Aggregated 16")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		public virtual string Aggregated16 { get; set; }

		public abstract class aggregated16 : BqlString.Field<aggregated16> { }

		#endregion

		#region AggregatedOk

		// this should not trigger the diagnostic, as all data types have correct length
		[AggregatedLen25StringKey]
		[PXUIField(DisplayName = "Aggregated Ok")]
		[PXSelector(typeof(SYSubstitution.substitutionID))]
		public virtual string AggregatedOk { get; set; }

		public abstract class aggregatedOk : BqlString.Field<aggregatedOk> { }

		#endregion
	}
}
