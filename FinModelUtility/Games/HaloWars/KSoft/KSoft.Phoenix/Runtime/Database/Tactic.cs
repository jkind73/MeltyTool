
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public sealed class Tactic
			: IO.IEndianStreamSerializable
		{
			public string[] protoActions, weapons;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				BSaveGame.StreamArray(s, ref this.protoActions);
				BSaveGame.StreamArray(s, ref this.weapons);
			}
			#endregion
		};
		static readonly CondensedListInfo KTacticsListInfo = new CondensedListInfo()
		{
			IndexSize=sizeof(short),
		};
	};
}