
namespace KSoft.Phoenix.Resource
{
	/*public*/ sealed class EraFileHeader
		: IO.IEndianStreamSerializable
	{
		const uint K_SIGANTURE_ = 0x17FDBA9C;
		const int K_HEADER_SIZE_ = 0x1E00;

		public static int CalculateHeaderSize() { return K_HEADER_SIZE_; }

		ECF.EcfHeader mHeader_;
		EraFileSignature mSignature_ = new EraFileSignature();

		public int FileCount { get { return this.mHeader_.chunkCount; } }

		public EraFileHeader()
		{
			this.mHeader_.InitializeChunkInfo(K_SIGANTURE_, EraFileEntryChunk.K_EXTRA_DATA_SIZE);
			this.mHeader_.headerSize = K_HEADER_SIZE_;
		}

		public void UpdateFileCount(int count)
		{
			this.mHeader_.chunkCount = (short)count;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var eraFile = s.Owner as EraFileUtil;

			if (s.IsWriting)
			{
				this.mHeader_.UpdateTotalSize(s.BaseStream);
			}

			long headerPosition = s.BaseStream.CanSeek
				? s.BaseStream.Position
				: -1;

			// write the header, but it won't have the correct CRC if things have changed,
			// or if this is a fresh new archive
			this.mHeader_.Serialize(s);
			this.mSignature_.Serialize(s);

			var leftoversSize = this.mHeader_.headerSize - s.BaseStream.Position;
			s.Pad((int)leftoversSize);

			// verify or update the header checksum
			if (s.IsReading)
			{
				if (headerPosition != -1 &&
					!eraFile.options.Test(EraFileUtilOptions.SKIP_VERIFICATION))
				{
					var actualAdler = this.mHeader_.ComputeAdler32(s.BaseStream, headerPosition);
					if (actualAdler != this.mHeader_.adler32)
					{
						throw new System.IO.InvalidDataException(string.Format(
							"ERA header adler32 {0} does not match actual adler32 {1}",
							this.mHeader_.adler32.ToString("X8"),
							actualAdler.ToString("X8")
							));
					}
				}
			}
			else if (s.IsWriting)
			{
				if (headerPosition != -1)
				{
					this.mHeader_.ComputeAdler32AndWrite(s, headerPosition);
				}
			}
		}
		#endregion

		public static bool VerifyIsEraAndDecrypted(IO.EndianReader s)
		{
			return ECF.EcfHeader.VerifyIsEcf(s);
		}
	};
}