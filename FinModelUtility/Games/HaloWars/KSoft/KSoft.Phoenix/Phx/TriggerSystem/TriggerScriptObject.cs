using System;

namespace KSoft.Phoenix.Phx
{
	/// <summary>Script objects which map to a editor-only database</summary>
	public abstract class TriggerScriptObject : TriggerScriptCodeObject
	{
		#region Xml constants
		protected const string K_XML_ATTR_TYPE = "Type";
		const string K_XML_ATTR_DB_ID_ = "DBID";
		const string K_XML_ATTR_VERSION_ = "Version";
		#endregion

//		string mTypeStr; // TODO: temporary!
		int mDbId_ = TypeExtensions.K_NONE;
		public int DbId { get { return this.mDbId_; } }

		int mVersion_ = TypeExtensions.K_NONE;
		public int Version { get { return this.mVersion_; } }

		protected void StreamType<TTypeEnum, TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, ref TTypeEnum type)
			where TTypeEnum : struct, IComparable, IFormattable, IConvertible
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttributeEnum(K_XML_ATTR_TYPE, ref type);
		}
		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(K_XML_ATTR_DB_ID_, ref this.mDbId_);
			s.StreamAttribute(K_XML_ATTR_VERSION_, ref this.mVersion_);
			// Stream it last, so when we save it ourselves, the (relatively) fixed width stuff comes first
//			XML.XmlUtil.StreamInternString(s, kXmlAttrType, ref mTypeStr, false);
		}
	};
}