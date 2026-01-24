#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BMatrix = System.Numerics.Matrix4x4;

using BVisualAsset = System.UInt64; // unknown fields

namespace KSoft.Phoenix.Runtime
{
	public sealed class BVisualItem
		: IO.IEndianStreamSerializable
	{
		const int K_UV_OFFSETS_SIZE_ = 0x18;
	public BMatrix matrix;
		public uint subUpdateNumber, grannySubUpdateNumber;
		public BMatrix matrix1, matrix2;
		public BVector
			combinedMinCorner, combinedMaxCorner,
			minCorner, maxCorner;
		public BVisualAsset modelAsset;
		public byte[] modelUvOffsets = new byte[K_UV_OFFSETS_SIZE_]; // BVisualModelUVOffsets
		public uint flags;
		public BVisualItem[] attachments;

		#region IEndianStreamSerializable Members
		void StreamFlags(IO.EndianStream s)
		{
			const byte kSizeInBytes = sizeof(uint);

			s.StreamSignature(kSizeInBytes);
			s.Stream(ref this.flags);
		}
		void StreamAttachments(IO.EndianStream s)
		{
			Contract.Assert(false); // TODO
		}
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamMatrix(s, ref this.matrix);
			s.Stream(ref this.subUpdateNumber);
			s.Stream(ref this.grannySubUpdateNumber);
			BSaveGame.StreamMatrix(s, ref this.matrix1);
			BSaveGame.StreamMatrix(s, ref this.matrix2);
			s.StreamV(ref this.combinedMinCorner); s.StreamV(ref this.combinedMaxCorner);
			s.StreamV(ref this.minCorner); s.StreamV(ref this.maxCorner);
			s.Stream(ref this.modelAsset);
			if (s.StreamCond(this.modelUvOffsets, offsets => !offsets.EqualsZero()))
				s.Stream(this.modelUvOffsets);
			this.StreamFlags(s);
			this.StreamAttachments(s);
		}
		#endregion
	};

	public sealed class BVisual
		: IO.IEndianStreamSerializable
	{
		public int protoId;

		public long userData;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.userData);
		}
		#endregion
	};

	public static class BVisualManager
	{
		static BVisual NewVisual()
		{
			return new BVisual();
		}
		static void SetProtoId(BVisual visual, int id)
		{
			visual.protoId = id;
		}
		static int GetProtoId(BVisual visual)
		{
			return visual.protoId;
		}
		internal static void Stream(IO.EndianStream s, ref BVisual visual)
		{
			if (BSaveGame.StreamObjectId(s, ref visual, NewVisual, SetProtoId, GetProtoId))
			{
				visual.Serialize(s);
			}
		}
	};
}