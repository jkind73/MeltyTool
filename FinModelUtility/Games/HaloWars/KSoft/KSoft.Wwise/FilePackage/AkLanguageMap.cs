namespace KSoft.Wwise.FilePackage
{
	public sealed class AkLanguageMap
		: IO.IEndianStreamSerializable
	{
	AkLanguageMapEntry[] mEntries_;

		bool mUseAsciiStrings_;

		public AkLanguageMap(bool useAsciiStrings)
		{
			this.mUseAsciiStrings_ = useAsciiStrings;
		}

		internal uint totalMapSize;

		public uint CalculateTotalMapSize()
		{
			uint charSize = (uint)(this.mUseAsciiStrings_ ? sizeof(byte) : sizeof(short));

			uint result = sizeof(uint) + (uint)(this.mEntries_.Length * AkLanguageMapEntry.K_SIZE_OF);
			foreach (var se in this.mEntries_)
				result += (uint)(se.value.Length + 1) * charSize;

			return result;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			Memory.Strings.StringStorage ss;
			if (this.mUseAsciiStrings_)
				ss = Memory.Strings.StringStorage.CStringAscii;
			else
			{
				ss = s.ByteOrder == Shell.EndianFormat.LITTLE
					? Memory.Strings.StringStorage.CStringUnicode
					: Memory.Strings.StringStorage.CStringUnicodeBigEndian;
			}

			s.StreamArrayInt32(ref this.mEntries_);
			for (int x = 0; x < this.mEntries_.Length; x++)
				s.Stream(ref this.mEntries_[x].value, ss);
		}
		#endregion
	};
}
