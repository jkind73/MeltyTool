using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using gx;

using jsystem.G3D_Binary_File_Format;
using jsystem.schema.j3dgraph.bmd;
using jsystem.schema.j3dgraph.bmd.mat3;

using schema.binary;

#pragma warning disable CS8604


namespace jsystem.GCN;

public partial class Bmd {
  public partial class Mat3Section {
    public const string SIGNATURE = "MAT3";
    public DataBlockHeader header;
    public ushort nrMaterials;
    public uint[] offsets;
    public MaterialEntry[] materialEntries;
    public BmdPopulatedMaterial[] PopulatedMaterials { get; set; }
    public ushort[] materialEntryIndieces;
    public short[] textureIndices;
    public GxCullMode[] cullModes;
    public Color[] materialColor;
    public Color[] lightColors;
    public Color[] ambientColors;
    public Color[] tevColors;
    public Color[] tevKonstColors;
    public AlphaCompare[] alphaCompares;
    public BlendFunction[] blendFunctions;
    public DepthFunction[] depthFunctions;
    public TevStageProps[] tevStages;
    public TevSwapMode[] tevSwapModes;
    public TevSwapModeTable[] tevSwapModeTables;
    public TexCoordGen[] texCoordGens;
    public ColorChannelControl[] colorChannelControls;
    public TextureMatrixInfo[] textureMatrices;
    public TevOrder[] tevOrders;
    public StringTable materialNameTable;

    public readonly List<IndirectTexture> indirectTextures = [];

    public Mat3Section(IBinaryReader br, out bool ok) {
      long position1 = br.Position;
      bool ok1;
      this.header = new DataBlockHeader(br, "MAT3", out ok1);
      if (!ok1) {
        ok = false;
      } else {
        this.nrMaterials = br.ReadUInt16();

        br.AssertUInt16(0xffff); // padding

        this.offsets = br.ReadUInt32s(30);
        int[] sectionLengths = this.GetSectionLengths();
        long position2 = br.Position;

        // TODO: There is a bunch more data that isn't even read yet:
        // https://github.com/RenolY2/SuperBMD/blob/ccc86e21493275bcd9d86f65b516b85d95c83abd/SuperBMDLib/source/Materials/Enums/Mat3OffsetIndex.cs

        br.Position = position1 + (long) this.offsets[0];
        this.materialEntries = new MaterialEntry[sectionLengths[0] / 332];
        for (int index = 0; index < sectionLengths[0] / 332; ++index)
          this.materialEntries[index] = br.ReadNew<MaterialEntry>();

        br.Position = position1 + (long) this.offsets[1];
        this.materialEntryIndieces = br.ReadUInt16s((int) this.nrMaterials);

        br.Position = position1 + (long) this.offsets[2];
        this.materialNameTable = br.ReadNew<StringTable>();

        var indirectTexturesOffset =
            br.Position = position1 + this.offsets[3];
        this.indirectTextures.Clear();
        while ((br.Position - indirectTexturesOffset) < sectionLengths[3]) {
          this.indirectTextures.Add(
              br.ReadNew<IndirectTexture>());
        }

        br.Position = position1 + (long) this.offsets[4];
        this.cullModes = new GxCullMode[sectionLengths[4] / 4];
        for (var index = 0; index < sectionLengths[4] / 4; ++index)
          this.cullModes[index] = (GxCullMode) br.ReadInt32();

        br.Position = position1 + (long) this.offsets[5];
        this.materialColor = new Color[sectionLengths[5] / 4];
        for (int index = 0; index < sectionLengths[5] / 4; ++index)
          this.materialColor[index] = br.ReadColor8();

        br.Position = position1 + (long) this.offsets[7];
        this.colorChannelControls =
            new ColorChannelControl[sectionLengths[7] / 8];
        for (var i = 0; i < this.colorChannelControls.Length; ++i) {
          this.colorChannelControls[i] = br.ReadNew<ColorChannelControl>();
        }

        br.Position = position1 + (long) this.offsets[8];
        this.ambientColors = new Color[sectionLengths[8] / 4];
        for (int index = 0; index < sectionLengths[8] / 4; ++index)
          this.ambientColors[index] = br.ReadColor8();

        br.Position = position1 + this.offsets[9];
        this.lightColors = new Color[sectionLengths[9] / 8];
        for (int index = 0; index < this.lightColors.Length; ++index) {
          this.lightColors[index] = br.ReadColor16();
        }

        // TODO: Add support for texgen counts (10)

        br.Position = position1 + this.offsets[11];
        this.texCoordGens = br.ReadNews<TexCoordGen>(sectionLengths[11] / 4);

        // TODO: Add support for post tex coord gens (12)

        br.Position = position1 + (long) this.offsets[13];
        this.textureMatrices =
            br.ReadNews<TextureMatrixInfo>(sectionLengths[13] / 100);

        // TODO: Add support for post tex matrices (14)

        br.Position = position1 + (long) this.offsets[15];
        this.textureIndices = br.ReadInt16s(sectionLengths[15] / 2);

        br.Position = position1 + (long) this.offsets[16];
        this.tevOrders = br.ReadNews<TevOrder>(sectionLengths[16] / 4);

        br.Position = position1 + (long) this.offsets[17];
        this.tevColors = new Color[sectionLengths[17] / 8];
        for (int index = 0; index < this.tevColors.Length; ++index)
          this.tevColors[index] = br.ReadColor16();

        br.Position = position1 + (long) this.offsets[18];
        this.tevKonstColors = new Color[sectionLengths[18] / 4];
        for (int index = 0; index < this.tevKonstColors.Length; ++index)
          this.tevKonstColors[index] = br.ReadColor8();

        // TODO: Add support for tev counts (19)

        br.Position = position1 + (long) this.offsets[20];
        this.tevStages = br.ReadNews<TevStageProps>(sectionLengths[20] / 20);

        br.Position = position1 + (long) this.offsets[21];
        this.tevSwapModes = br.ReadNews<TevSwapMode>(sectionLengths[21] / 4);

        br.Position = position1 + (long) this.offsets[22];
        this.tevSwapModeTables =
            br.ReadNews<TevSwapModeTable>(sectionLengths[22] / 4);

        // TODO: Add support for fog modes (23)

        br.Position = position1 + (long) this.offsets[24];
        this.alphaCompares = br.ReadNews<AlphaCompare>(sectionLengths[24] / 8);

        br.Position = position1 + (long) this.offsets[25];
        this.blendFunctions =
            br.ReadNews<BlendFunction>(sectionLengths[25] / 4);

        br.Position = position1 + (long) this.offsets[26];
        this.depthFunctions =
            br.ReadNews<DepthFunction>(sectionLengths[26] / 4);

        br.Position = position1 + (long) this.header.size;
        ok = true;

        // TODO: Add support for nbt scale (29)

        this.PopulatedMaterials = this.materialEntries
                                      .Select(
                                          (entry, index)
                                              => new BmdPopulatedMaterial(
                                                  this,
                                                  index,
                                                  entry))
                                      .ToArray();
      }
    }

    public int[] GetSectionLengths() {
      int[] numArray = new int[30];
      for (int index1 = 0; index1 < 30; ++index1) {
        int num1 = 0;
        if (this.offsets[index1] != 0U) {
          int num2 = (int) this.header.size;
          for (int index2 = index1 + 1; index2 < 30; ++index2) {
            if (this.offsets[index2] != 0U) {
              num2 = (int) this.offsets[index2];
              break;
            }
          }

          num1 = num2 - (int) this.offsets[index1];
        }

        numArray[index1] = num1;
      }

      return numArray;
    }
  }
}