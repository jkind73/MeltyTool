using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerArg
		: IO.ITagElementStringNameStreamable
		, IComparable<BTriggerArg>
		, IEqualityComparer<BTriggerArg>
	{
		static readonly BTriggerArg KInvalid = new BTriggerArg();
		public bool IsInvalid { get { return ReferenceEquals(this, KInvalid); } }

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BTriggerArg> KBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BTriggerArg>(10)
			{
				kTypeGetInvalid = () => KInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BTriggerArg> KBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BTriggerArg>(null, K_XML_ATTR_SIG_ID_);

		const string K_XML_ATTR_SIG_ID_ = "SigID";
		const string K_XML_ATTR_OPTIONAL_ = "Optional";
		#endregion

		BTriggerParamType mType_ = BTriggerParamType.INVALID; // TODO: temporary!
		public BTriggerParamType Type { get { return this.mType_; } }

		string mName_; // TODO: temporary!
		public string Name { get { return this.mName_; } }

		int mSigId_ = TypeExtensions.K_NONE;
		public int SigId { get { return this.mSigId_; } }

		bool mOptional_; // TODO: temporary!
		public bool Optional { get { return this.mOptional_; } }

		int mVarId_ = TypeExtensions.K_NONE;

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsReading)
				s.ReadCursorName(ref this.mType_);
			s.StreamAttribute(K_XML_ATTR_SIG_ID_, ref this.mSigId_);
			s.StreamAttribute(DatabaseNamedObject.K_XML_ATTR_NAME_N, ref this.mName_);
			s.StreamAttribute(K_XML_ATTR_OPTIONAL_, ref this.mOptional_);
			s.StreamCursor(ref this.mVarId_);
		}
		#endregion

		#region IComparable<BTriggerArg> Members
		public int CompareTo(BTriggerArg other)
		{
			return this.mSigId_ - other.mSigId_;
		}
		#endregion

		#region IEqualityComparer<BTriggerArg> Members
		public bool Equals(BTriggerArg x, BTriggerArg y)
		{
			return x.mSigId_ == y.mSigId_;
		}

		public int GetHashCode(BTriggerArg obj)
		{
			return this.mSigId_;
		}
		#endregion

		public BTriggerVarType GetVarType(BTriggerSystem root)
		{
			return root.GetVar(this.mVarId_).Type;
		}
	};
}