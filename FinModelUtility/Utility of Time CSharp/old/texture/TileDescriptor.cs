namespace UoT {
  /// <summary>
  ///   A given "tile descriptor" defined by F3DZEX2 display lists. This can be
  ///   thought of as the params for a texture. The display lists support
  ///   indexing 3 bits worth, so it seems that there can be at most 8 tile
  ///   descriptors active at once.
  ///
  ///   This *must* be a struct for now, as the parser expects for data to be
  ///   copied by value instead of by reference.
  /// </summary>
  public struct TileDescriptor {
    public bool enabled;
    // TODO: Get rid of this field.
    public int id;
    public int dxt;
    public int height;
    public int width;
    public int loadWidth;
    public int loadHeight;
    public double textureHRatio;
    public double textureWRatio;
    public uint texBytes;
    public int address;
    public int imageBank;
    public ushort tmemOffset;
    public byte palette;
    public int paletteBank;
    public int offset;
    public int paletteOffset;
    // TODO: Delete this.
    public int jankFormat;
    public ColorFormat colorFormat;
    public BitSize bitSize;
    public int cms;
    public int cmt;
    public double sScale;
    public double scale;
    public double shiftS;
    public double shiftT;
    public double tShiftS;
    public double tShiftT;
    public int originalMaskS;
    public int originalMaskT;
    public int maskS;
    public int maskT;
    public int lineSize;
    public int uls;
    public int ult;
    public int lrs;
    public int lrt;
    public uint oglTexObj;
    public Color4UByte[] palette32;

    public long Uuid {
      get {
        // These values impact how texture-clamping parameters are set in
        // OpenGl; they require unique texture instances.

        // The lower-right coords are each 3 bytes (12 bits) long.
        long lrs = this.lrs;
        long lrt = this.lrt;

        // The clamp flag values are 2 bits long.
        long cms = this.cms;
        long cmt = this.cmt;

        long uuid = this.address;
        uuid |= lrs << (32 + 4 + 12);
        uuid |= lrt << (32 + 4);
        uuid |= cms << (32 + 2);
        uuid |= cmt << 32;

        return uuid;
      }
    }
  }
}
