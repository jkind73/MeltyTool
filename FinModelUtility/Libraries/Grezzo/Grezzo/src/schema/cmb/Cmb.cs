using System;
using System.Linq;

using fin.image;
using fin.schema.data;
using fin.util.strings;

using grezzo.schema.cmb.luts;
using grezzo.schema.cmb.mats;
using grezzo.schema.cmb.qtrs;
using grezzo.schema.cmb.skl;
using grezzo.schema.cmb.sklm;
using grezzo.schema.cmb.tex;
using grezzo.schema.cmb.vatr;

using schema.binary;
using schema.binary.attributes;

namespace grezzo.schema.cmb;

[Endianness(Endianness.LittleEndian)]
public sealed class Cmb : IBinaryDeserializable {
  public readonly CmbHeader header = new();

  private const int TWEAK_AUTO_SIZE_ = -8;

  public readonly AutoStringMagicUInt32SizedSection<Skl> skl
      = new("skl" + AsciiUtil.GetChar(0x20));

  public readonly AutoStringMagicUInt32SizedSection<Qtrs> qtrs
      = new("qtrs");

  /// <summary>
  ///   For some reason, the size for this section is wrong? We have to just ignore it.
  /// </summary>
  public AutoStringMagicJankSizedSection<Mats> Mats { get; set; } =
    new("mats") {
        TweakReadSize = TWEAK_AUTO_SIZE_,
    };

  public readonly AutoStringMagicUInt32SizedSection<Tex> tex
      = new("tex" + AsciiUtil.GetChar(0x20));

  public IImage[]? TextureImages { get; set; }

  public readonly AutoStringMagicUInt32SizedSection<Sklm> sklm =
      new("sklm");

  public readonly AutoStringMagicUInt32SizedSection<Luts> luts
      = new("luts");

  public readonly Vatr vatr = new();

  public void Read(IBinaryReader br) {
      br.PushLocalSpace();
      this.header.Read(br);

      br.Position = this.header.SklOffset;
      this.skl.Read(br);

      if (CmbHeader.Version > Version.OCARINA_OF_TIME_3D) {
        br.Position = this.header.QtrsOffset;
        this.qtrs.Read(br);
      }

      br.Position = this.header.MatsOffset;
      this.Mats.Read(br);

      br.Position = this.header.TexOffset;
      this.tex.Read(br);

      // TODO: Read this more accurately
      this.TextureImages
          = this.header.TextureDataOffset == 0
              ? null
              : this.tex.Data.Textures
                    .Select(t => br.SubreadAt(
                                this.header.TextureDataOffset + t.DataOffset,
                                () => t.GetImageReader().ReadImage(br)))
                    .ToArray();

      br.Position = this.header.SklmOffset;
      this.sklm.Read(br);

      br.Position = this.header.LutsOffset;
      this.luts.Read(br);

      br.Position = this.header.VatrOffset;
      this.vatr.Read(br);

      // Add face indices to primitive sets
      var sklm = this.sklm.Data;
      foreach (var shape in sklm.shapes.Shapes) {
        foreach (var pset in shape.PrimitiveSets) {
          var primitive = pset.primitive;
          // # Always * 2 even if ubyte is used...
          br.Position = this.header.FaceIndicesOffset +
                        2 * primitive.Offset;

          primitive.indices = new uint[primitive.indicesCount];
          for (var i = 0; i < primitive.indicesCount; ++i) {
            switch (primitive.dataType) {
              case DataType.U_BYTE: {
                primitive.indices[i] = br.ReadByte();
                break;
              }
              case DataType.U_SHORT: {
                primitive.indices[i] = br.ReadUInt16();
                break;
              }
              default: {
                throw new NotImplementedException();
              }
            }
          }
        }
      }

      br.PopLocalSpace();
    }
}


/*pset.primitive.indices =
[int(readDataType(f, pset.primitive.dataType)) for _ in
range(pset.primitive.indicesCount)]*/