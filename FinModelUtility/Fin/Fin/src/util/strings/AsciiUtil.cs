using System.Text;

namespace fin.util.strings;

public static class AsciiUtil {
  private static readonly byte[] BYTE_WRAPPER_ = new byte[1];
  private static readonly char[] CHAR_WRAPPER_ = new char[1];

  public static char GetChar(byte b) {
    BYTE_WRAPPER_[0] = b;
    Encoding.ASCII.GetChars(BYTE_WRAPPER_,
                            0,
                            1,
                            CHAR_WRAPPER_,
                            0);
    return CHAR_WRAPPER_[0];
  }
}