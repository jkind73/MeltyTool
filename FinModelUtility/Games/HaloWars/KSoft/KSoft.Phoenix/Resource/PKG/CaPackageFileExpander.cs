using System;
using System.IO;

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageFileExpanderOptions
	{
		/// <summary>Only the PKG's file listing (.xml) is generated</summary>
		ONLY_DUMP_LISTING,
		/// <summary>Files that already exist in the output directory will be skipped</summary>
		DONT_OVERWRITE_EXISTING_FILES,
		DONT_LOAD_ENTIRE_PKG_INTO_MEMORY,

		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public sealed class CaPackageFileExpander
		: CaPackageFileUtil
	{
		Stream mPkgBaseStream_;
		IO.EndianStream mPkgStream_;

		/// <see cref="CaPackageFileExpanderOptions"/>
		public Collections.BitVector32 expanderOptions;

		public CaPackageFileExpander(string pkgPath)
		{
			this.mSourceFile = pkgPath;
		}

		public override void Dispose()
		{
			base.Dispose();

			Util.DisposeAndNull(ref this.mPkgBaseStream_);
			Util.DisposeAndNull(ref this.mPkgStream_);
		}

		#region Reading
		public bool Read()
		{
			bool result = true;

			try { result &= this.ReadPkgFromFile(); }
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while trying to read the PKG: {0}", ex);
				result = false;
			}

			return result;
		}

		bool ReadPkgFromFile()
		{
			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Opening and reading PKG file {0}...",
				                              this.mSourceFile);

			if (this.expanderOptions.Test(CaPackageFileExpanderOptions.DONT_LOAD_ENTIRE_PKG_INTO_MEMORY))
				this.mPkgBaseStream_ = File.OpenRead(this.mSourceFile);
			else
			{
				byte[] ecfBytes = File.ReadAllBytes(this.mSourceFile);

				this.mPkgBaseStream_ = new MemoryStream(ecfBytes, writable: false);
			}

			this.mPkgStream_ = new IO.EndianStream(this.mPkgBaseStream_, Shell.EndianFormat.LITTLE, this, permissions: FileAccess.Read);
			this.mPkgStream_.StreamMode = FileAccess.Read;

			return this.ReadPkgFromStream();
		}

		bool ReadPkgFromStream()
		{
			bool result = true;

			result = CaPackageFile.VerifyIsPkg(this.mPkgStream_.Reader);
			if (!result)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tFailed: File is either not even an PKG file, or corrupt");
			}
			else
			{
				this.mPkgFile = new CaPackageFile();
				this.mPkgFile.Serialize(this.mPkgStream_);
			}

			return result;
		}
		#endregion

		#region Expanding
		public bool ExpandTo(string workPath, string listingName)
		{
			if (this.mPkgFile == null)
				return false;

			if (!Directory.Exists(workPath))
				Directory.CreateDirectory(workPath);

			bool result = true;

			if (this.ProgressOutput != null)
				this.ProgressOutput.WriteLine("Outputting listing...");

			try
			{
				this.PopulatePkgDefinitionFromPkgFile(workPath);
				this.SaveListing(workPath, listingName);
			}
			catch (Exception ex)
			{
				if (this.VerboseOutput != null)
					this.VerboseOutput.WriteLine("\tEncountered an error while outputting listing: {0}", ex);
				result = false;
			}

			if (result && !this.expanderOptions.Test(CaPackageFileExpanderOptions.ONLY_DUMP_LISTING))
			{
				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Expanding PKG to {0}...", workPath);

				try
				{
					this.ExpandEntriesToFiles(workPath);
				}
				catch (Exception ex)
				{
					if (this.VerboseOutput != null)
						this.VerboseOutput.WriteLine("\tEncountered an error while expanding PKG: {0}", ex);
					result = false;
				}

				if (this.ProgressOutput != null)
					this.ProgressOutput.WriteLine("Done");
			}

			this.mPkgStream_.Close();

			return result;
		}

		void SaveListing(string workPath, string listingName)
		{
			string listingFilename = Path.Combine(workPath, listingName);

			using (var xml = IO.XmlElementStream.CreateForWrite("PkgFile", this))
			{
				xml.InitializeAtRootElement();
				xml.StreamMode = FileAccess.Write;

				this.PkgDefinition.Serialize(xml);

				xml.Document.Save(listingFilename + CaPackageFileDefinition.K_FILE_EXTENSION);
			}
		}

		void PopulatePkgDefinitionFromPkgFile(string workPath)
		{
			foreach (var entry in this.mPkgFile.FileEntries)
			{
				this.PkgDefinition.FileNames.Add(entry.name);
			}
		}

		void ExpandEntriesToFiles(string workPath)
		{
			foreach (var entry in this.mPkgFile.FileEntries)
			{
				try
				{
					this.ExpandEntryToFile(workPath, entry);
				}
				catch (Exception e)
				{
					throw new Exception(string.Format(
						"ExpandEntriesToFiles failed on {0} in {1}",
						entry.name,
						this.mPkgStream_.StreamName
					), e);
				}
			}
		}

		void ExpandEntryToFile(string workPath, CaPackageEntry entry)
		{
			string filePath = Path.Combine(workPath, entry.name);

			if (!this.expanderOptions.Test(CaPackageFileExpanderOptions.DONT_OVERWRITE_EXISTING_FILES))
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
				var entryBytes = this.mPkgFile.ReadEntryBytes(this.mPkgStream_, entry);
				fs.Write(entryBytes, 0, entryBytes.Length);
			}
		}
		#endregion
	};
}

