using System;

namespace KSoft.Collections
{
	using PhxUtil = Phoenix.PhxUtil;

	public sealed class CodeEnum<TEnum>
		: IProtoEnum
		where TEnum : struct
	{
		static readonly string[] KNames;
		static readonly string KUnregisteredMessage;

		static CodeEnum()
		{
			var enumType = typeof(TEnum);

			KNames = Enum.GetNames(enumType);

			KUnregisteredMessage = string.Format("Unregistered {0}!", enumType.Name);
		}

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)
		{
			return Array.FindIndex(KNames, n => PhxUtil.StrEqualsIgnoreCase(n, memberName));
		}
		public string TryGetMemberName(int memberId)
		{
			return this.IsValidMemberId(memberId)
				? this.GetMemberName(memberId)
				: null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < KNames.Length;
		}
		public bool IsValidMemberName(string memberName)
		{
			int index = this.TryGetMemberId(memberName);

			return index.IsNotNone();
		}

		public int GetMemberId(string memberName)
		{
			int index = this.TryGetMemberId(memberName);

			if (index.IsNone())
				throw new ArgumentException(KUnregisteredMessage, memberName);

			return index;
		}
		public string GetMemberName(int memberId)
		{
			return KNames[memberId];
		}

		public int MemberCount { get { return KNames.Length; } }
		#endregion
	};
}