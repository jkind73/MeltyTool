using System;
using System.Diagnostics.CodeAnalysis;

namespace KSoft.Text
{
	using Memory.Strings;

	partial class StringStorageEncoding
	{
		#region CalculateByteCount
		/// <summary>Calculate how many additional bytes are needed to encode a raw <see cref="StringStorageType.C_STRING"/> string</summary>
		/// <param name="byteCount">Base characters byte count</param>
		/// <returns>Total byte count needed for encoding a <see cref="StringStorageType.C_STRING"/> string</returns>
		int CalcByteCountCString(int byteCount)
		{
			return byteCount + this.mNullCharacterSize_;
		}
		/// <summary>Calculate how many additional bytes are needed to encode a raw <see cref="StringStorageType.PASCAL"/> string</summary>
		/// <param name="byteCount">Base characters byte count</param>
		/// <returns>Total byte count needed for encoding a <see cref="StringStorageType.PASCAL"/> string</returns>
		int CalcByteCountPascal(int byteCount)
		{
			switch (this.mStorage_.LengthPrefix)
			{
				case StringStorageLengthPrefix.INT7: return byteCount + Bitwise.Encoded7BitInt.CalculateSize(byteCount);
				case StringStorageLengthPrefix.INT8: return byteCount + sizeof(byte);
				case StringStorageLengthPrefix.INT16:return byteCount + sizeof(short);
				case StringStorageLengthPrefix.INT32:return byteCount + sizeof(int);
				default:
					throw new Debug.UnreachableException(this.mStorage_.LengthPrefix.ToString());
			}
		}

	/// <summary>Calculate how many additional bytes are needed for encoding a raw string</summary>
	/// <param name="byteCount">Base characters byte count</param>
	/// <returns>Total byte count needed for encoding a string</returns>
	int CalculateByteCount(int byteCount)
		{
			if (this.mStorage_.IsFixedLength)
				return this.mFixedLengthByteLength_;

			switch (this.mStorage_.Type)
			{
				case StringStorageType.C_STRING: byteCount = this.CalcByteCountCString(byteCount); break;
				case StringStorageType.PASCAL:  byteCount = this.CalcByteCountPascal(byteCount); break;
				// CharArray doesn't do anything anyway
				case StringStorageType.CHAR_ARRAY:	/*byteCount = CalcByteCountCharArray(byteCount);*/ break;
				default:
					throw new Debug.UnreachableException(this.mStorage_.Type.ToString());
			}

			return byteCount;
		}
		#endregion

		/// <summary>If the storage requires a fixed length, this will clamp the count to be within that length</summary>
		/// <param name="charCount"></param>
		void ClampCharCount(ref int charCount)
		{
			if (!this.mStorage_.IsFixedLength)
				return;

			switch (this.mStorage_.Type)
			{
				case StringStorageType.C_STRING:
					int fixedLength = this.mStorage_.FixedLength - 1; // don't include null char

					if (charCount > fixedLength) charCount = fixedLength;
					break;
				case StringStorageType.CHAR_ARRAY:
					if (charCount > this.mStorage_.FixedLength) charCount = this.mStorage_.FixedLength;
					break;
				default:
					throw new Debug.UnreachableException(this.mStorage_.Type.ToString());
			}
		}

		#region Encode StringStorageType Data Prefix
		int EncStringStorageTypePrefixPascalData(int charCount, byte[] bytes, int byteIndex)
		{
			int prefixBytes;
			switch (this.mStorage_.LengthPrefix)
			{
				case StringStorageLengthPrefix.INT7:	Bitwise.Encoded7BitInt.Write(bytes, byteIndex, charCount);
					prefixBytes = Bitwise.Encoded7BitInt.CalculateSize(charCount); break;
				case StringStorageLengthPrefix.INT8:	bytes[byteIndex] = (byte)charCount;
					prefixBytes = sizeof(byte); break;
				case StringStorageLengthPrefix.INT16:	Bitwise.ByteSwap.ReplaceBytes(bytes, byteIndex, (short)charCount);
														Bitwise.ByteSwap.SwapInt16(bytes, byteIndex);
					prefixBytes = sizeof(short); break;
				case StringStorageLengthPrefix.INT32:	Bitwise.ByteSwap.ReplaceBytes(bytes, byteIndex, charCount);
														Bitwise.ByteSwap.SwapInt32(bytes, byteIndex);
					prefixBytes = sizeof(int); break;
				default:
					throw new Debug.UnreachableException(this.mStorage_.LengthPrefix.ToString());
			}

			return prefixBytes;
		}
		/// <summary>Encode any prefix related data for the <see cref="StringStorageType"/> into a byte array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="charIndex">The index of the first character to encode</param>
		/// <param name="charCount">The number of characters to encode</param>
		/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
		/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
		/// <returns>Number of prefix bytes written into <paramref name="bytes"/></returns>
		int EncodeStringStorageTypePrefixData(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			char[] chars,
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			int charIndex,
			int charCount, byte[] bytes, int byteIndex)
		{
			switch (this.mStorage_.Type)
			{
				// No prefix for CString
				case StringStorageType.C_STRING:		return 0;
				case StringStorageType.PASCAL:		return this.EncStringStorageTypePrefixPascalData(charCount, bytes, byteIndex);
				// CharArray doesn't do anything anyway
				case StringStorageType.CHAR_ARRAY:	return 0;
				default:
					throw new Debug.UnreachableException(this.mStorage_.Type.ToString());
			}
		}
		#endregion

		#region Encode StringStorageType Data Postfix
		int EncStringStoragePostfixCStringData(byte[] bytes, int byteIndex)
		{
			for (int x = byteIndex; x < this.mNullCharacterSize_; x++)
				bytes[x] = 0;

			return this.mNullCharacterSize_; // number of bytes written into [bytes]
		}
		/// <summary>Encode any additional <see cref="StringStorageType"/> related data into a byte array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="charIndex">The index of the first character to encode</param>
		/// <param name="charCount">The number of characters to encode</param>
		/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
		/// <param name="byteIndex">The index at which to start writing the resulting sequence of postfix bytes</param>
		/// <returns>The actual number of bytes written into <paramref name="bytes"/></returns>
		int EncodeStringStorageTypePostfixData(
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			char[] chars,
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			int charIndex,
			[SuppressMessage("Microsoft.Design", "CA1801:ReviewUnusedParameters")]
			int charCount,
			byte[] bytes, int byteIndex)
		{
			switch (this.mStorage_.Type)
			{
				case StringStorageType.C_STRING:		return this.EncStringStoragePostfixCStringData(bytes, byteIndex);
				// No postfix for Pascal
				case StringStorageType.PASCAL:		return 0;
				// CharArray doesn't do anything anyway
				case StringStorageType.CHAR_ARRAY:	return 0;
				default:
					throw new Debug.UnreachableException(this.mStorage_.Type.ToString());
			}
		}
		#endregion

		/// <summary>Converts a set of characters into a sequence of bytes.</summary>
		class Encoder : System.Text.Encoder
		{
			StringStorageEncoding mEncoding_;
			System.Text.Encoder mEnc_;
			public Encoder(StringStorageEncoding enc) {
				this.mEncoding_ = enc;
				this.mEnc_ = enc.mBaseEncoding_.GetEncoder(); }

			/// <summary>
			/// Calculates the number of bytes produced by encoding a set of characters from the specified character array.
			/// A parameter indicates whether to clear the internal state of the encoder after the calculation.
			/// </summary>
			/// <param name="chars">The character array containing the set of characters to encode</param>
			/// <param name="index">The index of the first character to encode</param>
			/// <param name="count">The number of characters to encode</param>
			/// <param name="flush"><b>true</b> to simulate clearing the internal state of the encoder after the calculation; otherwise, <b>false</b></param>
			/// <returns>The number of bytes produced by encoding the specified characters and any characters in the internal buffer</returns>
			/// <seealso cref="System.Text.Encoder.GetByteCount(Char[], Int32, Int32, Boolean) "/>
			public override int GetByteCount(char[] chars, int index, int count, bool flush)
			{
				int byteCount = this.mEnc_.GetByteCount(chars, index, count, this.mEncoding_.DontAlwaysFlush ? flush : true);

				byteCount = this.mEncoding_.CalculateByteCount(byteCount); // Add our String Storage calculations

				return byteCount;
			}

			/// <summary>
			/// Encodes a set of characters from the specified character array and any characters in the internal buffer into the specified byte array.
			/// A parameter indicates whether to clear the internal state of the encoder after the conversion.
			/// </summary>
			/// <param name="chars">The character array containing the set of characters to encode</param>
			/// <param name="charIndex">The index of the first character to encode</param>
			/// <param name="charCount">The number of characters to encode</param>
			/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
			/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
			/// <param name="flush">true to clear the internal state of the encoder after the conversion; otherwise, false</param>
			/// <returns>The actual number of bytes written into <paramref name="bytes"/></returns>
			/// <seealso cref="System.Text.Encoder.GetBytes(Char[], Int32, Int32, Byte[], Int32, Boolean) "/>
			public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
			{
				// Add our String Storage calculations
				int bytesWritten = this.mEncoding_.EncodeStringStorageTypePrefixData(chars, charCount, charCount, bytes, byteIndex);

				bytesWritten += this.mEnc_.GetBytes(chars, charIndex, charCount, bytes, byteIndex + bytesWritten, this.mEncoding_.DontAlwaysFlush ? flush : true);

				// Add our String Storage calculations
				bytesWritten += this.mEncoding_.EncodeStringStorageTypePostfixData(chars, charIndex, charCount, bytes, bytesWritten);

				return bytesWritten;
			}

			/// <summary>Sets the encoder back to its initial state</summary>
			public override void Reset()	{
				this.mEnc_.Reset(); }
		};

		#region WriteString
		internal void WriteString(IO.BitStream s, string value, int maxLength = -1, int prefixBitLength = -1)
		{
			if (prefixBitLength > 0)
				throw new NotSupportedException("Currently don't support unnatural bit lengths for prefixes on writes");

			int length = value.Length;
			if (maxLength > 0)
				length = Math.Min(maxLength, length);

			char[] chars = value.ToCharArray(0, length);
			byte[] bytes = this.GetBytes(chars);
			s.Write(bytes);
		}
		#endregion
	};
}
