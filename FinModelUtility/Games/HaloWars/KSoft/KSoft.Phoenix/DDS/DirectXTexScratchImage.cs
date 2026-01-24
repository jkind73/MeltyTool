using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DirectXTexScratchImage
	{
		IntPtr Pointer;

		public bool IsNull { get { return this.Pointer == IntPtr.Zero; } }
		public bool IsNotNull { get { return this.Pointer != IntPtr.Zero; } }

		public void Dispose()
		{
			if (this.IsNull || DirectXTexDll.EntryPointsNotFound)
				return;

			try
			{
				DirectXTexDll.DirectXTex_ScratchImageFree(this);
				this.Pointer = IntPtr.Zero;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}
		}

		public static DirectXTexScratchImage New()
		{
			try
			{
				DirectXTexScratchImage image;
				var hresult = DirectXTexDll.DirectXTex_ScratchImageNew(out image);
				DirectXTexDll.ThrowIfFailed(hresult);
				return image;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}

			return new DirectXTexScratchImage();
		}

		public TexMetadata Metadata { get {
			if (this.IsNull || DirectXTexDll.EntryPointsNotFound)
				return TexMetadata.Empty;

			try
			{
				TexMetadata metadata;
				var hresult = DirectXTexDll.DirectXTex_ScratchImageGetMetadata(this, out metadata);
				DirectXTexDll.ThrowIfFailed(hresult);
				return metadata;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}

			return TexMetadata.Empty;
		} }
	};
}