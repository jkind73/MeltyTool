using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DirectXTexBlob
		: IDisposable
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
				DirectXTexDll.DirectXTex_BlobFree(this);
				this.Pointer = IntPtr.Zero;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}
		}

		public static DirectXTexBlob New()
		{
			try
			{
				DirectXTexBlob blob;
				var hresult = DirectXTexDll.DirectXTex_BlobNew(out blob);
				DirectXTexDll.ThrowIfFailed(hresult);
				return blob;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}

			return new DirectXTexBlob();
		}

		public IntPtr Buffer { get {
			if (this.IsNull || DirectXTexDll.EntryPointsNotFound)
				return IntPtr.Zero;

			try
			{
				IntPtr bufferPointer;
				uint bufferSize;
				var hresult = DirectXTexDll.DirectXTex_BlobGetBuffer(this, out bufferPointer, out bufferSize);
				DirectXTexDll.ThrowIfFailed(hresult);
				return bufferPointer;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}

			return IntPtr.Zero;
		} }

		public uint BufferSize { get {
			if (this.IsNull || DirectXTexDll.EntryPointsNotFound)
				return 0;

			try
			{
				IntPtr bufferPointer;
				uint bufferSize;
				var hresult = DirectXTexDll.DirectXTex_BlobGetBuffer(this, out bufferPointer, out bufferSize);
				DirectXTexDll.ThrowIfFailed(hresult);
				return bufferSize;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDll.HandleEntryPointNotFound(ex);
			}

			return 0;
		} }
	};
}
