#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.XML
{
	sealed class TriggerDatabaseBuilderSerializer
		: BXmlSerializerInterface
	{
		Phx.BDatabaseBase mDatabase_;
		internal override Phx.BDatabaseBase Database { get { return this.mDatabase_; } }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		public TriggerDatabaseBuilderSerializer(Engine.PhxEngine phx)
		{
			Contract.Requires(phx != null);

			this.mDatabase_ = phx.Database;
			this.TriggerDb = phx.TriggerDb;
		}

		#region IDisposable Members
		public override void Dispose() {}
		#endregion

		void ParseTriggerScript<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
//			s.SetSerializerInterface(this);

			var ts = new Phx.BTriggerSystem();
			ts.Serialize(s);
		}
		void ParseTriggerScriptSansSkrimishAi<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			// This HW script has all the debug info stripped :o
			if (s.StreamName.EndsWith("skirmishai.triggerscript"))
				return;

			this.ParseTriggerScript(s);
		}
		void ParseScenarioScripts<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
//			s.SetSerializerInterface(this);

			foreach (var e in s.ElementsByName(Phx.BTriggerSystem.K_XML_ROOT_NAME))
			{
				using (s.EnterCursorBookmark(e))
					new Phx.BTriggerSystem().Serialize(s);
			}
		}

		void ParseTriggerScripts(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.GAME,   Engine.GameDirectory.TRIGGER_SCRIPTS,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TRIGGER_SCRIPT),
			                        this.ParseTriggerScriptSansSkrimishAi, out result);

			this.ReadDataFilesAsync(Engine.ContentStorage.UPDATE, Engine.GameDirectory.TRIGGER_SCRIPTS,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.TRIGGER_SCRIPT),
			                        this.ParseTriggerScript, out result);
		}
		void ParseAbilities(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.GAME,   Engine.GameDirectory.ABILITY_SCRIPTS,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.ABILITY),
			                        this.ParseTriggerScript, out result);
		}
		void ParsePowers(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.GAME,   Engine.GameDirectory.POWER_SCRIPTS,
			                        Phx.BTriggerSystem.GetFileExtSearchPattern(Phx.BTriggerScriptType.POWER),
			                        this.ParseTriggerScript, out result);
		}
		void ParseScenarios(Engine.PhxEngine e)
		{
			System.Threading.Tasks.ParallelLoopResult result;

			this.ReadDataFilesAsync(Engine.ContentStorage.GAME, Engine.GameDirectory.SCENARIO,
			                        "*.scn",
			                        this.ParseScenarioScripts, out result);
		}

		public void ParseScriptFiles()
		{
			var e = this.GameEngine;

			this.ParseTriggerScripts(e);
			this.ParseAbilities(e);
			this.ParsePowers(e);
			this.ParseScenarios(e);
		}
	};
}