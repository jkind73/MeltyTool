using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	static partial class CSaveMarker
	{
		public const ushort 
			START = 0x2710, END = 0x2711,
			VERSIONS = 0x2712,
			DB = 0x2713,
			SETUP_PLAYER = 0x2714, SETUP_TEAM = 0x2715, USER1 = 0x2716, // User1 = SetupUser
			USER2 = 0x2717,
			WORLD = 0x2718,
			UI = 0x2719,

			FATALITY_MANAGER = 0x2710,

			ARROW = 0x2712 // BObjectiveManager
			;
	};

	sealed partial class BSaveGame
		: IO.IEndianStreamSerializable
//		, IO.IIndentedTextWritable
	{
		int saveFileType_;
		public BDatabase Database { get; private set; } = new BDatabase();

		public List<BSavePlayer> Players { get; private set; } = [];
		public List<BSaveTeam> Teams { get; private set; } = [];
		public BSaveUser UserSave { get; private set; } = new BSaveUser();

		public BWorld World { get; private set; } = new BWorld();
		public BuiManager UiManager { get; private set; } = new BuiManager();
		public BUser User { get; private set; } = new BUser();

		#region IEndianStreamSerializable Members
		void SerializeSetup(IO.EndianStream s)
		{
			StreamCollection(s, this.Players);
			StreamCollection(s, this.Teams);
			s.StreamSignature(CSaveMarker.SETUP_TEAM);
			s.Stream(this.UserSave);
		}
		void SerializeGameState(IO.EndianStream s)
		{
			s.Stream(this.World);
// 			s.StreamSignature(cSaveMarker.World);
// 			s.StreamObject(UIManager);
// 			s.StreamSignature(cSaveMarker.UI);
// 			s.StreamObject(User);
// 			s.StreamSignature(cSaveMarker.User2);
		}

		public void Serialize(IO.EndianStream s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				s.StreamSignature(KClassVersions.B_SAVE_GAME);
				s.Stream(ref this.saveFileType_);

				s.StreamSignature(CSaveMarker.START);
				KClassVersions.Serialize(s);
				s.Stream(this.Database);
				this.SerializeSetup(s);
				this.SerializeGameState(s);
				//s.StreamSignature(cSaveMarker.End);
			}
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				World.ToStream(s);
			}
		}
#endif
		#endregion
	};
}
