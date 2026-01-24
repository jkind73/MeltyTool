using System;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using SHA1CryptoServiceProvider = System.Security.Cryptography.SHA1CryptoServiceProvider;

namespace KSoft.Phoenix.Resource
{
	using PhxHash = Security.Cryptography.PhxHash;

	/*public*/ sealed class EraFileSignature
		: IO.IEndianStreamSerializable
	{
		const uint K_SIGNATURE_ = 0x05ABDBD8;
		const uint K_SIGNATURE_MARKER_ = 0xAAC94350;
		const byte K_DEFAULT_SIZE_BIT_ = 0x13;

		const int K_NON_SIGNATURE_BYTES_SIZE_ = sizeof(uint) + sizeof(byte) + sizeof(uint);

		const uint K_SHA1_SALT_ = 0xA7F95F9C;

		public byte sizeBit = K_DEFAULT_SIZE_BIT_;
		public byte[] signatureData;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			bool reading = s.IsReading;

			int sigDataLength = reading || this.signatureData == null
				? 0
				: this.signatureData.Length;
			int size = reading
				? 0
				: K_NON_SIGNATURE_BYTES_SIZE_ + sigDataLength;

			s.StreamSignature(K_SIGNATURE_);
			s.Stream(ref size);
			if (size < K_NON_SIGNATURE_BYTES_SIZE_)
				throw new System.IO.InvalidDataException(size.ToString("X8"));
			s.Pad64();

			s.StreamSignature(K_SIGNATURE_MARKER_);
			s.Stream(ref this.sizeBit);
			if (reading)
			{
				Array.Resize(ref this.signatureData, size - K_NON_SIGNATURE_BYTES_SIZE_);
				sigDataLength = this.signatureData.Length;
			}
			if (sigDataLength > 0)
			{
				s.Stream(this.signatureData);
			}
			s.StreamSignature(K_SIGNATURE_MARKER_);
		}
		#endregion

		internal static byte[] ComputeSignatureDigest(System.IO.Stream chunksStream
			, long chunksOffset
			, long chunksLength
			, ECF.EcfHeader header)
		{
			Contract.Requires(chunksStream != null);
			Contract.Requires(chunksStream.CanSeek && chunksStream.CanRead);
			Contract.Requires(chunksOffset >= 0);
			Contract.Requires(chunksLength > 0);

			using (var sha = new SHA1CryptoServiceProvider())
			{
				PhxHash.UInt32(sha, K_SHA1_SALT_);
				PhxHash.UInt32(sha, (uint)header.headerSize);
				PhxHash.UInt32(sha, (uint)header.chunkCount);
				PhxHash.UInt32(sha, (uint)header.ExtraDataSize);
				PhxHash.UInt32(sha, (uint)header.totalSize);

				PhxHash.Stream(sha,
					chunksStream, chunksOffset, chunksLength,
					isFinal: true);

				return sha.Hash;
			}
		}
	};
}