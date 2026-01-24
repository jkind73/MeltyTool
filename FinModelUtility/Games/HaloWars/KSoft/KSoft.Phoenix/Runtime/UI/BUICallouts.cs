using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	static partial class CSaveMarker
	{
		public const ushort 
			UI_CALLOUTS1 = 0x2710,
			UI_CALLOUTS2 = 0x2711,
			UI_CALLOUT = 0x2712
			;
	};

	sealed class BuiCallouts
		: IO.IEndianStreamSerializable
	{
		const byte C_NUM_CALLOUTS_ = 5;

		public sealed class BuiCallout
			: IO.IEndianStreamSerializable
		{
			public int id;
			public uint type;
			public BVector location;
			public BEntityID entityId, calloutEntityId;
			public int locStringIndex, uiCalloutId, x, y;
			public bool visible;

			internal BVector position; // Not actually part of this struct

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.id);
				s.Stream(ref this.type);
				s.StreamV(ref this.location);
				s.Stream(ref this.entityId); s.Stream(ref this.calloutEntityId);
				s.Stream(ref this.locStringIndex); s.Stream(ref this.uiCalloutId); s.Stream(ref this.x); s.Stream(ref this.y);
				s.Stream(ref this.visible);
				s.StreamSignature(CSaveMarker.UI_CALLOUT);

				s.StreamV(ref this.position);
			}
			#endregion
		};
		static readonly CondensedListInfo KCalloutsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
		};

		public List<CondensedListItem16<BuiCallout>> callouts = [];
		public int[] calloutWidgets = new int[C_NUM_CALLOUTS_];
		public int nextCalloutId;
		public bool panelVisible, calloutsVisible;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamList(s, this.callouts, KCalloutsListInfo);
			s.StreamSignature(CSaveMarker.UI_CALLOUTS1);
			s.StreamSignature(C_NUM_CALLOUTS_);
			for (int x = 0; x < this.calloutWidgets.Length; x++)
				s.Stream(ref this.calloutWidgets[x]);
			s.Stream(ref this.nextCalloutId);
			s.Stream(ref this.panelVisible); s.Stream(ref this.calloutsVisible);
			s.StreamSignature(CSaveMarker.UI_CALLOUTS2);
		}
		#endregion
	};
}