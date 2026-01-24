using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.XML
{
	partial class BDatabaseXmlSerializerBase
	{
		public bool forceNoRootElementStreaming = true;

		protected virtual void PreStreamXml(FA mode)
		{
			if (mode == FA.Read)
			{
				if (this.mIsPreloading_)
					this.PreloadTactics();
			}
		}
		protected virtual void PostStreamXml(FA mode)
		{
		}

		#region Tactics
		void PreloadTactics()
		{
			if (this.Database.Tactics.Count > 0)
				return;

			foreach (string tacticFilename in this.GameEngine.Directories.GetFiles(Engine.ContentStorage.GAME, Engine.GameDirectory.TACTICS,
			                                                                     "*" + Phx.BTacticData.K_FILE_EXT))
			{
				string tacticName = System.IO.Path.GetFileNameWithoutExtension(tacticFilename);

				var td = new Phx.BTacticData();
				td.SourceFileName = tacticFilename;

				this.Database.Tactics.DynamicAdd(td, tacticName);
			}

			foreach (string tacticFilename in this.GameEngine.Directories.GetFiles(Engine.ContentStorage.GAME, Engine.GameDirectory.TACTICS,
			                                                                     "*" + Phx.BTacticData.K_FILE_EXT + Xmb.XmbFile.K_FILE_EXT))
			{
				// get rid of .xmb, then .tactics
				string tacticName = System.IO.Path.GetFileNameWithoutExtension(tacticFilename);
				tacticName = System.IO.Path.GetFileNameWithoutExtension(tacticName);
				if (this.Database.Tactics.TryGetId(tacticName).IsNotNone())
					continue;

				var td = new Phx.BTacticData();
				td.SourceFileName = tacticFilename;
				td.SourceXmlFileIsXmb = true;

				this.Database.Tactics.DynamicAdd(td, tacticName);
			}
		}

		void StreamTactic(IO.XmlElementStream s, Phx.BTacticData tactic)
		{
			if (s.IsReading && this.IsNotPreloading)
				this.FixTacticsXml(s, tactic.Name);
			tactic.Serialize(s);
		}
		#endregion

		/// <remarks>For streaming directly from gamedata.xml</remarks>
		void StreamXmlGameData(IO.XmlElementStream s)
		{
			if(s.IsReading)
				this.FixGameDataXml(s);
			this.Database.GameData.StreamGameData(s);
		}

		/// <remarks>For streaming directly from hpbars.xml</remarks>
		void StreamXmlHpBars(IO.XmlElementStream s)
		{
			this.Database.HpBars.StreamHpBarData(s);
		}

		void PreloadStringTable(IO.XmlElementStream s)
		{
			this.Database.EnglishStringTable.Serialize(s);
		}

		#region DatabaseObjectKind stuff
		// #NOTE place new DatabaseObjectKind code here

		void PreloadDamageTypes(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mDamageTypesSerializer_, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from damagetypes.xml</remarks>
		void StreamXmlDamageTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.mDamageTypesSerializer_, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from impacteffects.xml</remarks>
		void StreamXmlImpactEffects(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.mImpactEffectsSerializer_, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from terraintiletypes.xml</remarks>
		void StreamXmlTerrainTileTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.TerrainTileTypes, Phx.TerrainTileType.KBListXmlParams, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from weapontypes.xml</remarks>
		void StreamXmlWeaponTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.WeaponTypes, Phx.BWeaponType.KBListXmlParams, this.forceNoRootElementStreaming);
			if (s.IsReading)
				this.FixWeaponTypes();
		}
		/// <remarks>For streaming directly from UserClasses.xml</remarks>
		void StreamXmlUserClasses(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.UserClasses, Phx.BUserClass.KBListXmlParams, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from objecttypes.xml</remarks>
		void StreamXmlObjectTypes(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.ObjectTypes, Phx.BDatabaseBase.KObjectTypesXmlParams, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from abilities.xml</remarks>
		void StreamXmlAbilities(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.Abilities, Phx.BAbility.KBListXmlParams, this.forceNoRootElementStreaming);
		}

		void PreloadObjects(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mObjectsSerializer_, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from objects.xml</remarks>
		void StreamXmlObjects(IO.XmlElementStream s)
		{
			if (s.IsReading)
			{
				this.FixObjectsXml(s);
			}

			XmlUtil.Serialize(s, this.mObjectsSerializer_, this.forceNoRootElementStreaming);
		}

		void PreloadSquads(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mSquadsSerializer_, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from squads.xml</remarks>
		void StreamXmlSquads(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixSquadsXml(s);
			XmlUtil.Serialize(s, this.mSquadsSerializer_, this.forceNoRootElementStreaming);

			XmlUtil.Serialize(s, this.Database.MergedSquads, Phx.BProtoMergedSquads.KBListXmlParams);
			this.Database.ShieldBubbleTypes.Serialize(s);
		}

		void PreloadPowers(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mPowersSerializer_, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from powers.xml</remarks>
		void StreamXmlPowers(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixPowersXml(s);
			XmlUtil.Serialize(s, this.mPowersSerializer_, this.forceNoRootElementStreaming);
		}

		void PreloadTechs(IO.XmlElementStream s)
		{
			XmlUtil.SerializePreload(s, this.mTechsSerializer_, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from techs.xml</remarks>
		void StreamXmlTechs(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixTechsXml(s);
			XmlUtil.Serialize(s, this.mTechsSerializer_, this.forceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from civs.xml</remarks>
		void StreamXmlCivs(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.Civs, Phx.BCiv.KBListXmlParams, this.forceNoRootElementStreaming);
		}
		/// <remarks>For streaming directly from leaders.xml</remarks>
		void StreamXmlLeaders(IO.XmlElementStream s)
		{
			XmlUtil.Serialize(s, this.Database.Leaders, Phx.BLeader.KBListXmlParams, this.forceNoRootElementStreaming);
		}

		#region Update
		/// <remarks>For streaming directly from objects_update.xml</remarks>
		void StreamXmlObjectsUpdate(IO.XmlElementStream s)
		{
			//if(s.IsReading) FixObjectsXml(s);
			XmlUtil.SerializeUpdate(s, this.mObjectsSerializer_, this.forceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from squads_update.xml</remarks>
		void StreamXmlSquadsUpdate(IO.XmlElementStream s)
		{
			//if (s.IsReading) FixSquadsXml(s);
			XmlUtil.SerializeUpdate(s, this.mSquadsSerializer_, this.forceNoRootElementStreaming);
		}

		/// <remarks>For streaming directly from techs_update.xml</remarks>
		void StreamXmlTechsUpdate(IO.XmlElementStream s)
		{
			if (s.IsReading)
				this.FixTechsXml(s);
			XmlUtil.SerializeUpdate(s, this.mTechsSerializer_, this.forceNoRootElementStreaming);
		}
		#endregion
		#endregion

		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var db = this.Database;

			this.PreStreamXml(s.StreamMode);

			// #NOTE place new DatabaseObjectKind code here
			XmlUtil.Serialize(s, db.DamageTypes, Phx.BDamageType.KBListXmlParams);
			XmlUtil.Serialize(s, db.WeaponTypes, Phx.BWeaponType.KBListXmlParams);
			XmlUtil.Serialize(s, db.ImpactEffects, Phx.BProtoImpactEffect.KBListXmlParams);
			XmlUtil.Serialize(s, db.TerrainTileTypes, Phx.TerrainTileType.KBListXmlParams);
			XmlUtil.Serialize(s, db.UserClasses, Phx.BUserClass.KBListXmlParams);
			XmlUtil.Serialize(s, db.ObjectTypes, Phx.BDatabaseBase.KObjectTypesXmlParams);
			db.HpBars.Serialize(s);
			// #NOTE since we don't preload, the Infection squad map won't properly resolve squads
			db.GameData.Serialize(s);
			XmlUtil.Serialize(s, db.Abilities, Phx.BAbility.KBListXmlParams);
			XmlUtil.Serialize(s, db.Objects, Phx.BProtoObject.KBListXmlParams);
			XmlUtil.Serialize(s, db.Squads, Phx.BProtoSquad.KBListXmlParams);
			XmlUtil.Serialize(s, db.Powers, Phx.BProtoPower.KBListXmlParams);
			XmlUtil.Serialize(s, db.Techs, Phx.BProtoTech.KBListXmlParams);
			XmlUtil.Serialize(s, db.Civs, Phx.BCiv.KBListXmlParams);
			XmlUtil.Serialize(s, db.Leaders, Phx.BLeader.KBListXmlParams);

			XmlUtil.Serialize(s, db.MergedSquads, Phx.BProtoMergedSquads.KBListXmlParams);
			db.ShieldBubbleTypes.Serialize(s);

			using (s.EnterCursorBookmark(Phx.LocStringTable.KBListXmlParams.rootName))
			{
				db.EnglishStringTable.Serialize(s);
			}

			this.PostStreamXml(s.StreamMode);
		}
	};
}