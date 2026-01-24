
using BVector = System.Numerics.Vector4;
using BObjectiveID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	static partial class CSaveMarker
	{
		public const ushort 
			UI_WIDGET = 0x2710
			;
	};

	sealed class BuiWidget
		: IO.IEndianStreamSerializable
	{
		public struct BReticulePointer
			: IO.IEndianStreamSerializable
		{
			public ulong unknown0, unknown8;
			public uint unknown10;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.unknown0); s.Stream(ref this.unknown8); s.Stream(ref this.unknown10);
			}
			#endregion
		};

		public sealed class BuiTalkingHeadControl
			: IO.IEndianStreamSerializable
		{
			public string talkingHeadText;
			public int objectiveId, lastCount;
			public bool showBackground, objectiveVisible, talkingHeadVisible, isShown;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalWideString32(ref this.talkingHeadText);
				s.Stream(ref this.objectiveId); s.Stream(ref this.lastCount);
				s.Stream(ref this.showBackground); s.Stream(ref this.objectiveVisible); s.Stream(ref this.talkingHeadVisible); s.Stream(ref this.isShown);
			}
			#endregion
		};

		public sealed class BuiObjectiveProgressControl
			: IO.IEndianStreamSerializable
		{
			public struct BuiLabel
				: IO.IEndianStreamSerializable
			{
				public bool isShown;
				public string text;

				#region IEndianStreamSerializable Members
				public void Serialize(IO.EndianStream s)
				{
					s.Stream(ref this.isShown);
					s.StreamPascalWideString32(ref this.text);
				}
				#endregion
			};

			public struct BuiObjectiveProgressData
				: IO.IEndianStreamSerializable
			{
				public BObjectiveID objectiveId;
				public int lastCount;
				public uint fadeTime;
				public int labelIndex, minIncrement;

				#region IEndianStreamSerializable Members
				public void Serialize(IO.EndianStream s)
				{
					s.Stream(ref this.objectiveId);
					s.Stream(ref this.lastCount);
					s.Stream(ref this.fadeTime);
					s.Stream(ref this.labelIndex); s.Stream(ref this.minIncrement);
				}
				#endregion
			};

			public BuiLabel[] objectiveLabels = new BuiLabel[4];
			public BuiObjectiveProgressData[] objectives;

			public BuiObjectiveProgressControl()
			{
				for (int x = 0; x < this.objectiveLabels.Length; x++)
					this.objectiveLabels[x] = new BuiLabel();
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				for (int x = 0; x < this.objectiveLabels.Length; x++) s.Stream(ref this.objectiveLabels[x]);
				BSaveGame.StreamArray(s, ref this.objectives);
			}
			#endregion
		};

		public int counterCurrent, counterMax, timerId, timerLabelId;
		public float elapsedTimerTime;
		public int numCitizensSaved, numCitizensNeeded;

		public ushort garrisonContainerVisible0; public byte garrisonContainerVisible2;
		public int garrisonContainerEntities0, garrisonContainerEntities4, garrisonContainerEntities8;
		public ushort garrisonContainerUseEntity0; public byte garrisonContainerUseEntity2;
		public int garrisonContainerCounts0, garrisonContainerCounts4, garrisonContainerCounts8;

		public uint reticulePointersVisible0; public byte reticulePointersVisible4;
		public BReticulePointer reticulePointerType;
		public BVector[] reticulePointerArea = new BVector[3];
		public BReticulePointer reticulePointerEntities, pointerRotation, pointerRotationFloat;
		public BuiTalkingHeadControl talkingHeadControl = new BuiTalkingHeadControl();
		public BuiObjectiveProgressControl objectiveProgressControl = new BuiObjectiveProgressControl();
		public bool widgetPanelVisible, timerVisible, citizensSavedVisible,
			counterVisible, timerShown;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.counterCurrent); s.Stream(ref this.counterMax); s.Stream(ref this.timerId); s.Stream(ref this.timerLabelId);
			s.Stream(ref this.elapsedTimerTime);
			s.Stream(ref this.numCitizensSaved); s.Stream(ref this.numCitizensNeeded);

			s.Stream(ref this.garrisonContainerVisible0); s.Stream(ref this.garrisonContainerVisible2);
			s.Stream(ref this.garrisonContainerEntities0); s.Stream(ref this.garrisonContainerEntities4); s.Stream(ref this.garrisonContainerEntities8);
			s.Stream(ref this.garrisonContainerUseEntity0); s.Stream(ref this.garrisonContainerUseEntity2);
			s.Stream(ref this.garrisonContainerCounts0); s.Stream(ref this.garrisonContainerCounts4); s.Stream(ref this.garrisonContainerCounts8);

			s.Stream(ref this.reticulePointersVisible0); s.Stream(ref this.reticulePointersVisible4);
			s.Stream(ref this.reticulePointerType);
			for (int x = 0; x < this.reticulePointerArea.Length; x++) s.StreamV(ref this.reticulePointerArea[x]);
			s.Stream(ref this.reticulePointerEntities);
			s.Stream(ref this.pointerRotation);
			s.Stream(ref this.pointerRotationFloat);
			s.Stream(this.talkingHeadControl);
			s.Stream(this.objectiveProgressControl);
			s.Stream(ref this.widgetPanelVisible); s.Stream(ref this.timerVisible); s.Stream(ref this.citizensSavedVisible);
			s.Stream(ref this.counterVisible); s.Stream(ref this.timerShown);
			s.StreamSignature(CSaveMarker.UI_WIDGET);
		}
		#endregion
	};
}