
namespace KSoft.Phoenix.Phx
{
	public sealed partial class BProtoObjectTrainLimit
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "TrainLimit",
		};
		#endregion

		#region Type
		LimitType mType_ = LimitType.INVALID;
		public LimitType Type
		{
			get { return this.mType_; }
			set { this.mType_ = value; }
		}
		#endregion

		#region ID
		int mId_ = TypeExtensions.K_NONE;
		public int Id
		{
			get { return this.mId_; }
			set { this.mId_ = value; }
		}
		#endregion

		#region Count
		int mCount_;
		public int Count
		{
			get { return this.mCount_; }
			set { this.mCount_ = value; }
		}

		public bool IsCountValid { get { return this.Count >= byte.MinValue && this.Count < byte.MaxValue; } }
		#endregion

		#region Bucket
		int mBucket_;
		public int Bucket
		{
			get { return this.mBucket_; }
			set { this.mBucket_ = value; }
		}

		public bool IsBucketValid { get { return this.Bucket >= byte.MinValue && this.Bucket < byte.MaxValue; } }
		#endregion

		public bool IsValid { get {
			return this.Type != LimitType.INVALID
				&&
				this.Id.IsNotNone()
				&&
				this.IsCountValid
				&&
				this.IsBucketValid;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttributeEnum("Type", ref this.mType_);

			switch (this.mType_)
			{
				case LimitType.UNIT:
					xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, DatabaseObjectKind.OBJECT, false, XML.XmlUtil.K_SOURCE_CURSOR);
					break;
				case LimitType.SQUAD:
					xs.StreamDbid(s, XML.XmlUtil.K_NO_XML_NAME, ref this.mId_, DatabaseObjectKind.SQUAD, false, XML.XmlUtil.K_SOURCE_CURSOR);
					break;
			}

			s.StreamAttributeOpt("Count", ref this.mCount_, Predicates.IsNotZero);
			s.StreamAttributeOpt("Bucket", ref this.mBucket_, Predicates.IsNotZero);
		}
		#endregion
	};
}