
namespace KSoft.Phoenix.Phx
{
	/// <summary>Script objects which can be "commented" out</summary>
	public abstract class TriggerScriptCodeObject
		: TriggerScriptIdObject
	{
		#region Xml constants
		const string K_XML_ATTR_COMMENT_OUT_ = "CommentOut";
		#endregion

		bool mCommentOut_;
		public bool CommentOut { get { return this.mCommentOut_; } }

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			base.Serialize(s);

			s.StreamAttribute(K_XML_ATTR_COMMENT_OUT_, ref this.mCommentOut_);
		}
	};
}