using System;
using System.IO;
#if CONTRACTS_FULL_SHIM

#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Resource.PKG
{
	public enum CaPackageFileBuilderOptions
	{
		ALWAYS_USE_XML_OVER_XMB,

		[Obsolete(EnumBitEncoderBase.K_OBSOLETE_MSG, true)] K_NUMBER_OF,
	};

	public sealed class CaPackageFileBuilder
		: CaPackageFileUtil
	{
		/// <see cref="CaPackageFileBuilderOptions"/>
		public Collections.BitVector32 builderOptions;

		public CaPackageFileBuilder(string listingPath)
		{
			if (Path.GetExtension(listingPath) != CaPackageFileDefinition.K_FILE_EXTENSION)
				listingPath += CaPackageFileDefinition.K_FILE_EXTENSION;

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
				this.mPkgFile = new CaPackageFile();

				using (var xml = new IO.XmlElementStream(this.mSourceFile, FileAccess.Read, this))
				{
					xml.InitializeAtRootElement();
					this.PkgDefinition.Serialize(xml);
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

	#endregion
  };
}

