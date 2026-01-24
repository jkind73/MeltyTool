using System;
using System.Collections.Generic;

namespace KSoft.Wwise.FilePackage
{
	sealed class AkFileLookupTable
		: IO.IEndianStreamSerializable
		, IEnumerable<AkFileLookupTableEntry>
	{
		AkFileLookupTableEntry[] mEntries_;

		internal uint totalSize;

		public int Count { get { return this.mEntries_.Length; } }

		public AkFileLookupTableEntry this[int index]	{ get { return this.mEntries_[index]; } }

		public AkFileLookupTableEntry Find(ulong id)
		{
			return Array.Find(this.mEntries_, e => e.FileId64 == id);
		}

		public uint CalculateTotalSize()
		{
			return sizeof(uint) + (uint)(this.mEntries_.Length * AkFileLookupTableEntry.K_SIZE_OF);
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamArrayInt32(ref this.mEntries_);
		}
		#endregion

		#region IEnumerable<FileEntry> Members
		public IEnumerator<AkFileLookupTableEntry> GetEnumerator()
		{
			return (this.mEntries_ as IEnumerable<AkFileLookupTableEntry>).GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.mEntries_.GetEnumerator();
		}
		#endregion
	};
}