namespace LIBMIO0 {
  struct Mio0Header {
    public uint destSize;
    public uint compOffset;
    public uint uncompOffset;
    public bool bigEndian;
  };

  class Mio0 {
    private const int MIO0_HEADER_LENGTH_ = 16;

    private static int GET_BIT(byte[] buf, int offset, int bit) {
      return buf[(bit / 8) + offset] & (1 << (7 - (bit % 8)));
    }

    private static bool
        CompareByteArrays_(byte[] buf1, byte[] buf2, int length) {
      for (int i = 0; i < length; ++i)
        if (buf1[i] != buf2[i])
          return false;
      return true;
    }

    private static uint read_u32_be(byte[] buf, int off) {
      return (uint) (((buf)[off + 0] << 24) + ((buf)[off + 1] << 16) +
                     ((buf)[off + 2] << 8) + ((buf)[off + 3]));
    }

    private static uint read_u32_le(byte[] buf, int off) {
      return (uint) (((buf)[off + 1] << 24) + ((buf)[off + 0] << 16) +
                     ((buf)[off + 3] << 8) + ((buf)[off + 2]));
    }

    private static void write_u32_be(byte[] buf, uint val, int off) {
      buf[off + 0] = (byte) ((val >> 24) & 0xFF);
      buf[off + 1] = (byte) ((val >> 16) & 0xFF);
      buf[off + 2] = (byte) ((val >> 8) & 0xFF);
      buf[off + 3] = (byte) (val & 0xFF);
    }

    ///<summary>
    /// decode MIO0 header<para/>
    /// returns true if valid header, false otherwise
    ///</summary>
    public static bool decode_header(byte[] buf, ref Mio0Header head) {
      byte[] mio0AsciiBe = [0x4D, 0x49, 0x4F, 0x30];
      byte[] mio0AsciiLe = [0x49, 0x4D, 0x30, 0x4F];

      if (CompareByteArrays_(buf, mio0AsciiBe, 4)) {
        head.destSize = read_u32_be(buf, 4);
        head.compOffset = read_u32_be(buf, 8);
        head.uncompOffset = read_u32_be(buf, 12);
        head.bigEndian = true;
        return true;
      } else if (CompareByteArrays_(buf, mio0AsciiLe, 4)) {
        head.destSize = read_u32_le(buf, 4);
        head.compOffset = read_u32_le(buf, 8);
        head.uncompOffset = read_u32_le(buf, 12);
        head.bigEndian = false;
        return true;
      }

      return false;
    }

    ///<summary>
    /// encode MIO0 header from struct
    ///</summary>
    public static void encode_header(byte[] buf, ref Mio0Header head) {
      write_u32_be(buf, 0x4D494F30, 0); // write "MIO0" at start of buffer
      write_u32_be(buf, head.destSize, 4);
      write_u32_be(buf, head.compOffset, 8);
      write_u32_be(buf, head.uncompOffset, 12);
    }

    ///<summary>
    /// decode MIO0 data<para/>
    /// mio0_buf: buffer containing MIO0 data<para/>
    /// returns the raw data as a byte array
    ///</summary>
    public static byte[]? mio0_decode(byte[] mio0Buf) {
      Mio0Header head = new Mio0Header();
      uint bytesWritten = 0;
      int bitIdx = 0;
      int compIdx = 0;
      int uncompIdx = 0;
      bool valid;

      // extract header
      valid = decode_header(mio0Buf, ref head);
      // verify MIO0 header
      if (!valid) {
        Console.WriteLine("Error: MIO0 Header is not valid.");
        return null;
      }

      if (!head.bigEndian) {
        Console.WriteLine("Error: Sorry, only big endian supported right now.");
        return null;
      }

      byte[] decoded = new byte[head.destSize];

      //Console.WriteLine("Decoded Length: 0x"+decoded.Length.ToString("X"));

      // decode data
      while (bytesWritten < head.destSize) {
        if (GET_BIT(mio0Buf, MIO0_HEADER_LENGTH_, bitIdx) > 0) {
          // 1 - pull uncompressed data
          decoded[bytesWritten] = mio0Buf[head.uncompOffset + uncompIdx];
          bytesWritten++;
          uncompIdx++;
        } else {
          // 0 - read compressed data
          byte a = mio0Buf[head.compOffset + compIdx + 0];
          byte b = mio0Buf[head.compOffset + compIdx + 1];
          compIdx += 2;
          int length = ((a & 0xF0) >> 4) + 3;
          int idx = ((a & 0x0F) << 8) + b + 1;
          for (int i = 0; i < length; i++) {
            decoded[bytesWritten] = decoded[bytesWritten - idx];
            bytesWritten++;
          }
        }
        bitIdx++;
      }
      return decoded;
    }
  }
}