using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix
{
	static partial class PhxUtil
	{
		public static Security.Cryptography.Crc16.Definition kCrc16Definition =
			new Security.Cryptography.Crc16.Definition(
				initialValue: ushort.MinValue,
				xorIn: ushort.MaxValue,
				xorOut: ushort.MaxValue);
		public static Security.Cryptography.Crc32.Definition kCrc32Definition =
			new Security.Cryptography.Crc32.Definition(
				initialValue: uint.MinValue,
				xorIn: uint.MaxValue,
				xorOut: uint.MaxValue);

		public const int K_OBJECT_KIND_NONE = 0;

		public const float K_INVALID_SINGLE = (float)TypeExtensions.K_NONE;
		public const float K_INVALID_SINGLE_NA_N = float.NaN;

		/// <summary>Sentinel for cases which reference undefined data (eg, an undefined ProtoObject)</summary>
		public const int K_INVALID_REFERENCE = TypeExtensions.K_NONE - 1;

		private static Func<int> gGetInvalidInt32_;
		public static Func<int> KGetInvalidInt32 { get {
			if (gGetInvalidInt32_ == null)
				gGetInvalidInt32_ = () => TypeExtensions.K_NONE;

			return gGetInvalidInt32_;
		} }

		private static Func<float> gGetInvalidSingle_;
		public static Func<float> KGetInvalidSingle { get {
			if (gGetInvalidSingle_ == null)
				gGetInvalidSingle_ = () => K_INVALID_SINGLE;

			return gGetInvalidSingle_;
		} }

		public static bool StrEqualsIgnoreCase(string str1, string str2)
		{
			return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
		}

		public static string ToLowerIfContainsUppercase(this string str)
		{
			if (str == null)
				return str;

			for (int x = 0; x < str.Length; x++)
			{
				char c = str[x];
				if (c >= 'A' && c <= 'Z')
					return str.ToLowerInvariant();
			}

			return str;
		}

		public static bool StreamPointerizedCString(IO.EndianStream s, ref Values.PtrHandle pointer, ref string value)
		{
			Contract.Requires(s != null);

			bool streamed = false;

			if (s.IsReading)
			{
				if (!pointer.IsInvalidHandle)
				{
					s.Seek((long)pointer.u64, System.IO.SeekOrigin.Begin);
					value = s.Reader.ReadString(Memory.Strings.StringStorage.CStringAscii);
					streamed = true;
				}
			}
			else if (s.IsWriting)
			{
				if (string.IsNullOrEmpty(value))
				{
					pointer = pointer.Is64bit
						? Values.PtrHandle.InvalidHandle64
						: Values.PtrHandle.InvalidHandle32;
				}
				else
				{
					pointer = new Values.PtrHandle(pointer, (ulong)s.BaseStream.Position);
					s.Writer.Write(value, Memory.Strings.StringStorage.CStringAscii);
					streamed = true;
				}
			}

			return streamed;
		}

		public static int CalculateHashCodeForDbiDs(IList<int> dbidList)
		{
			if (dbidList == null || dbidList.Count == 0)
				return 0;

			int hashCode = 0;
			for (int x = 0; x < dbidList.Count; x++)
			{
				int dbid = dbidList[x];
				hashCode ^= dbid.GetHashCode();
			}

			return hashCode;
		}

		[ThreadStatic]
		private static List<string> gParseBVectorStringScratchList_;
		public static BVector? ParseBVectorString(string vectorString)
		{
			var vector = new BVector();
			if (vectorString.IsNullOrEmpty())
				return vector;

			if (gParseBVectorStringScratchList_ == null)
				gParseBVectorStringScratchList_ = new List<string>(4);
			var list = gParseBVectorStringScratchList_;

			if (!Util.ParseStringList(vectorString, list))
				return null;

			if (list.Count >= 1 && !Numbers.FloatTryParseInvariant(list[0], out vector.X))
				return null;
			if (list.Count >= 2 && !Numbers.FloatTryParseInvariant(list[1], out vector.Y))
				return null;
			if (list.Count >= 3 && !Numbers.FloatTryParseInvariant(list[2], out vector.Z))
				return null;
			if (list.Count >= 4 && !Numbers.FloatTryParseInvariant(list[3], out vector.W))
				return null;

			return vector;
		}

		public static string ToBVectorString(this BVector vector, int length = 3)
		{
			if (PhxPredicates.IsZero(vector))
			{
				switch (length)
				{
					case 1:
						return "0";
					case 2:
						return "0,0";
					case 3:
						return "0,0,0";
					case 4:
						return "0,0,0,0";

					default:
						return "";
				}
			}

			var sb = new System.Text.StringBuilder(32);
			if (length >= 1)
				sb.Append(vector.X.ToStringInvariant(Numbers.K_FLOAT_ROUND_TRIP_FORMAT_SPECIFIER));
			if (length >= 2)
				sb.AppendFormat(",{0}", vector.Y.ToStringInvariant(Numbers.K_FLOAT_ROUND_TRIP_FORMAT_SPECIFIER));
			if (length >= 3)
				sb.AppendFormat(",{0}", vector.Z.ToStringInvariant(Numbers.K_FLOAT_ROUND_TRIP_FORMAT_SPECIFIER));
			if (length >= 4)
				sb.AppendFormat(",{0}", vector.W.ToStringInvariant(Numbers.K_FLOAT_ROUND_TRIP_FORMAT_SPECIFIER));

			return sb.ToString();
		}

		public static int CompareTo(this BVector vector, BVector other)
		{
			if (vector.X != other.X)
				return vector.X.CompareTo(other.X);

			if (vector.Y != other.Y)
				return vector.Y.CompareTo(other.Y);

			if (vector.Z != other.Z)
				return vector.Z.CompareTo(other.Z);

			return vector.W.CompareTo(other.W);
		}

		/// <summary>Get the index of the next token</summary>
		/// <param name="src">string to process</param>
		/// <param name="tokens">characters to use as tokens</param>
		/// <param name="srcIndex">Where to start processing for tokens</param>
		/// <param name="distance">Number of characters traversed during this call</param>
		/// <returns></returns>
		public static int NextToken(string src, string tokens, int srcIndex, ref int distance)
		{
			Contract.Requires(!tokens.IsNullOrEmpty());

			if (src.IsNullOrEmpty())
				return -1;
			if (srcIndex >= src.Length)
				return -1;

			if (srcIndex < 0)
				srcIndex = 0;

			// skip any initial tokens
			int count = 0;
			for (; count < src.Length; count++, srcIndex++)
			{
				char c = src[srcIndex];
				if (tokens.IndexOf(c) < 0)
					break;
			}

			// figure out the distance until the next token
			int copyLength = 0;
			for (; count < src.Length && srcIndex+copyLength < src.Length; count++, copyLength++)
			{
				char c = src[srcIndex+copyLength];
				if (tokens.IndexOf(c) >= 0)
					break;
			}

			if (copyLength == 0)
				return -1;

			distance = copyLength;
			return srcIndex + copyLength;
		}

		const string K_TOKENIZE_TOKENS_ = " ,\t\r\n";

		public static bool TokenizeIntegerColor(string src, byte defaultAlpha, ref System.Drawing.Color color)
		{
			int nextIndex = -1;
			int vLength = -1;

			int v1 = 0;
			nextIndex = NextToken(src, K_TOKENIZE_TOKENS_, nextIndex, ref vLength);
			if (nextIndex < 0)
				return false;
			if (!Numbers.TryParseRange(src, out v1, startIndex: nextIndex - vLength, length: vLength))
				return false;

			int v2 = 0;
			nextIndex = NextToken(src, K_TOKENIZE_TOKENS_, nextIndex, ref vLength);
			if (nextIndex < 0)
				return false;
			if (!Numbers.TryParseRange(src, out v2, startIndex: nextIndex - vLength, length: vLength))
				return false;

			int v3 = 0;
			nextIndex = NextToken(src, K_TOKENIZE_TOKENS_, nextIndex, ref vLength);
			if (nextIndex < 0)
				return false;
			if (!Numbers.TryParseRange(src, out v3, startIndex: nextIndex - vLength, length: vLength))
				return false;

			int v4 = 0;
			nextIndex = NextToken(src, K_TOKENIZE_TOKENS_, nextIndex, ref vLength);
			// if v4 is present, ARGB, else RGB
			if (nextIndex >= 0)
			{
				if (!Numbers.TryParseRange(src, out v4, startIndex: nextIndex - vLength, length: vLength))
					return false;

				color = System.Drawing.Color.FromArgb(v1, v2, v3, v4);
			}
			else
			{
				color = System.Drawing.Color.FromArgb(defaultAlpha, v1, v2, v3);
			}

			return true;
		}

		public static string ToIntegerColorString(this System.Drawing.Color color, byte defaultAlpha = 0xFF)
		{
			if (PhxPredicates.IsZero(color) || (color.A==defaultAlpha && PhxPredicates.IsRgbZero(color)))
			{
				return "0 0 0";
			}

			var sb = new System.Text.StringBuilder(16);
			if (color.A != defaultAlpha)
			{
				sb.Append(color.A);
				sb.Append(' ');
			}

			sb.Append(' ');
			sb.Append(color.R);

			sb.Append(' ');
			sb.Append(color.G);

			sb.Append(' ');
			sb.Append(color.B);

			return sb.ToString();
		}

		#region Dummy comparer
		private sealed class DummyComparerAlwaysNonZero<T>
			: IComparer<T>
		{
			public int Compare(T x, T y)
			{
				return -1;
			}
		};

		public static IComparer<T> CreateDummyComparerAlwaysNonZero<T>()
		{
			return new DummyComparerAlwaysNonZero<T>();
		}
		#endregion

		public static bool UpdateResultWithTaskResults(ref bool r, List<Task<bool>> tasks, List<Exception> exceptions = null)
		{
			foreach (var task in tasks)
			{
				try
				{
					task.Wait();
				} catch (Exception ex)
				{
					ex.UnusedExceptionVar();
				}

				if (task.IsFaulted)
				{
					r = false;
					if (exceptions != null)
						exceptions.Add(task.Exception.GetOnlyExceptionOrAll());
				}
				else
				{
					r &= task.Result;
				}
			}

			return r;
		}

		public static bool FindBytePattern(List<int> results, byte[] input, ref int inOutOffset, params short[] pattern)
		{
			int end = input.Length - pattern.Length;
			int endOffset = inOutOffset;

			bool foundPattern = false;
			for (int start = inOutOffset; start < end && !foundPattern; start++, endOffset++)
			{
				var firstByte = pattern[0];
				if (firstByte != input[start])
					continue;

				for (int offset = 1; offset < pattern.Length; offset++)
				{
					int index = start + offset;
					var nextByte = pattern[offset];
					if (nextByte < 0)
						continue;
					else if (nextByte != input[index])
						break;
					else if (offset == pattern.Length-1)
					{
						results.Add(start);
						endOffset += pattern.Length;
						foundPattern = true;
						break;
					}
				}
			}

			inOutOffset = endOffset;
			return foundPattern;
		}

		[ThreadStatic]
		private static byte[] gSharedBufferForSuperFastHash_;
		public static byte[] GetBufferForSuperFastHash(int bufferSize)
		{
			if (gSharedBufferForSuperFastHash_ == null)
				gSharedBufferForSuperFastHash_ = new byte[16];
			else
				gSharedBufferForSuperFastHash_.FastClear();

			var buffer = gSharedBufferForSuperFastHash_;
			if (bufferSize > buffer.Length)
				buffer = new byte[bufferSize];

			return buffer;
		}
		public static uint SuperFastHash(byte[] buffer, uint initialValue = 0)
		{
			Contract.Requires(buffer != null);

			return SuperFastHash(buffer, 0, buffer.Length, initialValue);
		}
		public static uint SuperFastHash(byte[] buffer, int startIndex, int length, uint initialValue = 0)
		{
			Contract.Requires(buffer != null);
			Contract.Requires(startIndex >= 0 && length >= 0);
			Contract.Requires(startIndex+length <= buffer.Length);

			// Based on code by Paul Hsieh
			// http://www.azillionmonkeys.com/qed/hash.html

			uint hash = initialValue;

			int lengthRem = length & (sizeof(uint)-1);
			int words = length / sizeof(uint);
			int index = startIndex;

			// Main loop
			for (; words > 0; words--)
			{
				hash += (uint)(BitConverter.ToUInt16(buffer, index));
				index += sizeof(ushort);
				hash ^= hash << 16;
				hash ^= (uint)(BitConverter.ToUInt16(buffer, index) << 11);
				index += sizeof(ushort);
				hash += hash >> 11;
			}

			// Handle end cases
			switch (lengthRem)
			{
				case sizeof(ushort)+1:
					hash += (uint)(BitConverter.ToUInt16(buffer, index));
					index += sizeof(ushort);
					hash ^= (uint)(buffer[index] << 18);
					hash += hash >> 11;
					break;
				case sizeof(ushort):
					hash += (uint)(BitConverter.ToUInt16(buffer, index));
					index += sizeof(ushort);
					hash ^= hash << 11;
					hash += hash >> 17;
					break;
				case sizeof(byte):
					hash += buffer[index++];
					hash ^= hash << 10;
					hash += hash >> 1;
					break;
			}

			// Force "avalanching" of final 127 bits
			hash ^= hash << 3;
			hash += hash >> 5;
			hash ^= hash << 2;
			hash += hash >> 15;
			hash ^= hash << 10;

			return hash;
		}
	};
}