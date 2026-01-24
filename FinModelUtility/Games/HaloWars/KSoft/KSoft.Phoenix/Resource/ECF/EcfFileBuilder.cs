using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource.ECF
{
	public enum EcfFileBuilderOptions
	{
		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public sealed class EcfFileBuilder
		: EcfFileUtil
	{
		/// <see cref="EcfFileBuilderOptions"/>
		public Collections.BitVector32 builderOptions;

		public EcfFileBuilder(string listingPath)
		{
			if (Path.GetExtension(listingPath) != EcfFileDefinition.K_FILE_EXTENSION)
				listingPath += EcfFileDefinition.K_FILE_EXTENSION;

			this.mSourceFile = listingPath;
		}

		#region Reading
		bool ReadInternal()
		{
			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Trying to read source listing {0}...", this.mSourceFile);

			if (!File.Exists(this.mSourceFile))
				result = false;
			else
			{
				using (var xml = new IO.XmlElementStream(this.mSourceFile, FA.Read, this))
				{
					xml.InitializeAtRootElement();
					this.EcfDefinition.Serialize(xml);
				}

				this.EcfDefinition.CullChunksPossiblyWithoutFileData((chunkIndex, chunk) =>
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\t\tCulling chunk #{0} since it has no associated file data",
						                          chunkIndex);
				});
			}

			if (result == false)
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("\tFailed!");
			}

			return result;
		}
		public bool Read() // read the listing definition
		{
			bool result = true;

			try { result &= this.ReadInternal(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read listing: {0}", ex);
				result = false;
			}

			return result;
		}
		#endregion

		#region Building
		public bool Build(string workPath, string outputPath = null)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = workPath;

			bool result = true;

			try
			{
				result = this.BuildInternal(workPath, outputPath);
			} catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while building the ECF: {0}", ex);
				result = false;
			}

			return result;
		}

		bool BuildInternal(string workPath, string outputPath)
		{
			this.EcfDefinition.WorkingDirectory = workPath;

			string ecfName = this.EcfDefinition.EcfName;
			if (ecfName.IsNotNullOrEmpty())
			{
				ecfName = Path.GetFileNameWithoutExtension(this.mSourceFile);
			}

			string ecfFilename = Path.Combine(outputPath, ecfName);
			#if false // I'm no longer doing this since we don't strip the file ext off listing when expanding
			// #TODO I bet a user could forget to include the preceding dot
			if (EcfDefinition.EcfFileExtension.IsNotNullOrEmpty())
				ecf_filename += EcfDefinition.EcfFileExtension;
			#endif

			if (File.Exists(ecfFilename))
			{
				var attrs = File.GetAttributes(ecfFilename);
				if (attrs.HasFlag(FileAttributes.ReadOnly))
					throw new IOException("ECF file is readonly, can't build: " + ecfFilename);
			}

			this.mEcfFile = new EcfFile();
			this.mEcfFile.SetupHeaderAndChunks(this.EcfDefinition);

			const FA kMode = FA.Write;
			const int kInitialBufferSize = 8 * IntegerMath.K_MEGA; // 8MB

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Building {0} to {1}...", ecfName, outputPath);

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("\tAllocating memory...");
			bool result = true;
			using (var ms = new MemoryStream(kInitialBufferSize))
			using (var ecfMemory = new IO.EndianStream(ms, Shell.EndianFormat.BIG, this, permissions: kMode))
			{
				ecfMemory.StreamMode = kMode;
				ecfMemory.VirtualAddressTranslationInitialize(Shell.ProcessorSize.X32);

				long preambleSize = this.mEcfFile.CalculateHeaderAndChunkEntriesSize();
				ms.SetLength(preambleSize);
				ms.Seek(preambleSize, SeekOrigin.Begin);

				// now we can start embedding the files
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("\tPacking chunks...");

				result = result && this.PackChunks(ecfMemory);

				if (result)
				{
					if (this.ProgressOutput != null)
						this.ProgressOutput.WriteLine("\tFinializing...");

					// seek back to the start of the ECF and write out the finalized header and chunk descriptors
					ms.Seek(0, SeekOrigin.Begin);
					this.mEcfFile.Serialize(ecfMemory);

					Contract.Assert(ecfMemory.BaseStream.Position == preambleSize,
						"Written ECF header size is NOT EQUAL what we calculated");

					// Update sizes and checksums
					ms.Seek(0, SeekOrigin.Begin);
					this.mEcfFile.SerializeBegin(ecfMemory, isFinalizing: true);
					this.mEcfFile.SerializeEnd(ecfMemory);

					// finally, bake the ECF memory stream into a file

					using (var fs = new FileStream(ecfFilename, FileMode.Create, FA.Write))
						ms.WriteTo(fs);
				}
			}

			return result;
		}

		bool PackChunks(IO.EndianStream ecfStream)
		{
			bool success = true;

			foreach (var chunk in this.EcfDefinition.Chunks)
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.Write("\r\t\t{0} ", chunk.Id.ToString("X16"));

				success = success && this.BuildChunkToStream(ecfStream, chunk);

				if (!success)
					break;
			}

			if (success && this.ProgressOutput != null)
				this.ProgressOutput.Write("\r\t\t{0} \r", new string(' ', 16));

			return success;
		}

		bool BuildChunkToStream(IO.EndianStream ecfStream, EcfFileChunkDefinition chunk)
		{
			bool success = true;

			var rawChunk = this.mEcfFile.GetChunk(chunk.rawChunkIndex);

			using (var chunkMs = this.EcfDefinition.GetChunkFileDataStream(chunk))
			{
				rawChunk.BuildBuffer(ecfStream, chunkMs);
			}

			return success;
		}
		#endregion
	};
}