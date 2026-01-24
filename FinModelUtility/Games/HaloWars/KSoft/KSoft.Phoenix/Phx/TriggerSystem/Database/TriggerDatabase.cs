using System.Collections.Generic;
using System.Threading.Tasks;

namespace KSoft.Phoenix.Phx
{
	public sealed class TriggerDatabase
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public const string K_XML_ROOT_NAME = "TriggerDatabase";
		#endregion

		public Collections.BListAutoId<BTriggerProtoCondition> Conditions { get; private set; } = new Collections.BListAutoId<BTriggerProtoCondition>();
		public Collections.BListAutoId<BTriggerProtoEffect> Effects { get; private set; } = new Collections.BListAutoId<BTriggerProtoEffect>();
		public Dictionary<uint, TriggerSystemProtoObject> LookupTable { get; private set; } = new Dictionary<uint, TriggerSystemProtoObject>();
		System.Collections.BitArray mUsedIds_ = new System.Collections.BitArray(1088);

		#region ITagElementStreamable<string> Members
		static int SortById(TriggerSystemProtoObject x, TriggerSystemProtoObject y)
		{
			if(x.DbId != y.DbId)
				return x.DbId - y.DbId;

			return x.Version - y.Version;
		}
		int WriteUnknowns<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			int count = 0;
			for (int x = 1; x < this.mUsedIds_.Length; x++)
			{
				if (!this.mUsedIds_[x])
				{
					s.WriteElement("Unknown", x);
					count++;
				}
			}
			return count;
		}
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (s.IsWriting)
			{
				var taskSortCond = Task.Factory.StartNew(() => this.Conditions.Sort(SortById));
				var taskSortEffe = Task.Factory.StartNew(() => this.Effects.Sort(SortById));

				var taskUnknowns = Task<int>.Factory.StartNew(() =>
				{
					using (s.EnterCursorBookmark("Unknowns"))
						return this.WriteUnknowns(s);
				});
				s.WriteAttribute("UnknownCount", taskUnknowns.Result);
				s.WriteAttribute("ConditionsCount", this.Conditions.Count);
				s.WriteAttribute("EffectsCount", this.Effects.Count);

				Task.WaitAll(taskSortCond, taskSortEffe);
			}

			XML.XmlUtil.Serialize(s, this.Conditions, BTriggerProtoCondition.KBListXmlParams);
			XML.XmlUtil.Serialize(s, this.Effects, BTriggerProtoEffect.KBListXmlParams);

			if (s.IsReading)
			{
				foreach (var c in this.Conditions)
					this.LookupTableAdd(c);
				foreach (var e in this.Effects)
					this.LookupTableAdd(e);
			}
		}
		#endregion

		static uint GenerateHandle(TriggerSystemProtoObject dbo)
		{
			return ((uint)dbo.DbId << 8) | (uint)dbo.Version;
		}
		static uint GenerateHandle(TriggerScriptObject dbo)
		{
			return ((uint)dbo.DbId << 8) | (uint)dbo.Version;
		}

	void LookupTableAdd(TriggerSystemProtoObject dbo)
		{
			this.mUsedIds_[dbo.DbId] = true;
			this.LookupTable.Add(GenerateHandle(dbo), dbo);
		}
		bool LookupTableContains<T>(T obj, out TriggerSystemProtoObject dbo)
			where T : TriggerScriptObject
		{
			return this.LookupTable.TryGetValue(GenerateHandle(obj), out dbo);
		}

		static void TraceUpdate(BTriggerSystem ts, TriggerSystemProtoObject dbo)
		{
			Debug.Trace.Engine.TraceInformation(
				"TriggerProtoDbObject: {0} - Updated {1}/{2}",
				ts, dbo.DbId.ToString(), dbo.Name);
		}
		void TryUpdate(BTriggerSystem ts, BTriggerCondition cond)
		{
			TriggerSystemProtoObject dbo;
			if (!this.LookupTableContains(cond, out dbo))
			{
				var dboCond = new BTriggerProtoCondition(ts, cond);

				this.Conditions.DynamicAdd(dboCond, dboCond.Name);
				this.LookupTableAdd(dboCond);
			}
			else
			{
				int diff = dbo.CompareTo(ts, cond);
				if (diff < 0)
				{
					var dboCond = new BTriggerProtoCondition(ts, cond);
					this.LookupTable[GenerateHandle(cond)] = dboCond;
					TraceUpdate(ts, dboCond);
				}
			}
		}
		void TryUpdate(BTriggerSystem ts, BTriggerEffect effe)
		{
			TriggerSystemProtoObject dbo;
			if (!this.LookupTableContains(effe, out dbo))
			{
				var dboEffe = new BTriggerProtoEffect(ts, effe);

				this.Effects.DynamicAdd(dboEffe, dboEffe.Name);
				this.LookupTableAdd(dboEffe);
			}
			else
			{
				int diff = dbo.CompareTo(ts, effe);
				if (diff < 0)
				{
					var dboEffe = new BTriggerProtoEffect(ts, effe);
					this.LookupTable[GenerateHandle(effe)] = dboEffe;
					TraceUpdate(ts, dboEffe);
				}
			}
		}
		internal void UpdateFromGameData(BTriggerSystem ts)
		{
			lock (this.LookupTable)
			{
				foreach (var t in ts.Triggers)
				{
					foreach (var c in t.Conditions)
						this.TryUpdate(ts, c);
					foreach (var e in t.EffectsOnTrue)
						this.TryUpdate(ts, e);
					foreach (var e in t.EffectsOnFalse)
						this.TryUpdate(ts, e);
				}
			}
		}
		public void Save(string path, BDatabaseBase db)
		{
			using (var s = IO.XmlElementStream.CreateForWrite(K_XML_ROOT_NAME))
			{
				s.InitializeAtRootElement();
				s.StreamMode = System.IO.FileAccess.Write;
				s.SetSerializerInterface(XML.BXmlSerializerInterface.GetNullInterface(db));
				this.Serialize(s);

				s.Document.Save(path);
			}
		}
	};
}
