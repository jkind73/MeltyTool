using System;
using System.Runtime.InteropServices;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.LowLevel.Util
{
	/// <summary>Manager for interfacing with direct byte representation of <typeparamref name="T"/> objects</summary>
	/// <typeparam name="T">Value type to manage</typeparam>
	public sealed class StructBitManager<T> : IDisposable
		where T : struct
	{
		/// <summary>Size-of (in bytes) of the value type we're managing</summary>
		public static readonly int KSizeOf = Marshal.SizeOf<T>();
		/// <summary>Size-of (in bytes) of the value type we're managing</summary>
		public int SizeOf { get { return KSizeOf; } }

		IntPtr mHandle_;

		/// <summary>Initialize the manager and allocate the underlying object</summary>
		public StructBitManager()
		{
			this.mHandle_ = Unmanaged.New<T>();
		}

		#region IDisposable Members
		~StructBitManager()	{
			this.Dispose(false); }

		public void Dispose()
		{
			this.Dispose(true);

			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			KSoft.Util.MarkUnusedVariable(ref disposing);

			// check to see if we've already been called
			if (this.mHandle_ != IntPtr.Zero)
			{
				Unmanaged.Delete(this.mHandle_);
				this.mHandle_ = IntPtr.Zero;
			}
		}
		#endregion

		/// <summary>Copies a byte representation of <typeparamref name="T"/> from a buffer</summary>
		/// <param name="buffer">Buffer holding the bytes of a <typeparamref name="T"/></param>
		/// <param name="startIndex">Index of the first byte of the object</param>
		public void FromBuffer(byte[] buffer, int startIndex = 0)
		{
			Contract.Requires(buffer != null);
			Contract.Requires(startIndex < buffer.Length);
			Contract.Requires((startIndex+KSizeOf) < buffer.Length);

			Marshal.Copy(buffer, startIndex, this.mHandle_, KSizeOf);
		}

		/// <summary>Copies the underlying <typeparamref name="T"/> object to a buffer</summary>
		/// <param name="buffer">Buffer to copy the object into</param>
		/// <param name="startIndex">Byte index to start the copy at</param>
		[Contracts.Pure]
		public void ToBuffer(byte[] buffer, int startIndex = 0)
		{
			Contract.Requires(buffer != null);
			Contract.Requires(startIndex < buffer.Length);
			Contract.Requires((startIndex+KSizeOf) < buffer.Length);

			Marshal.Copy(this.mHandle_, buffer, startIndex, KSizeOf);
		}
		/// <summary>Get a buffer containing the underlying <typeparamref name="T"/> object's bytes</summary>
		/// <returns>A buffer holding the underlying object's bytes</returns>
		[Contracts.Pure]
		public byte[] ToBuffer()
		{
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] buffer = new byte[KSizeOf];
			Marshal.Copy(this.mHandle_, buffer, 0, KSizeOf);

			return buffer;
		}

		/// <summary>Copy an existing object value into the underlying object</summary>
		/// <param name="value"></param>
		public void FromValue(T value)
		{
			Unmanaged.StructureToPtr(value, this.mHandle_);
		}

		/// <summary>Get the underlying <typeparamref name="T"/> object</summary>
		/// <returns>A copy of the underlying value type object</returns>
		[Contracts.Pure]
		public T ToValue()
		{
			return Unmanaged.IntPtrToStructure<T>(this.mHandle_);
		}
	};
}
