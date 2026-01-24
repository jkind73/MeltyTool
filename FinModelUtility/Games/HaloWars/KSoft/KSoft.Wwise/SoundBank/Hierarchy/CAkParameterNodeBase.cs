#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	public sealed class CAkParameterNodeBase
		: IO.IEndianStreamSerializable
	{
		struct FxData
			: IO.IEndianStreamSerializable
		{
			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Pad8(); // FXIndex
				s.Pad32(); // fxID
				s.Pad8(); // IsRendered
				s.Pad32(); // ulSize
			}
			#endregion
		};
		struct AkbkStateItem
			: IO.IEndianStreamSerializable
		{
			/// <summary>Size of this struct on disk</summary>
			internal const uint K_SIZE_OF = 9;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Pad32(); // State
				s.Pad8(); // IsCustom
				s.Pad32(); // ID
			}
			#endregion
		};

		byte numFx_;
		FxData[] fx_;

		public uint overrideBusId, directParentId;
		byte priority_;
		bool priorityOverrideParent_, priorityApplyDistFactor_;
		sbyte priorityDistanceOffset_;

		bool positioningInfoOverrideParent_;

		ushort numStates_;

		ushort numRtpc_;

		#region IEndianStreamSerializable Members
		void SerializeFxParams(IO.EndianStream s)
		{
			s.Pad8(); // IsOverrideParentFX
			s.Stream(ref this.numFx_);
			Contract.Assert(this.numFx_ <= 4);
			if (s.IsReading)
				this.fx_ = new FxData[this.numFx_];
			if (this.numFx_ > 0) s.Pad8(); // bitsFXBypass
			s.StreamArray(this.fx_);
		}
		void SerializeParams(IO.EndianStream s)
		{
			s.Pad32(); // float VolumeMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // float LFEVolumeMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // float PitchMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // float LPFMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // StateGroupID
		}
		void SerializePositioningParams(IO.EndianStream s)
		{
			s.Stream(ref this.positioningInfoOverrideParent_);
			Contract.Assert(!this.positioningInfoOverrideParent_);
			if (this.positioningInfoOverrideParent_)
			{
				s.Pad32(); // CenterPct
			}
		}
		void SerializeAdvSettingsParams(IO.EndianStream s)
		{
			s.Pad8(); // AkVirtualQueueBehavior VirtualQueueBehavior
			s.Pad8(); // KillNewest
			s.Pad16(); // MaxNumInstance
			s.Pad8(); // AkBelowThresholdBehavior BelowThresholdBehavior
			s.Pad8(); // bool MaxNumInstOverrideParent
			s.Pad8(); // bool VVoicesOptOverrideParent
		}
		void SerializeRtpc(IO.EndianStream s)
		{
			const int kSizeOf =
				4 + // FXID
				1 + // FXIndex
				4 + // RTPCID
				4 + // ParamID
				4 + // rtpcCurveID
				1 + // AkCurveScaling Scaling
				2 // Size
				;

			s.Stream(ref this.numRtpc_);
			s.Pad((int)(this.numRtpc_ * kSizeOf));
		}
		void SerializeFeedbackInfo(IO.EndianStream s)
		{
			if (!(s.Owner as AkSoundBank).HasFeedback) return;

			s.Pad32(); // BusId
			s.Pad32(); // float FeedbackVolume
			s.Pad32(); // float FeedbackModifierMin
			s.Pad32(); // float FeedbackModifierMax
			s.Pad32(); // float FeedbackLPF
			s.Pad32(); // float FeedbackLPFModMin
			s.Pad32(); // float FeedbackLPFModMax
		}
		public void Serialize(IO.EndianStream s)
		{
			this.SerializeFxParams(s); // 0x2...
			{ // 0xC
				s.Stream(ref this.overrideBusId);
				s.Stream(ref this.directParentId);
				s.Stream(ref this.priority_);
				s.Stream(ref this.priorityOverrideParent_);
				s.Stream(ref this.priorityApplyDistFactor_);
				s.Stream(ref this.priorityDistanceOffset_);
			}
			this.SerializeParams(s);            // 0x34
			this.SerializePositioningParams(s); // 0x1...
			this.SerializeAdvSettingsParams(s);    // 0x7
			{                                   // 0x2...
				s.Pad8();                       // StateSyncType
				s.Stream(ref this.numStates_);
				s.Pad((int)(this.numStates_ * AkbkStateItem.K_SIZE_OF));
			}
			this.SerializeRtpc(s);      // 0x2...
			this.SerializeFeedbackInfo(s); // // 0x1C
		}
		#endregion
	};
}