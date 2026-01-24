using System;
using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	public sealed class BTriggerParam
		: IO.ITagElementStringNameStreamable
		, IComparable<BTriggerParam>
		, IEqualityComparer<BTriggerParam>
	{
		static readonly BTriggerParam KInvalid = new BTriggerParam();
		public bool IsInvalid { get { return ReferenceEquals(this, KInvalid); } }

		#region Xml constants
		public static readonly Collections.BListExplicitIndexParams<BTriggerParam> KBListExplicitIndexParams = new
			Collections.BListExplicitIndexParams<BTriggerParam>(10)
			{
				kTypeGetInvalid = () => KInvalid
			};
		public static readonly XML.BListExplicitIndexXmlParams<BTriggerParam> KBListExplicitIndexXmlParams = new
			XML.BListExplicitIndexXmlParams<BTriggerParam>(/*null*/"Param", K_XML_ATTR_SIG_ID_);

		const string K_XML_ATTR_TYPE_ = "Type";
		const string K_XML_ATTR_SIG_ID_ = "SigID";
		const string K_XML_ATTR_OPTIONAL_ = "Optional";
		#endregion

		#region Properties
		BTriggerParamType mType_ = BTriggerParamType.INVALID;
		public BTriggerParamType Type { get { return this.mType_; } }

		string mName_;
		public string Name { get { return this.mName_; } }

		int mSigId_ = TypeExtensions.K_NONE;
		public int SigId { get { return this.mSigId_; } }

		BTriggerVarType mVarType_ = BTriggerVarType.NONE;
		public BTriggerVarType VarType { get { return this.mVarType_; } }

		bool mOptional_;
		public bool Optional { get { return this.mOptional_; } }
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			//if (s.IsReading) s.ReadCursorName(ref mType);
			s.StreamAttributeEnum(K_XML_ATTR_TYPE_, ref this.mType_);
			s.StreamAttribute(K_XML_ATTR_SIG_ID_, ref this.mSigId_);
			s.StreamAttribute(DatabaseNamedObject.K_XML_ATTR_NAME_N, ref this.mName_);
			s.StreamAttributeOpt(K_XML_ATTR_OPTIONAL_, ref this.mOptional_, Predicates.IsTrue);
			s.StreamCursorEnum(ref this.mVarType_);
		}
		#endregion

		#region IComparable<BTriggerParam> Members
		public int CompareTo(BTriggerParam other)
		{
			return this.mSigId_ - other.mSigId_;
		}
		#endregion

		#region IEqualityComparer<BTriggerParam> Members
		public bool Equals(BTriggerParam x, BTriggerParam y)
		{
			return x.mSigId_ == y.mSigId_;
		}

		public int GetHashCode(BTriggerParam obj)
		{
			return this.mSigId_;
		}
		#endregion

		public static Collections.BListExplicitIndex<BTriggerParam> BuildDefinition(
			BTriggerSystem root, Collections.BListExplicitIndex<BTriggerArg> args)
		{
			var p = new Collections.BListExplicitIndex<BTriggerParam>(KBListExplicitIndexParams);
			p.ResizeCount(args.Count);

			foreach (var arg in args)
			{
				if (arg.IsInvalid) continue;

				var param = new BTriggerParam();
				param.mType_ = arg.Type;
				param.mName_ = arg.Name;
				param.mSigId_ = arg.SigId;
				param.mOptional_ = arg.Optional;
				param.mVarType_ = arg.GetVarType(root);

				p[param.mSigId_-1] = param;
			}

			p.OptimizeStorage();
			return p;
		}
	};
}