using System;
using System.IO;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfFileExpanderOptions
	{
		/// <summary>Only the ECF's file listing (.xml) is generated</summary>
		ONLY_DUMP_LISTING,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DONT_OVERWRITE_EXISTING_FILES,
		DONT_SAVE_CHUNKS_TO_FILES,
		DONT_LOAD_ENTIRE_ECF_INTO_MEMORY,

		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public sealed class EcfFileExpander
		: EcfFileUtil
	{
		Stream mEcfBaseStream_;
		IO.EndianStream mEcfStream_;

		/// <see cref="EcfFileExpanderOptions"/>
		public Collections.BitVector32 expanderOptions;

		public EcfFileExpander(string ecfPath)
		{
			this.mSourceFile = ecfPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref this.mEcfBaseStream_);
			Util.DisposeAndNull(ref this.mEcfStream_);
		}

		#region Reading
		public bool Read()
		{
			bool result = true;

			try { result &= this.ReadEcfFromFile(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read the ECF: {0}", ex);
				result = false;
			}

			return result;
		}

		bool ReadEcfFromFile()
		{
			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Opening and reading ECF file {0}...",
				                              this.mSourceFile);

			if (this.expanderOptions.Test(EcfFileExpanderOptions.DONT_LOAD_ENTIRE_ECF_INTO_MEMORY))
				this.mEcfBaseStream_ = File.OpenRead(this.mSourceFile);
			else
			{
				byte[] ecfBytes = File.ReadAllBytes(this.mSourceFile);

				this.mEcfBaseStream_ = new MemoryStream(ecfBytes, writable: false);
			}

			this.mEcfStream_ = new IO.EndianStream(this.mEcfBaseStream_, Shell.EndianFormat.BIG, this, permissions: FileAccess.Read);
			this.mEcfStream_.StreamMode = FileAccess.Read;

			return this.ReadEcfFromStream();
		}

		bool ReadEcfFromStream()
		{
			bool result = true;

			result = EcfHeader.VerifyIsEcf(this.mEcfStream_.Reader);
			if (!result)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tFailed: File is either not even an ECF-based file, or corrupt");
			}
			else
			{
				this.mEcfFile = new EcfFile();
				this.mEcfFile.Serialize(this.mEcfStream_);
			}

			return result;
		}
		#endregion

		#region Expanding
		bool WriteChunksToFile { get { return this.expanderOptions.Test(EcfFileExpanderOptions.DONT_SAVE_CHUNKS_TO_FILES) == false; } }

		public bool ExpandTo(string workPath, string listingName)
		{
			if (this.mEcfFile == null)
				return false;

			if (!Directory.Exists(workPath))
				Directory.CreateDirectory(workPath);

			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Outputting listing...");

			try
			{
				this.PopulateEcfDefinitionFromEcfFile(workPath);
				this.SaveListing(workPath, listingName);
			}
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !this.expanderOptions.Test(EcfFileExpanderOptions.ONLY_DUMP_LISTING) && this.WriteChunksToFile)
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Expanding ECF to {0}...", workPath);

				try
				{
					this.ExpandChunksToFiles();
				}
				catch (Exception ex)
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tEncountered an error while expanding ECF: {0}", ex);
					result = false;
				}

				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Done");
			}

			this.mEcfStream_.Close();

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listingFilename = Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("EcfFile", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Write;

				this.EcfDefinition.Serialize(xml);

				xml.Document.Save(listingFilename + EcfFileDefinition.K_FILE_EXTENSION);
			}
		}

		void PopulateEcfDefinitionFromEcfFile(string workPath)
		{
			this.EcfDefinition.WorkingDirectory = workPath;
			this.EcfDefinition.Initialize(this.mSourceFile);

			this.mEcfFile.CopyHeaderDataTo(this.EcfDefinition);

			int rawChunkIndex = 0;
			foreach (var rawChunk in this.mEcfFile)
			{
				var chunk = this.EcfDefinition.Add(rawChunk, rawChunkIndex++);

				if (this.WriteChunksToFile)
					chunk.SetFilePathFromParentNameAndId();
			}

			if (!this.WriteChunksToFile)
				this.ReadEcfChunksToDefinitionBytes();
		}

		void ReadEcfChunksToDefinitionBytes()
		{
			foreach (var chunk in this.EcfDefinition.Chunks)
			{
				var rawChunk = this.mEcfFile.GetChunk(chunk.rawChunkIndex);

				try
				{
					var chunkBytes = rawChunk.GetBuffer(this.mEcfStream_);
					chunk.SetFileBytes(chunkBytes);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ReadEcfChunksToDefinitionBytes failed on chunk {0} in {1}",
						chunk.Id.ToString("X8"),
						this.mEcfStream_.StreamName
					), e);
				}
			}
		}

		void ExpandChunksToFiles()
		{
			foreach (var chunk in this.EcfDefinition.Chunks)
			{
				var rawChunk = this.mEcfFile.GetChunk(chunk.rawChunkIndex);

				try
				{
					this.ExpandChunkToFile(chunk, rawChunk);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ExpandChunksToFiles failed on chunk {0} in {1}",
						chunk.Id.ToString("X8"),
						this.mEcfStream_.StreamName
					), e);
				}
			}
		}

		void ExpandChunkToFile(EcfFileChunkDefinition chunk, EcfChunk rawChunk)
		{
			string filePath = this.EcfDefinition.GetChunkAbsolutePath(chunk);

			if (!this.expanderOptions.Test(EcfFileExpanderOptions.DONT_OVERWRITE_EXISTING_FILES))
			{
				if (File.Exists(filePath))
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tSkipping chunk, output file already exists: {0}", filePath);

					return;
				}
			}

			using (var fs = File.OpenWrite(filePath))
			{
				var chunkBytes = rawChunk.GetBuffer(this.mEcfStream_);
				fs.Write(chunkBytes, 0, chunkBytes.Length);
			}
		}
		#endregion
	};
}