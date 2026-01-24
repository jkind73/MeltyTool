
namespace KSoft.Phoenix.Phx
{
	public sealed class BWeaponType
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams("WeaponType")
		{
			dataName = "Name",
			flags = XML.BCollectionXmlParamsFlags.USE_ELEMENT_FOR_DATA
		};
		public static readonly Engine.XmlFileInfo KXmlFileInfo = new Engine.XmlFileInfo
		{
			Directory = Engine.GameDirectory.DATA,
			FileName = "WeaponTypes.xml",
			RootName = KBListXmlParams.rootName
		};
		public static readonly Engine.ProtoDataXmlFileInfo KProtoFileInfo = new Engine.ProtoDataXmlFileInfo(
			Engine.XmlFilePriority.LISTS,
			KXmlFileInfo);
		#endregion

		#region DeathAnimation
		string mDeathAnimation_;
		public string DeathAnimation
		{
			get { return this.mDeathAnimation_; }
			set { this.mDeathAnimation_ = value; }
		}
		#endregion

		public Collections.BTypeValues<BWeaponModifier> Modifiers { get; private set; } = new Collections.BTypeValues<BWeaponModifier>(BWeaponModifier.KBListParams);

		#region BListObjectBase Members
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamElementOpt("DeathAnimation", ref this.mDeathAnimation_, Predicates.IsNotNullOrEmpty);

			XML.XmlUtil.Serialize(s, this.Modifiers, BWeaponModifier.KBListXmlParams);
		}
		#endregion
	};
}
