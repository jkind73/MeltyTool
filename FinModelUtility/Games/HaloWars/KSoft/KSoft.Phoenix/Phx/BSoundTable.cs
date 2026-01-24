using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BSoundTable
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		internal static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "SoundTable.xml",
			RootName = "Table"
		};
		#endregion

		public Dictionary<uint, string> mEventsMap = new Dictionary<uint, string>();
		public IReadOnlyDictionary<uint, string> EventsMap { get { return this.mEventsMap; } }

		#region ITagElementTextStreamable Members
		void FromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			foreach (var element in s.ElementsByName("Sound"))
				using (s.EnterCursorBookmark(element))
				{
					string name = null; uint eventId = 0;
					s.StreamElement("CueName", ref name);
					s.StreamElement("CueIndex", ref eventId);

					this.mEventsMap.Add(eventId, name);
				}
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
				this.FromStream(s);
		}
		#endregion
	};
}