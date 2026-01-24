
namespace KSoft.Phoenix.XML
{
	public abstract partial class BDatabaseXmlSerializerBase
		: BXmlSerializerInterface
		, IO.ITagElementStringNameStreamable
	{
		IBListAutoIdXmlSerializer mDamageTypesSerializer_
			, mImpactEffectsSerializer_
			, mObjectsSerializer_
			, mSquadsSerializer_
			, mPowersSerializer_
			, mTechsSerializer_
			;

		protected BDatabaseXmlSerializerBase()
		{
		}

		#region IDisposable Members
		public override void Dispose()
		{
			this.AutoIdSerializersDispose();
		}
		#endregion

		protected virtual void AutoIdSerializersInitialize()
		{
			if (this.mDamageTypesSerializer_ == null)
				this.mDamageTypesSerializer_ = XmlUtil.CreateXmlSerializer(this.Database.DamageTypes, Phx.BDamageType.KBListXmlParams);
			if (this.mImpactEffectsSerializer_ == null)
				this.mImpactEffectsSerializer_ = XmlUtil.CreateXmlSerializer(this.Database.ImpactEffects, Phx.BProtoImpactEffect.KBListXmlParams);

			if (this.mObjectsSerializer_ == null)
				this.mObjectsSerializer_ = XmlUtil.CreateXmlSerializer(this.Database.Objects, Phx.BProtoObject.KBListXmlParams);
			if (this.mSquadsSerializer_ == null)
				this.mSquadsSerializer_ = XmlUtil.CreateXmlSerializer(this.Database.Squads, Phx.BProtoSquad.KBListXmlParams);
			if (this.mPowersSerializer_ == null)
				this.mPowersSerializer_ = XmlUtil.CreateXmlSerializer(this.Database.Powers, Phx.BProtoPower.KBListXmlParams);
			if (this.mTechsSerializer_ == null)
				this.mTechsSerializer_ = XmlUtil.CreateXmlSerializer(this.Database.Techs, Phx.BProtoTech.KBListXmlParams);
		}
		protected virtual void AutoIdSerializersDispose()
		{
			Util.DisposeAndNull(ref this.mDamageTypesSerializer_);
			Util.DisposeAndNull(ref this.mImpactEffectsSerializer_);

			Util.DisposeAndNull(ref this.mObjectsSerializer_);
			Util.DisposeAndNull(ref this.mSquadsSerializer_);
			Util.DisposeAndNull(ref this.mPowersSerializer_);
			Util.DisposeAndNull(ref this.mTechsSerializer_);
		}
	};
}