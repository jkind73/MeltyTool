using System.Collections.Generic;

using BProtoSquadID = System.Int32;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoMergedSquads
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "MergedSquads",
		};
		#endregion

		BProtoSquadID mToMergeSquadId_ = TypeExtensions.K_NONE;
		[Meta.BProtoSquadReference]
		public BProtoSquadID ToMergeSquadId
		{
			get { return this.mToMergeSquadId_; }
			set { this.mToMergeSquadId_ = value; }
		}

		[Meta.BProtoSquadReference]
		public List<BProtoSquadID> BaseSquadIDs { get; private set; } = [];

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mToMergeSquadId_, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_CURSOR);
			s.StreamElements("MergedSquad", this.BaseSquadIDs, xs, XML.BXmlSerializerInterface.StreamSquadId);
		}
		#endregion
	};
}
