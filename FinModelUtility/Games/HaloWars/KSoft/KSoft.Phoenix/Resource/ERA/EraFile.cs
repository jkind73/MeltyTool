using System;
using System.Collections.Generic;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource
{
	public sealed class EraFile
		: IO.IEndianStreamSerializable
		, IDisposable
	{
		public const string K_EXTENSION_ENCRYPTED = ".era";
		public const string K_EXTENSION_DECRYPTED = ".bin";

		const int K_ALIGNMENT_BIT_ = IntegerMath.K_FOUR_KILO_ALIGNMENT_BIT;
		const string K_FILE_NAMES_TABLE_NAME_ = "_file_names.bin";
		static readonly Memory.Strings.StringMemoryPoolSettings KFileNamesTablePoolConfig = new Memory.Strings.
			StringMemoryPoolSettings(Memory.Strings.StringStorage.CStringAscii, false);

		public string FileName { get; set; }

		private EraFileHeader mHeader_ = new EraFileHeader();
		private List<EraFileEntryChunk> mFiles_ = [];
		private Dictionary<string, string> mLocalFiles_ = new Dictionary<string, string>();
		private Dictionary<string, EraFileEntryChunk> mFileNameToChunk_ = new Dictionary<string, EraFileEntryChunk>();

		public DateTime BuildModeDefaultTimestamp { get; set; }
			= DateTime.Now;

		public Security.Cryptography.TigerHashBase TigerHasher { get; private set; } = Security.Cryptography.PhxHash.CreateHaloWarsTigerHash();

		private int FileChunksFirstIndex { get {
			// First comes the filenames table in mFiles, then all the files defined in the listing
			return 1;
		} }

	/// <summary>Number of files destined for the ERA, excluding the internal filenames table</summary>
	private int FileChunksCount { get {
			// Exclude the first chunk from the count, as it is the filenames table
			return this.mFiles_.Count - this.FileChunksFirstIndex;
		} }

		#region IDisposable Members
		public void Dispose()
		{
			if (this.TigerHasher != null)
			{
				this.TigerHasher.Dispose();
				this.TigerHasher = null;
			}
		}
		#endregion

		public int CalculateHeaderAndFileChunksSize()
		{
			return
				EraFileHeader.CalculateHeaderSize() +
				EraFileEntryChunk.CalculateFileChunksSize(this.mFiles_.Count);
		}

		private void ValidateAdler32(EraFileEntryChunk fileEntry, IO.EndianStream blockStream)
		{
			var actualAdler = fileEntry.ComputeAdler32(blockStream);

			if (actualAdler != fileEntry.adler32)
			{
				string chunkName = !string.IsNullOrEmpty(fileEntry.fileName)
					? fileEntry.fileName
					: "FileNames";//fileEntry.EntryId.ToString("X16");

				throw new InvalidDataException(string.Format(
					"Invalid chunk adler32 for '{0}' offset={1} size={2} " +
					"expected {3} but got {4}",
					chunkName, fileEntry.dataOffset, fileEntry.dataSize.ToString("X8"),
					fileEntry.adler32.ToString("X8"),
					actualAdler.ToString("X8")
					));
			}
		}

		private void ValidateHashes(EraFileEntryChunk fileEntry, IO.EndianStream blockStream)
		{
			fileEntry.ComputeHash(blockStream, this.TigerHasher);

			if (!fileEntry.compressedDataTiger128.EqualsArray(this.TigerHasher.Hash))
			{
				string chunkName = !string.IsNullOrEmpty(fileEntry.fileName)
					? fileEntry.fileName
					: "FileNames";//fileEntry.EntryId.ToString("X16");

				throw new InvalidDataException(string.Format(
					"Invalid chunk hash for '{0}' offset={1} size={2} " +
					"expected {3} but got {4}",
					chunkName, fileEntry.dataOffset, fileEntry.dataSize.ToString("X8"),
					Text.Util.ByteArrayToString(fileEntry.compressedDataTiger128),
					Text.Util.ByteArrayToString(this.TigerHasher.Hash, 0, EraFileEntryChunk.K_COMPRESSSED_DATA_TIGER_HASH_SIZE)
					));
			}

			if (fileEntry.CompressionType == ECF.EcfCompressionType.STORED)
			{
				ulong tiger64;
				this.TigerHasher.TryGetAsTiger64(out tiger64);

				if (fileEntry.DecompressedDataTiger64 != tiger64)
				{
					string chunkName = !string.IsNullOrEmpty(fileEntry.fileName)
						? fileEntry.fileName
						: "FileNames";//fileEntry.EntryId.ToString("X16");

					throw new InvalidDataException(string.Format(
						"Chunk id mismatch for '{0}' offset={1} size={2} " +
						"expected {3} but got {4}",
						chunkName, fileEntry.dataOffset, fileEntry.dataSize.ToString("X8"),
						fileEntry.DecompressedDataTiger64.ToString("X16"),
						Text.Util.ByteArrayToString(this.TigerHasher.Hash, 0, sizeof(ulong))
						));
				}
			}
		}

		private int FileIndexToListingIndex(int fileIndex)
		{
			return fileIndex - 1;
		}

		private void BuildFileNameMaps(TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; )
			{
				var file = this.mFiles_[x];

				EraFileEntryChunk existingFile;
				if (this.mFileNameToChunk_.TryGetValue(file.fileName, out existingFile))
				{
					if (verboseOutput != null)
					{
						verboseOutput.WriteLine("Removing duplicate {0} entry at #{1}",
							file.fileName,
							this.FileIndexToListingIndex(x));
					}

					this.mFiles_.RemoveAt(x);
					continue;
				}

				this.mFileNameToChunk_.Add(file.fileName, file);
				x++;
			}
		}

		private void RemoveXmbFilesWhereXmlExists(TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
			{
				var file = this.mFiles_[x];
				if (!ResourceUtils.IsXmbFile(file.fileName))
					continue;

				string xmlName = file.fileName;
				ResourceUtils.RemoveXmbExtension(ref xmlName);
				EraFileEntryChunk xmlFile;
				if (!this.mFileNameToChunk_.TryGetValue(xmlName, out xmlFile))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tRemoving XMB file #{0} '{1}' from listing since its XML already exists {2}",
					                        this.FileIndexToListingIndex(x),
						file.fileName,
						xmlFile.fileName);

				this.mFiles_.RemoveAt(x);
				x--;
			}
		}

		private void RemoveXmlFilesWhereXmbExists(TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
			{
				var file = this.mFiles_[x];
				if (!ResourceUtils.IsXmlBasedFile(file.fileName))
					continue;

				string xmbName = file.fileName;
				xmbName += Xmb.XmbFile.K_FILE_EXT;
				EraFileEntryChunk xmbFile;
				if (!this.mFileNameToChunk_.TryGetValue(xmbName, out xmbFile))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tRemoving XML file #{0} '{1}' from listing since its XMB already exists {2}",
					                        this.FileIndexToListingIndex(x),
						file.fileName,
						xmbFile.fileName);

				this.mFiles_.RemoveAt(x);
				x--;
			}
		}

		public void TryToReferenceXmlOverXmbFies(string workPath, TextWriter verboseOutput)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
			{
				var file = this.mFiles_[x];
				if (!ResourceUtils.IsXmbFile(file.fileName))
					continue;

				string xmlName = file.fileName;
				ResourceUtils.RemoveXmbExtension(ref xmlName);

				// if the user already references the XML file too, just skip doing anything
				EraFileEntryChunk xmlFile;
				if (this.mFileNameToChunk_.TryGetValue(xmlName, out xmlFile))
					continue;

				// does the XML file exist?
				string xmlPath = Path.Combine(workPath, xmlName);
				if (!File.Exists(xmlPath))
					continue;

				if (verboseOutput != null)
					verboseOutput.WriteLine("\tReplacing XMB ref with {0}",
						xmlName);

				// right now, all we should need to do to update things is remove the XMB mapping and replace it with the XML we found
				bool removed = this.mFileNameToChunk_.Remove(file.fileName);
				file.fileName = xmlName;
				if (removed)
					this.mFileNameToChunk_.Add(xmlName, file);
			}
		}

		#region Xml definition Streaming
		static EraFileEntryChunk GenerateFileNamesTableEntryChunk()
		{
			var chunk = new EraFileEntryChunk();
			chunk.CompressionType = ECF.EcfCompressionType.DEFLATE_STREAM;

			return chunk;
		}
		private void ReadChunks(IO.XmlElementStream s)
		{
			foreach (var n in s.ElementsByName(ECF.EcfChunk.K_XML_ELEMENT_STREAM_NAME))
			{
				var f = new EraFileEntryChunk();
				using (s.EnterCursorBookmark(n))
				{
					f.Read(s, false);
				}

				this.mFiles_.Add(f);
			}
		}
		private void ReadLocalFiles(IO.XmlElementStream s)
		{
			foreach (var n in s.ElementsByName("file"))
			{
				using (s.EnterCursorBookmark(n))
				{
					string fileName = null;
					s.ReadAttribute("name", ref fileName);

					string fileData = "";
					s.ReadCursor(ref fileData);

					if (!string.IsNullOrEmpty(fileName))
						this.mLocalFiles_[fileName] = fileData;
				}
			}
		}

		private void WriteChunks(IO.XmlElementStream s)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
			{
				this.mFiles_[x].Write(s, false);
			}
		}
		private void WriteLocalFiles(IO.XmlElementStream s)
		{
			foreach (var kvp in this.mLocalFiles_)
			{
				string fileName = kvp.Key;
				string fileData = kvp.Value;

				using (s.EnterCursorBookmark("file"))
				{
					s.WriteAttribute("name", fileName);
					s.WriteCursor(fileData);
				}
			}
		}

		public bool ReadDefinition(IO.XmlElementStream s)
		{
			this.mFiles_.Clear();

			// first entry should always be the null terminated filenames table
			this.mFiles_.Add(GenerateFileNamesTableEntryChunk());

			using (s.EnterCursorBookmark("Files"))
				this.ReadChunks(s);

			using (var bm = s.EnterCursorBookmarkOpt("LocalFiles")) if (bm.IsNotNull)
				this.ReadLocalFiles(s);

			this.AddVersionFile();

			// there should be at least one file destined for the ERA, excluding the filenames table
			return this.FileChunksCount != 0;
		}
		public void WriteDefinition(IO.XmlElementStream s)
		{
			using (s.EnterCursorBookmark("Files"))
				this.WriteChunks(s);

			using (var bm = s.EnterCursorBookmarkOpt("LocalFiles", this.mLocalFiles_, Predicates.HasItems)) if (bm.IsNotNull)
				this.WriteLocalFiles(s);
		}
		#endregion

		#region Expand
		public void ExpandTo(IO.EndianStream blockStream, string workPath)
		{
			Contract.Requires(blockStream.IsReading);

			var eraExpander = (EraFileExpander)blockStream.Owner;

			if (eraExpander.ProgressOutput != null)
			{
				eraExpander.ProgressOutput.WriteLine("\tUnpacking files...");
			}

			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
			{
				var file = this.mFiles_[x];

				if (eraExpander.ProgressOutput != null)
				{
					eraExpander.ProgressOutput.Write("\r\t\t{0} ", file.entryId.ToString("X16"));
				}

				this.TryUnpack(blockStream, workPath, eraExpander, file);
			}

			if (eraExpander.ProgressOutput != null)
			{
				eraExpander.ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));
				eraExpander.ProgressOutput.WriteLine("\t\tDone");
			}

			this.mDirsThatExistForUnpacking_ = null;
		}

		private bool TryUnpack(IO.EndianStream blockStream, string workPath, EraFileExpander expander, EraFileEntryChunk file)
		{
			if (IsIgnoredLocalFile(file.fileName))
				return false;

			string fullPath = Path.Combine(workPath, file.fileName);

			if (ResourceUtils.IsLocalScenarioFile(file.fileName))
			{
				return false;
			}
			else if (!this.ShouldUnpack(expander, fullPath))
			{
				return false;
			}

			this.CreatePathForUnpacking(fullPath);

			this.UnpackToDisk(blockStream, fullPath, expander, file);
			return true;
		}

		private void UnpackToDisk(IO.EndianStream blockStream, string fullPath, EraFileExpander expander, EraFileEntryChunk file)
		{
			byte[] buffer = file.GetBuffer(blockStream);

			using (var fs = File.Create(fullPath))
			{
				fs.Write(buffer, 0, buffer.Length);
			}

			File.SetCreationTimeUtc(fullPath, file.FileDateTime);
			File.SetLastWriteTimeUtc(fullPath, file.FileDateTime);

			if (ResourceUtils.IsXmbFile(fullPath))
			{
				if (!expander.expanderOptions.Test(EraFileExpanderOptions.DONT_TRANSLATE_XMB_FILES))
				{
					var vaSize = Shell.ProcessorSize.X32;
					var builtFor64Bit = expander.options.Test(EraFileUtilOptions.X64);
					if (builtFor64Bit)
					{
						vaSize = Shell.ProcessorSize.X64;
					}

					this.TransformXmbToXml(buffer, fullPath, blockStream.ByteOrder, vaSize);
				}
			}
			else if (ResourceUtils.IsScaleformFile(fullPath))
			{
				if (expander.expanderOptions.Test(EraFileExpanderOptions.DECOMPRESS_UI_FILES))
				{
					bool success = false;

					try
					{
						success = this.DecompressUiFileToDisk(buffer, fullPath);
					}
					catch (Exception ex)
					{
						Debug.Trace.Resource.TraceEvent(System.Diagnostics.TraceEventType.Error, TypeExtensions.K_NONE,
							"Exception during {0} of '{1}': {2}",
							EraFileExpanderOptions.DECOMPRESS_UI_FILES, fullPath, ex);
						success = false;
					}

					if (!success && expander.VerboseOutput != null)
					{
						expander.VerboseOutput.WriteLine("Option {0} failed on '{1}'",
							EraFileExpanderOptions.DECOMPRESS_UI_FILES, fullPath);
					}
				}
				if (expander.expanderOptions.Test(EraFileExpanderOptions.TRANSLATE_GFX_FILES))
				{
					var result = TransformGfxToSwfFileResult.FAILED;

					try
					{
						result = this.TransformGfxToSwfFile(buffer, fullPath);
					}
					catch (Exception ex)
					{
						Debug.Trace.Resource.TraceEvent(System.Diagnostics.TraceEventType.Error, TypeExtensions.K_NONE,
							"Exception during {0} of '{1}': {2}",
							EraFileExpanderOptions.TRANSLATE_GFX_FILES, fullPath, ex);
					}

					if (expander.VerboseOutput != null)
					{
						if (result == TransformGfxToSwfFileResult.FAILED)
						{
							expander.VerboseOutput.WriteLine("Option {0} failed on '{1}'",
								EraFileExpanderOptions.TRANSLATE_GFX_FILES, fullPath);
						}
						else if (result == TransformGfxToSwfFileResult.INPUT_IS_ALREADY_SWF)
						{
							expander.VerboseOutput.WriteLine("Option {0} skipped on '{1}', it is already an SWF-based file",
								EraFileExpanderOptions.TRANSLATE_GFX_FILES, fullPath);
						}
					}
				}
			}
		}

		private void TransformXmbToXml(byte[] eraFileEntryBuffer, string fullPath, Shell.EndianFormat byteOrder, Shell.ProcessorSize vaSize)
		{
			byte[] xmbBuffer;

			using (var xmb = new ECF.EcfFileXmb())
			using (var ms = new MemoryStream(eraFileEntryBuffer))
			using (var es = new IO.EndianStream(ms, byteOrder, permissions: FileAccess.Read))
			{
				es.StreamMode = FileAccess.Read;
				xmb.Serialize(es);

				xmbBuffer = xmb.fileData;
			}

			string xmbPath = fullPath;
			ResourceUtils.RemoveXmbExtension(ref xmbPath);

			var context = new Xmb.XmbFileContext()
			{
				pointerSize = vaSize,
			};

			using (var ms = new MemoryStream(xmbBuffer, false))
			using (var s = new IO.EndianReader(ms, byteOrder))
			{
				s.UserData = context;

				using (var xmbf = new Xmb.XmbFile())
				{
					xmbf.Read(s);
					xmbf.ToXml(xmbPath);
				}
			}
		}

		private bool DecompressUiFileToDisk(byte[] eraFileEntryBuffer, string fullPath)
		{
			bool success = false;

			using (var ms = new MemoryStream(eraFileEntryBuffer, false))
			using (var s = new IO.EndianReader(ms, Shell.EndianFormat.LITTLE))
			{
				uint bufferSignature;
				if (ResourceUtils.IsScaleformBuffer(s, out bufferSignature))
				{
					int decompressedSize = s.ReadInt32();
					int compressedSize = (int)(ms.Length - ms.Position);

					byte[] decompressedData = ResourceUtils.DecompressScaleform(eraFileEntryBuffer, decompressedSize);
					using (var fs = File.Create(fullPath + ".bin"))
					{
						fs.Write(decompressedData, 0, decompressedData.Length);
					}

					success = true;
				}
			}

			return success;
		}

		enum TransformGfxToSwfFileResult
		{
			SUCCESS,
			FAILED,
			INPUT_IS_ALREADY_SWF,
		}
		private TransformGfxToSwfFileResult TransformGfxToSwfFile(byte[] eraFileEntryBuffer, string fullPath)
		{
			var result = TransformGfxToSwfFileResult.FAILED;

			using (var ms = new MemoryStream(eraFileEntryBuffer, false))
			using (var s = new IO.EndianReader(ms, Shell.EndianFormat.LITTLE))
			{
				uint bufferSignature;
				if (ResourceUtils.IsScaleformBuffer(s, out bufferSignature))
				{
					if (ResourceUtils.IsSwfHeader(bufferSignature))
						result = TransformGfxToSwfFileResult.INPUT_IS_ALREADY_SWF;
					else
					{
						this.TransformGfxToSwfFileInternal(eraFileEntryBuffer, fullPath, bufferSignature);
						result = TransformGfxToSwfFileResult.SUCCESS;
					}
				}
			}

			return result;
		}
		private void TransformGfxToSwfFileInternal(byte[] eraFileEntryBuffer, string fullPath, uint bufferSignature)
		{
			uint swfSignature = ResourceUtils.GfxHeaderToSwf(bufferSignature);
			using (var fs = File.Create(fullPath + ".swf"))
			using (var outS = new IO.EndianWriter(fs, Shell.EndianFormat.LITTLE))
			{
				outS.Write(swfSignature);
				outS.Write(eraFileEntryBuffer, sizeof(uint), eraFileEntryBuffer.Length - sizeof(uint));
			}
		}

		private HashSet<string> mDirsThatExistForUnpacking_;
		private void CreatePathForUnpacking(string fullPath)
		{
			if (this.mDirsThatExistForUnpacking_ == null)
				this.mDirsThatExistForUnpacking_ = [];

			string folder = Path.GetDirectoryName(fullPath);
			// don't bother checking the file system if we've already encountered this folder
			if (this.mDirsThatExistForUnpacking_.Add(folder))
			{
				if (!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
			}
		}

		private bool ShouldUnpack(EraFileExpander expander, string path)
		{
			if (expander.expanderOptions.Test(EraFileExpanderOptions.DONT_OVERWRITE_EXISTING_FILES))
			{
				// it's an XMB file and the user didn't say NOT to translate them
				if (ResourceUtils.IsXmbFile(path) && !expander.expanderOptions.Test(EraFileExpanderOptions.DONT_TRANSLATE_XMB_FILES))
				{
					ResourceUtils.RemoveXmbExtension(ref path);
				}

				if (File.Exists(path))
				{
					return false;
				}
			}

			if (expander.expanderOptions.Test(EraFileExpanderOptions.IGNORE_NON_DATA_FILES))
			{
				if (!ResourceUtils.IsDataBasedFile(path))
					return false;
			}

			return true;
		}
		#endregion

		#region Build
		private bool BuildFileNamesTable(IO.EndianStream blockStream)
		{
			Contract.Requires(blockStream.IsWriting);

			using (var ms = new MemoryStream(this.mFiles_.Count * 128))
			using (var s = new IO.EndianWriter(ms, blockStream.ByteOrder))
			{
				var smp = new Memory.Strings.StringMemoryPool(KFileNamesTablePoolConfig);
				for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
				{
					var file = this.mFiles_[x];

					file.fileNameOffset = smp.Add(file.fileName).u32;
				}
				smp.WriteStrings(s);

				var filenamesChunk = this.mFiles_[0];
				this.PackFileNames(blockStream, ms, filenamesChunk);

				return true;
			}
		}

		public bool Build(IO.EndianStream blockStream, string workPath)
		{
			Contract.Requires(blockStream.IsWriting);

			var builder = blockStream.Owner as EraFileBuilder;

			Contract.Assert(blockStream.BaseStream.Position == this.CalculateHeaderAndFileChunksSize());

			this.BuildFileNameMaps(builder != null ? builder.VerboseOutput : null);
			bool success = this.BuildFileNamesTable(blockStream);
			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count && success; x++)
			{
				var file = this.mFiles_[x];
				if (builder != null && builder.ProgressOutput != null)
				{
					builder.ProgressOutput.Write("\r\t\t{0} ", file.entryId.ToString("X16"));
				}

				success &= this.TryPack(blockStream, workPath, file);

				if (!success &&
					builder != null && builder.VerboseOutput != null)
				{
					builder.VerboseOutput.WriteLine("Couldn't pack file into {0}: {1}",
					                                this.FileName, file.fileName);
				}
			}

			if (builder != null && builder.ProgressOutput != null &&
				success)
			{
				builder.ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));
			}

			if (success)
			{
				blockStream.AlignToBoundry(K_ALIGNMENT_BIT_);
			}

#if false
			if (success)
			{
				if (builder != null && !builder.Options.Test(EraFileUtilOptions.SkipVerification))
				{
					var filenames_chunk = mFiles[0];

					ValidateAdler32(filenames_chunk, blockStream);
					ValidateHashes(filenames_chunk, blockStream);

					ValidateFileHashes(blockStream);
				}
			}
#endif

			return success;
		}

		private void PackFileData(IO.EndianStream blockStream, Stream source, EraFileEntryChunk file)
		{
			file.BuildBuffer(blockStream, source, this.TigerHasher);

#if false
			ValidateAdler32(file, blockStream);
			ValidateHashes(file, blockStream);
#endif
		}

		private void PackFileNames(IO.EndianStream blockStream, MemoryStream source, EraFileEntryChunk file)
		{
			file.FileDateTime = this.BuildModeDefaultTimestamp;
			this.PackFileData(blockStream, source, file);
		}

		private bool TryPack(IO.EndianStream blockStream, string workPath,
			EraFileEntryChunk file)
		{
			if (this.mLocalFiles_.ContainsKey(file.fileName))
				return this.TryPackLocalFile(blockStream, file);

			return this.TryPackFileFromDisk(blockStream, workPath, file);
		}

		private bool TryPackLocalFile(IO.EndianStream blockStream,
			EraFileEntryChunk file)
		{
			string fileData;
			if (!this.mLocalFiles_.TryGetValue(file.fileName, out fileData))
			{
				Debug.Trace.Resource.TraceInformation("Couldn't pack local-file into {0}, local-file does not exist: {1}",
				                                      this.FileName, file.fileName);
				return false;
			}

			byte[] fileBytes = System.Text.Encoding.ASCII.GetBytes(fileData);
			using (var ms = new MemoryStream(fileBytes, false))
			{
				this.PackFileData(blockStream, ms, file);
			}

			return true;
		}

		private bool TryPackFileFromDisk(IO.EndianStream blockStream, string workPath,
			EraFileEntryChunk file)
		{
			string path = Path.Combine(workPath, file.fileName);
			if (!File.Exists(path))
			{
				Debug.Trace.Resource.TraceInformation("Couldn't pack file into {0}, file does not exist: {1}",
				                                      this.FileName, file.fileName);
				return false;
			}

			try
			{
				file.FileDateTime = GetMostRecentTimeStamp(path);

				byte[] fileBytes = File.ReadAllBytes(path);
				using (var ms = new MemoryStream(fileBytes, false))
				{
					this.PackFileData(blockStream, ms, file);
				}
			} catch (Exception ex)
			{
				Debug.Trace.Resource.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.K_NONE,
					string.Format("Couldn't pack file into {0}, encountered exception dealing with {1}", this.FileName, file.fileName),
					ex);
				return false;
			}

			return true;
		}
		#endregion

		#region IEndianStreamSerializable Members
		public void ReadPostprocess(IO.EndianStream s)
		{
			if (this.mFiles_.Count == 0)
			{
				return;
			}

			var expander = s.Owner as EraFileExpander;
			var progressOutput = expander != null ? expander.ProgressOutput : null;
			var verboseOutput = expander != null ? expander.VerboseOutput : null;

			this.ReadFileNamesChunk(s);
			this.ValidateFileHashes(s);

			this.BuildFileNameMaps(verboseOutput);

			if (expander != null && !expander.expanderOptions.Test(EraFileExpanderOptions.DONT_REMOVE_XML_OR_XMB_FILES))
			{
				if (expander.expanderOptions.Test(EraFileExpanderOptions.DONT_TRANSLATE_XMB_FILES))
				{
					if (progressOutput != null)
						progressOutput.WriteLine("Removing any XML files if their XMB counterpart exists...");

					this.RemoveXmlFilesWhereXmbExists(verboseOutput);
				}
				else
				{
					if (progressOutput != null)
						progressOutput.WriteLine("Removing any XMB files if their XML counterpart exists...");

					this.RemoveXmbFilesWhereXmlExists(verboseOutput);
				}
			}

			this.BuildLocalFiles(s);
		}

		void ReadFileNamesChunk(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			var filenamesChunk = this.mFiles_[0];

			if (eraUtil != null &&
				!eraUtil.options.Test(EraFileUtilOptions.SKIP_VERIFICATION))
			{
				this.ValidateAdler32(filenamesChunk, s);
				this.ValidateHashes(filenamesChunk, s);
			}

			filenamesChunk.fileName = K_FILE_NAMES_TABLE_NAME_;

			byte[] filenamesBuffer = filenamesChunk.GetBuffer(s);
			using (var ms = new MemoryStream(filenamesBuffer, false))
			using (var er = new IO.EndianReader(ms, s.ByteOrder))
			{
				for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
				{
					var file = this.mFiles_[x];

					if (file.fileNameOffset != er.BaseStream.Position)
					{
						throw new InvalidDataException(string.Format(
							"#{0} {1} has bad filename offset {2} != {3}",
							this.FileIndexToListingIndex(x),
							file.entryId.ToString("X16"),
							file.fileNameOffset.ToString("X8"),
							er.BaseStream.Position.ToString("X8")
							));
					}

					file.fileName = er.ReadString(Memory.Strings.StringStorage.CStringAscii);
				}
			}
		}

		void ValidateFileHashes(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			if (eraUtil != null &&
				eraUtil.options.Test(EraFileUtilOptions.SKIP_VERIFICATION))
			{
				return;
			}

			if (eraUtil != null && eraUtil.ProgressOutput != null)
			{
				eraUtil.ProgressOutput.WriteLine("\tVerifying file hashes...");
			}

			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
			{
				var file = this.mFiles_[x];

				if (eraUtil != null && eraUtil.ProgressOutput != null)
				{
					eraUtil.ProgressOutput.Write("\r\t\t{0} ", file.entryId.ToString("X16"));
				}

				this.ValidateAdler32(file, s);
				this.ValidateHashes(file, s);
			}

			if (eraUtil != null && eraUtil.ProgressOutput != null)
			{
				eraUtil.ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));
				eraUtil.ProgressOutput.WriteLine("\t\tDone");
			}
		}

		void BuildLocalFiles(IO.EndianStream s)
		{
			for (int x = this.FileChunksFirstIndex; x < this.mFiles_.Count; x++)
			{
				var file = this.mFiles_[x];
				if (!ResourceUtils.IsLocalScenarioFile(file.fileName))
					continue;

				byte[] fileBytes = file.GetBuffer(s);
				using (var ms = new MemoryStream(fileBytes, false))
				using (var sr = new StreamReader(ms))
				{
					string fileData = sr.ReadToEnd();

					this.mLocalFiles_[file.fileName] = fileData;
				}
			}
		}

	public void Serialize(IO.EndianStream s)
		{
			var eraUtil = s.Owner as EraFileUtil;

			if (s.IsWriting)
			{
				this.mHeader_.UpdateFileCount(this.mFiles_.Count);
			}

			this.mHeader_.Serialize(s);

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.WriteLine("Header position end: {0}",
					s.BaseStream.Position);
				eraUtil.DebugOutput.WriteLine();
			}

			this.SerializeFileEntryChunks(s);

			if (eraUtil != null && eraUtil.DebugOutput != null)
			{
				eraUtil.DebugOutput.WriteLine();
			}
		}

		public void SerializeFileEntryChunks(IO.EndianStream s)
		{
			if (s.IsReading)
			{
				this.mFiles_.Capacity = this.mHeader_.FileCount;

				for (int x = 0; x < this.mFiles_.Capacity; x++)
				{
					var file = new EraFileEntryChunk();
					file.Serialize(s);

					this.mFiles_.Add(file);
				}
			}
			else if (s.IsWriting)
			{
				foreach (var f in this.mFiles_)
				{
					f.Serialize(s);
				}
			}
		}
		#endregion

		#region Local file utils
		private static bool IsIgnoredLocalFile(string fileName)
		{
			if (0==string.Compare(fileName, "version.txt", StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			return false;
		}

		private void AddVersionFile()
		{
			var file = new EraFileEntryChunk();
			file.CompressionType = ECF.EcfCompressionType.STORED;
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			file.fileName = "version.txt";
			file.FileDateTime = this.BuildModeDefaultTimestamp;
			string version = string.Format("{0}\n{1}\n{2}",
				assembly.FullName,
				assembly.GetName().Version,
				System.Reflection.Assembly.GetEntryAssembly().FullName);
			this.mLocalFiles_[file.fileName] = version;
			this.mFiles_.Add(file);
		}
		#endregion

		internal static DateTime GetMostRecentTimeStamp(string path)
		{
			var creationTime = File.GetCreationTimeUtc(path);
			var writeTime = File.GetLastWriteTimeUtc(path);
			return writeTime > creationTime
				? writeTime
				: creationTime;
		}
	};
}
