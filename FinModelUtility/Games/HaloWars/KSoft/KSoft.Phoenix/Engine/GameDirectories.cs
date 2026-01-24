using System;
using System.Collections.Generic;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Engine
{
	public enum GetXmlOrXmbFileResult
	{
		FILE_NOT_FOUND,
		XML,
		XMB,

		K_NUMBER_OF
	};

	public sealed class GameDirectories
	{
		#region Art
		const string K_ART_PATH_ = @"art\";

		const string K_PARTICLE_EFFECT_PATH_ = @"effects\";
		const string K_SKY_BOX_PATH_ = @"environment\sky\";
		const string K_TERRAIN_TEXTURES_PATH_ = @"terrain\";
		const string K_FLASH_UI_PATH_ = @"ui\flash\";
		const string K_MINIMAP_PATH_ = @"ui\flash\minimaps\";
		const string K_LOADMAP_PATH_ = @"ui\flash\pregame\textures\";
		const string K_CLIP_ART_PATH_ = @"clipart\";
		const string K_ROADS_PATH_ = @"roads\";
		const string K_FOLIAGE_PATH_ = @"foliage\";
		#endregion
		#region Data
		const string K_DATA_PATH_ = @"data\";

		const string K_ABILITIES_PATH_ = @"abilities\";
		const string K_AI_PATH_ = @"ai\";
		const string K_POWERS_PATH_ = @"powers\";
		const string K_TACTICS_PATH_ = @"tactics\";
		const string K_TRIGGER_SCRIPTS_PATH_ = @"triggerscripts\";
		#endregion
		const string K_PHYSICS_PATH_ = @"physics\";
		const string K_SCENARIOS_PATH_ = @"scenario\";
		const string K_SOUND_PATH_ = @"sound\";

	/*public*/
	string RootDirectory { get; /*private*/ set; }
		/*public*/ string UpdateDirectory { get; /*private*/ set; }
		bool UpdateDirectoryIsValid { get { return this.UpdateDirectory != null; } }
		public bool UseTitleUpdates { get; set; }

		public GameDirectories(string root, string updateRoot = null)
		{
			this.RootDirectory = root;
			this.UpdateDirectory = updateRoot;
			this.UseTitleUpdates = true;

			// Leave some breadcrumbs for the programmer in the event that they're confused as why an update file isn't loading.
			if (!this.UpdateDirectoryIsValid)
				Debug.Trace.Engine.TraceInformation("GameDirectories: No matching update directory for '{0}'", updateRoot);

			this.ArtPath = K_ART_PATH_;//Path.Combine(RootDirectory, kArtPath);
			this.ParticleEffectPath = Path.Combine(this.ArtPath, K_PARTICLE_EFFECT_PATH_);
			this.SkyBoxPath = Path.Combine(this.ArtPath, K_SKY_BOX_PATH_);
			this.TerrainTexturesPath = Path.Combine(this.ArtPath, K_TERRAIN_TEXTURES_PATH_);
			this.FlashUiPath = Path.Combine(this.ArtPath, K_FLASH_UI_PATH_);
			this.MinimapPath = Path.Combine(this.ArtPath, K_MINIMAP_PATH_);
			this.LoadmapPath = Path.Combine(this.ArtPath, K_LOADMAP_PATH_);
			this.ClipArtPath = Path.Combine(this.ArtPath, K_CLIP_ART_PATH_);
			this.RoadsPath = Path.Combine(this.ArtPath, K_ROADS_PATH_);
			this.FoliagePath = Path.Combine(this.ArtPath, K_FOLIAGE_PATH_);

			this.DataPath = K_DATA_PATH_;//Path.Combine(RootDirectory, kDataPath);
			this.AbilityScriptsPath = Path.Combine(this.DataPath, K_ABILITIES_PATH_);
			this.AiDataPath = Path.Combine(this.DataPath, K_AI_PATH_);
			this.PowerScriptsPath = Path.Combine(this.DataPath, K_POWERS_PATH_);
			this.TacticsPath = Path.Combine(this.DataPath, K_TACTICS_PATH_);
			this.TriggerScriptsPath = Path.Combine(this.DataPath, K_TRIGGER_SCRIPTS_PATH_);

			this.PhysicsPath = K_PHYSICS_PATH_;
			this.ScenarioPath = K_SCENARIOS_PATH_;
			this.SoundPath = K_SOUND_PATH_;
		}

		#region Art
		public string ArtPath { get; protected set; }

		public string ParticleEffectPath { get; protected set; }
		public string SkyBoxPath { get; protected set; }
		public string TerrainTexturesPath { get; protected set; }
		public string FlashUiPath { get; protected set; }
		public string MinimapPath { get; protected set; }
		public string LoadmapPath { get; protected set; }
		public string ClipArtPath { get; protected set; }
		public string RoadsPath { get; protected set; }
		public string FoliagePath { get; protected set; }
		#endregion
		#region Data
		public string DataPath { get; protected set; }

		public string AbilityScriptsPath { get; protected set; }
		public string AiDataPath { get; protected set; }
		public string PowerScriptsPath { get; protected set; }
		public string TacticsPath { get; protected set; }
		public string TriggerScriptsPath { get; protected set; }
		#endregion
		public string PhysicsPath { get; protected set; }
		public string ScenarioPath { get; protected set; }
		public string SoundPath { get; protected set; }

		public string GetContentLocation(ContentStorage location)
		{
			switch (location)
			{
			case ContentStorage.GAME: return this.RootDirectory;
			case ContentStorage.UPDATE:
				return this.UpdateDirectoryIsValid ? this.UpdateDirectory : this.RootDirectory;

			default: throw new NotImplementedException();
			}
		}
		public string GetDirectory(GameDirectory dir)
		{
			switch (dir)
			{
			#region Art
			case GameDirectory.ART: return this.ArtPath;

			#endregion
			#region Data
			case GameDirectory.DATA: return this.DataPath;

			case GameDirectory.ABILITY_SCRIPTS: return this.AbilityScriptsPath;
			case GameDirectory.AI_DATA:         return this.AiDataPath;
			case GameDirectory.POWER_SCRIPTS:   return this.PowerScriptsPath;
			case GameDirectory.TACTICS:        return this.TacticsPath;
			case GameDirectory.TRIGGER_SCRIPTS: return this.TriggerScriptsPath;
			#endregion
			case GameDirectory.PHYSICS:  return this.PhysicsPath;
			case GameDirectory.SCENARIO: return this.ScenarioPath;
			case GameDirectory.SOUND:    return this.SoundPath;

			default: throw new NotImplementedException();
			}
		}
		public string GetAbsoluteDirectory(ContentStorage loc, GameDirectory gameDir)
		{
			string root = this.GetContentLocation(loc);
			string dir = this.GetDirectory(gameDir);
			return Path.Combine(root, dir);
		}

		bool TryGetFileImpl(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			file = null;

			string root = this.GetContentLocation(loc);
			string dir = this.GetDirectory(gameDir);
			string filePath = Path.Combine(root, dir, filename.ToLowerInvariant());
			if (!string.IsNullOrEmpty(ext))
				filePath += ext;

			return (file = new FileInfo(filePath)).Exists;
		}
		bool TryGetFileFromUpdateOrGame(GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			file = null;

			if (!this.UseTitleUpdates)
				return this.TryGetFileImpl(ContentStorage.GAME, gameDir, filename, out file, ext);

			//////////////////////////////////////////////////////////////////////////
			// Try to get the file from the TU storage first
			string dir = this.GetDirectory(gameDir);
			string filePath = Path.Combine(dir, filename.ToLowerInvariant());
			if (!string.IsNullOrEmpty(ext))
				filePath += ext;

			string fullPath;

			if (this.UpdateDirectoryIsValid)
			{
				fullPath = Path.Combine(this.UpdateDirectory, filePath);
				file = new FileInfo(fullPath);
			}

			//////////////////////////////////////////////////////////////////////////
			// No update file exists, fall back to regular game storage
			if (file == null || !file.Exists)
			{
				fullPath = Path.Combine(this.RootDirectory, filePath);
				file = new FileInfo(fullPath);
				return file.Exists;
			}
			return true;
		}
		public bool TryGetFile(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(filename));
			file = null;

			return loc == ContentStorage.UPDATE_OR_GAME
				? this.TryGetFileFromUpdateOrGame(gameDir, filename, out file, ext)
				: this.TryGetFileImpl(loc, gameDir, filename, out file, ext);
		}
		public GetXmlOrXmbFileResult TryGetXmlOrXmbFile(ContentStorage loc, GameDirectory gameDir, string filename, out FileInfo file,
			string ext = null)
		{
			Contract.Requires(!string.IsNullOrEmpty(filename));
			file = null;

			if (this.TryGetFile(loc, gameDir, filename, out file, ext))
				return GetXmlOrXmbFileResult.XML;

			if (ext.IsNotNullOrEmpty())
				filename += ext;

			filename += Xmb.XmbFile.K_FILE_EXT;

			// purposely don't pass ext through in the XMB round
			bool xmbFound = loc == ContentStorage.UPDATE_OR_GAME
				? this.TryGetFileFromUpdateOrGame(gameDir, filename, out file, ext: null)
				: this.TryGetFileImpl(loc, gameDir, filename, out file, ext: null);
			if (xmbFound)
				return GetXmlOrXmbFileResult.XMB;

			return GetXmlOrXmbFileResult.FILE_NOT_FOUND;
		}

		public IEnumerable<string> GetFiles(ContentStorage loc, GameDirectory gameDir, string searchPattern)
		{
			Contract.Requires(loc != ContentStorage.UPDATE_OR_GAME, "Must iterate storages separately");
			Contract.Requires(!string.IsNullOrEmpty(searchPattern));

			string dir = this.GetAbsoluteDirectory(loc, gameDir);

			if (!Directory.Exists(dir))
				throw new DirectoryNotFoundException(dir);

			return Directory.EnumerateFiles(dir, searchPattern);
		}
	};
}