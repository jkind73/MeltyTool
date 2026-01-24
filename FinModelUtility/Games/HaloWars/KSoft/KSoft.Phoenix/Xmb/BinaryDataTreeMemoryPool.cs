using System;
using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Xmb
{
	public sealed partial class BinaryDataTreeMemoryPool
		: IDisposable
	{
		/// <summary>Default amount of entry memory allocated for use</summary>
		const int K_ENTRY_START_COUNT_ = 16;

		Dictionary<uint, PoolEntry> mEntries_;
		Dictionary<uint, uint> mDataOffsetToSizeValue_;
		uint mPoolSize_;

		public uint Size { get { return this.mPoolSize_; } }

		IO.EndianReader mBuffer_;
		uint mBufferedDataRemaining_;

		internal IO.EndianReader InternalBuffer { get { return this.mBuffer_; } }

		public BinaryDataTreeMemoryPool(int initialEntryCount = K_ENTRY_START_COUNT_)
		{
			if (initialEntryCount < 0)
				initialEntryCount = K_ENTRY_START_COUNT_;

			this.mEntries_ = new Dictionary<uint, PoolEntry>(K_ENTRY_START_COUNT_);
			this.mDataOffsetToSizeValue_ = new Dictionary<uint, uint>();
		}
		public BinaryDataTreeMemoryPool(byte[] buffer, Shell.EndianFormat byteOrder = Shell.EndianFormat.BIG)
			: this()
		{
			Contract.Requires(buffer != null);

			this.mPoolSize_ = (uint)buffer.Length;
			var ms = new System.IO.MemoryStream(buffer, false);
			this.mBuffer_ = new IO.EndianReader(ms, byteOrder, this);
			this.mBufferedDataRemaining_ = this.mPoolSize_;
		}

		#region IDisposable Members
		void DisposeBuffer()
		{
			Util.DisposeAndNull(ref this.mBuffer_);
		}
		public void Dispose()
		{
			this.DisposeBuffer();

			if (this.mEntries_ != null)
			{
				this.mEntries_.Clear();
				this.mEntries_ = null;
			}
		}
		#endregion

		#region Add
		#endregion

		#region Get
		bool ValidOffset(uint offset) { return offset < this.mPoolSize_; }

		public uint GetSizeValue(uint dataOffset)
		{
			if (!this.ValidOffset(dataOffset))
				throw new ArgumentOutOfRangeException("dataOffset", string.Format("{0} > {1}",
					dataOffset.ToString("X8"),
					this.mPoolSize_.ToString("X6")));
			if (dataOffset < sizeof(uint))
				throw new ArgumentOutOfRangeException("dataOffset", "Offset doesn't have room for a size value");

			uint sizeValue;
			if (!this.mDataOffsetToSizeValue_.TryGetValue(dataOffset, out sizeValue))
			{
				if (this.mBufferedDataRemaining_ == 0)
					throw new InvalidOperationException("No data left in buffer");
				else if (this.mBuffer_ == null)
					throw new InvalidOperationException("No underlying buffer");

				uint sizeOffset = dataOffset - sizeof(uint);
				// Great, now read the entry's value data
				this.mBuffer_.Seek32(sizeOffset);
				sizeValue = this.mBuffer_.ReadUInt32();

				// Update how much data is still remaining
				this.mBufferedDataRemaining_ -= sizeof(uint);

				if (this.mBufferedDataRemaining_ == 0)
					this.DisposeBuffer();

				this.mDataOffsetToSizeValue_.Add(dataOffset, sizeValue);
			}

			return sizeValue;
		}

		internal PoolEntry DeBuffer(BinaryDataTreeNameValue nameValue)
		{
			var typeDesc = nameValue.GuessTypeDesc();
			uint offset = nameValue.Offset;
			bool sizeIsIndirect = nameValue.SizeIsIndirect;
			return this.DeBuffer(typeDesc, offset, sizeIsIndirect);
		}

		PoolEntry DeBuffer(BinaryDataTreeVariantTypeDesc desc, uint offset, bool sizeIsIndirect = false)
		{
			if (!this.ValidOffset(offset))
				throw new ArgumentOutOfRangeException("offset", string.Format("{0} > {1}",
					offset.ToString("X8"),
					this.mPoolSize_.ToString("X6")));

			PoolEntry e;
			if (!this.mEntries_.TryGetValue(offset, out e))
			{
					 if (this.mBufferedDataRemaining_ == 0)	throw new InvalidOperationException("No data left in buffer");
				else if (this.mBuffer_ == null)				throw new InvalidOperationException("No underlying buffer");

				// Create our new entry, setting any additional properties
				e = PoolEntry.New(desc);
				if (sizeIsIndirect)
				{
					uint size = this.GetSizeValue(offset);
					e.ArrayLength = (int)(size >> desc.SizeBit);
				}
				// Great, now read the entry's value data
				this.mBuffer_.Seek32(offset);
				e.Read(this.mBuffer_);

				// Update how much data is still remaining
				uint bytesRead = (uint)(this.mBuffer_.BaseStream.Position - offset);
				this.mBufferedDataRemaining_ -= bytesRead;

				if (this.mBufferedDataRemaining_ == 0)
					this.DisposeBuffer();

				this.mEntries_.Add(offset, e);
			}

			return e;
		}
		#endregion

		public void Write(IO.EndianWriter s)
		{
			foreach (var e in this.mEntries_.Values)
				e.Write(s);
		}
	};
}
