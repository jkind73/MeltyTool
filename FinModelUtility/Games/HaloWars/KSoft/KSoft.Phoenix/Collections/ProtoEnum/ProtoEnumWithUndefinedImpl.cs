using System.Collections.ObjectModel;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using PhxUtil = Phoenix.PhxUtil;

	internal sealed class ProtoEnumWithUndefinedImpl
		: IProtoEnumWithUndefined
	{
		IProtoEnum mRoot_;
		ObservableCollection<string> mUndefined_;

		public ProtoEnumWithUndefinedImpl(IProtoEnum root)
		{
			Contract.Requires(root != null);

			this.mRoot_ = root;
		}

		void InitializeUndefined()
		{
			if (this.mUndefined_ == null)
				this.mUndefined_ = [];
		}

		public void Clear()
		{
			if (this.mUndefined_ != null)
				this.mUndefined_.Clear();
		}

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)		{ return this.mRoot_.TryGetMemberId(memberName); }
		public string TryGetMemberName(int memberId)		{ return this.mRoot_.TryGetMemberName(memberId); }
		public bool IsValidMemberId(int memberId)			{ return this.mRoot_.IsValidMemberId(memberId); }
		public bool IsValidMemberName(string memberName)	{ return this.mRoot_.IsValidMemberName(memberName); }
		public int GetMemberId(string memberName)			{ return this.mRoot_.GetMemberId(memberName); }
		public string GetMemberName(int memberId)			{ return this.mRoot_.GetMemberName(memberId); }
		public int MemberCount						{ get	{ return this.mRoot_.MemberCount; } }
		#endregion

		#region IProtoEnumWithUndefined Members
		public int TryGetMemberIdOrUndefined(string memberName)
		{
			int id = this.TryGetMemberId(memberName);

			if (id.IsNone() && this.MemberUndefinedCount != 0)
			{
				id = this.mUndefined_.FindIndex(str => PhxUtil.StrEqualsIgnoreCase(str, memberName));
				if (id.IsNotNone())
					id = PhxUtil.GetUndefinedReferenceHandle(id);
			}

			return id;
		}

		public int GetMemberIdOrUndefined(string memberName)
		{
			int id = this.TryGetMemberIdOrUndefined(memberName);

			if (id.IsNone())
			{
				this.InitializeUndefined();

				id = this.mUndefined_.Count;
				this.mUndefined_.Add(memberName);
				id = PhxUtil.GetUndefinedReferenceHandle(id);
			}

			return id;
		}

		public string GetMemberNameOrUndefined(int memberId)
		{
			string name;

			if (PhxUtil.IsUndefinedReferenceHandle(memberId))
			{
				Contract.Assert(this.mUndefined_ != null);
				name = this.mUndefined_[PhxUtil.GetUndefinedReferenceDataIndex(memberId)];
			}
			else
			{
				name = this.GetMemberName(memberId);
			}

			return name;
		}

		public int MemberUndefinedCount { get {
			return this.mUndefined_ != null
				? this.mUndefined_.Count
				: 0;
		} }

		public ObservableCollection<string> UndefinedMembers { get { return this.mUndefined_; } }
		#endregion
	};
}
