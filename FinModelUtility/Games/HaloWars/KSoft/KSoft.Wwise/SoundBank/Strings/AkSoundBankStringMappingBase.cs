using System.Runtime.InteropServices;

namespace KSoft.Wwise.SoundBank
{
	using StringTypeStreamer = IO.EnumBinaryStreamer<AkSoundBankStringMappingBase.StringType>;

	partial class AkSoundBankObjectBase
	{
		static readonly Values.GroupTagData32 KStringMappingSignature =
					new Values.GroupTagData32("STID", "audiokinetic_string_mapping"); // BankStrMapChunkID

		static readonly AkSoundBankStringMapping KBankNamesMappingObject = new AkSoundBankStringMapping();

		static AkSoundBankObjectBase NewStid(uint generatorVersion)
		{
			if (AkVersion.BankHasOldStid(generatorVersion))
				return new AkSoundBankStringMapping2007();

			return KBankNamesMappingObject;
		}
	};

	abstract class AkSoundBankStringMappingBase
		: AkSoundBankObjectBase
	{
		[System.Reflection.Obfuscation(Exclude=true)]
		internal enum StringType : uint
		{
			NONE,
			BANK,

			OLD_EVENTS = 1,
			OLD2, // states?
			OLD3, // skip
			OLD4,
			OLD5, // switches?
			OLD6, // skip
			OLD7,
			OLD8,
			OLD9,
			OLD10, // skip
			OLD11,
		};

		[StructLayout(LayoutKind.Explicit)]
		protected struct AkbkHashHeader
			: IO.IEndianStreamSerializable
		{
			[FieldOffset(0)] public StringType Type;
			[FieldOffset(0)] public uint Hash; // 2007
			[FieldOffset(4)] public uint Size;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Type, StringTypeStreamer.Instance);
				s.Stream(ref this.Size);
			}
			#endregion
		};

		protected static long EndOfStream(IO.EndianStream s, AkbkHashHeader header)
		{
			return s.BaseStream.Position + header.Size;
		}
	};
}