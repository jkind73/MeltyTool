using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	public sealed class LocStringTableIndexRange
	{
		public LocStringTableIndexRange PrevRange { get; private set; }
		public LocStringTableIndexRange NextRange { get; private set; }
		public LocStringTableIndexRange Parent { get; private set; }
		public List<LocStringTableIndexRange> SubRanges { get; private set; }
		internal int Depth { get; private set; }
		public int StartIndex { get; set; }
		public int Count { get; set; }
		public string ReservedFor { get; private set; }

		public int EndIndex => (this.StartIndex + this.Count) - 1;

		public override string ToString()
		{
			return string.Format("{0}-{1} {2}",
			                     this.StartIndex,
			                     this.EndIndex,
			                     this.ReservedFor);
		}

		public LocStringTableIndexRange(int count, string reservedFor)
			: this(null, count, reservedFor)
		{
		}

		public LocStringTableIndexRange(LocStringTableIndexRange prev, int count, string reservedFor)
		{
			if (prev != null)
			{
				this.PrevRange = prev;
				this.Parent = prev.Parent;
				this.Depth = prev.Depth;
				this.StartIndex = prev.Count;
			}
			else
			{
				this.StartIndex = 0;
			}

			this.Count = count;
			this.ReservedFor = reservedFor;

			if (this.Parent != null)
				this.Parent.SubRanges.Add(this);
		}

		internal LocStringTableIndexRange MakeNextRange(int count, string reservedFor)
		{
			var next = new LocStringTableIndexRange(this, count, reservedFor);
			this.NextRange = next;

			return next;
		}
		internal LocStringTableIndexRange MakeNextRange(int absoluteStartIndex, int absoluteEndIndex, string reservedFor)
		{
			Contract.Requires(absoluteEndIndex > absoluteStartIndex);

			int expectedStartIndex = this.StartIndex + this.Count;
			if (expectedStartIndex != absoluteStartIndex)
				throw new ArgumentException(absoluteStartIndex + " != " + expectedStartIndex, nameof(absoluteStartIndex));

			int count = (absoluteEndIndex+1) - absoluteStartIndex;
			var next = this.MakeNextRange(count, reservedFor);
			next.StartIndex = absoluteStartIndex;

			return next;
		}

		internal LocStringTableIndexRange StartSubRange(int count, string reservedFor)
		{
			var firstChild = new LocStringTableIndexRange(count, reservedFor);
			firstChild.Parent = this;
			firstChild.Depth = this.Depth + 1;
			firstChild.StartIndex = this.StartIndex;

			this.SubRanges = [
				firstChild
			];

			return firstChild;
		}

		internal LocStringTableIndexRange EndSubRange()
		{
			return this.Parent;
		}
	};

	public sealed class LocStringTable
		: ObservableCollection<LocString>
		, IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("StringTable",
			XML.BCollectionXmlParamsFlags.REQUIRES_DATA_NAME_PRELOADING);
		public static readonly Engine.XmlFileInfo KXmlFileInfoEnglish = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "StringTable-en.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfoEnglish = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.LISTS,
			KXmlFileInfoEnglish);
		#endregion

		#region IndexRanges
		private static LocStringTableIndexRange gIndexRanges_;
		public static LocStringTableIndexRange IndexRanges { get {
			if (gIndexRanges_ == null)
			{
				gIndexRanges_ = new LocStringTableIndexRange(1000, "code");
				gIndexRanges_
						.StartSubRange(100, "unused1")
						.MakeNextRange(100, 229, "UI help panel")
						.MakeNextRange(230, 349, "Unit prereqs")
						.MakeNextRange(350, 399, "Circle menu text")
						.MakeNextRange(400, 449, "Unit stats")
						.MakeNextRange(450, 499, "unused2")
						.MakeNextRange(500, 549, "Unit roles")
						.MakeNextRange(550, 999, "unused3")
						.EndSubRange()
					.MakeNextRange(1000, 2999, "technology names and descriptions")
					.MakeNextRange(3000, 4999, "squad names and descriptions")
					.MakeNextRange(5000, 5999, "powers")
					.MakeNextRange(6000, 6999, "abilities")
					.MakeNextRange(7000, 7999, "leaders")
					.MakeNextRange(8000, 8999, "game modes")
						.StartSubRange(100, "skirmish")
						.EndSubRange()
					.MakeNextRange(9000, 9999, "nothing1")
					.MakeNextRange(10000, 19999, "object names and descriptions")
					.MakeNextRange(20000, 29999, "UI")
						.StartSubRange(2000, "UI1")
						.MakeNextRange(22000, 22199, "Player Notifications")
						.MakeNextRange(22200, 29999, "UI2")
						.EndSubRange()
					.MakeNextRange(30000, 39999, "campaign")
					.MakeNextRange(40000, 49999, "cinematics")
					.MakeNextRange(50000, 59999, "nothing2")
					// not actually a reserved range, but I'm just padding to ushort.MaxRange
					.MakeNextRange(60000, 65345-1, "nothing3")
					;
			}
			return gIndexRanges_;
		} }

		public static LocStringTableIndexRange FindRangeDefinition(int index)
		{
			Contract.Requires(index >= 0);

			LocStringTableIndexRange foundRange = null;

			for (var range = IndexRanges; range != null; )
			{
				if (index >= range.StartIndex && index <= range.EndIndex)
				{
					foundRange = range;

					if (range.SubRanges.IsNotNullOrEmpty())
					{
						range = range.SubRanges[0];
						continue;
					}

					break;
				}

				range = range.NextRange;
			}

			return foundRange;
		}
		#endregion

		string mLanguage_;
		public string Language
		{
			get { return this.mLanguage_; }
			set { this.mLanguage_ = value; }
		}

		bool mDoNotUpdateUsedIndices_;
		public Collections.BitSet UsedIndices { get; private set; }
			= [];

		public LocStringTable()
		{
			this.CollectionChanged += this.OnStringTableChanged;
		}

		protected override void InsertItem(int index, LocString item)
		{
			if (item == null)
				return;

			if (item.Id.IsNone())
				throw new ArgumentOutOfRangeException(nameof(item));

			index = this.FindInsertIndex(item);

			base.InsertItem(index, item);
		}

		private int FindInsertIndex(LocString item)
		{
			int id = item.Id;
			int insertIndex = 0;

			foreach (int bitIndex in this.UsedIndices.SetBitIndices)
			{
				if (bitIndex > id)
					break;
				if (bitIndex == id)
				{
					var existingItem = this[insertIndex + 1];
					throw new InvalidOperationException(string.Format(
						"Can't insert {0} as there is already a string with that ID: {1}",
						item, existingItem));
				}

				insertIndex++;
			}

			return insertIndex;
		}

		#region UsedIndices updating
		private void OnStringTableChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (this.mDoNotUpdateUsedIndices_)
				return;

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					this.RefreshUsedIndices(e.NewItems, state: true);
					break;

				case NotifyCollectionChangedAction.Remove:
					this.RefreshUsedIndices(e.OldItems, state: false);
					break;

				case NotifyCollectionChangedAction.Replace:
					this.RefreshUsedIndicesForReplace(e.NewItems, e.OldItems);
					break;

				case NotifyCollectionChangedAction.Reset:
					this.RefreshUsedIndices(this.Items, clearFirst: true);
					break;

				default:
					throw new NotImplementedException(e.Action.ToString());
			}
		}

		private void RefreshUsedIndices(IEnumerable list, bool state = true, bool clearFirst = false)
		{
			if (list == null)
				throw new ArgumentNullException(nameof(list));

			this.mDoNotUpdateUsedIndices_ = true;
			if (clearFirst)
				this.UsedIndices.Clear();
			foreach (LocString str in list)
			{
				int id = str.Id;
				if (id < this.UsedIndices.Length)
				{
					if (this.UsedIndices[id] == state)
						throw new ArgumentException(string.Format(
							"LocString #{0} '{1}' is already {2}",
							str.Id, str.Text,
							state ? "set" : "unset"));
				}
				else if (id >= this.UsedIndices.Length)
				{
					this.UsedIndices.Length = id + 1;
				}

				this.UsedIndices[id] = state;
			}

			this.mDoNotUpdateUsedIndices_ = false;
		}
		private void RefreshUsedIndicesForReplace(IList newList, IList oldList)
		{
			Contract.Requires<ArgumentNullException>(newList != null && oldList != null);
			Contract.Requires<ArgumentOutOfRangeException>(newList.Count == oldList.Count);

			this.mDoNotUpdateUsedIndices_ = true;
			for (int x = 0; x < newList.Count; x++)
			{
				var newItem = (LocString)newList[x];
				var oldItem = (LocString)oldList[x];

				if (newItem == oldItem)
				{
				}
				else if (newItem != null && oldItem != null)
				{
					if (newItem.Id != oldItem.Id)
						throw new InvalidOperationException(string.Format(
							"ID mismatch: {0} != {1}",
							newItem, oldItem));
				}
				else if (newItem != null)
				{
					this.UsedIndices[newItem.Id] = true;
				}
				else if (oldItem != null)
				{
					this.UsedIndices[oldItem.Id] = false;
				}
			}

			this.mDoNotUpdateUsedIndices_ = false;
		}
		#endregion

		public int NextFreeId(LocStringTableIndexRange range)
		{
			Contract.Requires(range != null);

			if (this.UsedIndices.Length == 0)
				return range.StartIndex;

			int maxIndex = this.UsedIndices.Length - 1;

			if (range.StartIndex > maxIndex)
				return range.StartIndex;

			for (int clearBitIndex = range.StartIndex; (clearBitIndex = this.UsedIndices.NextClearBitIndex(clearBitIndex)) > 0; )
			{
				if (clearBitIndex > range.EndIndex)
					break;

				return clearBitIndex;
			}

			if (range.EndIndex > maxIndex)
				return maxIndex + 1;

			return TypeExtensions.K_NONE;
		}

		public int GetLocStringIndex(int id)
		{
			if (id < 0)
				return TypeExtensions.K_NONE;

			int index = TypeExtensions.K_NONE;
			foreach (int bitIndex in this.UsedIndices.SetBitIndices)
			{
				index++;

				if (bitIndex == id)
					break;
				if (bitIndex > id)
					index = TypeExtensions.K_NONE;
			}

			return index;
		}

		public sealed class RangeStatsData
		{
			public LocStringTableIndexRange Range { get; set; }
			public int UsedCount { get; set; }
			public int FreeCount { get { return this.Range.Count - this.UsedCount; } }

			public override string ToString()
			{
				if (this.Range == null)
					return base.ToString();

				return string.Format("{0}Total={1},Used={2},Free={3}, {4}",
					new string('\t', this.Range.Depth),
					this.Range.Count,
					this.UsedCount,
					this.FreeCount,
					this.Range.ReservedFor);
			}
		};
		public Dictionary<LocStringTableIndexRange, RangeStatsData> RangeStats { get {
			var result = new Dictionary<LocStringTableIndexRange, RangeStatsData>();

			for (var range = IndexRanges; range != null; )
			{
				int count = this.CountNumberUsed(range);
				var data = new RangeStatsData()
				{
					Range = range,
					UsedCount = count,
				};
				result.Add(data.Range, data);

				if (range.SubRanges.IsNotNullOrEmpty())
				{
					range = range.SubRanges[0];
					continue;
				}

				range = range.NextRange ?? range.Parent?.NextRange;
			}

			return result;
		} }

		private int CountNumberUsed(LocStringTableIndexRange range)
		{
			Contract.Requires(range != null);

			int usedCount = 0;
			if (range.StartIndex >= this.UsedIndices.Length)
				return usedCount;

			foreach (int bitIndex in this.UsedIndices.SetBitIndicesStartingAt(range.StartIndex))
			{
				if (bitIndex > range.EndIndex)
					break;

				usedCount++;
			}

			return usedCount;
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			using (s.EnterCursorBookmark("Language"))
			{
				s.StreamAttribute("name", ref this.mLanguage_);

				if (s.IsReading)
				{
					var tempList = new List<LocString>();
					bool isSorted = true;
					LocString prevStr = null;
					foreach (var n in s.ElementsByName("String"))
					{
						using (s.EnterCursorBookmark(n))
						{
							var str = new LocString();
							str.Serialize(s);
							tempList.Add(str);

							if (isSorted && prevStr != null && prevStr.Id >= str.Id)
								isSorted = false;

							prevStr = str;
						}
					}

					if (!isSorted)
						tempList.Sort((x, y) => x.Id.CompareTo(y.Id));

					int lastId = tempList[tempList.Count - 1].Id;
					this.UsedIndices.Length = lastId + 1;

					this.mDoNotUpdateUsedIndices_ = true;
					foreach (var str in tempList)
					{
						int id = str.Id;
						if (this.UsedIndices[id])
							s.ThrowReadException(new System.IO.InvalidDataException(string.Format(
								"Duplicate LocString: #{0} '{1}'",
								str.Id, str.Text)));

						this.UsedIndices[id] = true;
					}
					this.AddRange(tempList);
					this.mDoNotUpdateUsedIndices_ = false;
				}
				else if (s.IsWriting)
				{
					s.WriteStreamableElements("String", this);
				}
			}
		}
		#endregion
	};
}