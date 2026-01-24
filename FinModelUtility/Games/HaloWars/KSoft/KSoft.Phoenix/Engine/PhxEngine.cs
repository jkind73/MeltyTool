using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Engine
{
	public partial class PhxEngine
	{
		public PhxEngineBuild Build { get; private set; }

		public bool TargetsXbox360 { get; private set; }

		public GameDirectories Directories { get; private set; }

		public Phx.BDatabaseBase Database { get; private set; }

		public Phx.TriggerDatabase TriggerDb { get; private set; }

		internal Dictionary<XmlFileInfo, XmlFileLoadState> XmlFileLoadStatus { get; private set; }
			= new Dictionary<XmlFileInfo, XmlFileLoadState>();

		public event EventHandler<XmlFileLoadStateChangedArgs> XmlFileLoadStateChanged;

		internal void UpdateFileLoadStatus(XmlFileInfo file, XmlFileLoadState state)
		{
			Contract.Requires(file != null);

			lock (this.XmlFileLoadStatus)
				this.XmlFileLoadStatus[file] = state;

			var handler = this.XmlFileLoadStateChanged;
			if (handler != null)
			{
				var args = new XmlFileLoadStateChangedArgs(file, state);
				handler(this, args);
			}
		}

		public XmlFileLoadState GetFileLoadStatus(XmlFileInfo file)
		{
			Contract.Requires(file != null);

			XmlFileLoadState state;
			lock (this.XmlFileLoadStatus)
				if (!this.XmlFileLoadStatus.TryGetValue(file, out state))
					return XmlFileLoadState.NOT_LOADED;

			return state;
		}

		public bool HasAlreadyPreloaded { get; private set; }
		public virtual bool Preload()
		{
			if (this.HasAlreadyPreloaded)
			{
				Debug.Trace.Engine.TraceDataSansId(System.Diagnostics.TraceEventType.Error,
					"Failed to load preload data: " + "preload has already be performed");
				return false;
			}

			bool success = this.Database.Preload();

			if (success)
			{
				this.HasAlreadyPreloaded = true;
			}

			return success;
		}

		public bool HasAlreadyLoaded { get; private set; }
		public virtual bool Load()
		{
			Exception exception = null;
			bool success = false;

			try
			{
				do
				{
					if (this.HasAlreadyLoaded)
					{
						exception = new Exception("Load has already been performed");
						break;
					}

					if (!this.Database.Load())
					{
						exception = new Exception("Database.Load failed");
						break;
					}

					if (!this.Database.LoadAllTactics())
					{
						exception = new Exception("Database.LoadAllTactics failed");
						break;
					}

					success = true;

				} while (false);
			} catch (Exception ex)
			{
				exception = ex;
			}

			if (!success)
			{
				Debug.Trace.Engine.TraceDataSansId(System.Diagnostics.TraceEventType.Error,
					"Failed to load engine data",
					exception);
			}
			else
			{
				this.HasAlreadyLoaded = true;
			}

			return success;
		}

		public IO.XmlElementStream OpenXmlOrXmbForRead(string fileName)
		{
			string ext = System.IO.Path.GetFileNameWithoutExtension(fileName);

			var xmlOrXmb = ext.Equals(Xmb.XmbFile.K_FILE_EXT, StringComparison.OrdinalIgnoreCase)
				? GetXmlOrXmbFileResult.XMB
				: GetXmlOrXmbFileResult.XML;

			return this.OpenXmlOrXmbForRead(xmlOrXmb, fileName);
		}

		public IO.XmlElementStream OpenXmlOrXmbForRead(GetXmlOrXmbFileResult xmlOrXmb, string fileName)
		{
			switch (xmlOrXmb)
			{
				case GetXmlOrXmbFileResult.XML:
					return new IO.XmlElementStream(fileName, System.IO.FileAccess.Read);

				case GetXmlOrXmbFileResult.XMB:
					return this.OpenXmbForRead(fileName);
			}

			return null;
		}
		public IO.XmlElementStream OpenXmbForRead(string xmbFile)
		{
			var vaSize = this.TargetsXbox360
				? Shell.ProcessorSize.X32
				: Shell.ProcessorSize.X64;
			var endianFormat = Shell.EndianFormat.BIG;

			byte[] fileBytes = System.IO.File.ReadAllBytes(xmbFile);

			using (var xmbMs = new System.IO.MemoryStream(fileBytes, false))
			using (var xmb = new IO.EndianStream(xmbMs, endianFormat, System.IO.FileAccess.Read))
			using (var xmlMs = new System.IO.MemoryStream(IntegerMath.K_MEGA * 1))
			{
				xmb.StreamMode = System.IO.FileAccess.Read;

				Resource.ResourceUtils.XmbToXml(xmb, xmlMs, vaSize);
				// need to do this else we'll get a Root element is missing exception
				xmlMs.Position = 0;

				var xml = new IO.XmlElementStream(xmlMs, System.IO.FileAccess.Read, streamNameOverride: xmbFile);
				return xml;
			}
		}

		public ObjectDatabaseForFileResult GetObjectDatabase(XmlFileInfo file)
		{
			if (file == null)
				return ObjectDatabaseForFileResult.Null;

			var status = this.GetFileLoadStatus(file);
			if (status < XmlFileLoadState.PRELOADED)
			{
				throw new InvalidOperationException(string.Format(
					"GetObjectDatabase called on {0} when its load status was {1}",
					file, status));
			}

			var kvp = this.GetObjectDatabaseForFile(file);

			if (kvp.Key == null)
			{
				throw new InvalidOperationException(string.Format(
					"GetObjectDatabase called on {0} which didn't resolve to a DB"));
			}

			return new ObjectDatabaseForFileResult(file, kvp.Key, kvp.Value);
		}

		protected virtual KeyValuePair<Phx.ProtoDataObjectDatabase, int> GetObjectDatabaseForFile(XmlFileInfo file)
		{
			Phx.ProtoDataObjectDatabase db = null;
			int specificObjectKind = TypeExtensions.K_NONE;

			if (file == Phx.BGameData.KXmlFileInfo)
			{
				db = this.Database.GameData.ObjectDatabase;
				specificObjectKind = PhxUtil.K_OBJECT_KIND_NONE;
			}
			else if (file == Phx.HpBarData.KXmlFileInfo)
			{
				db = this.Database.HpBars.ObjectDatabase;
				specificObjectKind = PhxUtil.K_OBJECT_KIND_NONE;
			}
			else if (file == Phx.BAbility.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.ABILITY;
			}
			else if (file == Phx.BCiv.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.CIV;
			}
			else if (file == Phx.BDamageType.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.DAMAGE_TYPE;
			}
			else if (file == Phx.BProtoImpactEffect.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.IMPACT_EFFECT;
			}
			else if (file == Phx.BLeader.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.LEADER;
			}
			else if (
				file == Phx.BProtoObject.KXmlFileInfo ||
				file == Phx.BProtoObject.KXmlFileInfoUpdate)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.OBJECT;
			}
			else if (file == Phx.BDatabaseBase.KObjectTypesXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.OBJECT_TYPE;
			}
			else if (file == Phx.BProtoPower.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.POWER;
			}
			else if (
				file == Phx.BProtoSquad.KXmlFileInfo ||
				file == Phx.BProtoSquad.KXmlFileInfoUpdate)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.SQUAD;
			}
			else if (file.Directory == GameDirectory.TACTICS)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.TACTIC;
			}
			else if (
				file == Phx.BProtoTech.KXmlFileInfo ||
				file == Phx.BProtoTech.KXmlFileInfoUpdate)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.TECH;
			}
			else if (file == Phx.TerrainTileType.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.TERRAIN_TILE_TYPE;
			}
			else if (file == Phx.BUserClass.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.USER_CLASS;
			}
			else if (file == Phx.BWeaponType.KXmlFileInfo)
			{
				db = this.Database.ObjectDatabase;
				specificObjectKind = (int)Phx.DatabaseObjectKind.WEAPON_TYPE;
			}
			else if (
				file.Directory == GameDirectory.ABILITY_SCRIPTS ||
				file.Directory == GameDirectory.POWER_SCRIPTS ||
				file.Directory == GameDirectory.TRIGGER_SCRIPTS)
			{
				throw new NotImplementedException(file.ToString());
			}

			return new KeyValuePair<Phx.ProtoDataObjectDatabase, int>(db, specificObjectKind);
		}
	};

	public struct ObjectDatabaseForFileResult
	{
		public XmlFileInfo File { get; private set; }
		public Phx.ProtoDataObjectDatabase Database { get; private set; }
		public int SpecificObjectKind { get; private set; }

		public ObjectDatabaseForFileResult(XmlFileInfo file, Phx.ProtoDataObjectDatabase db, int objectKind)
		{
			this.File = file;
			this.Database = db;
			this.SpecificObjectKind = objectKind;
		}

		public bool IsNull { get { return this.Database == null; } }

		public static ObjectDatabaseForFileResult Null { get {
			return new ObjectDatabaseForFileResult(null, null, TypeExtensions.K_NONE);
		} }
	};
}