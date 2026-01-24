using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	public enum EraFileBuilderOptions
	{
		ENCRYPT,
		ALWAYS_USE_XML_OVER_XMB,

		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public sealed class EraFileBuilder
		: EraFileUtil
	{
		/// <summary>Extension of the file listing used to build ERAs</summary>
		public const string K_NAME_EXTENSION = ".eradef";

		/// <see cref="EraFileBuilderOptions"/>
		public Collections.BitVector32 builderOptions;

		public EraFileBuilder(string listingPath)
		{
			if (Path.GetExtension(listingPath) != K_NAME_EXTENSION)
				listingPath += K_NAME_EXTENSION;

			this.mSourceFile = listingPath;
		}

		bool ReadInternal()
		{
			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Trying to read source listing {0}...", this.mSourceFile);

			if (!File.Exists(this.mSourceFile))
				result = false;
			else
			{
				this.mEraFile = new EraFile();
				this.mEraFile.BuildModeDefaultTimestamp = EraFile.GetMostRecentTimeStamp(this.mSourceFile);

				using (var xml = new IO.XmlElementStream(this.mSourceFile, FA.Read, this))
				{
					xml.InitializeAtRootElement();
					result &= this.mEraFile.ReadDefinition(xml);
				}
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

		void EncryptFileBytes(byte[] eraBytes, int eraBytesLength)
		{
			using (var eraInMs = new MemoryStream(eraBytes, 0, eraBytesLength, writable: false))
			using (var eraOutMs = new MemoryStream(eraBytes, 0, eraBytesLength, writable: true))
			using (var eraReader = new IO.EndianReader(eraInMs, Shell.EndianFormat.BIG))
			using (var eraWriter = new IO.EndianWriter(eraOutMs, Shell.EndianFormat.BIG))
			{
				CryptStream(eraReader, eraWriter,
					Security.Cryptography.CryptographyTransformType.ENCRYPT);
			}
		}

		bool BuildInternal(string workPath, string eraName, string outputPath)
		{
			string eraFilename = Path.Combine(outputPath, eraName);
			if (!this.builderOptions.Test(EraFileBuilderOptions.ENCRYPT))
			{
				eraFilename += EraFileExpander.K_NAME_EXTENSION;
			}
			else
			{
				eraFilename += K_EXTENSION_ENCRYPTED;
			}

			this.mEraFile.FileName = eraFilename;

			if (File.Exists(eraFilename))
			{
				var attrs = File.GetAttributes(eraFilename);
				if (attrs.HasFlag(FileAttributes.ReadOnly))
					throw new IOException("ERA file is readonly, can't build: " + eraFilename);
			}

			if (this.builderOptions.Test(EraFileBuilderOptions.ALWAYS_USE_XML_OVER_XMB))
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Finding XML files to use over XMB references...");

				this.mEraFile.TryToReferenceXmlOverXmbFies(workPath, this.VerboseOutput);
			}

			const FA kMode = FA.Write;
			const int kInitialBufferSize = 24 * IntegerMath.K_MEGA; // 24MB

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Building {0} to {1}...", eraName, outputPath);

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("\tAllocating memory...");
			bool result = true;
			using (var ms = new MemoryStream(kInitialBufferSize))
			using (var eraMemory = new IO.EndianStream(ms, Shell.EndianFormat.BIG, this, permissions: kMode))
			{
				eraMemory.StreamMode = kMode;
				// we can use our custom VAT system to generate relative-offset (to a given chunk) information which ECFs use
				eraMemory.VirtualAddressTranslationInitialize(Shell.ProcessorSize.X32);

				// create null bytes for the header and embedded file chunk descriptors
				// previously just used Seek to do this, but it doesn't update Length.
				long preambleSize = this.mEraFile.CalculateHeaderAndFileChunksSize();
				ms.SetLength(preambleSize);
				ms.Seek(preambleSize, SeekOrigin.Begin);

				// now we can start embedding the files
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("\tPacking files...");
				result &= this.mEraFile.Build(eraMemory, workPath);

				if (result)
				{
					if (this.ProgressOutput != null)
						this.ProgressOutput.WriteLine("\tFinializing...");

					// seek back to the start of the ERA and write out the finalized header and file chunk descriptors
					ms.Seek(0, SeekOrigin.Begin);
					this.mEraFile.Serialize(eraMemory);

					// Right now we don't actually perform any file removing (eg, duplicates) until EraFile.Build so
					// we also allow the written size to be LESS THAN the assumed preamble size
					Contract.Assert(eraMemory.BaseStream.Position <= preambleSize,
						"Written ERA header size is greater than what we calculated");

					// finally, bake the ERA memory stream into a file
					if (this.builderOptions.Test(EraFileBuilderOptions.ENCRYPT))
					{
						if (this.ProgressOutput != null)
							this.ProgressOutput.WriteLine("\tEncrypting...");

						var eraBytes = ms.GetBuffer();
						this.EncryptFileBytes(eraBytes, (int)ms.Length);
					}
					else // not encrypted
					{
					}

					using (var fs = new FileStream(eraFilename, FileMode.Create, FA.Write))
						ms.WriteTo(fs);
				}
			}
			return result;
		}
		/// <summary>Builds the actual ERA file</summary>
		/// <param name="workPath">Base path of the ERA's files (defined by the listing xml)</param>
		/// <param name="eraName">Name of the final ERA file (without any directory or extension data)</param>
		/// <param name="outputPath">(Optional) The path to output the final ERA file. Defaults to <paramref name="workPath"/></param>
		/// <returns>True if all build operations were successful, false otherwise</returns>
		public bool Build(string workPath, string eraName, string outputPath = null)
		{
			if (string.IsNullOrWhiteSpace(outputPath))
				outputPath = workPath;

			bool result = true;

			try
			{
				result = this.BuildInternal(workPath, eraName, outputPath);
			} catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while building the archive: {0}", ex);
				result = false;
			}

			return result;
		}
	};
}