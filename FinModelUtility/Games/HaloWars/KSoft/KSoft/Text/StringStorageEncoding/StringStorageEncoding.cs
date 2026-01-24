using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Text
{
	using Memory.Strings;

	/// <summary>Encapsulates an <see cref="Encoding"/> from a <see cref="StringStorage"/> definition</summary>
	/// <remarks>
	/// For <see cref="StringStorageType.C_STRING"/> cases, the encoding does not check if there
	/// is an existing '\0' character in the user supplied strings. If you pass such strings to
	/// the encoding for streaming the results will be undefined. Oh and bad.
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public sealed partial class StringStorageEncoding
		: Encoding
		, IEquatable<StringStorageEncoding>, IEqualityComparer<StringStorageEncoding>
		, IComparer<StringStorageEncoding>, IComparable<StringStorageEncoding>
	{
		Encoding mBaseEncoding_;
		#region Storage
		StringStorage mStorage_;
		/// <summary>The string storage definition for this encoding</summary>
		public StringStorage Storage { get => this.mStorage_; }
		#endregion
		Options mOptions_;
		bool DontAlwaysFlush { get => (this.mOptions_ & Options.DONT_ALWAYS_FLUSH) != 0; }
		/// <summary>Number of bytes a null character consumes</summary>
		int mNullCharacterSize_;
		/// <summary>
		/// Number of bytes used to store a fixed length character array using
		/// the <see cref="StringStorageType"/> defined in <see cref="storage"/>
		/// </summary>
		int mFixedLengthByteLength_;

		#region Ctor
		/// <summary>Initialize an encoding for this library's methods of String Storages</summary>
		/// <param name="storage">Storage definition</param>
		/// <param name="options"><see cref="System.Text.Encoding"/> options</param>
		public StringStorageEncoding(StringStorage storage, Options options = 0)
		{
			bool useBom = (options & Options.USE_BYTE_ORDER_MARK) != 0;
			bool bigEndian = storage.ByteOrder == Shell.EndianFormat.BIG;
			bool throwOnInvalid = (options & Options.THROW_ON_INVALID_BYTES) != 0;

			this.mNullCharacterSize_ = sizeof(byte); // The majority of encodings only require 1 byte for the null character
			switch (storage.WidthType)
			{
				case StringStorageWidthType.ASCII:
					this.mBaseEncoding_ = ASCII;
					break;
				case StringStorageWidthType.UNICODE:
					this.mBaseEncoding_ = new UnicodeEncoding(bigEndian, useBom, throwOnInvalid);
					this.mNullCharacterSize_ = UnicodeEncoding.CharSize; // #REVIEW: should we really do this?
					break;
				case StringStorageWidthType.UTF7:
					this.mBaseEncoding_ = new UTF7Encoding(!throwOnInvalid);
					break;
				case StringStorageWidthType.UTF8:
					this.mBaseEncoding_ = new UTF8Encoding(useBom, throwOnInvalid);
					break;
				case StringStorageWidthType.UTF32:
					this.mBaseEncoding_ = new UTF32Encoding(bigEndian, useBom, throwOnInvalid);
					this.mNullCharacterSize_ = 4 * sizeof(byte);
					break;

				default: throw new Debug.UnreachableException();
			}

			this.mFixedLengthByteLength_ = !storage.IsFixedLength
				? 0
				: this.GetMaxCleanByteCount(storage.FixedLength);

			this.mStorage_ = storage;
			this.mOptions_ = options;
		}

		public override object Clone()
		{
			return new StringStorageEncoding(this.mStorage_, this.mOptions_);
		}
		#endregion

		#region Implementation
		/// <summary>Calculates the number of bytes produced by encoding a set of characters from the specified character array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="index">The index of the first character to encode</param>
		/// <param name="count">The number of characters to encode</param>
		/// <returns>The number of bytes produced by encoding the specified characters</returns>
		public override int GetByteCount(char[] chars, int index, int count)
		{
			// In case someone is trying to encode a string outside of the storage's bounds
			this.ClampCharCount(ref count);

			int byteCount = this.mBaseEncoding_.GetByteCount(chars, index, count);

			byteCount = this.CalculateByteCount(byteCount); // Add our String Storage calculations

			return byteCount;
		}
		/// <summary>Encodes a set of characters from the specified character array into the specified byte array</summary>
		/// <param name="chars">The character array containing the set of characters to encode</param>
		/// <param name="charIndex">The index of the first character to encode</param>
		/// <param name="charCount">The number of characters to encode</param>
		/// <param name="bytes">The byte array to contain the resulting sequence of bytes</param>
		/// <param name="byteIndex">The index at which to start writing the resulting sequence of bytes</param>
		/// <returns>The actual number of bytes written into bytes</returns>
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			// In case someone is trying to encode a string outside of the storage's bounds
			this.ClampCharCount(ref charCount);

			// Add our String Storage calculations
			int bytesWritten = this.EncodeStringStorageTypePrefixData(chars, charIndex, charCount, bytes, byteIndex);

			bytesWritten += this.mBaseEncoding_.GetBytes(chars, charIndex, charCount, bytes, byteIndex + bytesWritten);

			// Add our String Storage calculations
			bytesWritten += this.EncodeStringStorageTypePostfixData(chars, charIndex, charCount, bytes, bytesWritten);

			return bytesWritten;
		}
		/// <summary>Calculates the number of characters produced by decoding a sequence of bytes from the specified byte array</summary>
		/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
		/// <param name="index">The index of the first byte to decode</param>
		/// <param name="count">The number of bytes to decode</param>
		/// <returns>The number of characters produced by decoding the specified sequence of bytes.</returns>
		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			count = this.CalculateCharByteCount(bytes, ref index, count); // Remove our String Storage calculations

			int charCount = this.mBaseEncoding_.GetCharCount(bytes, index, count);

			return charCount;
		}
		/// <summary>Decodes a sequence of bytes from the specified byte array into the specified character array</summary>
		/// <param name="bytes">The byte array containing the sequence of bytes to decode</param>
		/// <param name="byteIndex">The index of the first byte to decode</param>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <param name="chars">The character array to contain the resulting set of characters.</param>
		/// <param name="charIndex">The index at which to start writing the resulting set of characters</param>
		/// <returns>The actual number of characters written into chars</returns>
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			byteCount = this.CalculateCharByteCount(bytes, ref byteIndex, byteCount); // Remove our String Storage calculations

			int charsWritten = this.mBaseEncoding_.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

			return charsWritten;
		}
		/// <summary>Calculates the maximum number of bytes produced by encoding the specified number of characters</summary>
		/// <param name="charCount">The number of characters to encode</param>
		/// <returns>The maximum number of bytes produced by encoding the specified number of characters</returns>
		public override int GetMaxByteCount(int charCount)
		{
			int maxCount = this.mBaseEncoding_.GetMaxByteCount(charCount);

			maxCount = this.CalculateByteCount(maxCount); // Add our String Storage calculations

			return maxCount;
		}
		/// <summary>
		/// Calculates the maximum number of bytes produced by encoding the specified number of characters WITHOUT
		/// THE EXTRA SURROGATE JIZZ
		/// </summary>
		/// <param name="charCount">The number of characters to encode</param>
		/// <returns></returns>
		public int GetMaxCleanByteCount(int charCount)
		{
			int maxCount = this.mBaseEncoding_.GetMaxByteCount(charCount);

			// NOTE: that GetMaxByteCount considers potential leftover surrogates from a previous decoder operation.
			// Because of the decoder, passing a value of 1 to the method retrieves 2 for a single-byte encoding,
			// such as ASCII. Your application should use the IsSingleByte property if this information is necessary.
			// ...That being said, it looks like they internally use a null character. So for streaming related cases,
			// we have to circumcise the fucking count.
			maxCount -= this.mNullCharacterSize_;

			return maxCount;
		}
		/// <summary>Calculates the maximum number of characters produced by decoding the specified number of bytes</summary>
		/// <param name="byteCount">The number of bytes to decode</param>
		/// <returns>The maximum number of characters produced by decoding the specified number of bytes</returns>
		public override int GetMaxCharCount(int byteCount)
		{
			// For Pascal type strings, this will give a larger count
			// than usual, even for Max standards, since we can't
			// sneak a peak at the length prefix bytes
			byteCount = this.CalculateCharByteCount(byteCount); // Remove our String Storage calculations

			int maxCount = this.mBaseEncoding_.GetMaxCharCount(byteCount);

			return maxCount;
		}
		// #REVIEW: override?
		//public override string ToString()		{ return mBaseEncoding.ToString(); }
		#endregion

		#region overrides to baseEncoding
		public override string BodyName			=> this.mBaseEncoding_.BodyName;
		public override int CodePage			=> this.mBaseEncoding_.CodePage;
		public override string EncodingName		=> this.mBaseEncoding_.EncodingName;
		public override string HeaderName		=> this.mBaseEncoding_.HeaderName;
		public override bool IsBrowserDisplay	=> this.mBaseEncoding_.IsBrowserDisplay;
		public override bool IsBrowserSave		=> this.mBaseEncoding_.IsBrowserSave;
		public override bool IsMailNewsDisplay	=> this.mBaseEncoding_.IsMailNewsDisplay;
		public override bool IsMailNewsSave		=> this.mBaseEncoding_.IsMailNewsSave;
		public override bool IsSingleByte		=> this.mBaseEncoding_.IsSingleByte;
		public override string WebName			=> this.mBaseEncoding_.WebName;
		public override int WindowsCodePage		=> this.mBaseEncoding_.WindowsCodePage;

		/// <summary>Compares this to another object testing for equality</summary>
		/// <param name="obj"></param>
		/// <returns>
		/// True if both this object and <paramref name="obj"/> are equal.
		/// False if <paramref name="obj"/> is not a <see cref="StringStorageEncoding"/></returns>
		public override bool Equals(object value)
		{
			//return mBaseEncoding.Equals(value);
			if (value is StringStorageEncoding e)
				return this.Equals(e);

			return false;
		}
		public override System.Text.Decoder GetDecoder() => new Decoder(this);
		public override System.Text.Encoder GetEncoder() => new Encoder(this);
		public override int GetHashCode() => this.mBaseEncoding_.GetHashCode();
		public override byte[] GetPreamble() => this.mBaseEncoding_.GetPreamble();
		public override bool IsAlwaysNormalized(NormalizationForm form) => this.mBaseEncoding_.IsAlwaysNormalized(form);
		#endregion

		#region IEquatable<StringStorageEncoding> Members
		/// <summary>
		/// Compares this to another <see cref="StringStorageEncoding"/> object testing
		/// their underlying fields for equality
		/// </summary>
		/// <param name="obj">other <see cref="StringStorageEncoding"/> object</param>
		/// <returns>true if both this object and <paramref name="obj"/> are equal</returns>
		public bool Equals(StringStorageEncoding other)
		{
			return this.mOptions_ == other.mOptions_ &&
			       this.mStorage_.Equals(other.mStorage_);
		}

		public bool Equals(StringStorageEncoding x, StringStorageEncoding y) => x.Equals(y);

		public int GetHashCode(StringStorageEncoding obj) => obj.GetHashCode();
		#endregion

		#region IComparer<StringStorageEncoding> Members
		/// <summary>Compare two <see cref="StringStorageEncoding"/> objects for similar underlying values</summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(StringStorageEncoding x, StringStorageEncoding y) => x.CompareTo(y);
		/// <summary>Compare this with another <see cref="StringStorageEncoding"/> object for similar underlying values</summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public int CompareTo(StringStorageEncoding other)
		{
			int cmp = this.mStorage_.CompareTo(other.mStorage_);

			if (cmp == 0)
				return ((int) this.mOptions_) - ((int)other.mOptions_);

			return cmp;
		}
		#endregion


		#region Static encodings
		internal static readonly StringStorageEncoding[] KStorageEncodingList = EncodingArrayFromStorageArray(StringStorage.KStorageTypesList);
		static StringStorageEncoding[] EncodingArrayFromStorageArray(StringStorage[] storageArray)
		{
			var encodings = new StringStorageEncoding[storageArray.Length];
			for (int x = 0; x < encodings.Length; x++)
				encodings[x] = new StringStorageEncoding(storageArray[x]);
			return encodings;
		}

		/// <summary>
		/// Try and get an existing <b>static</b> <see cref="StringStorageEncoding"/> instance
		/// based on a provided definition
		/// </summary>
		/// <param name="storageDesc">Storage to base the result on</param>
		/// <returns>
		/// If an instance is found with <paramref name="storageDesc"/>, a static based
		/// object will be returned. Otherwise, a new <see cref="StringStorageEncoding"/>
		/// object will be created using the definition.
		/// </returns>
		public static StringStorageEncoding TryAndGetStaticEncoding(StringStorage storageDesc)
		{
			Contract.Ensures(Contract.Result<StringStorageEncoding>() != null);

			StringStorageEncoding sse = Array.Find(KStorageEncodingList,
				x => x.mStorage_.Equals(storageDesc));

			return sse ?? new StringStorageEncoding(storageDesc);
		}
		#endregion
	};
}
