using System;
using System.Collections.Generic;

namespace KSoft.Collections
{
	using PhxUtil = Phoenix.PhxUtil;

	public sealed class BListAutoIdParams
		: BListParams;

	public sealed class BListAutoId<T>
		: BListBase<T>
		, IBTypeNames
		// For now, I don't see a reason to support struct types in AutoIds
		// If structs are needed, the streaming logic will need to be adjusted
		where T : class, IListAutoIdObject, new()
	{
		readonly string kUnregisteredMessage_;

		static string BuildUnRegisteredMsg()
		{
			return string.Format("Unregistered {0}!", typeof(T).Name);
		}
		public BListAutoId(BListAutoIdParams @params = null) : base(@params)
		{
			this.kUnregisteredMessage_ = BuildUnRegisteredMsg();
			this.mUndefinedInterface_ = new ProtoEnumWithUndefinedImpl(this);
		}

		public override void Clear()
		{
			base.Clear();

			if (this.mDbi_ != null)
				this.mDbi_.Clear();

			if (this.mUndefinedInterface_ != null)
				this.mUndefinedInterface_.Clear();
		}

		#region Database interfaces
		/// <remarks>Mainly a hack for adding new items dynamically</remarks>
		void PreAdd(T item, string itemName, int id = TypeExtensions.K_NONE)
		{
			item.AutoId = id.IsNotNone()
				? id
				: this.Count;

			if (itemName != null)
				item.Data = itemName;
		}
		internal int DynamicAdd(T item, string itemName, int id = TypeExtensions.K_NONE)
		{
			this.PreAdd(item, itemName, id);
			if (this.mDbi_ != null)
			{
				if (this.mDbi_.ContainsKey(itemName))
				{
					throw new ArgumentException(string.Format(
						"There is already a {0} named {1}",
						typeof(T).Name, itemName
						), nameof(itemName));
				}

				this.mDbi_.Add(item.Data, item);

				if (this.Params != null && this.Params.ToLowerDataNames)
				{
					string lowerName = PhxUtil.ToLowerIfContainsUppercase(item.Data);
					if (!ReferenceEquals(lowerName, item.Data))
						this.mDbi_.Add(lowerName, item);
				}
			}
			this.AddItem(item);

			return item.AutoId;
		}

		Dictionary<string, T> mDbi_;
		internal void SetupDatabaseInterface()
		{
			this.mDbi_ = new Dictionary<string, T>(this.Params != null ? this.Params.initialCapacity : BCollectionParams.K_DEFAULT_CAPACITY);
		}

		internal int TryGetId(string name)
		{
			int id = TypeExtensions.K_NONE;
			if (this.mDbi_ == null)
				return id;

			T obj;
			if (this.mDbi_.TryGetValue(name, out obj))
				id = obj.AutoId;

			return id;
		}
		#endregion

		#region IProtoEnum Members
		public int TryGetMemberId(string memberName)
		{
			return this.mList.FindIndex(n => PhxUtil.StrEqualsIgnoreCase(n.Data, memberName));
		}
		public string TryGetMemberName(int memberId)
		{
			return this.IsValidMemberId(memberId) ? this.GetMemberName(memberId) : null;
		}
		public bool IsValidMemberId(int memberId)
		{
			return memberId >= 0 && memberId < this.Count;
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
				throw new ArgumentException(this.kUnregisteredMessage_, memberName);

			return index;
		}
		public string GetMemberName(int memberId)
		{
			return this[memberId].Data;
		}

		public int MemberCount { get { return this.Count; } }
		#endregion

		public override object GetObject(int id)
		{
			if (id.IsNone())
				return null;

			if (PhxUtil.IsUndefinedReferenceHandle(id))
				return Phoenix.TypeExtensionsPhx.GetUndefinedObject(this.mUndefinedInterface_, id);

			return base.GetObject(id);
		}

		private ProtoEnumWithUndefinedImpl mUndefinedInterface_;
		IProtoEnumWithUndefined IHasUndefinedProtoMemberInterface.UndefinedInterface { get { return this.mUndefinedInterface_; } }
		internal IProtoEnumWithUndefined UndefinedInterface { get { return this.mUndefinedInterface_; } }
	};
}

namespace KSoft.Phoenix
{
	partial class TypeExtensionsPhx
	{
		internal static string TryGetName<T>(this Collections.BListAutoId<T> dbi, int id)
			where T : class, Collections.IListAutoIdObject, new()
		{
			if (dbi == null)
				return null;

			if (id >= 0 && id < dbi.Count)
				return dbi[id].Data;

			return null;
		}
	};
}
