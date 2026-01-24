using System.Collections.Generic;

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.TRIGGER_SCRIPT)]
	public sealed class BTriggerSystem
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string K_XML_ROOT_NAME = "TriggerSystem";

		const string K_XML_ATTR_TYPE_ = "Type";
		const string K_XML_ATTR_NEXT_TRIGGER_VAR_ = "NextTriggerVarID";
		const string K_XML_ATTR_NEXT_TRIGGER_ = "NextTriggerID";
		const string K_XML_ATTR_NEXT_CONDITION_ = "NextConditionID";
		const string K_XML_ATTR_NEXT_EFFECT_ = "NextEffectID";
		const string K_XML_ATTR_EXTERNAL_ = "External";
		#endregion

		#region File Util
		public static string GetFileExt(BTriggerScriptType type)
		{
			switch (type)
			{
				case BTriggerScriptType.TRIGGER_SCRIPT: return ".triggerscript";
				case BTriggerScriptType.ABILITY: return ".ability";
				case BTriggerScriptType.POWER: return ".power";

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		public static string GetFileExtSearchPattern(BTriggerScriptType type)
		{
			switch (type)
			{
				case BTriggerScriptType.TRIGGER_SCRIPT: return "*.triggerscript";
				case BTriggerScriptType.ABILITY: return "*.ability";
				case BTriggerScriptType.POWER: return "*.power";

				default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
		#endregion

		public BTriggerSystem Owner { get; private set; }

		string mName_;
		public string Name { get { return this.mName_; } }
		public override string ToString() { return this.mName_; }

		BTriggerScriptType mType_;
		int mNextTriggerVarId_ = TypeExtensions.K_NONE;
		int mNextTriggerId_ = TypeExtensions.K_NONE;
		int mNextConditionId_ = TypeExtensions.K_NONE;
		int mNextEffectId_ = TypeExtensions.K_NONE;
		bool mExternal_;

		public Collections.BListAutoId<BTriggerGroup> Groups { get; private set; } = new Collections.BListAutoId<BTriggerGroup>();

		public Collections.BListAutoId<BTriggerVar> Vars { get; private set; } = new Collections.BListAutoId<BTriggerVar>();
		public Collections.BListAutoId<BTrigger> Triggers { get; private set; } = new Collections.BListAutoId<BTrigger>();

		public BTriggerEditorData EditorData { get; private set; }

		#region Database interfaces
		Dictionary<int, BTriggerGroup> mDbiGroups_;
		Dictionary<int, BTriggerVar> mDbiVars_;
		Dictionary<int, BTrigger> mDbiTriggers_;

		static void BuildDictionary<T>(out Dictionary<int, T> dic, Collections.BListAutoId<T> list)
			where T : TriggerScriptIdObject, new()
		{
			dic = new Dictionary<int, T>(list.Count);

			foreach (var item in list)
				dic.Add(item.Id, item);
		}

		public BTriggerVar GetVar(int varId)
		{
			BTriggerVar var;
			this.mDbiVars_.TryGetValue(varId, out var);

			return var;
		}
		#endregion

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			s.StreamAttribute(DatabaseNamedObject.K_XML_ATTR_NAME_N, ref this.mName_);
			s.StreamAttributeEnum(K_XML_ATTR_TYPE_, ref this.mType_);
			s.StreamAttribute(K_XML_ATTR_NEXT_TRIGGER_VAR_, ref this.mNextTriggerVarId_);
			s.StreamAttribute(K_XML_ATTR_NEXT_TRIGGER_, ref this.mNextTriggerId_);
			s.StreamAttribute(K_XML_ATTR_NEXT_CONDITION_, ref this.mNextConditionId_);
			s.StreamAttribute(K_XML_ATTR_NEXT_EFFECT_, ref this.mNextEffectId_);
			s.StreamAttribute(K_XML_ATTR_EXTERNAL_, ref this.mExternal_);

			using (s.EnterUserDataBookmark(this))
			{
				XML.XmlUtil.Serialize(s, this.Groups, BTriggerGroup.KBListXmlParams);
				if (s.IsReading) BuildDictionary(out this.mDbiGroups_, this.Groups);

				XML.XmlUtil.Serialize(s, this.Vars, BTriggerVar.KBListXmlParams);
				if (s.IsReading) BuildDictionary(out this.mDbiVars_, this.Vars);
				XML.XmlUtil.Serialize(s, this.Triggers, BTrigger.KBListXmlParams);
				if (s.IsReading) BuildDictionary(out this.mDbiTriggers_, this.Triggers);
			}

			if(s.IsReading)
				(xs as XML.BTriggerScriptSerializer).TriggerDb.UpdateFromGameData(this);
		}
		#endregion
	};
}
