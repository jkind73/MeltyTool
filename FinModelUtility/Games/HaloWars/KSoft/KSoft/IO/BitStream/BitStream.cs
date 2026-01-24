using System;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	[System.Diagnostics.DebuggerDisplay("Bit/Length = {BitLength}/{Length}, Bit = {BitPosition}")]
	public partial class BitStream
		: IKSoftStream
		, IKSoftStreamModeable
		, IDisposable
	{
		static BitStream()
		{
			InitializeBitmaskLookUpTable(out KBitmaskLut);
		}

		#region BaseStream
		/// <summary>Stream to push/pull bits to/from</summary>
		public Stream BaseStream { get; private set; }
		/// <summary>byte offset in <see cref="BaseStream"/> where we started streaming at</summary>
		long mStartPosition_;
		/// <summary>byte offset in <see cref="BaseStream"/> to stop streaming at, or zero</summary>
		long mEndPosition_;
		/// <summary>More efficient to use buffer Read/Writes than to do Read/WriteByte calls</summary>
		byte[] mIoBuffer_ = new byte[1];
		#endregion

		#region Cache
		/// <summary>current bit index in <see cref="mCache_"/></summary>
		int mCacheBitIndex_;
		/// <summary>Actual number of bits streamed to/form <see cref="mCache_"/></summary>
		int mCacheBitsStreamedCount_;

		/// <summary>Number of bits still left in <see cref="mCache_"/> that can be read/written</summary>
		int CacheBitsRemaining	{ get => K_WORD_BIT_COUNT_ - this.mCacheBitIndex_; }
		#endregion

		#region IKSoftStream
		/// <summary>Owner of this stream</summary>
		public object Owner { get; set; }

		public object UserData { get; set; }

		/// <summary>Name for this bitstream, or an empty string</summary>
		public string StreamName			{ get; private set; }
		/// <returns><see cref="StreamName"/></returns>
		public override string ToString()	=> this.StreamName;
		#endregion

		#region IKSoftStreamModeable
		/// <remarks>0 means this stream is closed</remarks>
		public FileAccess StreamPermissions { get; private set; }

		FileAccess mStreamMode_;
		/// <summary>Current data streaming state</summary>
		/// <remarks>Read or Write, not both</remarks>
		public FileAccess StreamMode { get { return this.mStreamMode_; } set {
			if (value != this.mStreamMode_)
			{
				// if we're switching to Read, flush then warm the cache
				if (value == FileAccess.Read)
				{
					this.FlushCache();
					this.FillCache();
				}

				this.mStreamMode_ = value;
			}
		} }
		public bool IsReading { get => this.mStreamMode_ == FileAccess.Read; }
		public bool IsWriting { get => this.mStreamMode_ == FileAccess.Write; }
		#endregion

		/// <summary>Access operations to throw exceptions on when they result in overflows</summary>
		/// <remarks>Bad read/writes will throw <see cref="EndOfStreamException"/>s</remarks>
		public FileAccess ThrowOnOverflow { get; set; }

		/// <summary>Do we own the base stream?</summary>
		/// <remarks>If we don't own the stream, when this object is disposed, the <see cref="BaseStream"/> won't be closed\disposed</remarks>
		public bool BaseStreamOwner { get; set; }

		/// <summary>Byte length of the bitstream or <see cref="BaseStream"/>'s Length</summary>
		/// <exception cref="NotSupportedException"><see cref="BaseStream"/> does not support seeking</exception>
		public long Length { get => this.mEndPosition_ > 0
				? this.mEndPosition_
				: this.BaseStream.Length;
		}
		/// <summary>Number of bits in the bitstream</summary>
		/// <exception cref="NotSupportedException"><see cref="BaseStream"/> does not support seeking</exception>
		public long BitLength { get => (this.Length - this.mStartPosition_) * Bits.K_BYTE_BIT_COUNT; }
		/// <summary>Current bit index within the bitstream</summary>
		/// <exception cref="NotSupportedException"><see cref="BaseStream"/> does not support seeking</exception>
		public long BitPosition	{ get {
			long position = ((this.BaseStream.Position - this.mStartPosition_) * Bits.K_BYTE_BIT_COUNT) + this.mCacheBitIndex_;
			// When reading we initialize the cache, meaning we've already advanced BaseStream.Position
			return this.IsWriting
				? position
				: position - this.mCacheBitsStreamedCount_;
		} }

		bool IsEndOfStream { get => this.CanSeek
				? this.BaseStream.Position >= this.Length
				: false;
		}

		internal void SeekToStart()
		{
			Contract.Requires<InvalidOperationException>(this.CanSeek);

			this.FlushCache();
			this.mCacheBitsStreamedCount_ = 0;

			this.BaseStream.Seek(this.mStartPosition_, SeekOrigin.Begin);
		}

		/// <summary>Constuct a new bitsream using an underlying <see cref="Stream"/> object</summary>
		/// <param name="baseStream">Underlying stream to read/write bits to</param>
		/// <param name="permissions">Access permissions for <paramref name="baseStream"/></param>
		/// <param name="startPos">Start of the bitstream in <paramref name="baseStream"/>, or -1 to use default value</param>
		/// <param name="endPos">End of the bitstream in <paramref name="baseStream"/>, or -1 to use default value</param>
		/// <param name="streamName">Optional name for this bitstream</param>
		/// <remarks>
		/// If <paramref name="baseStream"/> CanSeek, the default value for <paramref name="startPos"/> is the current Position of the stream.
		/// Else the default value is zero.
		///
		/// The default value for <paramref name="endPos"/> is zero. When it is zero, <paramref name="Stream"/>'s Length is always used at
		/// runtime. So it will acknowledge changes in the base stream length after the constructor finishes.
		/// </remarks>
		public BitStream(Stream baseStream, FileAccess permissions = FileAccess.ReadWrite,
			long startPos = TypeExtensions.K_NONE, long endPos = TypeExtensions.K_NONE,
			string streamName = "")
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<ArgumentOutOfRangeException>(!baseStream.CanSeek || endPos <= baseStream.Length);
			Contract.Requires(permissions != 0);
			Contract.Requires<ArgumentNullException>(streamName != null);

			this.StreamPermissions = permissions;
			this.BaseStream = baseStream;
			this.StreamName = streamName;

			if (baseStream.CanSeek)
			{
				if (startPos >= 0)
				{
					this.mStartPosition_ = startPos;
					this.BaseStream.Seek(startPos, SeekOrigin.Begin);
				}
				else
					this.mStartPosition_ = baseStream.Position;

				if (endPos > 0)
					this.mEndPosition_ = endPos;
			}
			else
			{
				this.mStartPosition_ = 0;
				this.mEndPosition_ = 0;
			}
		}

		#region IDisposable Members
		protected virtual void Dispose(bool disposing)
		{
			if (disposing && this.BaseStream != null)
			{
				if (this.IsWriting)
					this.FlushCache();

				if (this.BaseStreamOwner)
					this.BaseStream.Dispose();

				this.BaseStream = null;
				this.StreamPermissions = 0;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Stream-like interfaces
		public bool CanRead		{ get => this.StreamPermissions.CanRead(); }
		public bool CanWrite	{ get => this.StreamPermissions.CanWrite(); }
		public bool CanSeek		{ get => this.BaseStream.CanSeek; }

		public void Close()		=> this.Dispose();
		public void Flush()		=> this.FlushCache();
		#endregion

		#region Boolean
		public void Read(out bool value) => value = this.ReadBoolean();

		public void Write(bool value) => this.WriteWord(value ? 1U : 0U, Bits.K_BOOLEAN_BIT_COUNT);
		#endregion

		#region Single
		public float ReadSingle()
		{
			this.Read(out uint data, Bits.K_INT32_BIT_COUNT);

			return Bitwise.ByteSwap.SingleFromUInt32(data);
		}
		public void Read(out float value) => value = this.ReadSingle();

		public void Write(float value) => this.Write(Bitwise.ByteSwap.SingleToUInt32(value), Bits.K_INT32_BIT_COUNT);
		#endregion

		#region Double
		public double ReadDouble()
		{
			this.Read(out long data, Bits.K_INT64_BIT_COUNT);

			return BitConverter.Int64BitsToDouble(data);
		}
		public void Read(out double value) => value = this.ReadDouble();

		public void Write(double value) => this.Write(BitConverter.DoubleToInt64Bits(value), Bits.K_INT32_BIT_COUNT);
		#endregion

		#region DateTime
		public DateTime ReadDateTime(int bitCount = Bits.K_INT64_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_INT64_BIT_COUNT);

			this.Read(out long time64, bitCount);

			return Util.ConvertDateTimeFromUnixTime(time64);
		}
		public void Read(out DateTime value, int bitCount = Bits.K_INT64_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_INT64_BIT_COUNT);

			value = this.ReadDateTime(bitCount);
		}
		public void Write(DateTime value, int bitCount = Bits.K_INT64_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_INT64_BIT_COUNT);

			long time64 = Util.ConvertDateTimeToUnixTime(value);

			this.Write(time64, bitCount);
		}
		public BitStream Stream(ref DateTime value, int bitCount = Bits.K_INT64_BIT_COUNT)
		{
			Contract.Requires(bitCount <= Bits.K_INT64_BIT_COUNT);

				 if (this.IsReading) value = this.ReadDateTime(bitCount);
			else if (this.IsWriting)
				this.Write(value, bitCount);

			return this;
		}
		#endregion

		#region String
		// Verify that we have enough information to correctly stream a string
		static void ValidateStringStorageForStreaming(Memory.Strings.StringStorage s, int length)
		{
			// There are going to be issues if we try to read back a willy nilly char array string
			if (s.Type == Memory.Strings.StringStorageType.CHAR_ARRAY && !s.IsFixedLength && length <= 0)
			{
				throw new InvalidDataException(string.Format(Util.InvariantCultureInfo,
					"Provided string storage and length is invalid for Endian streaming: {0}, {1}",
					s.ToString(), length.ToString(Util.InvariantCultureInfo)));
			}
		}

		/// <summary>Read a string using a <see cref="Memory.Strings.StringStorage"/> definition and a provided character length</summary>
		/// <param name="storage">Definition for the string's characteristics</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <param name="prefixBitLength">Pascal only: Number of bits in the prefix count</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="storage"/> defines or
		/// doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Memory.Strings.StringStorage storage, int length = TypeExtensions.K_NONE,
			int maxLength = TypeExtensions.K_NONE, int prefixBitLength = TypeExtensions.K_NONE)
		{
			ValidateStringStorageForStreaming(storage, length);

			var sse = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);

			return sse.ReadString(this, length, maxLength, prefixBitLength);
		}
		/// <summary>Read a string using a <see cref="Text.StringStorageEncoding"/> definition and a provided character length</summary>
		/// <param name="encoding">Encoding to use for character streaming</param>
		/// <param name="length">Length, in characters, of the string.</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <param name="prefixBitLength">Pascal only: Number of bits in the prefix count</param>
		/// <returns></returns>
		/// <remarks>
		/// Length can be non-positive if <paramref name="storage"/> defines or
		/// doesn't require an explicit character length. If you do provide the
		/// length, this operation will perform faster in some cases.
		/// </remarks>
		public string ReadString(Text.StringStorageEncoding encoding, int length = TypeExtensions.K_NONE,
			int maxLength = TypeExtensions.K_NONE, int prefixBitLength = TypeExtensions.K_NONE)
		{
			Contract.Requires(encoding != null);
			ValidateStringStorageForStreaming(encoding.Storage, length);

			return encoding.ReadString(this, length, maxLength, prefixBitLength);
		}

		/// <summary>Writes a string based on a <see cref="Memory.Strings.StringStorage"/> definition</summary>
		/// <param name="value">String value to writee. Null defaults to an empty string</param>
		/// <param name="storage">Definition for how we're streaming the string</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		public void Write(string value, Memory.Strings.StringStorage storage,
			int maxLength = TypeExtensions.K_NONE/*, int prefixBitLength = TypeExtensions.kNone*/)
		{
			var sse = Text.StringStorageEncoding.TryAndGetStaticEncoding(storage);
			sse.WriteString(this, value ?? string.Empty, maxLength,
				prefixBitLength: TypeExtensions.K_NONE);
		}
		/// <summary>Writes a string using a <see cref="Text.StringStorageEncoding"/></summary>
		/// <param name="value">String value to writee. Null defaults to an empty string</param>
		/// <param name="encoding">Encoding to use for character streaming</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		public void Write(string value, Text.StringStorageEncoding encoding,
			int maxLength = TypeExtensions.K_NONE/*, int prefixBitLength = TypeExtensions.kNone*/)
		{
			Contract.Requires(encoding != null);

			encoding.WriteString(this, value ?? string.Empty, maxLength,
				prefixBitLength: TypeExtensions.K_NONE);
		}

		/// <summary>Serializes a string based on a <see cref="Memory.Strings.StringStorage"/> definition</summary>
		/// <param name="value"></param>
		/// <param name="storage">Definition for how we're streaming the string</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <returns></returns>
		public BitStream Stream(ref string value, Memory.Strings.StringStorage storage,
			int maxLength = TypeExtensions.K_NONE)
		{
				 if (this.IsReading) value = this.ReadString(storage, maxLength: maxLength);
			else if (this.IsWriting)
				this.Write(value, storage, maxLength: maxLength);

			return this;
		}
		/// <summary>Serializes a string using a <see cref="Text.StringStorageEncoding"/></summary>
		/// <param name="value"></param>
		/// <param name="encoding">Encoding to use for character streaming</param>
		/// <param name="maxLength">CString only: Optional maximum length of this specific string (exclusive of terminator)</param>
		/// <returns></returns>
		public BitStream Stream(ref string value, Text.StringStorageEncoding encoding,
			int maxLength = TypeExtensions.K_NONE)
		{
			Contract.Requires(encoding != null);

				 if (this.IsReading) value = this.ReadString(encoding, maxLength: maxLength);
			else if (this.IsWriting)
				this.Write(value, encoding, maxLength: maxLength);

			return this;
		}
		#endregion

		#region byte[]
		public void Read(byte[] buffer, int index, int count, int bitCount = Bits.K_BYTE_BIT_COUNT)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(index+count <= buffer.Length);
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);
#if false // #TODO redo optimization
			if (mCacheBitIndex == 0 && bitCount == Bits.kByteBitCount && count >= kWordByteCount)
			{
				LowLevel.Data.ByteSwap.ReplaceBytes(buffer, index, mCache);
				index += sizeof(TWord); count -= sizeof(TWord);

				// #TODO: need to handle unaligned reads
				BaseStream.Read(buffer, index, count);
				mCacheBitsStreamedCount += count * Bits.kByteBitCount;
				FillCache();
			}
			else
#endif
			{
				for (int x = index; x < count; x++)
					this.Read(out buffer[x], bitCount);
			}
		}
		public void Write(byte[] buffer, int index, int count, int bitCount = Bits.K_BYTE_BIT_COUNT)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(index+count <= buffer.Length);
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);
#if false // #TODO redo optimization
			if (mCacheBitIndex == 0 && bitCount == Bits.kByteBitCount)
			{
				// #TODO: need to handle unaligned writes
				BaseStream.Write(buffer, index, count);
				mCacheBitsStreamedCount += count * Bits.kByteBitCount;
			}
			else
#endif
			{
				for (int x = index; x < count; x++)
					this.Write(buffer[x], bitCount);
			}
		}
		public BitStream Stream(byte[] buffer, int index, int count, int bitCount = Bits.K_BYTE_BIT_COUNT)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(index+count <= buffer.Length);
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);

				 if (this.IsReading)
					 this.Read( buffer, index, count, bitCount);
			else if (this.IsWriting)
				this.Write(buffer, index, count, bitCount);

			return this;
		}

		public byte[] ReadBytes(int byteCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(byteCount >= 0);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			byte[] buffer = new byte[byteCount];

			if (byteCount > 0)
				this.Read(buffer, 0, byteCount);

			return buffer;
		}
		public byte[] Read(byte[] buffer, int bitCount = Bits.K_BYTE_BIT_COUNT)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);
			Contract.Ensures(Contract.Result<byte[]>() != null);

			if (buffer.Length > 0)
				this.Read(buffer, 0, buffer.Length, bitCount);

			return buffer;
		}
		public void Write(byte[] buffer, int bitCount = Bits.K_BYTE_BIT_COUNT)
		{
			Contract.Requires<ArgumentNullException>(buffer != null);
			Contract.Requires(bitCount <= Bits.K_BYTE_BIT_COUNT);

			if (buffer.Length > 0)
				this.Write(buffer, 0, buffer.Length, bitCount);
		}
		#endregion
	};
}
