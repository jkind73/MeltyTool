#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	public abstract class TriggerSystemProtoObject
		: Collections.BListAutoIdObject
	{
		#region Xml constants
		const string K_XML_ATTR_DB_ID_ = "DBID";
		const string K_XML_ATTR_VERSION_ = "Version";
		#endregion

		int mDbId_ = TypeExtensions.K_NONE;
		public int DbId { get { return this.mDbId_; } }

		int mVersion_ = TypeExtensions.K_NONE;
		public int Version { get { return this.mVersion_; } }

		public Collections.BListExplicitIndex<BTriggerParam> Params { get; private set; }

		protected TriggerSystemProtoObject()
		{
			this.Params = new Collections.BListExplicitIndex<BTriggerParam>(BTriggerParam.KBListExplicitIndexParams);
		}
		protected TriggerSystemProtoObject(BTriggerSystem root, TriggerScriptObjectWithArgs instance)
		{
			this.Name = instance.Name;

			this.mDbId_ = instance.DbId;
			this.mVersion_ = instance.Version;
			this.Params = BTriggerParam.BuildDefinition(root, instance.Args);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			s.StreamAttribute(K_XML_ATTR_DB_ID_, ref this.mDbId_);
			s.StreamAttribute(K_XML_ATTR_VERSION_, ref this.mVersion_);

			XML.XmlUtil.Serialize(s, this.Params, BTriggerParam.KBListExplicitIndexXmlParams);
		}

		static bool ContainsUserClassTypeVar(BTriggerSystem ts, TriggerScriptObjectWithArgs obj)
		{
			foreach (var arg in obj.Args)
			{
				if (arg.IsInvalid)
					continue;
				if (arg.GetVarType(ts) == BTriggerVarType.USER_CLASS_TYPE)
					return true;
			}
			return false;
		}
		public virtual int CompareTo(BTriggerSystem ts, TriggerScriptObjectWithArgs obj)
		{
			if (this.Name != obj.Name)
				Debug.Trace.Engine.TraceInformation(
					"TriggerProtoDbObject: '{0}' - Encountered different names for {1}, '{2}' != '{3}'",
					ts, this.DbId.ToString(), this.Name, obj.Name);

			if (ContainsUserClassTypeVar(ts, obj))
			{
				Debug.Trace.Engine.TraceInformation(
					"TriggerProtoDbObject: {0} - Encountered {1}/{2} which has a UserClassType Var, skipping comparison",
					ts,
					this.DbId.ToString(),
					this.Name);
				return 0;
			}

			Contract.Assert(this.Version == obj.Version);
			Contract.Assert(this.Params.Count == obj.Args.Count);

			int diff = 0;
			for (int x = 0; x < this.Params.Count; x++)
			{
				int sig = this.Params[x].SigId;
				int objSig = obj.Args[x].SigId;
				sig = sig < 0 ? 0 : sig;
				objSig = objSig < 0 ? 0 : objSig;

				diff += sig - objSig;
			}

			return diff;
		}
	};
}