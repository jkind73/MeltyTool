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
	public abstract class BXmlSerializerInterface
		: IDisposable
	{
		#region NullInterface
		sealed class NullInterface : BXmlSerializerInterface
		{
			Phx.BDatabaseBase mDatabase_;
			internal override Phx.BDatabaseBase Database { get { return this.mDatabase_; } }

			public NullInterface(Phx.BDatabaseBase db) {
				this.mDatabase_ = db; }

			public override void Dispose() {}
		};
		public static BXmlSerializerInterface GetNullInterface(Phx.BDatabaseBase db)
		{
			Contract.Requires(db != null);

			return new NullInterface(db);
		}
		#endregion

		internal abstract Phx.BDatabaseBase Database { get; }
		internal Engine.PhxEngine GameEngine { get { return this.Database.Engine; } }

		#region IDisposable Members
		public abstract void Dispose();
		#endregion

		#region Stream files utils
		static void SetupStream(IO.XmlElementStream s, FA mode, BXmlSerializerInterface xs)
		{
			s.IgnoreCaseOnEnums = true;
			s.ExceptionOnEnumParseFail = true;
			s.StreamMode = mode;
			s.InitializeAtRootElement();
			s.SetSerializerInterface(xs);
		}

		public bool TryStreamData<TContext>(
			Engine.XmlFileInfo xfi, FA mode,
			Action<IO.XmlElementStream, TContext> streamProc, TContext ctxt,
			string ext = null)
		{
			Contract.Requires(xfi != null);
			Contract.Requires(streamProc != null);

			bool result = false;

			if (mode == FA.Read)
			{
				result = true;
				System.IO.FileInfo file;
				var xmlOrXmb = this.GameEngine.Directories.TryGetXmlOrXmbFile(xfi.Location, xfi.Directory, xfi.FileName, out file, ext);

				if (xmlOrXmb == Engine.GetXmlOrXmbFileResult.FILE_NOT_FOUND)
				{
					this.GameEngine.UpdateFileLoadStatus(xfi, Engine.XmlFileLoadState.FILE_DOES_NOT_EXIST);
					throw new System.IO.FileNotFoundException("Neither XML or XMB exists: " + file.FullName);
				}

				try
				{
					if (result) using (var s = this.GameEngine.OpenXmlOrXmbForRead(xmlOrXmb, file.FullName))
					{
						SetupStream(s, mode, this);
						streamProc(s, ctxt);

						this.GameEngine.UpdateFileLoadStatus(xfi, Engine.XmlFileLoadState.LOADED);
					}
				} catch (Exception ex)
				{
					ex.UnusedExceptionVar();
					this.GameEngine.UpdateFileLoadStatus(xfi, Engine.XmlFileLoadState.FAILED);
					throw;
				}
			}
			else if (mode == FA.Write)
			{
				System.IO.FileInfo file;
				result = this.GameEngine.Directories.TryGetFile(xfi.Location, xfi.Directory, xfi.FileName, out file, ext);

				if (Engine.XmlFileInfo.RespectWritableFlag)
					result = result && xfi.Writable;

				if (result) using (var s = IO.XmlElementStream.CreateForWrite(xfi.RootName))
				{
					SetupStream(s, mode, this);
					streamProc(s, ctxt);
					s.Document.Save(file.FullName);
				}
			}

			return result;
		}
		public bool TryStreamData(
			Engine.XmlFileInfo xfi, FA mode,
			Action<IO.XmlElementStream> streamProc,
			string ext = null)
		{
			Contract.Requires(xfi != null);
			Contract.Requires(streamProc != null);

			bool result = false;

			if (mode == FA.Read)
			{
				result = true;
				System.IO.FileInfo file;
				var xmlOrXmb = this.GameEngine.Directories.TryGetXmlOrXmbFile(xfi.Location, xfi.Directory, xfi.FileName, out file, ext);

				if (xmlOrXmb == Engine.GetXmlOrXmbFileResult.FILE_NOT_FOUND)
				{
					this.GameEngine.UpdateFileLoadStatus(xfi, Engine.XmlFileLoadState.FILE_DOES_NOT_EXIST);
					throw new System.IO.FileNotFoundException("Neither XML or XMB exists: " + file.FullName);
				}

				try
				{
					if (result) using (var s = this.GameEngine.OpenXmlOrXmbForRead(xmlOrXmb, file.FullName))
					{
						SetupStream(s, mode, this);
						streamProc(s);

						this.GameEngine.UpdateFileLoadStatus(xfi, Engine.XmlFileLoadState.LOADED);
					}
				} catch (Exception ex)
				{
					ex.UnusedExceptionVar();
					this.GameEngine.UpdateFileLoadStatus(xfi, Engine.XmlFileLoadState.FAILED);
					throw;
				}
			}
			else if (mode == FA.Write)
			{
				System.IO.FileInfo file;
				result = this.GameEngine.Directories.TryGetFile(xfi.Location, xfi.Directory, xfi.FileName, out file, ext);

				if (Engine.XmlFileInfo.RespectWritableFlag)
					result = result && xfi.Writable;

				if (result) using (var s = IO.XmlElementStream.CreateForWrite(xfi.RootName))
				{
					SetupStream(s, mode, this);
					streamProc(s);
					s.Document.Save(file.FullName);
				}
			}

			return result;
		}
		public void ReadDataFilesAsync(
			Engine.ContentStorage loc, Engine.GameDirectory gameDir, string searchPattern,
			Action<IO.XmlElementStream> streamProc,
			out ParallelLoopResult result)
		{
			Contract.Requires(!string.IsNullOrEmpty(searchPattern));

			result = Parallel.ForEach(this.GameEngine.Directories.GetFiles(loc, gameDir, searchPattern), (filename) =>
			{
				const FA kMode = FA.Read;

				using (var s = new IO.XmlElementStream(filename, kMode))
				{
					SetupStream(s, kMode, this);
					streamProc(s);
				}
			});
		}
		#endregion

		public bool StreamStringId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string xmlName,
			ref int value, IO.TagElementNodeType xmlSource = XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XmlUtil.K_NO_XML_NAME));

			bool wasStreamed = false;

			if (xmlSource == XmlUtil.K_SOURCE_ELEMENT)
				wasStreamed = s.StreamElementOpt(xmlName, ref value, Predicates.IsNotNone);
			else if (xmlSource == XmlUtil.K_SOURCE_ATTR)
				wasStreamed = s.StreamAttributeOpt(xmlName, ref value, Predicates.IsNotNone);
			else if (xmlSource == XmlUtil.K_SOURCE_CURSOR)
			{
				wasStreamed = true;
				s.StreamCursor(ref value);
			}

			if (s.IsReading)
			{
				if (value.IsNotNone())
					this.Database.AddStringIdReference(value);
			}

			return wasStreamed;
		}

		[System.Diagnostics.Conditional("TRACE")]
		protected static void TraceUndefinedHandle<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, string name,
			string xmlName,
			int id, string kind)
			where TDoc : class
			where TCursor : class
		{
			Contract.Assert(s.IsReading);

			var lineInfo = Text.TextLineInfo.Empty;
			var cursorName = "<unknown element>";
			var textStream = s as IO.TagElementTextStream<TDoc, TCursor>;
			if (textStream != null)
			{
				cursorName = textStream.CursorName;
				lineInfo = textStream.TryGetLastReadLineInfo();
			}

			Debug.Trace.Xml.TraceEvent(System.Diagnostics.TraceEventType.Warning, TypeExtensions.K_NONE,
				"{0} ({1}): Generated UndefinedHandle for '{2}.{3}' ({4}). {5}={6}",
				s.StreamName, Text.TextLineInfo.ToString(lineInfo, verboseString: true),
				cursorName, xmlName ?? "InnerText",
				kind, name, PhxUtil.GetUndefinedReferenceDataIndex(id).ToString());
		}


		protected static bool ToLowerName(Phx.DatabaseObjectKind kind)
		{
			switch (kind)
			{
#if false
			case Phx.DatabaseObjectKind.Object:
			case Phx.DatabaseObjectKind.Unit:
				return Phx.BProtoObject.kBListXmlParams.ToLowerDataNames;

			case Phx.DatabaseObjectKind.Squad:
				return Phx.BProtoSquad.kBListXmlParams.ToLowerDataNames;

			case Phx.DatabaseObjectKind.Tech:
				return Phx.BProtoTech.kBListXmlParams.ToLowerDataNames;
#endif

			default:
				return false;
			}
		}
		public bool StreamTypeName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string xmlName, ref int dbid,
			Phx.GameDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XmlUtil.K_NO_XML_NAME));

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
					dbid = this.Database.GetId(kind, idName);
					Contract.Assert(dbid.IsNotNone());
					if (PhxUtil.IsUndefinedReferenceHandle(dbid))
						TraceUndefinedHandle(s, idName, xmlName, dbid, kind.ToString());
				}
				else
					dbid = TypeExtensions.K_NONE;
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					wasStreamed = false;
					return wasStreamed;
				}

				idName = this.Database.GetName(kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(idName));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);
			}

			return wasStreamed;
		}
		public bool StreamHpBarName<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string xmlName, ref int dbid,
			Phx.HpBarDataObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XmlUtil.K_NO_XML_NAME));

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
					dbid = this.Database.GetId(kind, idName);
					Contract.Assert(dbid.IsNotNone());
					if (PhxUtil.IsUndefinedReferenceHandle(dbid))
						TraceUndefinedHandle(s, idName, xmlName, dbid, kind.ToString());
				}
				else
					dbid = TypeExtensions.K_NONE;
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					wasStreamed = false;
					return wasStreamed;
				}

				idName = this.Database.GetName(kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(idName));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);
			}

			return wasStreamed;
		}
		public bool StreamDbid<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s,
			string xmlName, ref int dbid,
			Phx.DatabaseObjectKind kind,
			bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XmlUtil.K_NO_XML_NAME));

			string idName = null;
			bool wasStreamed = true;
			bool toLower = ToLowerName(kind);

			if (s.IsReading)
			{
				if (isOptional)
					wasStreamed = s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);

				if (wasStreamed)
				{
					dbid = this.Database.GetId(kind, idName);
					Contract.Assert(dbid.IsNotNone());
					if (PhxUtil.IsUndefinedReferenceHandle(dbid))
						TraceUndefinedHandle(s, idName, xmlName, dbid, kind.ToString());
				}
				else
					dbid = TypeExtensions.K_NONE;
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					wasStreamed = false;
					return wasStreamed;
				}

				idName = this.Database.GetName(kind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(idName));

				if (isOptional)
					s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
				else
					s.StreamString(xmlName, ref idName, toLower, xmlSource, intern: true);
			}

			return wasStreamed;
		}

		public bool StreamDbid<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName, List<int> dbidList
			, Phx.DatabaseObjectKind kind
			, bool isOptional = true, IO.TagElementNodeType xmlSource = XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			Contract.Requires(xmlSource.RequiresName() == (xmlName != XmlUtil.K_NO_XML_NAME));
			Contract.Requires(xmlSource != IO.TagElementNodeType.ATTRIBUTE);

			bool wasStreamed = false;

			if (s.IsReading)
			{
				XmlUtil.ReadDetermineListSize(s, dbidList);

				foreach (var n in XmlUtil.ReadGetNodes(s, xmlName, xmlSource))
				{
					using (s.EnterCursorBookmark(n))
					{
						int dbid = TypeExtensions.K_NONE;
						if (this.StreamDbid(s, xmlName, ref dbid, kind, isOptional, xmlSource))
						{
							wasStreamed = true;
							dbidList.Add(dbid);
						}
					}
				}
			}
			else if (s.IsWriting && dbidList.Count > 0)
			{
				wasStreamed = true;

				foreach (int dbid in dbidList)
				{
					int dbidCopy = dbid;
					using (s.EnterCursorBookmark(xmlName))
						this.StreamDbid(s, xmlName, ref dbidCopy, kind, isOptional, xmlSource);
				}
			}

			return wasStreamed;
		}

		public bool StreamTactic<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s
			, string xmlName
			, ref int dbid
			, IO.TagElementNodeType xmlSource = XmlUtil.K_SOURCE_ELEMENT)
			where TDoc : class
			where TCursor : class
		{
			const Phx.DatabaseObjectKind kDbKind = Phx.DatabaseObjectKind.TACTIC;

			Contract.Requires(xmlSource.RequiresName() == (xmlName != XmlUtil.K_NO_XML_NAME));

			string idName = null;
			bool wasStreamed = true;
			bool toLower = false;

			if (s.IsReading)
			{
				wasStreamed = s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);

				if (wasStreamed)
				{
					idName = System.IO.Path.GetFileNameWithoutExtension(idName);

					dbid = this.Database.GetId(kDbKind, idName);
					Contract.Assert(dbid.IsNotNone(), idName);

					if (PhxUtil.IsUndefinedReferenceHandle(dbid))
						TraceUndefinedHandle(s, idName, xmlName, dbid, kDbKind.ToString());
				}
			}
			else if (s.IsWriting)
			{
				if (dbid.IsNone())
				{
					wasStreamed = false;
					return wasStreamed;
				}

				idName = this.Database.GetName(kDbKind, dbid);
				Contract.Assert(!string.IsNullOrEmpty(idName));

				idName += Phx.BTacticData.K_FILE_EXT;
				s.StreamStringOpt(xmlName, ref idName, toLower, xmlSource, intern: true);
			}

			return wasStreamed;
		}

		/// <summary>Stream the current element's Text as a a string</summary>
		internal static void StreamStringValue<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			ref string value)
			where TDoc : class
			where TCursor : class
		{
			s.StreamCursor(ref value);
		}
		/// <summary>Stream the current element's Text as a DamageType</summary>
		internal static void StreamDamageType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.BDamageTypeReference] ref int damangeType)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDbid(s, XmlUtil.K_NO_XML_NAME, ref damangeType, Phx.DatabaseObjectKind.DAMAGE_TYPE,
				false, XmlUtil.K_SOURCE_CURSOR);
		}
		/// <summary>Stream the current element's Text as a ObjectType</summary>
		internal static void StreamObjectType<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.ObjectTypeReference] ref int objectType)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDbid(s, XmlUtil.K_NO_XML_NAME, ref objectType, Phx.DatabaseObjectKind.OBJECT_TYPE,
				false, XmlUtil.K_SOURCE_CURSOR);
		}
		/// <summary>Stream the current element's Text as a ProtoObject</summary>
		internal static void StreamObjectId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.BProtoObjectReference] ref int objectProtoId)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDbid(s, XmlUtil.K_NO_XML_NAME, ref objectProtoId, Phx.DatabaseObjectKind.OBJECT,
				false, XmlUtil.K_SOURCE_CURSOR);
		}
		/// <summary>Stream the current element's Text as a ProtoSquad</summary>
		internal static void StreamSquadId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.BProtoSquadReference] ref int squadProtoId)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDbid(s, XmlUtil.K_NO_XML_NAME, ref squadProtoId, Phx.DatabaseObjectKind.SQUAD,
				false, XmlUtil.K_SOURCE_CURSOR);
		}
		/// <summary>Stream the current element's Text as a ProtoTech</summary>
		internal static void StreamTechId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			[Phx.Meta.BProtoTechReference] ref int techProtoId)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDbid(s, XmlUtil.K_NO_XML_NAME, ref techProtoId, Phx.DatabaseObjectKind.OBJECT,
				false, XmlUtil.K_SOURCE_CURSOR);
		}
		/// <summary>Stream the current element's Text as a ProtoObject or ObjectType</summary>
		internal static void StreamUnitId<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s, BXmlSerializerInterface xs,
			ref int unitProtoId)
			where TDoc : class
			where TCursor : class
		{
			xs.StreamDbid(s, XmlUtil.K_NO_XML_NAME, ref unitProtoId, Phx.DatabaseObjectKind.UNIT,
				false, XmlUtil.K_SOURCE_CURSOR);
		}
	};
}