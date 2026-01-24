using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	public sealed class BTypeNamesWithCode
		: BTypeNames
	{
		IProtoEnum mCodeTypes_;

		public BTypeNamesWithCode(IProtoEnum codeTypes)
		{
			Contract.Requires<ArgumentNullException>(codeTypes != null);

			this.mCodeTypes_ = codeTypes;
		}

		#region IProtoEnum Members
		public override int TryGetMemberId(string memberName)
		{
			int idx = base.TryGetMemberId(memberName);

			if (idx.IsNone())
			{
				idx = this.mCodeTypes_.TryGetMemberId(memberName);
				if (idx.IsNotNone())
					idx += this.Count;
			}

			return idx;
		}
		public override string TryGetMemberName(int memberId)
		{
			string name = base.TryGetMemberName(memberId);

			if (name == null)
				return this.mCodeTypes_.TryGetMemberName(memberId);

			return name;
		}

		public override string GetMemberName(int memberId)
		{
			if (memberId < this.Count)
				return base.GetMemberName(memberId);

			memberId -= this.Count;
			return this.mCodeTypes_.GetMemberName(memberId);
		}

		public override int MemberCount { get {
			return this.Count + this.mCodeTypes_.MemberCount;
		} }
		#endregion
	};
}