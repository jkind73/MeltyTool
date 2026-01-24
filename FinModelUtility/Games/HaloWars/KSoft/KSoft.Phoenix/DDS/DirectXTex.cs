using System;
using System.IO;
using System.Runtime.InteropServices;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.DDS
{
	public static class DirectXTex
	{
		// BMP, JPG, JPEG, PNG, TIF, TIFF, WDP
		public static string defaultFileExtensionForWic = ".png";

		public static string ToFileExtension(this DirectXTexFileType type)
		{
			switch(type)
			{
				case DirectXTexFileType.DDS:
					return ".dds";
				case DirectXTexFileType.HDR:
					return ".hdr";
				case DirectXTexFileType.TGA:
					return ".tga";
				case DirectXTexFileType.WIC:
					return defaultFileExtensionForWic;

				default:
					return "";
			}
		}

		public static DirectXTexFileType FileTypeFromFileExtension(string file)
		{
			Contract.Requires(!string.IsNullOrEmpty(file));

			string ext = Path.GetExtension(file);
			ext = ext.ToLowerInvariant();

			switch (ext)
			{
				case ".dds":
				case ".ddx": // #NOTE specific to Phoneix code
					return DirectXTexFileType.DDS;
				case ".hdr":
					return DirectXTexFileType.HDR;
				case ".tga":
					return DirectXTexFileType.TGA;

				case ".bmp":
				case ".jpg":
				case ".jpeg":
				case ".png":
				case ".tif":
				case ".tiff":
				case ".wdp":
					return DirectXTexFileType.WIC;

				default:
					return DirectXTexFileType.UNKNOWN;
			}
		}

		public static TexMetadata GetMetadataFromFile(string file
			, DirectXTexFileType fileType = DirectXTexFileType.UNKNOWN
			, uint flags = 0)
		{
			Contract.Requires(!string.IsNullOrEmpty(file));

			if (!File.Exists(file))
				throw new FileNotFoundException(file);

			if (fileType == DirectXTexFileType.UNKNOWN)
			{
				fileType = FileTypeFromFileExtension(file);
			}
			if (fileType == DirectXTexFileType.UNKNOWN)
				throw new NotSupportedException(file);

			var result = TexMetadata.Empty;

			if (DirectXTexDll.EntryPointsNotFound)
				return result;

			try
			{
				var hresult = DirectXTexDll.DirectXTex_GetMetadataFromFile(
					fileType, out result, file, flags);
				DirectXTexDll.ThrowIfFailed(hresult);
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}

			return result;
		}

		public static TexMetadata GetMetadataFromMemory(
			byte[] buffer, int startIndex, int length
			, DirectXTexFileType fileType
			, uint flags = 0)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(startIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(length >= 0);
			Contract.Requires<ArgumentOutOfRangeException>((startIndex+length) <= buffer.Length);

			var result = TexMetadata.Empty;

			if (DirectXTexDll.EntryPointsNotFound)
				return result;

			var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			try
			{
				var buf = bufferHandle.AddrOfPinnedObject();
				buf += startIndex;

				var hresult = DirectXTexDll.DirectXTex_GetMetadataFromMemory(
					fileType, out result, buf, (uint)length, flags);
				DirectXTexDll.ThrowIfFailed(hresult);
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}
			finally
			{
				bufferHandle.Free();
			}

			return result;
		}
	};
}