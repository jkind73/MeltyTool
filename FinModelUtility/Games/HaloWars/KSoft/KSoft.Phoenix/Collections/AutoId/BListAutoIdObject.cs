using System.ComponentModel;

namespace KSoft.Collections
{
	public abstract class BListAutoIdObject
		: ObjectModel.BasicViewModel
		, IListAutoIdObject
	{
		private string mName_ = Phoenix.Phx.BDatabaseBase.K_INVALID_STRING;
		[Browsable(false)]
		public string Name
		{
			get { return this.mName_; }
			protected set
			{
				if (this.SetFieldObj(ref this.mName_, value))
				{
					this.OnPropertyChanged(nameof(IListAutoIdObject.Data));
				}
			}
		}

		#region IListAutoIdObject Members
		private int mAutoId_;
		public int AutoId
		{
			get { return this.mAutoId_; }
			set { this.SetFieldVal(ref this.mAutoId_, value); }
		}

		string IListAutoIdObject.Data
		{
			get { return this.mName_; }
			set { this.Name = value; }
		}

		public abstract void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class;
		#endregion

		public override string ToString() { return this.mName_; }
	};
}
