
namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankHierarchyRanSeqCntr
		: AkSoundBankHierarchyObjectBase
	{
		public CAkParameterNodeBase parameterNode = new CAkParameterNodeBase();
		public AkPlaylistItem[] playlist;

	void SerializeReverseHack2008(IO.EndianStream s)
		{
			const long kSeekAmount = -(sizeof(uint) + AkPlaylistItem.K_SIZE_OF);
			int itemCount = 1;
			long terminator = s.BaseStream.Position + 0x74;
			bool readPlaylist = false;

			s.Seek(s.VirtualBufferStart + s.VirtualBufferLength);
			do{
				s.Seek(kSeekAmount, System.IO.SeekOrigin.Current);
				if (s.Reader.ReadInt32() == itemCount)
				{
					readPlaylist = true;
					break;
				}

				itemCount++;
			}while(s.BaseStream.Position > terminator);

			if (readPlaylist)
			{
				this.playlist = new AkPlaylistItem[itemCount];
				s.StreamArray(this.playlist);
			}
		}
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			uint genVer = (s.Owner as AkSoundBank).GeneratorVersion;

			if (genVer == AkVersion.K2008.BANK_GENERATOR)
				this.SerializeReverseHack2008(s);
			else
			{
				s.Stream(this.parameterNode);
				// 0x18
				s.Pad16(); // LoopCount
				s.Pad32(); // float TransitionTime
				s.Pad32(); // float TransitionTimeModMin
				s.Pad32(); // float TransitionTimeModMax
				s.Pad16(); // AvoidRepeatCount
				s.Pad8(); // TransitionMode
				s.Pad8(); // RandomMode
				s.Pad8(); // Mode
				s.Pad8(); // IsUsingWeight
				s.Pad8(); // ResetPlayListAtEachPlay
				s.Pad8(); // IsRestartBackward
				s.Pad8(); // IsContinuous
				s.Pad8(); // IsGlobal
			}
		}
	};
}