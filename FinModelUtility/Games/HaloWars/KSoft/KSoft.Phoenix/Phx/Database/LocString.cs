
namespace KSoft.Phoenix.Phx
{
	public enum LocStringCategory
	{
		NONE,

		CODE,
		TECHS,
		SQUADS,
		POWERS,
		ABILITIES,
		LEADERS,
		OBJECTS,
		UI,
		CAMPAIGN,
		CINEMATICS,
		SKIRMISH,

		STEAM_VERSION,

		K_NUMBER_OF
	};

	public sealed class LocString
		: ObjectModel.BasicViewModel
		, IO.ITagElementStringNameStreamable
	{
		#region ID
		int mId_ = TypeExtensions.K_NONE;
		public int Id
		{
			get { return this.mId_; }
			private set { this.SetFieldVal(ref this.mId_, value); }
		}
		#endregion

		#region Category
		// #NOTE this engine doesn't specifically limit the category values to the stuff in the enum, but, to reduce memory overhead, I am
		LocStringCategory mCategory_ = LocStringCategory.NONE;
		public LocStringCategory Category
		{
			get { return this.mCategory_; }
			set { this.SetFieldEnum(ref this.mCategory_, value); }
		}
		#endregion

		#region Scenario
		string mScenario_;
		public string Scenario
		{
			get { return this.mScenario_; }
			set { this.SetFieldObj(ref this.mScenario_, value); }
		}
		#endregion

		#region IsSubtitle
		bool mIsSubtitle_;
		public bool IsSubtitle
		{
			get { return this.mIsSubtitle_; }
			set { this.SetFieldVal(ref this.mIsSubtitle_, value); }
		}
		#endregion

		#region IsUpdate
		bool mIsUpdate_;
		public bool IsUpdate
		{
			get { return this.mIsUpdate_; }
			set { this.SetFieldVal(ref this.mIsUpdate_, value); }
		}
		#endregion

		#region MouseKeyboardID
		int mMouseKeyboardId_ = TypeExtensions.K_NONE;
		public int MouseKeyboardId
		{
			get { return this.mMouseKeyboardId_; }
			set { this.SetFieldVal(ref this.mMouseKeyboardId_, value); }
		}
		#endregion

		#region OriginalID
		// this is a string because there are cases with "and" in them. eg:
		// "25045 and 23441"
		string mOriginalId_;
		public string OriginalId
		{
			get { return this.mOriginalId_; }
			set { this.SetFieldObj(ref this.mOriginalId_, value); }
		}
		#endregion

		#region Text
		string mText_;
		public string Text
		{
			get { return this.mText_; }
			set { this.SetFieldObj(ref this.mText_, value); }
		}
		#endregion

		public LocString()
		{
		}

		public LocString(int id)
		{
			this.mId_ = id;
		}

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("_locID", ref this.mId_);
			s.StreamAttributeEnumOpt("category", ref this.mCategory_, e => e != LocStringCategory.NONE);
			s.StreamAttributeOpt("scenario", ref this.mScenario_, Predicates.IsNotNullOrEmpty);
			s.StreamAttributeOpt("subtitle", ref this.mIsSubtitle_, Predicates.IsTrue);
			s.StreamAttributeOpt("Update", ref this.mIsUpdate_, Predicates.IsTrue);
			s.StreamAttributeOpt("_mouseKeyboard", ref this.mMouseKeyboardId_, Predicates.IsNotNone);
			s.StreamAttributeOpt("originally", ref this.mOriginalId_, Predicates.IsNotNullOrEmpty);
			if (s.IsReading || this.mText_.IsNotNullOrEmpty())
				s.StreamCursor(ref this.mText_);
		}
		#endregion

		public override string ToString()
		{
			return string.Format("({0}) '{1}'",
			                     this.Id,
			                     this.Text ?? "");
		}
	};
}