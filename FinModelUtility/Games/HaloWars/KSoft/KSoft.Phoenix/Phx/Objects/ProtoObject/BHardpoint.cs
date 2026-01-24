
namespace KSoft.Phoenix.Phx
{
	// #TODO we shouldn't serialize out hardpoints without a name, the engine ASSERTs
	public sealed class BHardpoint
		: IO.ITagElementStringNameStreamable
	{
		const double C_PI_OVER12_ = (float)((1.0 / 12.0) * System.Math.PI);
		const float C_PI_OVER12_IN_DEGREES_ = (float)(C_PI_OVER12_ * TypeExtensions.K_DEGREES_PER_RADIAN);

		#region Xml constants
		public static readonly XML.BListXmlParams KBListXmlParams = new XML.BListXmlParams
		{
			elementName = "Hardpoint",
		};
		#endregion

		#region Name
		string mName_;
		public string Name
		{
			get { return this.mName_; }
			set { this.mName_ = value; }
		}
		#endregion

		#region YawAttachment
		string mYawAttachment_;
		[Meta.AttachmentTypeReference]
		public string YawAttachment
		{
			get { return this.mYawAttachment_; }
			set { this.mYawAttachment_ = value; }
		}
		#endregion

		#region PitchAttachment
		string mPitchAttachment_;
		[Meta.AttachmentTypeReference]
		public string PitchAttachment
		{
			get { return this.mPitchAttachment_; }
			set { this.mPitchAttachment_ = value; }
		}
		#endregion

		#region AutoCenter
		bool mAutoCenter_ = true;
		public bool AutoCenter
		{
			get { return this.mAutoCenter_; }
			set { this.mAutoCenter_ = value; }
		}
		#endregion

		#region SingleBoneIK
		bool mSingleBoneIk_;
		public bool SingleBoneIk
		{
			get { return this.mSingleBoneIk_; }
			set { this.mSingleBoneIk_ = value; }
		}
		#endregion

		#region Combined
		bool mCombined_;
		public bool Combined
		{
			get { return this.mCombined_; }
			set { this.mCombined_ = value; }
		}
		#endregion

		#region HardPitchLimits
		bool mHardPitchLimits_;
		public bool HardPitchLimits
		{
			get { return this.mHardPitchLimits_; }
			set { this.mHardPitchLimits_ = value; }
		}
		#endregion

		#region RelativeToUnit
		bool mRelativeToUnit_;
		public bool RelativeToUnit
		{
			get { return this.mRelativeToUnit_; }
			set { this.mRelativeToUnit_ = value; }
		}
		#endregion

		#region UseYawAndPitchAsTolerance
		bool mUseYawAndPitchAsTolerance_;
		public bool UseYawAndPitchAsTolerance
		{
			get { return this.mUseYawAndPitchAsTolerance_; }
			set { this.mUseYawAndPitchAsTolerance_ = value; }
		}
		#endregion

		#region InfiniteRateWhenHasTarget
		bool mInfiniteRateWhenHasTarget_;
		public bool InfiniteRateWhenHasTarget
		{
			get { return this.mInfiniteRateWhenHasTarget_; }
			set { this.mInfiniteRateWhenHasTarget_ = value; }
		}
		#endregion

		#region YawRotationRate
		float mYawRotationRate_ = C_PI_OVER12_IN_DEGREES_;
		// angle
		public float YawRotationRate
		{
			get { return this.mYawRotationRate_; }
			set { this.mYawRotationRate_ = value; }
		}
		#endregion

		#region PitchRotationRate
		float mPitchRotationRate_ = C_PI_OVER12_IN_DEGREES_;
		// angle
		public float PitchRotationRate
		{
			get { return this.mPitchRotationRate_; }
			set { this.mPitchRotationRate_ = value; }
		}
		#endregion

		#region YawLeftMaxAngle
		const float C_DEFAULT_YAW_LEFT_MAX_ANGLE_ = (float)-(System.Math.PI * TypeExtensions.K_DEGREES_PER_RADIAN);

		float mYawLeftMaxAngle_ = C_DEFAULT_YAW_LEFT_MAX_ANGLE_;
		// angle
		public float YawLeftMaxAngle
		{
			get { return this.mYawLeftMaxAngle_; }
			set { this.mYawLeftMaxAngle_ = value; }
		}
		#endregion

		#region YawRightMaxAngle
		const float C_DEFAULT_YAW_RIGHT_MAX_ANGLE_ = (float)+(System.Math.PI * TypeExtensions.K_DEGREES_PER_RADIAN);

		float mYawRightMaxAngle_ = C_DEFAULT_YAW_RIGHT_MAX_ANGLE_;
		// angle
		public float YawRightMaxAngle
		{
			get { return this.mYawRightMaxAngle_; }
			set { this.mYawRightMaxAngle_ = value; }
		}
		#endregion

		#region PitchMaxAngle
		const float C_DEFAULT_PITCH_MAX_ANGLE_ = (float)-(System.Math.PI * TypeExtensions.K_DEGREES_PER_RADIAN);

		float mPitchMaxAngle_ = C_DEFAULT_PITCH_MAX_ANGLE_;
		// angle
		public float PitchMaxAngle
		{
			get { return this.mPitchMaxAngle_; }
			set { this.mPitchMaxAngle_ = value; }
		}
		#endregion

		#region PitchMinAngle
		const float C_DEFAULT_PITCH_MIN_ANGLE_ = (float)+(System.Math.PI * TypeExtensions.K_DEGREES_PER_RADIAN);

		float mPitchMinAngle_ = C_DEFAULT_PITCH_MIN_ANGLE_;
		// angle
		public float PitchMinAngle
		{
			get { return this.mPitchMinAngle_; }
			set { this.mPitchMinAngle_ = value; }
		}
		#endregion

		#region StartYawSound
		string mStartYawSound_;
		[Meta.SoundCueReference]
		public string StartYawSound
		{
			get { return this.mStartYawSound_; }
			set { this.mStartYawSound_ = value; }
		}
		#endregion

		#region StopYawSound
		string mStopYawSound_;
		[Meta.SoundCueReference]
		public string StopYawSound
		{
			get { return this.mStopYawSound_; }
			set { this.mStopYawSound_ = value; }
		}
		#endregion

		#region StartPitchSound
		string mStartPitchSound_;
		[Meta.SoundCueReference]
		public string StartPitchSound
		{
			get { return this.mStartPitchSound_; }
			set { this.mStartPitchSound_ = value; }
		}
		#endregion

		#region StopPitchSound
		string mStopPitchSound_;
		[Meta.SoundCueReference]
		public string StopPitchSound
		{
			get { return this.mStopPitchSound_; }
			set { this.mStopPitchSound_ = value; }
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			s.StreamAttribute("name", ref this.mName_);
			s.StreamStringOpt("yawattachment", ref this.mYawAttachment_, toLower: false);
			s.StreamStringOpt("pitchattachment", ref this.mPitchAttachment_, toLower: false);
			s.StreamAttributeOpt("autocenter", ref this.mAutoCenter_, Predicates.IsFalse);
			s.StreamAttributeOpt("singleboneik", ref this.mSingleBoneIk_, Predicates.IsTrue);
			s.StreamAttributeOpt("combined", ref this.mCombined_, Predicates.IsTrue);
			s.StreamAttributeOpt("hardpitchlimits", ref this.mHardPitchLimits_, Predicates.IsTrue);
			s.StreamAttributeOpt("relativeToUnit", ref this.mRelativeToUnit_, Predicates.IsTrue);
			s.StreamAttributeOpt("useYawAndPitchAsTolerance", ref this.mUseYawAndPitchAsTolerance_, Predicates.IsTrue);
			s.StreamAttributeOpt("infiniteRateWhenHasTarget", ref this.mInfiniteRateWhenHasTarget_, Predicates.IsTrue);
			s.StreamAttributeOpt("yawrate", ref this.mYawRotationRate_, x => x != C_PI_OVER12_IN_DEGREES_);
			s.StreamAttributeOpt("pitchrate", ref this.mPitchRotationRate_, x => x != C_PI_OVER12_IN_DEGREES_);

			#region YawLeftMaxAngle and YawRightMaxAngle
			if (s.IsReading)
			{
				float yawMaxAngle = PhxUtil.K_INVALID_SINGLE_NA_N;
				if (s.ReadAttributeOpt("yawMaxAngle", ref yawMaxAngle) ||
					// #HACK fucking deal with original HW game data that was hand edited, but only when reading
					s.ReadAttributeOpt("yawmaxangle", ref yawMaxAngle))
				{
					this.mYawLeftMaxAngle_ = -yawMaxAngle;
					this.mYawRightMaxAngle_ = yawMaxAngle;
				}

				s.StreamAttributeOpt("YawLeftMaxAngle", ref this.mYawLeftMaxAngle_, x => x != C_DEFAULT_YAW_LEFT_MAX_ANGLE_);
				s.StreamAttributeOpt("YawRightMaxAngle", ref this.mYawRightMaxAngle_, x => x != C_DEFAULT_YAW_RIGHT_MAX_ANGLE_);
			}
			else if (s.IsWriting)
			{
				if (this.mYawLeftMaxAngle_ == C_DEFAULT_YAW_LEFT_MAX_ANGLE_ &&
				    this.mYawRightMaxAngle_ == C_DEFAULT_YAW_RIGHT_MAX_ANGLE_)
				{
					// don't stream anything
				}
				else if (System.Math.Abs(this.mYawLeftMaxAngle_) == this.mYawRightMaxAngle_)
				{
					s.WriteAttribute("yawMaxAngle", this.mYawRightMaxAngle_);
				}
				else
				{
					s.StreamAttributeOpt("YawLeftMaxAngle", ref this.mYawLeftMaxAngle_, x => x != C_DEFAULT_YAW_LEFT_MAX_ANGLE_);
					s.StreamAttributeOpt("YawRightMaxAngle", ref this.mYawRightMaxAngle_, x => x != C_DEFAULT_YAW_RIGHT_MAX_ANGLE_);
				}
			}
			#endregion

			s.StreamAttributeOpt("pitchMaxAngle", ref this.mPitchMaxAngle_, x => x != C_DEFAULT_PITCH_MAX_ANGLE_);
			s.StreamAttributeOpt("pitchMinAngle", ref this.mPitchMinAngle_, x => x != C_DEFAULT_PITCH_MIN_ANGLE_);

			s.StreamStringOpt("StartYawSound", ref this.mStartYawSound_, toLower: false);
			s.StreamStringOpt("StopYawSound", ref this.mStopYawSound_, toLower: false);
			s.StreamStringOpt("StartPitchSound", ref this.mStartPitchSound_, toLower: false);
			s.StreamStringOpt("StopPitchSound", ref this.mStopPitchSound_, toLower: false);
		}
		#endregion
	};
}