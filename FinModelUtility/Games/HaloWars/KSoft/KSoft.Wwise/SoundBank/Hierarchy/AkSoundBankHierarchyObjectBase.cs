
namespace KSoft.Wwise.SoundBank
{
	abstract partial class AkSoundBankHierarchyObjectBase
		: IO.IEndianStreamSerializable
	{
		public uint id;

		#region Factory
		class AkSoundBankHierarchyDefaultImpl
			: AkSoundBankHierarchyObjectBase
		{
			HircType mType_;

			public AkSoundBankHierarchyDefaultImpl(HircType type)
			{
				this.mType_ = type;
			}

			public override string ToString()
			{
				return this.mType_.ToString();
			}
		};

		public static AkSoundBankHierarchyObjectBase New(HircType type)
		{
			switch (type)
			{
			case HircType.SOUND:		return new AkSoundBankHierarchySound();
			case HircType.ACTION:		return new AkSoundBankHierarchyAction();
			case HircType.EVENT:		return new AkSoundBankHierarchyEvent();
			case HircType.RAN_SEQ_CNTR:	return new AkSoundBankHierarchyRanSeqCntr();

			default:					return new AkSoundBankHierarchyDefaultImpl(type);
			}
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.id);
		}
		#endregion
	};
}