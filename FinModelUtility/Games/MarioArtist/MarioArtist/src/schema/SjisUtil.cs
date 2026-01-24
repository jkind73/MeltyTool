using System.Text;

using fin.util.strings;

using schema.binary;

namespace marioartist.schema;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/LuigiBlood/mfs_manager/blob/master/mfs_library/Util/SJISUtil.cs
/// </summary>
public static class SjisUtil {
  static Dictionary<ushort, char> leoMapping_ = new Dictionary<ushort, char>();
  static bool isMappingReady_ = false;

  static void PrepareMapping_() {
    if (isMappingReady_)
      return;

    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    leoMapping_ = new Dictionary<ushort, char>();
    leoMapping_.Add(0x86A3, '\u2660'); //Spade
    leoMapping_.Add(0x86A4, '\u2663'); //Club
    leoMapping_.Add(0x86A5, '\u2665'); //Heart
    leoMapping_.Add(0x86A6, '\u2666'); //Diamond

    isMappingReady_ = true;
  }

  public static string ReadString(IBinaryReader br, int maxLength) {
    PrepareMapping_();

    Span<byte> bytes = stackalloc byte[maxLength];
    br.ReadBytes(bytes);

    return Encoding
           .GetEncoding(932,
                        new EncoderReplacementFallback(),
                        new CustomDecoder())
           .GetString(bytes)
           .SubstringUpTo('\x00');
  }

  //Decoder
  public sealed class CustomDecoder : DecoderFallback {
    public string defaultString;
    internal Dictionary<ushort, char> mapping_;

    public CustomDecoder() : this("*") { }

    public CustomDecoder(string defaultString) {
      this.defaultString = defaultString;

      // Create table of mappings
      this.mapping_ = leoMapping_;
    }

    public override DecoderFallbackBuffer CreateFallbackBuffer() {
      return new CustomDecoderFallbackBuffer(this);
    }

    public override int MaxCharCount {
      get { return 2; }
    }
  }

  public sealed class CustomDecoderFallbackBuffer : DecoderFallbackBuffer {
    int count_ = -1; // Number of characters to return
    int index_ = -1; // Index of character to return
    CustomDecoder fb_;
    string charsToReturn_;

    public CustomDecoderFallbackBuffer(CustomDecoder fallback) {
      this.fb_ = fallback;
    }

    public override bool Fallback(byte[] bytesUnknown, int index) {
      // Return false if there are already characters to map.
      if (this.count_ >= 1) return false;

      // Determine number of characters to return.
      this.charsToReturn_ = String.Empty;

      if (bytesUnknown.Length == 2) {
        ushort key = (ushort) ((bytesUnknown[0] << 8) + bytesUnknown[1]);
        if (this.fb_.mapping_.TryGetValue(key, out char value)) {
          this.charsToReturn_ = Convert.ToString(value);
          this.count_ = 1;
        } else {
          // Return default.
          this.charsToReturn_ = this.fb_.defaultString;
          this.count_ = 1;
        }

        this.index_ = this.charsToReturn_.Length - 1;
      } else {
        //Only full width
        this.charsToReturn_ = this.fb_.defaultString;
        this.count_ = 1;
      }

      return true;
    }

    public override char GetNextChar() {
      // We'll return a character if possible, so subtract from the count of chars to return.
      this.count_--;
      // If count is less than zero, we've returned all characters.
      if (this.count_ < 0)
        return '\u0000';

      this.index_--;
      return this.charsToReturn_[this.index_ + 1];
    }

    public override bool MovePrevious() {
      // Original: if count >= -1 and pos >= 0
      if (this.count_ >= -1) {
        this.count_++;
        return true;
      } else {
        return false;
      }
    }

    public override int Remaining {
      get { return this.count_ < 0 ? 0 : this.count_; }
    }

    public override void Reset() {
      this.count_ = -1;
      this.index_ = -1;
    }
  }
}