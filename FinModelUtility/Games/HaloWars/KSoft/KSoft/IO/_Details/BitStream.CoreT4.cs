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
		#region Read\Write uint Impl
		internal void ReadWord(out uint word, int bitCount)
		{
			Contract.Requires(bitCount <= K_WORD_BIT_COUNT_);

			int bitsRemaining = this.CacheBitsRemaining;

			// if the requested bits are contained entirely in the cache...
			if (bitCount <= bitsRemaining)
			{
				word = (uint) this.ExtractWordFromCache(bitCount);
				this.mCacheBitIndex_ += bitCount;

				// If we consumed the rest of the cache after that last extraction
				if (this.mCacheBitIndex_ == K_WORD_BIT_COUNT_ && !this.IsEndOfStream)
					this.FillCache();
			}
			else // else the cache only has a portion of the bits (or needs to be re-filled)
			{
				int wordBitsRemaining = bitCount;

				// will always be negative, so abs it
				int msbShift = -(bitsRemaining - wordBitsRemaining);
				// get the word bits (MSB) that are left in the cache
				word = (uint) this.ExtractWordFromCache(bitsRemaining);
				wordBitsRemaining -= bitsRemaining;
				// adjust the bits to the MSB
				word <<= msbShift;

				this.FillCache(); // fill the cache with the next round of bits

				// get the 'rest' of the bits that weren't initially in our cache
				TWord moreBits = this.ExtractWordFromCache(wordBitsRemaining);

				word |= (uint)moreBits;
				this.mCacheBitIndex_ = wordBitsRemaining;
			}
		}
		internal void WriteWord(uint word, int bitCount)
		{
			Contract.Requires(bitCount <= K_WORD_BIT_COUNT_);

			int bitsRemaining = this.CacheBitsRemaining;

			// if the bits to write can be held entirely in the cache...
			if (bitCount <= bitsRemaining)
			{
				this.PutWordInCache((TWord)word, bitCount);
				this.mCacheBitIndex_ += bitCount;

				if (this.mCacheBitIndex_ == K_WORD_BIT_COUNT_)
					this.FlushCache();
			}
			else // else we have to split the cache writes between a flush
			{
				int wordBitsRemaining = bitCount;

				// will always be negative, so abs it
				int msbShift = -(bitsRemaining - wordBitsRemaining);
				// write the upper (MSB) word bits to the remaining cache bits
				this.PutWordInCache((TWord)(word >> msbShift), bitsRemaining);
				wordBitsRemaining -= bitsRemaining;

				// Flush determines the amount of bytes to write based on the current
				// bit index. This causes it to write all the bytes of the TWord
				this.mCacheBitIndex_ += bitsRemaining;
				this.FlushCache(); // flush the MSB results and reset the cache

				this.PutWordInCache((TWord)word, wordBitsRemaining);
				this.mCacheBitIndex_ = wordBitsRemaining;
			}
		}
		#endregion

		#region Read\Write ulong Impl
		internal void ReadWord(out ulong word, int bitCount)
		{
			Contract.Requires(bitCount <= K_WORD_BIT_COUNT_);

			int bitsRemaining = this.CacheBitsRemaining;

			// if the requested bits are contained entirely in the cache...
			if (bitCount <= bitsRemaining)
			{
				word = (ulong) this.ExtractWordFromCache(bitCount);
				this.mCacheBitIndex_ += bitCount;

				// If we consumed the rest of the cache after that last extraction
				if (this.mCacheBitIndex_ == K_WORD_BIT_COUNT_ && !this.IsEndOfStream)
					this.FillCache();
			}
			else // else the cache only has a portion of the bits (or needs to be re-filled)
			{
				int wordBitsRemaining = bitCount;

				// will always be negative, so abs it
				int msbShift = -(bitsRemaining - wordBitsRemaining);
				// get the word bits (MSB) that are left in the cache
				word = (ulong) this.ExtractWordFromCache(bitsRemaining);
				wordBitsRemaining -= bitsRemaining;
				// adjust the bits to the MSB
				word <<= msbShift;

				this.FillCache(); // fill the cache with the next round of bits

				// get the 'rest' of the bits that weren't initially in our cache
				TWord moreBits = this.ExtractWordFromCache(wordBitsRemaining);

				word |= (ulong)moreBits;
				this.mCacheBitIndex_ = wordBitsRemaining;
			}
		}
		internal void WriteWord(ulong word, int bitCount)
		{
			Contract.Requires(bitCount <= K_WORD_BIT_COUNT_);

			int bitsRemaining = this.CacheBitsRemaining;

			// if the bits to write can be held entirely in the cache...
			if (bitCount <= bitsRemaining)
			{
				this.PutWordInCache((TWord)word, bitCount);
				this.mCacheBitIndex_ += bitCount;

				if (this.mCacheBitIndex_ == K_WORD_BIT_COUNT_)
					this.FlushCache();
			}
			else // else we have to split the cache writes between a flush
			{
				int wordBitsRemaining = bitCount;

				// will always be negative, so abs it
				int msbShift = -(bitsRemaining - wordBitsRemaining);
				// write the upper (MSB) word bits to the remaining cache bits
				this.PutWordInCache((TWord)(word >> msbShift), bitsRemaining);
				wordBitsRemaining -= bitsRemaining;

				// Flush determines the amount of bytes to write based on the current
				// bit index. This causes it to write all the bytes of the TWord
				this.mCacheBitIndex_ += bitsRemaining;
				this.FlushCache(); // flush the MSB results and reset the cache

				this.PutWordInCache((TWord)word, wordBitsRemaining);
				this.mCacheBitIndex_ = wordBitsRemaining;
			}
		}
		#endregion

	};
}