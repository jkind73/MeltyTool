using System;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Collections
{
	using StateFilterEnumerator = ReadOnlyBitSetEnumerators.StateFilterEnumerator;

	using StateFilterEnumeratorWrapper = EnumeratorWrapper<int, ReadOnlyBitSetEnumerators.StateFilterEnumerator>;

	partial class BitSet
	{
		/// <summary>Get the bit index of the next bit which is 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next clear bit index, or -1 if one isn't found</returns>
		[Contracts.Pure]
		public int NextClearBitIndex(int startBitIndex = 0)
		{
			return this.NextBitIndex(startBitIndex, false);
		}
		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		public StateFilterEnumeratorWrapper ClearBitIndices { get {
			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, false));
		} }
		/// <summary>Enumeration of bit indexes in this BitSet which are 0 (clear)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		public StateFilterEnumeratorWrapper ClearBitIndicesStartingAt(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < this.Length);

			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, false, startBitIndex));
		}

		/// <summary>Get the bit index of the next bit which is 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		/// <returns>The next set bit index, or -1 if one isn't found</returns>
		[Contracts.Pure]
		public int NextSetBitIndex(int startBitIndex = 0)
		{
			return this.NextBitIndex(startBitIndex, true);
		}
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		public StateFilterEnumeratorWrapper SetBitIndices { get {
			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, true));
		} }
		/// <summary>Enumeration of bit indexes in this BitSet which are 1 (set)</summary>
		/// <param name="startBitIndex">Bit index to start at</param>
		public StateFilterEnumeratorWrapper SetBitIndicesStartingAt(int startBitIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex < this.Length);

			return new StateFilterEnumeratorWrapper(new StateFilterEnumerator(this, true, startBitIndex));
		}


		public void ClearBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= this.Length);

			if (bitCount <= 0)
				return ;

			var fromWordMask = KVectorElementSectionBitMask(startBitIndex);
			var lastWordMask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int lastBitIndex = (startBitIndex+bitCount) - 1;
			var fromWordIndex = KVectorIndexInT(startBitIndex);
			var lastWordIndex = KVectorIndexInT(lastBitIndex);

			// target bits are only in one word...
			if (fromWordIndex == lastWordIndex)
			{
				var mask = fromWordMask;// & last_word_mask;
				this.RecalculateCardinalityUndoRound(fromWordIndex);
				Bitwise.Flags.Remove(ref this.mArray_[fromWordIndex], mask);
				this.RecalculateCardinalityRound(fromWordIndex);
				return;
			}
			// or the target bits are in multiple words...

			// handle the first word
			this.RecalculateCardinalityUndoRound(fromWordIndex);
			Bitwise.Flags.Remove(ref this.mArray_[fromWordIndex], fromWordMask);
			this.RecalculateCardinalityRound(fromWordIndex);

			// handle any words in between
			for (int x = fromWordIndex+1; x < lastWordIndex; x++)
			{
				this.RecalculateCardinalityUndoRound(x);
				this.mArray_[x] = K_WORD_ALL_BITS_CLEAR_;
			}

			// handle the last word
			this.RecalculateCardinalityUndoRound(lastWordIndex);
			Bitwise.Flags.Remove(ref this.mArray_[lastWordIndex], lastWordMask);
			this.RecalculateCardinalityRound(lastWordIndex);
		}

		public void SetBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= this.Length);

			if (bitCount <= 0)
				return ;

			var fromWordMask = KVectorElementSectionBitMask(startBitIndex);
			var lastWordMask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int lastBitIndex = (startBitIndex+bitCount) - 1;
			var fromWordIndex = KVectorIndexInT(startBitIndex);
			var lastWordIndex = KVectorIndexInT(lastBitIndex);

			// target bits are only in one word...
			if (fromWordIndex == lastWordIndex)
			{
				var mask = fromWordMask;// & last_word_mask;
				this.RecalculateCardinalityUndoRound(fromWordIndex);
				Bitwise.Flags.Add(ref this.mArray_[fromWordIndex], mask);
				this.RecalculateCardinalityRound(fromWordIndex);
				return;
			}
			// or the target bits are in multiple words...

			// handle the first word
			this.RecalculateCardinalityUndoRound(fromWordIndex);
			Bitwise.Flags.Add(ref this.mArray_[fromWordIndex], fromWordMask);
			this.RecalculateCardinalityRound(fromWordIndex);

			// handle any words in between
			for (int x = fromWordIndex+1; x < lastWordIndex; x++)
			{
				this.RecalculateCardinalityUndoRound(x);
				this.mArray_[x] = K_WORD_ALL_BITS_SET_;
				this.Cardinality += K_WORD_BIT_COUNT_;
			}

			// handle the last word
			this.RecalculateCardinalityUndoRound(lastWordIndex);
			Bitwise.Flags.Add(ref this.mArray_[lastWordIndex], lastWordMask);
			this.RecalculateCardinalityRound(lastWordIndex);
		}

		public void ToggleBits(int startBitIndex, int bitCount)
		{
			Contract.Requires<ArgumentOutOfRangeException>(startBitIndex >= 0 && startBitIndex < this.Length);
			Contract.Requires<ArgumentOutOfRangeException>((startBitIndex+bitCount) <= this.Length);

			if (bitCount <= 0)
				return ;

			var fromWordMask = KVectorElementSectionBitMask(startBitIndex);
			var lastWordMask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int lastBitIndex = (startBitIndex+bitCount) - 1;
			var fromWordIndex = KVectorIndexInT(startBitIndex);
			var lastWordIndex = KVectorIndexInT(lastBitIndex);

			// target bits are only in one word...
			if (fromWordIndex == lastWordIndex)
			{
				var mask = fromWordMask;// & last_word_mask;
				this.RecalculateCardinalityUndoRound(fromWordIndex);
				Bitwise.Flags.Toggle(ref this.mArray_[fromWordIndex], mask);
				this.RecalculateCardinalityRound(fromWordIndex);
				return;
			}
			// or the target bits are in multiple words...

			// handle the first word
			this.RecalculateCardinalityUndoRound(fromWordIndex);
			Bitwise.Flags.Toggle(ref this.mArray_[fromWordIndex], fromWordMask);
			this.RecalculateCardinalityRound(fromWordIndex);

			// handle any words in between
			for (int x = fromWordIndex+1; x < lastWordIndex; x++)
			{
				this.RecalculateCardinalityUndoRound(x);
				Bitwise.Flags.Toggle(ref this.mArray_[x], this.mArray_[x]);
				this.RecalculateCardinalityRound(x);
			}

			// handle the last word
			this.RecalculateCardinalityUndoRound(lastWordIndex);
			Bitwise.Flags.Toggle(ref this.mArray_[lastWordIndex], lastWordMask);
			this.RecalculateCardinalityRound(lastWordIndex);
		}

		[Contracts.Pure]
		public bool TestBits(int startBitIndex, int bitCount)
		{
			if (bitCount <= 0)
				return false;

			var fromWordMask = KVectorElementSectionBitMask(startBitIndex);
			var lastWordMask = GetCabooseRetainedBitsMask(startBitIndex+bitCount);

			int lastBitIndex = (startBitIndex+bitCount) - 1;
			var fromWordIndex = KVectorIndexInT(startBitIndex);
			var lastWordIndex = KVectorIndexInT(lastBitIndex);

			// target bits are only in one word...
			if (fromWordIndex == lastWordIndex)
			{
				var mask = fromWordMask;// & last_word_mask;
				return Bitwise.Flags.TestAny(this.mArray_[fromWordIndex], mask);
			}
			// or the target bits are in multiple words...

			// handle the first word
			if (Bitwise.Flags.TestAny(this.mArray_[fromWordIndex], fromWordMask))
				return true;

			// handle any words in between
			for (int x = fromWordIndex+1; x < lastWordIndex; x++)
			{
				if (this.mArray_[x] > K_WORD_ALL_BITS_CLEAR_)
					return true;
			}

			// handle the last word
			return Bitwise.Flags.TestAny(this.mArray_[lastWordIndex], lastWordMask);
		}

	};
}