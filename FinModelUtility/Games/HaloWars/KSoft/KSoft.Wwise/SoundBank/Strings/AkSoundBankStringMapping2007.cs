#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankStringMapping2007
		: AkSoundBankStringMappingBase
	{
		public struct StringHashEntry
			: IO.IEndianStreamSerializable
		{
			public uint offset; // if this is -1, we have to skip it. TODO: serialize entries into memory stream?
			public uint key, id;

			public string value;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.offset);
			}
			#endregion
		};
		public sealed class StringGroup
			: IO.IEndianStreamSerializable
		{
			AkbkHashHeader mHeader_;
			public StringHashEntry[] entries;

			public uint Id { get { return this.mHeader_.Hash; } }

			#region IEndianStreamSerializable Members
			void SerializeGroupEntries(IO.EndianStream s)
			{
				s.StreamArrayInt32(ref this.entries);
				for (int x = 0; x < this.entries.Length; x++)
				{
					s.Stream(ref this.entries[x].id);
					s.Stream(ref this.entries[x].value, Memory.Strings.StringStorage.CStringAscii);
					s.Stream(ref this.entries[x].key);
				}
			}
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.mHeader_);

				long eos = EndOfStream(s, this.mHeader_);
				this.SerializeGroupEntries(s);
				Contract.Assert(s.BaseStream.Position == eos);
			}
			#endregion
		};

		public StringHashEntry[] events;

		static void SerializeEntries(IO.EndianStream s, ref StringHashEntry[] entries)
		{
			s.StreamArrayInt32(ref entries);
			for (int x = 0; x < entries.Length; x++)
			{
				s.Stream(ref entries[x].value, Memory.Strings.StringStorage.CStringAscii);
				s.Stream(ref entries[x].key);
			}
		}

	void SerializeStringType(IO.EndianStream s, AkbkHashHeader hdr, AkSoundBank bank)
		{
			switch (hdr.Type)
			{
				case StringType.OLD_EVENTS:
					SerializeEntries(s, ref this.events);
					break;
			}
		}

		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			Contract.Assert(s.IsReading);

			var bank = s.Owner as AkSoundBank;

			long eos = EndOfStream(s, header);

			while (s.BaseStream.Position != eos)
			{
				AkbkHashHeader hdr = new AkbkHashHeader();
				hdr.Serialize(s);

				this.SerializeStringType(s, hdr, bank);
			}
		}
	};
}