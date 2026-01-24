
namespace KSoft.Phoenix.Phx
{
	/// <summary>Explicitly ID'd script objects</summary>
	public abstract class TriggerScriptIdObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		const string K_XML_ATTR_ID_ = "ID"; // EditorID
		#endregion

		int mId_ = TypeExtensions.K_NONE;
		public int Id { get { return this.mId_; } }

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute(K_XML_ATTR_ID_, ref this.mId_);
		}
	};
}