using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	using PhxEngineBuild = Engine.PhxEngineBuild;

	partial class BDatabaseXmlSerializerBase
	{
		public enum StreamXmlStage
		{
			PRELOAD,
			STREAM,
			STREAM_UPDATES,

			K_NUMBER_OF
		};
		public delegate void StreamXmlCallback(IO.XmlElementStream s);
		public sealed class StreamXmlContextData
		{
			public Engine.ProtoDataXmlFileInfo protoFileInfo;
			public Engine.XmlFileInfo FileInfo { get { return this.protoFileInfo.fileInfo; } }
			public Engine.XmlFileInfo FileInfoWithUpdates { get { return this.protoFileInfo.fileInfoWithUpdates; } }
			public Action<IO.XmlElementStream> preload;
			public Action<IO.XmlElementStream> stream;
			public Action<IO.XmlElementStream> streamUpdates;

			public StreamXmlContextData(Engine.ProtoDataXmlFileInfo protoFileInfo)
			{
				this.protoFileInfo = protoFileInfo;
			}
		};
		private List<StreamXmlContextData> mStreamXmlContexts_;
		private void SetupStreamXmlContexts()
		{
			if (this.mStreamXmlContexts_ != null)
				return;

			// #NOTE place new DatabaseObjectKind code here

			this.mStreamXmlContexts_ = new List<StreamXmlContextData>()
			{
				#region Lists
				new StreamXmlContextData(Phx.LocStringTable.KProtoFileInfoEnglish)
				{
					preload= this.PreloadStringTable,
				},
				new StreamXmlContextData(Phx.BDatabaseBase.KObjectTypesProtoFileInfo)
				{
					preload= this.StreamXmlObjectTypes,
				},
				new StreamXmlContextData(Phx.BDamageType.KProtoFileInfo)
				{
					preload= this.PreloadDamageTypes,
					stream= this.StreamXmlDamageTypes,
				},
				new StreamXmlContextData(Phx.BProtoImpactEffect.KProtoFileInfo)
				{
					preload= this.StreamXmlImpactEffects,
				},
				new StreamXmlContextData(Phx.TerrainTileType.KProtoFileInfo)
				{
					preload= this.StreamXmlTerrainTileTypes,
				},
				new StreamXmlContextData(Phx.BWeaponType.KProtoFileInfo)
				{
					preload= this.StreamXmlWeaponTypes,
				},
				new StreamXmlContextData(Phx.BUserClass.KProtoFileInfo)
				{
					preload= this.StreamXmlUserClasses,
				},
				#endregion

				#region GameData
				new StreamXmlContextData(Phx.HpBarData.KProtoFileInfo)
				{
					stream= this.StreamXmlHpBars,
				},
				new StreamXmlContextData(Phx.BGameData.KProtoFileInfo)
				{
					stream= this.StreamXmlGameData,
				},
				new StreamXmlContextData(Phx.BAbility.KProtoFileInfo)
				{
					stream= this.StreamXmlAbilities,
				},
				#endregion

				#region ProtoData
				new StreamXmlContextData(Phx.BProtoObject.KProtoFileInfo)
				{
					preload= this.PreloadObjects,
					stream= this.StreamXmlObjects,
					streamUpdates= this.StreamXmlObjectsUpdate,
				},
				new StreamXmlContextData(Phx.BProtoSquad.KProtoFileInfo)
				{
					preload= this.PreloadSquads,
					stream= this.StreamXmlSquads,
					streamUpdates= this.StreamXmlSquadsUpdate,
				},
				new StreamXmlContextData(Phx.BProtoPower.KProtoFileInfo)
				{
					preload= this.PreloadPowers,
					stream= this.StreamXmlPowers,
				},
				new StreamXmlContextData(Phx.BProtoTech.KProtoFileInfo)
				{
					preload= this.PreloadTechs,
					stream= this.StreamXmlTechs,
					streamUpdates= this.StreamXmlTechsUpdate,
				},

				new StreamXmlContextData(Phx.BCiv.KProtoFileInfo)
				{
					//Preload=PreloadCivs,
					stream= this.StreamXmlCivs,
				},
				new StreamXmlContextData(Phx.BLeader.KProtoFileInfo)
				{
					//Preload=PreloadLeaders,
					stream= this.StreamXmlLeaders,
				},
				#endregion
			};
		}
		private void ProcessStreamXmlContexts(ref bool r, FA mode)
		{
			this.ProcessStreamXmlContexts(ref r, mode
			                              , StreamXmlStage.PRELOAD, StreamXmlStage.K_NUMBER_OF
			                              , Engine.XmlFilePriority.LISTS, Engine.XmlFilePriority.K_NUMBER_OF);
		}

		private struct ProcessStreamXmlContextStageArgs
		{
			public FA mode;
			public StreamXmlStage stage;
			public Engine.XmlFilePriority firstPriority;
			public Engine.XmlFilePriority lastPriorityPlusOne;

			public List<Task<bool>> tasks;
			public List<Exception> taskExceptions;

			public ProcessStreamXmlContextStageArgs(FA mode, StreamXmlStage stage
				, Engine.XmlFilePriority firstPriority
				, Engine.XmlFilePriority lastPriorityPlusOne)
			{
				this.mode = mode;
				this.stage = stage;
				this.firstPriority = firstPriority;
				this.lastPriorityPlusOne = lastPriorityPlusOne;

				this.tasks = null;
				this.taskExceptions = null;
				this.tasks = [];
				this.taskExceptions = [];
			}

			public bool UpdateResultWithTaskResults(ref bool r)
			{
				PhxUtil.UpdateResultWithTaskResults(ref r, this.tasks, this.taskExceptions);

				return r;
			}

			public void ClearTaskData()
			{
				this.tasks.Clear();
				this.taskExceptions.Clear();
			}
		};

		private void ProcessStreamXmlContexts(ref bool r, FA mode
			, StreamXmlStage firstStage// = StreamXmlStage.Preload
			, StreamXmlStage lastStagePlusOne// = StreamXmlStage.kNumberOf
			, Engine.XmlFilePriority firstPriority = Engine.XmlFilePriority.LISTS
			, Engine.XmlFilePriority lastPriorityPlusOne = Engine.XmlFilePriority.K_NUMBER_OF
			)
		{
			this.SetupStreamXmlContexts();

			for (var s = firstStage; s < lastStagePlusOne; s++)
			{
				var args = new ProcessStreamXmlContextStageArgs(mode, s, firstPriority, lastPriorityPlusOne);
				this.ProcessStreamXmlContextStage(ref r, args);
			}
		}

		private void ProcessStreamXmlContextStage(ref bool r, ProcessStreamXmlContextStageArgs args)
		{
			var mode = args.mode;

			for (var p = args.firstPriority; p < args.lastPriorityPlusOne; p++)
			{
				foreach (var ctxt in this.mStreamXmlContexts_)
				{
					if (ctxt.protoFileInfo.priority != p)
						continue;

					switch (args.stage)
					{
						#region Preload
						case StreamXmlStage.PRELOAD:
						{
							if (ctxt.preload == null)
								break;

							var task = Task<bool>.Factory.StartNew(() => this.TryStreamData(ctxt.FileInfo, mode, ctxt.preload));
							args.tasks.Add(task);
						} break;
						#endregion
						#region Stream
						case StreamXmlStage.STREAM:
						{
							if (ctxt.stream == null)
								break;

								var task = Task<bool>.Factory.StartNew(() => this.TryStreamData(ctxt.FileInfo, mode, ctxt.stream));
								args.tasks.Add(task);
						} break;
						#endregion
						#region StreamUpdates
						case StreamXmlStage.STREAM_UPDATES:
						{
							if (ctxt.FileInfoWithUpdates == null)
								break;
							if (ctxt.streamUpdates == null)
								break;

							var task = Task<bool>.Factory.StartNew(() => this.TryStreamData(ctxt.FileInfoWithUpdates, mode, ctxt.streamUpdates));
							args.tasks.Add(task);
						} break;
						#endregion
					}
				}

				if (!args.UpdateResultWithTaskResults(ref r))
				{
					var inner = args.taskExceptions.ToAggregateExceptionOrNull().GetOnlyExceptionOrAllWhenAggregate();

					throw new InvalidOperationException(string.Format(
						"Failed to process one or more files for priority={0}",
						p),
						inner);
				}

				args.ClearTaskData();
			}
		}

		void StreamTacticsAsync(ref bool r, FA mode)
		{
			var tactics = this.Database.Tactics;
			var tasks = new List<Task<bool>>(tactics.Count);
			var taskExceptions = new List<Exception>(tactics.Count);

			foreach (var tactic in tactics)
			{
				if (mode == FA.Read)
				{
					if (tactic.SourceXmlFile != null)
						continue;

					tactic.SourceXmlFile = Phx.BTacticData.CreateFileInfo(mode, tactic.Name);
				}
				else if (mode == FA.Write)
				{
					Contract.Assert(tactic.SourceXmlFile != null, tactic.Name);
				}

				var engine = this.GameEngine;
				if (mode == FA.Read)
					engine.UpdateFileLoadStatus(tactic.SourceXmlFile, Engine.XmlFileLoadState.LOADING);

				var arg = tactic;
				var task = Task<bool>.Factory.StartNew((state) =>
				{
					var tactic = state as Phx.BTacticData;
					return this.TryStreamData(tactic.SourceXmlFile, mode, this.StreamTactic, tactic, Phx.BTacticData.K_FILE_EXT);
				}, arg);
				tasks.Add(task);
			}
			PhxUtil.UpdateResultWithTaskResults(ref r, tasks, taskExceptions);

			if (!r)
			{
				Debug.Trace.Xml.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.K_NONE,
					"Failed to " + mode + " tactics",
					taskExceptions.ToAggregateExceptionOrNull().GetOnlyExceptionOrAllWhenAggregate());
			}
		}
		bool StreamTactics(FA mode)
		{
			if (this.GameEngine.Build == PhxEngineBuild.ALPHA)
			{
				Debug.Trace.Xml.TraceInformation("BDatabaseXmlSerializer: Alpha build detected, skipping Tactics streaming");
				return false;
			}

			bool r = true;
			this.StreamTacticsAsync(ref r, mode);
			return r;
		}

	private bool mIsPreloading_;
		protected bool IsNotPreloading { get { return !this.mIsPreloading_; } }
		public bool Preload()
		{
			if (this.Database.LoadState == Phx.DatabaseLoadState.FAILED)
			{
				Debug.Trace.Phoenix.TraceInformation("Not preloading Database because an earlier load stage failed");
				return false;
			}
			if (this.Database.LoadState >= Phx.DatabaseLoadState.PRELOADING)
			{
				Debug.Trace.Phoenix.TraceInformation("Skipping preloading of Database because it already is at a later stage");
				return true;
			}

			const FA kMode = FA.Read;

			this.mIsPreloading_ = true;
			this.Database.LoadState = Phx.DatabaseLoadState.PRELOADING;

			this.AutoIdSerializersInitialize();
			this.PreStreamXml(kMode);

			bool r = true;
			this.ProcessStreamXmlContexts(ref r, kMode
			                              , StreamXmlStage.PRELOAD
			                              , StreamXmlStage.STREAM);

			this.PostStreamXml(kMode);
			//AutoIdSerializersDispose();

			this.mIsPreloading_ = false;
			this.Database.LoadState = r
				? Phx.DatabaseLoadState.PRELOADED
				: Phx.DatabaseLoadState.FAILED;

			return r;
		}

		public bool Load()
		{
			if (this.Database.LoadState == Phx.DatabaseLoadState.FAILED)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Database because an earlier load stage failed");
				return false;
			}
			if (this.Database.LoadState >= Phx.DatabaseLoadState.LOADING)
			{
				Debug.Trace.Phoenix.TraceInformation("Skipping loading of Database because it already is at a later stage");
				return true;
			}

			const FA kMode = FA.Read;

			this.Database.LoadState = Phx.DatabaseLoadState.LOADING;
			this.AutoIdSerializersInitialize();
			this.PreStreamXml(kMode);

			bool r = true;
			this.ProcessStreamXmlContexts(ref r, kMode
			                              , StreamXmlStage.STREAM
			                              , StreamXmlStage.K_NUMBER_OF);

			this.PostStreamXml(kMode);
			//AutoIdSerializersDispose();

			this.Database.LoadState = r
				? Phx.DatabaseLoadState.LOADED
				: Phx.DatabaseLoadState.FAILED;

			return r;
		}

		public bool LoadAllTactics()
		{
			if (this.Database.LoadState == Phx.DatabaseLoadState.FAILED)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Tactics because an earlier load stage failed");
				return false;
			}
			if (this.Database.LoadState < Phx.DatabaseLoadState.PRELOADED)
			{
				Debug.Trace.Phoenix.TraceInformation("Not loading Tactics because an earlier the database is not at least preloaded");
				return true;
			}

			const FA kMode = FA.Read;

			this.PreStreamXml(kMode);

			bool r = this.StreamTactics(kMode);

			this.PostStreamXml(kMode);

			if (!r)
				this.Database.LoadState = Phx.DatabaseLoadState.FAILED;

			return r;
		}
	};
}