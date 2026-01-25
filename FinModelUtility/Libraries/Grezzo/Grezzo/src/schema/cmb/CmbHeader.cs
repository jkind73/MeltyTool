using fin.schema;
using fin.util.asserts;
using fin.util.strings;

using schema.binary;

namespace grezzo.schema.cmb;

public sealed class CmbHeader : IBinaryDeserializable {
  // TODO: Better way to do this?
  public static Version Version { get; set; }

  public uint FileSize { get; private set; }
  public Version Version { get; private set; }
  public string Name { get; private set; }
  public uint FaceIndicesCount { get; private set; }
  public uint SklOffset { get; private set; }
  public uint QtrsOffset { get; private set; }
  public uint MatsOffset { get; private set; }
  public uint TexOffset { get; private set; }
  public uint SklmOffset { get; private set; }
  public uint LutsOffset { get; private set; }
  public uint VatrOffset { get; private set; }
  public uint FaceIndicesOffset { get; private set; }
  public uint TextureDataOffset { get; private set; }

  [Unknown]
  public uint Unk0 { get; private set; }

  public void Read(IBinaryReader br) {
      br.AssertString("cmb" + AsciiUtil.GetChar(0x20));

      this.FileSize = br.ReadUInt32();

      this.version = Version = (Version) br.ReadUInt32();


      Asserts.Equal(0, br.ReadInt32());
      this.Name = br.ReadString(16);
      this.FaceIndicesCount = br.ReadUInt32();
      this.SklOffset = br.ReadUInt32();

      if (this.version.SupportsQtrs()) {
        this.QtrsOffset = br.ReadUInt32();
      }

      this.MatsOffset = br.ReadUInt32();
      this.TexOffset = br.ReadUInt32();
      this.SklmOffset = br.ReadUInt32();
      this.LutsOffset = br.ReadUInt32();
      this.VatrOffset = br.ReadUInt32();
      this.FaceIndicesOffset = br.ReadUInt32();
      this.TextureDataOffset = br.ReadUInt32();

      if (this.version > cmb.Version.OCARINA_OF_TIME_3D) {
        this.Unk0 = br.ReadUInt32();
      }
    }
}