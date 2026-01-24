using System;

using FA = System.IO.FileAccess;

namespace KSoft.Phoenix.Resource
{
	public enum EraFileExpanderOptions
	{
		/// <summary>Only the ERA's file listing (.xml) is generated</summary>
		ONLY_DUMP_LISTING,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DONT_OVERWRITE_EXISTING_FILES,
		/// <summary>Don't perform XMB to XML translations</summary>
		DONT_TRANSLATE_XMB_FILES,
		/// <summary>Decompresses Scaleform data</summary>
		DECOMPRESS_UI_FILES,
		/// <summary>Translates GFX files to SWF</summary>
		TRANSLATE_GFX_FILES,
		DECRYPT,
		DONT_LOAD_ENTIRE_ERA_INTO_MEMORY,
		DONT_REMOVE_XML_OR_XMB_FILES,
		IGNORE_NON_DATA_FILES,

		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public sealed class EraFileExpander
		: EraFileUtil
	{
		public const string K_NAME_EXTENSION = ".era.bin";

		System.IO.Stream mEraBaseStream_;
		IO.EndianStream mEraStream_;

		/// <see cref="EraFileExpanderOptions"/>
		public Collections.BitVector32 expanderOptions;

		public EraFileExpander(string eraPath)
		{
			this.mSourceFile = eraPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref this.mEraStream_);
			Util.DisposeAndNull(ref this.mEraBaseStream_);
		}

		bool ReadEraFromStream()
		{
			bool result = true;

			result = EraFileHeader.VerifyIsEraAndDecrypted(this.mEraStream_.Reader);
			if (!result)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tFailed: File is either not decrypted, corrupt, or not even an ERA");
			}
			else
			{
				this.mEraStream_.VirtualAddressTranslationInitialize(Shell.ProcessorSize.X32);

				this.mEraFile = new EraFile();
				this.mEraFile.FileName = this.mSourceFile;
				this.mEraFile.Serialize(this.mEraStream_);
				this.mEraFile.ReadPostprocess(this.mEraStream_);
			}

			return result;
		}

		bool ReadEraFromFile()
		{
			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Opening and reading ERA file {0}...",
				                              this.mSourceFile);

			if (this.expanderOptions.Test(EraFileExpanderOptions.DONT_LOAD_ENTIRE_ERA_INTO_MEMORY))
				this.mEraBaseStream_ = System.IO.File.OpenRead(this.mSourceFile);
			else
			{
				byte[] eraBytes = System.IO.File.ReadAllBytes(this.mSourceFile);
				if (this.expanderOptions.Test(EraFileExpanderOptions.DECRYPT))
				{
					if (this.ProgressOutput != null)
						this.ProgressOutput.WriteLine("Decrypting...");

					this.DecryptFileBytes(eraBytes);
				}

				this.mEraBaseStream_ = new System.IO.MemoryStream(eraBytes, writable: false);
			}

			this.mEraStream_ = new IO.EndianStream(this.mEraBaseStream_, Shell.EndianFormat.BIG, this, permissions: FA.Read);
			this.mEraStream_.StreamMode = FA.Read;

			return this.ReadEraFromStream();
		}

		void DecryptFileBytes(byte[] eraBytes)
		{
			using (var eraInMs = new System.IO.MemoryStream(eraBytes, writable: false))
			using (var eraOutMs = new System.IO.MemoryStream(eraBytes, writable: true))
			using (var eraReader = new IO.EndianReader(eraInMs, Shell.EndianFormat.BIG))
			using (var eraWriter = new IO.EndianWriter(eraOutMs, Shell.EndianFormat.BIG))
			{
				// "Halo Wars Alpha 093106 Feb 21 2009" was released pre-decrypted, so try and detect if the file is already decrypted first
				if (!EraFileHeader.VerifyIsEraAndDecrypted(eraReader))
				{
					CryptStream(eraReader, eraWriter,
						Security.Cryptography.CryptographyTransformType.DECRYPT);
				}
			}
		}

		public bool Read()
		{
			bool result = true;

			try { result &= this.ReadEraFromFile(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read the ERA: {0}", ex);
				result = false;
			}

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listingFilename = System.IO.Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("EraArchive", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FA.Write;

				this.mEraFile.WriteDefinition(xml);

				xml.Document.Save(listingFilename + EraFileBuilder.K_NAME_EXTENSION);
			}
		}
		public bool ExpandTo(string workPath, string listingName)
		{
			if (this.mEraFile == null)
				return false;

			if (!System.IO.Directory.Exists(workPath))
				System.IO.Directory.CreateDirectory(workPath);

			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Outputting listing...");

			try {
				this.SaveListing(workPath, listingName); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !this.expanderOptions.Test(EraFileExpanderOptions.ONLY_DUMP_LISTING))
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Expanding archive to {0}...", workPath);

				try {
					this.mEraFile.ExpandTo(this.mEraStream_, workPath); }
				catch (Exception ex)
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tEncountered an error while expanding archive: {0}", ex);
					result = false;
				}

				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Done");
			}

			this.mEraStream_.Close();

			return result;
		}
	};
}