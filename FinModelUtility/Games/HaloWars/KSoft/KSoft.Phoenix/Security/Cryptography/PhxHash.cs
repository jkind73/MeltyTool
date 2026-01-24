using System;
using System.IO;
using System.Security.Cryptography;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	using Debug = Phoenix.Debug;

	static class PhxHash
	{
		public const int K_SHA1_SIZE_OF = 20;

		// NOTE: data is written to the buffer in MSB order
		static byte[] gUInt64Buffer_ = new byte[sizeof(ulong)];

		static void BufferFillUnicode(char unicode)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer_, 0, (ushort)unicode);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt16(gUInt64Buffer_, 0);
			}
		}

		public static void UInt8(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			gUInt64Buffer_[0] = (byte)(word >> 0);

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer_, 0, sizeof(byte));
			else
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(byte), null, 0);
		}
		public static void UInt16(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer_, 0, (ushort)word);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt16(gUInt64Buffer_, 0);
			}

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer_, 0, sizeof(ushort));
			else
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(ushort), null, 0);
		}
		public static void UInt32(SHA1CryptoServiceProvider sha, uint word, bool isFinal = false)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer_, 0, word);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt32(gUInt64Buffer_, 0);
			}

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer_, 0, sizeof(uint));
			else
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(uint), null, 0);
		}
		public static void UInt64(SHA1CryptoServiceProvider sha, ulong word, bool isFinal = false)
		{
			Bitwise.ByteSwap.ReplaceBytes(gUInt64Buffer_, 0, word);
			if (BitConverter.IsLittleEndian)
			{
				Bitwise.ByteSwap.SwapUInt64(gUInt64Buffer_, 0);
			}

			if (isFinal)
				sha.TransformFinalBlock(gUInt64Buffer_, 0, sizeof(ulong));
			else
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(ulong), null, 0);
		}

		public static void Ascii(SHA1CryptoServiceProvider sha, string str, int fixedLength = 0)
		{
			for (int x = 0; x < str.Length; x++)
			{
				gUInt64Buffer_[0] = (byte)(str[x] >> 0);
				BufferFillUnicode(str[x]);
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(byte), null, 0);
			}

			gUInt64Buffer_[0] = 0;
			for (int x = 0, nullCount = fixedLength - str.Length; x < nullCount; x++)
			{
				BufferFillUnicode('\0');
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(byte), null, 0);
			}
		}
		public static void Unicode(SHA1CryptoServiceProvider sha, string str, int fixedLength = 0)
		{
			for (int x = 0; x < str.Length; x++)
			{
				BufferFillUnicode(str[x]);
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(ushort), null, 0);
			}

			BufferFillUnicode('\0');
			for (int x = 0, nullCount = fixedLength - str.Length; x < nullCount; x++)
			{
				sha.TransformBlock(gUInt64Buffer_, 0, sizeof(ushort), null, 0);
			}
		}

		public static void Stream(SHA1CryptoServiceProvider sha
			, Stream inputStream
			, long inputOffset
			, long inputLength
			, bool isFinal = false)
		{
			const int kReadBlockSize = 4096;

			Contract.Requires(inputStream != null);
			Contract.Requires(inputStream.CanSeek && inputStream.CanRead);
			Contract.Requires(inputOffset >= 0);
			Contract.Requires(inputLength > 0);

			var scratchBuffer = new byte[kReadBlockSize];

			using (new IO.StreamPositionContext(inputStream))
			{
				inputStream.Seek(inputOffset, SeekOrigin.Begin);

				for (long inputBytesRead = 0; inputBytesRead < inputLength; )
				{
					long bytesRemaining = inputLength - inputBytesRead;
					int readBlockLength = System.Math.Min((int)bytesRemaining, scratchBuffer.Length);

					Array.Clear(scratchBuffer, 0, scratchBuffer.Length);
					for (int actualBytesRead = 0; actualBytesRead < readBlockLength; )
					{
						int subBlockOffset = actualBytesRead;
						int subBlockLength = readBlockLength - subBlockOffset;
						actualBytesRead += inputStream.Read(scratchBuffer, subBlockOffset, subBlockLength);
					}

					sha.TransformBlock(
						scratchBuffer, 0, readBlockLength,
						null, 0);
					inputBytesRead += readBlockLength;
				}
			}

			if (isFinal)
			{
				sha.TransformFinalBlock(scratchBuffer, 0, 0);
			}
		}

		public const int K_RESULT_SIZE = 0x18;

		public static void Sha1Hash(string str, byte[] result)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(str));
			Contract.Requires<ArgumentNullException>(result != null);
			Contract.Requires(result.Length >= K_RESULT_SIZE);

			Array.Clear(result, 0, result.Length);

			byte[] strBytes = System.Text.Encoding.ASCII.GetBytes(str);

			using (var sha = new SHA1CryptoServiceProvider())
			{
				byte[] result1;
				byte[] resultFinal;

				UInt32(sha, 0xA4800C14);
				//PhxHash.Ascii(sha, str);
				sha.TransformBlock(strBytes, 0, strBytes.Length, null, 0);
				UInt32(sha, 0x5AF4A9F1);
				UInt32(sha, 0xCA6884EC, true);
				result1 = sha.Hash;
				if (System.Diagnostics.Debugger.IsAttached)
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Result: {1}", str, Text.Util.ByteArrayToString(result1));

				sha.Initialize();
				UInt32(sha, 0xCB92EAEB);
				sha.TransformBlock(result1, 0, result1.Length, null, 0);
				UInt32(sha, 0x1D919BF8, true);
				resultFinal = sha.Hash;
				if (System.Diagnostics.Debugger.IsAttached)
					Debug.Trace.Security.TraceInformation("Sha1Hash: {0} Final: {1}", str, Text.Util.ByteArrayToString(resultFinal));

				Array.Copy(resultFinal, result, resultFinal.Length);
			}
		}

		public static bool Sha1HashFile(string fileName, byte[] result, out long fileLength)
		{
			Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(fileName));
			Contract.Requires<ArgumentNullException>(result != null);
			Contract.Requires(result.Length >= K_RESULT_SIZE);

			fileLength = -1;

			try
			{
				if (!File.Exists(fileName))
					return false;

				byte[] resultFinal;

				using (var fs = File.OpenRead(fileName))
				using (var sha = new SHA1CryptoServiceProvider())
				{
					resultFinal = sha.ComputeHash(fs, 0, fs.Length);

					fileLength = fs.Length;
				}

				Array.Copy(resultFinal, result, resultFinal.Length);
			}
			catch (Exception ex)
			{
				Debug.Trace.Security.TraceInformation(ex.ToString());
				return false;
			}

			return true;
		}

		public static TigerHashBase CreateHaloWarsTigerHash()
		{
			Contract.Ensures(Contract.Result<TigerHashBase>() != null);

			var tiger = TigerHashBase.Create(TigerHash.K_ALGORITHM_NAME);

			return tiger;
		}
	};
}