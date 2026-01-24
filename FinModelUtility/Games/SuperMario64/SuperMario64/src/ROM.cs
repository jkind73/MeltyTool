using f3dzex2.io;

using LIBMIO0;

using sm64.memory;
using sm64.src;

namespace sm64 {
  public enum RomRegion {
    JAPAN,
    JAPAN_SHINDOU,
    NORTH_AMERICA,
    EUROPE,
    CHINESE_IQUE
  };

  public enum RomType {
    VANILLA, // 8MB Compressed ROM
    EXTENDED // Uncompressed ROM
  };

  public enum RomEndian {
    BIG, // .z64
    LITTLE, // .n64
    MIXED // .v64
  };

  public sealed class Rom {
    private static Rom? instance_; // Singleton

    public static Rom Instance => instance_ ??= new Rom();

    public string Filepath { get; set; } = "";

    private byte[] writeMask_;

    //private uint[] segStart = new uint[0x20];
    //private bool[] segIsMIO0 = new bool[0x20];
    //private byte[][] segData = new byte[0x20][];
    private Dictionary<byte, SegBank> segData_ = new Dictionary<byte, SegBank>();

    private Dictionary<byte, Dictionary<byte, SegBank>> areaSegData_ = new();

    public uint Seg02UncompressedOffset { get; private set; } = 0;

    public bool Seg02IsFakeMio0 { get; private set; } = false;

    public RomRegion Region { get; private set; } = RomRegion.NORTH_AMERICA;

    public RomEndian Endian { get; private set; } = RomEndian.BIG;

    public RomType Type { get; private set; } = RomType.VANILLA;

    public byte[] Bytes { get; private set; }

    private void CheckRom_() {
      if (this.Bytes[0] == 0x80 && this.Bytes[1] == 0x37)
        this.Endian = RomEndian.BIG;
      else if (this.Bytes[0] == 0x37 && this.Bytes[1] == 0x80)
        this.Endian = RomEndian.MIXED;
      else if (this.Bytes[0] == 0x40 && this.Bytes[1] == 0x12)
        this.Endian = RomEndian.LITTLE;

      if (this.Endian == RomEndian.MIXED)
        this.SwapMixedBig_();
      else if (this.Endian == RomEndian.LITTLE)
        this.SwapLittleBig_();

      if (this.Bytes[0x3E] == 0x45)
        this.Region = RomRegion.NORTH_AMERICA;
      else if (this.Bytes[0x3E] == 0x50)
        this.Region = RomRegion.EUROPE;
      else if (this.Bytes[0x3E] == 0x4A) {
        if (this.Bytes[0x3F] < 3)
          this.Region = RomRegion.JAPAN;
        else
          this.Region = RomRegion.JAPAN_SHINDOU;
      } else if (this.Bytes[0x3E] == 0x00) {
        this.Region = RomRegion.CHINESE_IQUE;
      }

      Globals.MemoryConstants = this.GetMemoryConstantsForRegion_(this.Region);

      this.FindAndSetSegment02();

      if (this.Bytes[Globals.MemoryConstants.Segment15.Offset] == 0x17)
        this.Type = RomType.EXTENDED;
      else
        this.Type = RomType.VANILLA;
    }

    private MemoryConstants GetMemoryConstantsForRegion_(RomRegion region) {
      switch (region) {
        case RomRegion.NORTH_AMERICA: {
          var segment15Start = this.ReadWordUnsigned(0x2A622C);
          var segment15End = this.ReadWordUnsigned(0x2A6230);
          return MemoryConstants.NaConstants with {
              Segment15 = new Segment {
                  Offset = segment15Start,
                  Length = segment15End - segment15Start,
              }
          };
        }
        default:
          throw new NotImplementedException();
      }
    }

    private void SwapMixedBig_() {
      for (int i = 0; i < this.Bytes.Length; i += 2) {
        byte temp = this.Bytes[i];
        this.Bytes[i] = this.Bytes[i + 1];
        this.Bytes[i + 1] = temp;

        temp = this.writeMask_[i];
        this.writeMask_[i] = this.writeMask_[i + 1];
        this.writeMask_[i + 1] = temp;
      }
    }

    private void SwapLittleBig_() {
      byte[] temp = new byte[4];
      for (int i = 0; i < this.Bytes.Length; i += 4) {
        temp[0] = this.Bytes[i + 0];
        temp[1] = this.Bytes[i + 1];
        temp[2] = this.Bytes[i + 2];
        temp[3] = this.Bytes[i + 3];
        this.Bytes[i + 0] = temp[3];
        this.Bytes[i + 1] = temp[2];
        this.Bytes[i + 2] = temp[1];
        this.Bytes[i + 3] = temp[0];

        temp[0] = this.writeMask_[i + 0];
        temp[1] = this.writeMask_[i + 1];
        temp[2] = this.writeMask_[i + 2];
        temp[3] = this.writeMask_[i + 3];
        this.writeMask_[i + 0] = temp[3];
        this.writeMask_[i + 1] = temp[2];
        this.writeMask_[i + 2] = temp[1];
        this.writeMask_[i + 3] = temp[0];
      }
    }

    public void ClearSegments() {
      foreach (KeyValuePair<byte, SegBank> kvp in this.segData_.ToArray()) {
        if ((new[] {0x15, 2}).Contains(kvp.Key))
          continue;
        this.segData_.Remove(kvp.Key);
      }

      this.areaSegData_.Clear();
    }

    public void ReadFile(string filename) {
      this.Filepath = filename;
      this.Bytes = File.ReadAllBytes(filename);
      this.writeMask_ = new byte[this.Bytes.Length];
      this.CheckRom_();
    }

    public void SetSegment(uint index,
                           uint segmentStart,
                           uint segmentEnd,
                           bool isMio0,
                           byte? areaId) {
      this.SetSegment(index, segmentStart, segmentEnd, isMio0, false, 0, areaId);
    }

    public void SetSegment(uint index,
                           uint segmentStart,
                           uint segmentEnd,
                           bool isMio0,
                           bool fakeMio0,
                           uint uncompressedOffset,
                           byte? areaId) {
      if (segmentStart > segmentEnd)
        return;

      SegBank seg = new SegBank();
      seg.SegId = (byte) index;

      if (!isMio0) {
        seg.SegStart = segmentStart;
        seg.IsMio0 = false;
        uint size = segmentEnd - segmentStart;
        seg.Data = new byte[size];
        for (uint i = 0; i < size; i++)
          seg.Data[i] = this.Bytes[segmentStart + i];
      } else {
        if (fakeMio0) {
          seg.SegStart = segmentStart + uncompressedOffset;
          seg.IsMio0 = false;
        } else {
          seg.IsMio0 = true;
        }
        seg.Data =
            Mio0.mio0_decode(this.getSubArray_safe(this.Bytes, segmentStart,
                                                   segmentEnd - segmentStart))!;
      }

      this.SetSegment_(index, seg, areaId);
    }

    private void SetSegment_(uint index, SegBank seg, byte? areaId) {
      if (areaId != null) {
        if (!this.areaSegData_.ContainsKey(areaId.Value)) {
          Dictionary<byte, SegBank> dic = new Dictionary<byte, SegBank>();
          this.areaSegData_.Add(areaId.Value, dic);
        } else if (this.areaSegData_[areaId.Value].ContainsKey((byte) index)) {
          this.areaSegData_[areaId.Value].Remove((byte) index);
        }

        this.areaSegData_[areaId.Value].Add((byte) index, seg);
      } else {
        if (this.segData_.ContainsKey((byte) index)) {
          this.segData_.Remove((byte) index);
        }

        this.segData_.Add((byte) index, seg);
      }
    }

    public byte[]? GetSegment(ushort seg, byte? areaId)
      => this.GetSegBank_(seg, areaId)?.Data;

    private SegBank? GetSegBank_(ushort seg, byte? areaId) {
      if (areaId != null &&
          this.areaSegData_.ContainsKey(areaId.Value) &&
          this.areaSegData_[areaId.Value].ContainsKey((byte) (seg))) {
        return this.areaSegData_[areaId.Value][(byte) seg];
      }
      if (this.segData_.ContainsKey((byte) seg)) {
        return this.segData_[(byte) seg];
      }
      return null;
    }

    public uint DecodeSegmentAddress(uint segOffset, byte? areaId) {
      // Console.WriteLine("Decoding segment address: " + segOffset.ToString("X8"));
      byte seg = (byte) (segOffset >> 24);

      if (this.GetSegBank_(seg, areaId).IsMio0)
        throw new ArgumentException(
            "Cannot decode segment address (0x" + segOffset.ToString("X8") +
            ") from MIO0 data. (decodeSegmentAddress 1)");
      uint off = segOffset & 0x00FFFFFF;
      return this.GetSegBank_(seg, areaId).SegStart + off;
    }

    public uint DecodeSegmentAddress(byte segment, uint offset, byte? areaId) {
      SegBank seg = this.GetSegBank_(segment, areaId);

      if (seg.IsMio0)
        throw new ArgumentException(
            "Cannot decode segment address (0x" + segment.ToString("X2") +
            offset.ToString("X6") +
            ") from MIO0 data. (decodeSegmentAddress 2)");
      return seg.SegStart + offset;
    }

    public uint decodeSegmentAddress_safe(byte segment,
                                          uint offset,
                                          byte? areaId) {
      SegBank seg = this.GetSegBank_(segment, areaId);
      if (seg == null) return 0xFFFFFFFF;

      if (seg.IsMio0)
        return 0xFFFFFFFF;
      return seg.SegStart + offset;
    }

    public byte[] getSubArray_safe(byte[]? arr, uint offset, long size) {
      if (arr == null || arr.Length <= offset)
        return new byte[size];
      if ((arr.Length - offset) < size)
        size = (arr.Length - offset);
      byte[] newArr = new byte[size];
      Array.Copy(arr, offset, newArr, 0, size);
      return newArr;
    }

    public short ReadHalfword(uint offset) {
      return (short) (this.Bytes[offset] << 8 | this.Bytes[offset + 1]);
    }

    public ushort ReadHalfwordUnsigned(uint offset) {
      return (ushort) this.ReadHalfword(offset);
    }

    public int ReadWord(uint offset) {
      return this.Bytes[0 + offset] << 24 | this.Bytes[1 + offset] << 16
                                          | this.Bytes[2 + offset] << 8 |
                                          this.Bytes[3 + offset];
    }

    public uint ReadWordUnsigned(uint offset) {
      return (uint) (this.Bytes[0 + offset] << 24 | this.Bytes[1 + offset] << 16
                                                  | this.Bytes[2 + offset] << 8 |
                                                  this.Bytes[3 + offset]);
    }

    public bool IsSegmentMio0(byte seg, byte? areaId) {
      SegBank segBank = this.GetSegBank_(seg, areaId);
      if (segBank != null)
        return segBank.IsMio0;
      else return false;
    }

    public bool TestIfMio0IsFake(uint startAddr, int compOff, int uncompOff) {
      if (uncompOff - compOff == 2) {
        if (this.ReadHalfwordUnsigned((uint) (startAddr + compOff)) == 0x0000)
          return true; // Detected fake MIO0 header
      }
      return false;
    }

    public void FindAndSetSegment02() {
      AssemblyReader ar = new AssemblyReader();
      List<AssemblyReader.JalCall> funcCalls;
      SegBank seg = new SegBank();
      uint seg02Init;
      uint raMtoRom;
      switch (this.Region) {
        default:
        case RomRegion.NORTH_AMERICA:
          seg02Init = Globals.seg02InitNa;
          raMtoRom = Globals.raMtoRomNa;
          break;
        case RomRegion.EUROPE:
          seg02Init = Globals.seg02InitEu;
          raMtoRom = Globals.raMtoRomEu;
          break;
        case RomRegion.JAPAN:
          seg02Init = Globals.seg02InitJp;
          raMtoRom = Globals.raMtoRomJp;
          break;
        case RomRegion.JAPAN_SHINDOU:
          seg02Init = Globals.seg02InitJs;
          raMtoRom = Globals.raMtoRomJs;
          break;
        case RomRegion.CHINESE_IQUE:
          seg02Init = Globals.seg02InitIq;
          raMtoRom = Globals.raMtoRomIq;
          break;
      }

      funcCalls = ar.FindJaLsInFunction(seg02Init, raMtoRom);
      for (int i = 0; i < funcCalls.Count; i++) {
        if (funcCalls[i].a0 == 0x2) {
          Globals.MemoryConstants = Globals.MemoryConstants with {
              Segment2 = new Segment {
                  Offset = funcCalls[i].a1,
                  Length = funcCalls[i].a2 - funcCalls[i].a1,
              }
          };
          if (this.ReadWordUnsigned(funcCalls[i].a1) == 0x4D494F30) {
            seg.IsMio0 = true;
            this.Seg02IsFakeMio0 = this.TestIfMio0IsFake(
                funcCalls[i].a1,
                this.ReadWord(funcCalls[i].a1 + 0x8),
                this.ReadWord(funcCalls[i].a1 + 0xC)
            );
            seg.SegStart = funcCalls[i].a1;
            this.Seg02UncompressedOffset = this.ReadWordUnsigned(funcCalls[i].a1 + 0xC);
          }
        }
      }

      this.SetSegment_(0x2, seg, null);
    }

    public Dictionary<string, ushort> levelIDs = new Dictionary<string, ushort> {
        {"[C01] Bob-omb Battlefield", 0x09},
        {"[C02] Whomp's Fortress", 0x18},
        {"[C03] Jolly Roger Bay", 0x0C},
        {"[C04] Cool Cool Mountain", 0x05},
        {"[C05] Big Boo's Haunt", 0x04},
        {"[C06] Hazy Maze Cave", 0x07},
        {"[C07] Lethal Lava Land", 0x16},
        {"[C08] Shifting Sand Land", 0x08},
        {"[C09] Dire Dire Docks", 0x17},
        {"[C10] Snowman's Land", 0x0A},
        {"[C11] Wet Dry World", 0x0B},
        {"[C12] Tall Tall Mountain", 0x24},
        {"[C13] Tiny Huge Island", 0x0D},
        {"[C14] Tick Tock Clock", 0x0E},
        {"[C15] Rainbow Ride", 0x0F},
        {"[OW1] Castle Grounds", 0x10},
        {"[OW2] Inside Castle", 0x06},
        {"[OW3] Castle Courtyard", 0x1A},
        {"[BC1] Bowser Course 1", 0x11},
        {"[BC2] Bowser Course 2", 0x13},
        {"[BC3] Bowser Course 3", 0x15},
        {"[MCL] Metal Cap", 0x1C},
        {"[WCL] Wing Cap", 0x1D},
        {"[VCL] Vanish Cap", 0x12},
        {"[BB1] Bowser Battle 1", 0x1E},
        {"[BB2] Bowser Battle 2", 0x21},
        {"[BB3] Bowser Battle 3", 0x22},
        {"[SC1] Secret Aquarium", 0x14},
        {"[SC2] Rainbow Clouds", 0x1F},
        {"[SC3] End Cake Picture", 0x19},
        {"[SlC] Peach's Secret Slide", 0x1B}
    };
  }

  class SegBank {
    public byte[] Data { get; set; } = null;
    public bool IsMio0 { get; set; } = false;
    public uint SegStart { get; set; } = 0;
    public byte SegId { get; set; } = 0;
  }
}