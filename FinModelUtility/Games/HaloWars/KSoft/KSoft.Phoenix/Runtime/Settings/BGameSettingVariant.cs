using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Runtime
{
	using GameSettingTypeStreamer = IO.EnumBinaryStreamer<GameSettingType>;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	struct BGameSettingVariant
		: IO.IEndianStreamSerializable
	{
		[Interop.FieldOffset(0)] public bool Bool;
		[Interop.FieldOffset(0)] public byte Byte;
		[Interop.FieldOffset(0)] public int Int;
		[Interop.FieldOffset(0)] public float Float;
		[Interop.FieldOffset(0)] public long Long;

		[Interop.FieldOffset(8)] public GameSettingType Type;

		// #64BIT: +4 to offset 16
		[Interop.FieldOffset(12+4)] public string String;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Type, GameSettingTypeStreamer.Instance);
			switch (this.Type)
			{
				case GameSettingType.FLOAT:  s.Stream(ref this.Float); break;
				case GameSettingType.INT:    s.Stream(ref this.Int); break;
				case GameSettingType.BYTE:   s.Stream(ref this.Byte); break;
				case GameSettingType.BOOL:   s.Stream(ref this.Bool); break;
				case GameSettingType.LONG:   s.Stream(ref this.Long); break;
				case GameSettingType.STRING: s.Stream(ref this.String); break;
			}
		}
		#endregion
	};
}
