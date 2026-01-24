
namespace KSoft.Phoenix.Resource
{
	public sealed class SaveFile
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		Runtime.BSettings mSettings_ = new Runtime.BSettings();
		Runtime.BSaveGame mSaveGame_ = new Runtime.BSaveGame();

		long mLeftoversPos_;
		byte[] mLeftovers_;

		#region IEndianStreamSerializable Members
		void SerializeLeftovers(IO.EndianStream s)
		{
			s.TraceAndDebugPosition(ref this.mLeftoversPos_);

			if (s.IsReading)
				this.mLeftovers_ = new byte[s.BaseStream.Length - s.BaseStream.Position];

			s.Stream(this.mLeftovers_, 0, this.mLeftovers_.Length);
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(this.mSettings_);
			s.Stream(this.mSaveGame_);
			this.SerializeLeftovers(s);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				mSaveGame.ToStream(s);
			}
		}
#endif
		#endregion
	};
}