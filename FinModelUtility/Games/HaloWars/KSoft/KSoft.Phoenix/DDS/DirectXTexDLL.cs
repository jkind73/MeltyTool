using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	public enum DirectXTexFileType : uint
	{
		UNKNOWN,

		DDS,
		HDR,
		TGA,
		WIC,

		COUNT
	};

	/// <summary>
	/// https://github.com/KornnerStudios/DirectXTex/tree/DotNet
	/// </summary>
	static class DirectXTexDll
	{
		public enum LibraryMode : uint
		{
			DEBUG,
			RELEASE,
		};

		const string K_DLL_NAME_ = @"DDS\DirectXTexDLL.dll";
		const CallingConvention K_CALL_CONV_ = CallingConvention.StdCall;
		const CharSet K_CHAR_SET_ = CharSet.Unicode;

		public static bool Initialized { get; private set; }

		internal static bool gEntryPointsNotFound;
		public static bool EntryPointsNotFound
		{
			get
			{
				if (!Initialized && !gEntryPointsNotFound && !gIsInitializing_)
					Initialize();

				return gEntryPointsNotFound;
			}
			private set { gEntryPointsNotFound = value; }
		}

		public static void HandleEntryPointNotFound(EntryPointNotFoundException ex)
		{
			if (EntryPointsNotFound)
				return;

			EntryPointsNotFound = true;
			Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.K_NONE,
				"Failed to find a DirectXTexDLL method",
				ex);
		}

		private static bool gIsInitializing_;
		public static void Initialize()
		{
			if (Initialized)
				return;

			try
			{
				gIsInitializing_ = true;

				var libPointerSize = DirectXTex_GetPointerSize();
				if (libPointerSize != IntPtr.Size)
				{
					Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Critical, TypeExtensions.K_NONE,
						"DirectXTexDLL was built for a platform that doesn't match what we're currently running in",
						libPointerSize,
						IntPtr.Size);
					return;
				}

				var libMode = DirectXTex_GetLibraryMode();
				Phoenix.Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Information, TypeExtensions.K_NONE,
					"Finished loading DirectXTexDLL",
					libMode);

				Initialized = true;
			}
			catch (EntryPointNotFoundException ex)
			{
				HandleEntryPointNotFound(ex);
			}
			finally
			{
				gIsInitializing_ = false;
			}
		}

		public static void Dispose()
		{
			if (!Initialized)
				return;

			Initialized = false;
			EntryPointsNotFound = false;
		}

		public static void ThrowIfFailed(int hresult)
		{
			Marshal.ThrowExceptionForHR(hresult);
		}

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern uint DirectXTex_GetPointerSize();
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern LibraryMode DirectXTex_GetLibraryMode();

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_GetMetadataFromFile(
			DirectXTexFileType fileType,
			out TexMetadata metadata,
			string file,
			uint flags);
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_GetMetadataFromMemory(
			DirectXTexFileType fileType,
			out TexMetadata metadata,
			IntPtr source,
			uint size,
			uint flags);

		#region DirectX::Blob
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_BlobNew(
			out DirectXTexBlob outBlob);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_BlobFree(
			DirectXTexBlob blob);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_BlobGetBuffer(
			DirectXTexBlob blob,
			out IntPtr bufferPointer,
			out uint bufferSize);
		#endregion

		#region DirectX::ScratchImage
		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageNew(
			out DirectXTexScratchImage outImage);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageFree(
			DirectXTexScratchImage image);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageGetMetadata(
			DirectXTexScratchImage image,
			out TexMetadata metadata);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageGetRawBytes(
			DirectXTexScratchImage image,
			uint arrayIndex,
			uint mip,
			uint slice,
			out DirectXTexBlob outBlob);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageGenerateMipMaps(
			DirectXTexScratchImage image,
			out DirectXTexScratchImage outMipChain);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageCreateEmptyMipChain(
			DirectXTexScratchImage image,
			out DirectXTexScratchImage outMipChain);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageCreateTexture2D(
			byte[] pixels,
			uint width,
			uint height,
			DxgiFormat format,
			uint rowPitch,
			uint slicePitch,
			out DirectXTexScratchImage outImage);

		[DllImport(K_DLL_NAME_, CallingConvention=K_CALL_CONV_, CharSet=K_CHAR_SET_)]
		public static extern int DirectXTex_ScratchImageCreateTexture11(
			DirectXTexScratchImage image,
			IntPtr d11Device,
			out IntPtr outTexture);
		#endregion
	};
}