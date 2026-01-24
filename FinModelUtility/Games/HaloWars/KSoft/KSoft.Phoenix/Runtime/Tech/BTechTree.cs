
namespace KSoft.Phoenix.Runtime
{
	partial class CSaveMarker
	{
		public const ushort
			TECH_TREE = 0x2710
			;
	};

	sealed class BTechTree
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		public BTechNode[] techs;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray16(s, ref this.techs);
			s.StreamSignature(CSaveMarker.TECH_TREE);
		}
		#endregion

#if false
		public static void ToStream(IO.IndentedTextWriter s, string name, ITechNode node)
		{
			s.WriteLine("{0}Unique {1}\t{2}", node.Unique ? "Is " : "Non",
				node.Status, name);
		}
#endif

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			var sg = s.Owner as BSaveGame;

			for (int x = 0; x < Techs.Length; x++)
				ToStream(s, sg.Database.ProtoTechs[x], Techs[x]);
		}
#endif
		#endregion
	};
}