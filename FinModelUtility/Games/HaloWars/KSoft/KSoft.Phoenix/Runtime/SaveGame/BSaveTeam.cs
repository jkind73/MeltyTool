
namespace KSoft.Phoenix.Runtime
{
	sealed class BSaveTeam
		: IO.IEndianStreamSerializable
	{
		public int[] players;
		public byte[] relations; // BRelationType

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref this.players);
			BSaveGame.StreamArray(s, ref this.relations);
		}
		#endregion
	};
}