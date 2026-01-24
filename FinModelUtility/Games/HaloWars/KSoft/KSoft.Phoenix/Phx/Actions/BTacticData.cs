using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Phx
{
	[ProtoDataTypeObjectSourceKind(ProtoDataObjectSourceKind.TACTIC_DATA)]
	public sealed class BTacticData
		: Collections.BListAutoIdObject
		, IProtoDataObjectDatabaseProvider
	{
		public const string K_FILE_EXT = ".tactics";

		#region Xml constants
		public const string K_XML_ROOT = "TacticData";

		public static Engine.XmlFileInfo CreateFileInfo(System.IO.FileAccess mode, string filename)
		{
			return new Engine.XmlFileInfo()
			{
				Location = Engine.ContentStorage.UPDATE_OR_GAME,
				Directory = Engine.GameDirectory.TACTICS,

				RootName = K_XML_ROOT,
				FileName = filename,

				Writable = mode == System.IO.FileAccess.Write,
			};
		}
		#endregion

		public string SourceFileName { get; set; }
		public Engine.XmlFileInfo SourceXmlFile { get; set; }
		public bool SourceXmlFileIsXmb { get; set; }

		public Collections.BListAutoId<		BWeapon> Weapons { get; private set; }
			= new Collections.BListAutoId<	BWeapon>();
		public Collections.BListAutoId<		BProtoAction> Actions { get; private set; }
			= new Collections.BListAutoId<	BProtoAction>();

		public BTactic Tactic { get; private set; }
			= new BTactic();

		public BTacticData()
		{
			this.InitializeDatabaseInterfaces();
		}

		#region Database interfaces
		void InitializeDatabaseInterfaces()
		{
			this.Weapons.SetupDatabaseInterface();
			//TacticStates.SetupDatabaseInterface();
			this.Actions.SetupDatabaseInterface();
		}

		internal Collections.IBTypeNames GetNamesInterface(TacticDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != TacticDataObjectKind.NONE);

			switch (kind)
			{
			case TacticDataObjectKind.WEAPON:		return this.Weapons;
			//case TacticDataObjectKind.TacticState:	return TacticStates;
			case TacticDataObjectKind.ACTION:		return this.Actions;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}

		internal Collections.IHasUndefinedProtoMemberInterface GetMembersInterface(TacticDataObjectKind kind)
		{
			Contract.Requires<ArgumentOutOfRangeException>(kind != TacticDataObjectKind.NONE);

			switch (kind)
			{
			case TacticDataObjectKind.WEAPON:		return this.Weapons;
			//case TacticDataObjectKind.TacticState:	return TacticStates;
			case TacticDataObjectKind.ACTION:		return this.Actions;

			default: throw new KSoft.Debug.UnreachableException(kind.ToString());
			}
		}
		#endregion

		#region BListAutoIdObject Members
		internal bool StreamId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName, ref int dbid,
			TacticDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XML.XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XML.XmlUtil.K_NO_XML_NAME));
			Contract.Requires(kind != TacticDataObjectKind.NONE);

			string idName = null;
			bool wasStreamed = true;
			bool toLower = false;

			if (s.IsReading)
			{
				if (isOptional)
					wasStreamed = s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);

				if (wasStreamed)
				{
					IProtoDataObjectDatabaseProvider provider = this;
					dbid = provider.GetId((int)kind, idName);
					Contract.Assert(dbid.IsNotNone());
				}
				else
					dbid = TypeExtensions.K_NONE;
			}
			else if (s.IsWriting && dbid.IsNotNone())
			{
				IProtoDataObjectDatabaseProvider provider = this;
				idName = provider.GetName((int)kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(idName));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);
			}

			return wasStreamed;
		}
		internal static void StreamWeaponId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int id)
			where TDoc : class
			where TCursor : class
		{
			td.StreamId(s, XML.XmlUtil.K_NO_XML_NAME, ref id, TacticDataObjectKind.WEAPON, false, XML.XmlUtil.K_SOURCE_CURSOR);
		}
		internal static void StreamProtoActionId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BTacticData td,
			ref int protoActionId)
			where TDoc : class
			where TCursor : class
		{
			td.StreamId(s, XML.XmlUtil.K_NO_XML_NAME, ref protoActionId, TacticDataObjectKind.ACTION, false, XML.XmlUtil.K_SOURCE_CURSOR);
		}

		public override void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
		{
			using (s.EnterUserDataBookmark(this))
			{
				XML.XmlUtil.Serialize(s, this.Weapons, BWeapon.KBListXmlParams);
				//XML.XmlUtil.Serialize(s, TacticStates, BTacticState.kBListXmlParams);
				XML.XmlUtil.Serialize(s, this.Actions, BProtoAction.KBListXmlParams);
				this.Tactic.Serialize(s);
			}
		}
		#endregion

		#region IProtoDataObjectDatabaseProvider members
		Engine.XmlFileInfo IProtoDataObjectDatabaseProvider.SourceFileReference { get { return this.SourceXmlFile; } }

		Collections.IBTypeNames IProtoDataObjectDatabaseProvider.GetNamesInterface(int objectKind)
		{
			var kind = (TacticDataObjectKind)objectKind;
			return this.GetNamesInterface(kind);
		}

		Collections.IHasUndefinedProtoMemberInterface IProtoDataObjectDatabaseProvider.GetMembersInterface(int objectKind)
		{
			var kind = (TacticDataObjectKind)objectKind;
			return this.GetMembersInterface(kind);
		}
		#endregion
	};
}