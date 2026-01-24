
namespace KSoft.Phoenix.Phx
{
	//BSkullModifier
	public sealed class BCollectibleSkullEffect
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Effect",
		};
		#endregion

		BCollectibleSkullEffectType mType_ = BCollectibleSkullEffectType.INVALID;
		BCollectibleSkullTarget mTarget_ = BCollectibleSkullTarget.NONE;
		float mValue_ = PhxUtil.K_INVALID_SINGLE;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursorEnum(ref this.mType_);
			s.StreamAttributeEnumOpt("target", ref this.mTarget_, e => e != BCollectibleSkullTarget.NONE);
			s.StreamAttributeOpt("value", ref this.mValue_, PhxPredicates.IsNotInvalid);
		}
		#endregion
	};
}