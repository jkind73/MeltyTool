
namespace KSoft.Phoenix.XML
{
	internal static class ProtoEnumUndefinedMembers
	{
		public static void Write<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BListXmlParams p,
			Collections.IProtoEnumWithUndefined undefined)
			where TDoc : class
			where TCursor : class
		{
			if (p.DoNotWriteUndefinedData)
				return;

			if (undefined.MemberUndefinedCount == 0)
				return;

			string elementName = "Undefined" + p.elementName;

			foreach (string str in undefined.UndefinedMembers)
			{
				using (s.EnterCursorBookmark(elementName))
				{
					string temp = str;
					p.StreamDataName(s, ref temp);
				}
			}
		}
	};
}