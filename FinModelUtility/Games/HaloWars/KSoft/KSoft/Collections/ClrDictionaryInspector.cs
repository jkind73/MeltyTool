using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Reflect = System.Reflection;

namespace KSoft.Collections
{
	public sealed class ClrDictionaryInspector<TKey, TValue>
	{
		#region Dictionary field names
		const string K_DIC_BUCKETS_NAME_ = "buckets";
		const string K_DIC_ENTRIES_NAME_ = "entries";
		const string K_DIC_COUNT_NAME_ = "count";
		const string K_DIC_VERSION_NAME_ = "version";
		const string K_DIC_FREE_LIST_NAME_ = "freeList";
		const string K_DIC_FREE_COUNT_NAME_ = "freeCount";
		#endregion
		#region Dictionary.Entry field names
		const string K_ENTRY_TYPE_NAME_ = "Entry";
		const string K_ENTRY_HASH_CODE_NAME_ = "hashCode";
		const string K_ENTRY_NEXT_ENTRY_INDEX_NAME_ = "next";
		const string K_ENTRY_KEY_NAME_ = "key";
		const string K_ENTRY_VALUE_NAME_ = "value";
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct DicEntry
		{
			public int hashCode; // only the lower 31 bits of the actual hash code
			public int nextEntryIndex;
			public TKey key;
			public TValue value;

			public bool IsFree { get => this.hashCode.IsNone(); }
			public bool IsLast { get => this.nextEntryIndex.IsNone(); }

			public DicEntry GetNext(ClrDictionaryInspector<TKey, TValue> inspector)
			{
				Contract.Requires<ArgumentNullException>(inspector != null);
				Contract.Requires<InvalidOperationException>(!this.IsLast);

				return inspector.Entries[this.nextEntryIndex];
			}
		};

		#region Dictionary getters
		static readonly Func<Dictionary<TKey, TValue>, int[]> KGetDicBuckets;
		static readonly Func<Dictionary<TKey, TValue>, Array> KGetDicEntries;
		static readonly Func<Dictionary<TKey, TValue>, int> KGetDicCount;
		static readonly Func<Dictionary<TKey, TValue>, int> KGetDicVersion;
		static readonly Func<Dictionary<TKey, TValue>, int> KGetDicFreeList;
		static readonly Func<Dictionary<TKey, TValue>, int> KGetDicFreeCount;
		#endregion
		#region Dictionary.Entry getters
		static readonly Func<object, int> KGetEntryHashCode;
		static readonly Func<object, int> KGetEntryNextEntryIndex;
		static readonly Func<object, TKey> KGetEntryKey;
		static readonly Func<object, TValue> KGetEntryValue;
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ClrDictionaryInspector()
		{
			// implementations are totally different...
			if (Shell.Platform.IsMonoRuntime)
			{
				Debug.Trace.Collections.TraceDataSansId(System.Diagnostics.TraceEventType.Critical,
					nameof(ClrDictionaryInspector<TKey, TValue>) + " does not support Mono");
			}

			// "If a nested type is generic, this method returns its generic type definition. This is true even if the enclosing generic type is a closed constructed type."
			var dicEntryType = typeof(Dictionary<TKey, TValue>)
				.GetNestedType(K_ENTRY_TYPE_NAME_, Reflect.BindingFlags.NonPublic)
				.MakeGenericType(typeof(TKey), typeof(TValue));
			Contract.Assert(dicEntryType != null);

			#region Dictionary getters
			KGetDicBuckets =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int[]>(K_DIC_BUCKETS_NAME_);
			KGetDicEntries =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, Array>(K_DIC_ENTRIES_NAME_);
			KGetDicCount =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(K_DIC_COUNT_NAME_);
			KGetDicVersion =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(K_DIC_VERSION_NAME_);
			KGetDicFreeList =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(K_DIC_FREE_LIST_NAME_);
			KGetDicFreeCount =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(K_DIC_FREE_COUNT_NAME_);
			#endregion
			#region Dictionary.Entry getters
			KGetEntryHashCode =
				Reflection.Util.GenerateMemberGetter<int>(dicEntryType, K_ENTRY_HASH_CODE_NAME_);
			KGetEntryNextEntryIndex =
				Reflection.Util.GenerateMemberGetter<int>(dicEntryType, K_ENTRY_NEXT_ENTRY_INDEX_NAME_);
			KGetEntryKey =
				Reflection.Util.GenerateMemberGetter<TKey>(dicEntryType, K_ENTRY_KEY_NAME_);
			KGetEntryValue =
				Reflection.Util.GenerateMemberGetter<TValue>(dicEntryType, K_ENTRY_VALUE_NAME_);
			#endregion
		}

		readonly Dictionary<TKey, TValue> mDic_;
		readonly int mExpectedVersion_;
		DicEntry[] mEntries_;

		public ClrDictionaryInspector(Dictionary<TKey, TValue> dic)
		{
			Contract.Requires<ArgumentNullException>(dic != null);

			this.mDic_ = dic;
			this.mExpectedVersion_ = this.Version;
		}

		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()
		{
			Contract.Invariant(this.Version == this.mExpectedVersion_,
				"Tried to inspect a dictionary that has been modified since the inspector was created");
		}

		void InitializeEntries()
		{
			this.mEntries_ = new DicEntry[this.Buckets.Count];
			var array = KGetDicEntries(this.mDic_);

			for (int x = 0; x < array.Length; x++)
			{
				var entry = array.GetValue(x);

				this.mEntries_[x] = new DicEntry()
				{
					hashCode = KGetEntryHashCode(entry),
					nextEntryIndex = KGetEntryNextEntryIndex(entry),
					key = KGetEntryKey(entry),
					value = KGetEntryValue(entry),
				};
			}
		}

		public IReadOnlyList<int> Buckets { get {
			var buckets = KGetDicBuckets(this.mDic_);

			return buckets ?? [];
		} }
		public IReadOnlyList<DicEntry> Entries { get {
			if (this.mEntries_ == null)
				this.InitializeEntries();

			return this.mEntries_;
		} }
		public int Count { get => KGetDicCount(this.mDic_); }
		private int Version { get => KGetDicVersion(this.mDic_); }
		public int FreeList { get => KGetDicFreeList(this.mDic_); }
		public int FreeCount { get => KGetDicFreeCount(this.mDic_); }

		public IEnumerable<int> BucketsInUse { get => this.Buckets.Where(b => b >= 0); }

		public IEnumerable<DicEntry> GetEntriesInBucket(int bucketIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bucketIndex >= 0 && bucketIndex < this.Buckets.Count);

			for (int x = this.Buckets[bucketIndex]; x >= 0; x = this.Entries[x].nextEntryIndex)
				yield return this.Entries[x];
		}

		public IEnumerable<DicEntry> EntryCollisions(TKey key)
		{
			int hashCode = this.mDic_.Comparer.GetHashCode(key) & 0x7FFFFFFF;
			int targetBucket = hashCode % this.Buckets.Count;

#if false // result as entry indices
			for (int x = Buckets[target_bucket]; x >= 0; x = Entries[x].NextEntryIndex)
			{
				if (Entries[x].HashCode == hash_code && mDic.Comparer.Equals(Entries[x].Key, key))
					yield break;

				yield return x;
			}
#endif

			return
				from e in this.GetEntriesInBucket(targetBucket)
				where e.hashCode == hashCode && !this.mDic_.Comparer.Equals(e.key, key)
				select e;
		}
	};
}
