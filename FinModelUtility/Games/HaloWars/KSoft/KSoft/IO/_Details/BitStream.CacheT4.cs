using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using TWord = System.UInt32;

namespace KSoft.IO
{
	partial class BitStream
	{
		/// <summary>Number of bytes in <see cref="mCache_"/></summary>
		const int K_WORD_BYTE_COUNT_ = sizeof(TWord);
		/// <summary>Number of bits in <see cref="mCache_"/></summary>
		const int K_WORD_BIT_COUNT_ = sizeof(TWord) * Bits.K_BYTE_BIT_COUNT;
		/// <summary>Bit count to bit-mask look up table</summary>
		static readonly TWord[] KBitmaskLut;

		static void InitializeBitmaskLookUpTable(out TWord[] table)
		{
			bool success = Bits.GetBitConstants(typeof(TWord),
				out int wordByteCount, out int wordBitCount, out int wordBitShift, out int wordBitMod);
			Contract.Assert(success, "TWord is an invalid type for BitStream");

			Bits.BitmaskLookUpTableGenerate(K_WORD_BIT_COUNT_, out table);
		}


		/// <summary>The bit cache we use for streaming to/from <see cref="BaseStream"/></summary>
		TWord mCache_;

		// #REVIEW: change mIoBuffer to be kWordByteCount and do an entire Read/Write() instead of looping?
		// #REVIEW: maybe change the ReadWord implementation to not automatically populate the next word...
		#region Cache operations
		/// <summary>Fill the cache with <see cref="K_WORD_BYTE_COUNT_"/> or fewer bytes bytes</summary>
		void FillCache()
		{
			this.mCache_ = 0;
			this.mCacheBitIndex_ = 0;
			this.mCacheBitsStreamedCount_ = 0;

			int byteCount = K_WORD_BYTE_COUNT_-1; // number of bytes to try and read
			int shift = K_WORD_BIT_COUNT_-Bits.K_BYTE_BIT_COUNT; // start shifting to the MSB
			while (	!this.IsEndOfStream &&
					byteCount >= 0 &&
					this.BaseStream.Read(this.mIoBuffer_, 0, sizeof(byte)) != 0 )
			{
				this.mCache_ |= ((TWord) this.mIoBuffer_[0]) << shift;
				--byteCount;
				shift -= Bits.K_BYTE_BIT_COUNT;
				this.mCacheBitsStreamedCount_ += Bits.K_BYTE_BIT_COUNT;
			}

			if (byteCount != -1 && this.ThrowOnOverflow.CanRead())
				throw new System.IO.EndOfStreamException("Tried to read more bits than the stream has/can see");
		}
		/// <summary>Flush the cache to <see cref="BaseStream"/> with <see cref="K_WORD_BYTE_COUNT_"/> or fewer bytes bytes</summary>
		void FlushCache()
		{
			#if !CONTRACTS_FULL_SHIM // can't do this with our shim! ValueAtReturn sets out param to default ON ENTRY
			Contract.Ensures(Contract.ValueAtReturn(out mCache) == 0);
			Contract.Ensures(Contract.ValueAtReturn(out mCacheBitIndex) == 0);
			#endif

			if (this.mCacheBitIndex_ == 0) // no bits to flush!
			{
				Contract.Assert(this.mCache_ == 0, "Why is there data in the cache?");
				return;
			}

			this.mCacheBitsStreamedCount_ = 0;

			int byteCount = (this.mCacheBitIndex_-1) >> Bits.K_BYTE_BIT_SHIFT; // number of bytes to try and write
			int shift = K_WORD_BIT_COUNT_-Bits.K_BYTE_BIT_COUNT; // start shifting from the MSB
			while (	/*!IsEndOfStream &&*/
					byteCount >= 0)
			{
				this.mIoBuffer_[0] = (byte)(this.mCache_ >> shift);
				this.BaseStream.Write(this.mIoBuffer_, 0, sizeof(byte));
				--byteCount;
				shift -= Bits.K_BYTE_BIT_COUNT;
				this.mCacheBitsStreamedCount_ += Bits.K_BYTE_BIT_COUNT;
			}

			if (byteCount != -1 && this.ThrowOnOverflow.CanWrite())
				throw new System.IO.EndOfStreamException("Tried to write more bits than the stream has/can see");

			this.mCache_ = 0;
			this.mCacheBitIndex_ = 0;
		}

		/// <remarks>Don't call me unless you are ReadWord</remarks>
		[Contracts.Pure]
		TWord ExtractWordFromCache(int bitCount)
		{
			// amount to shift the bits extracted from mCache
			int shift = K_WORD_BIT_COUNT_ - (this.mCacheBitIndex_ + bitCount);
			TWord wordMask = KBitmaskLut[bitCount];

			TWord word = this.mCache_;
			word >>= shift;
			word &= wordMask;

			return word;
		}
		/// <remarks>Don't call me unless you are WriteWord</remarks>
		void PutWordInCache(TWord word, int bitCount)
		{
			Contract.Ensures(Contract.OldValue(this.mCacheBitIndex_) == this.mCacheBitIndex_);

			// amount to shift word before appending it to mCache bits
			int shift = (K_WORD_BIT_COUNT_ - this.mCacheBitIndex_) - bitCount;
			TWord wordMask = KBitmaskLut[bitCount];

			word &= wordMask;
			word <<= shift;
			this.mCache_ |= word;
		}
		#endregion

		public bool ReadBoolean()
		{
			this.ReadWord(out TWord word, Bits.K_BOOLEAN_BIT_COUNT);

			return 1 == word;
		}
	};
}
