using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using fin.schema;

using gx;

using jsystem.schema.j3dgraph.bmd.mat3;


namespace jsystem.GCN;

public sealed class BmdPopulatedMaterial : IPopulatedMaterial {
  public string Name { get; set; }
  public GxCullMode CullMode { get; set; }
  public byte colorChannelControlsCountIndex;
  public byte texGensCountIndex;
  public byte tevStagesCountIndex;
  public byte zCompLocIndex;
  public byte zModeIndex;
  public byte ditherIndex;

  public (int, Color)[] MaterialColors { get; set; }
  public IColorChannelControl?[] ColorChannelControls { get; set; }
  public (int, Color)[] AmbientColors { get; set; }
  public Color?[] LightColors { get; set; }

  public Color[] KonstColors { get; set; }
  public IColorRegister?[] ColorRegisters { get; set; }


  public ushort[] texGenInfo;

  public ushort[] texGenInfo2;
  public ITextureMatrixInfo?[] TextureMatrices { get; set; }
  public ushort[] dttMatrices;
  public short[] TextureIndices { get; set; }
  public ushort[] tevKonstColorIndexes;
  public byte[] constColorSel;
  public byte[] constAlphaSel;

  public ITevOrder?[] TevOrderInfos { get; set; }

  public ushort[] tevOrderInfoIndexes;
  public ushort[] tevColorIndexes;
  public ITevStageProps?[] TevStageInfos { get; set; }
  public ITevSwapMode?[] TevSwapModes { get; set; }
  public ITevSwapModeTable?[] TevSwapModeTables { get; set; }

  [Unknown]
  public ushort[] unknown2;

  public short fogInfoIndex;
  public IAlphaCompare AlphaCompare { get; set; }
  public IBlendFunction BlendMode { get; set; }

  [Unknown]
  public short unknownIndex;

  public IDepthFunction DepthFunction { get; }

  public ITexCoordGen?[] TexCoordGens { get; set; }

  public IIndirectTexture? IndirectTexture { get; set; }

  public BmdPopulatedMaterial(Bmd.Mat3Section mat3,
                              int index,
                              MaterialEntry entry) {
    this.Name = mat3.materialNameTable[index];

    this.CullMode = mat3.cullModes[entry.CullModeIndex];
    this.DepthFunction = mat3.depthFunctions[entry.DepthFunctionIndex];

    this.MaterialColors =
        entry.MaterialColorIndexes
             .Select(i => ((int) i, GetOrNull_(mat3.materialColor, i)))
             .ToArray();
    this.AmbientColors =
        entry.AmbientColorIndexes
             .Select(i => ((int) i, GetOrNull_(mat3.ambientColors, i)))
             .ToArray();

    this.LightColors =
        entry.LightColorIndexes
             .Select(i => GetOrNullStruct_(mat3.lightColors, i))
             .ToArray();

    this.ColorRegisters =
        entry.TevColorIndexes
             .Select(i => {
               var color = GetOrNull_(mat3.tevColors, i);
               if (color != null) {
                 return (IColorRegister) new GxColorRegister
                     {Color = color, Index = i,};
               }

               return null;
             })
             .ToArray();
    this.KonstColors =
        entry.TevKonstColorIndexes
             .Select(i => GetOrNull_(mat3.tevKonstColors, i))
             .ToArray();

    this.ColorChannelControls =
        entry.ColorChannelControlIndexes
             .Select(i => GetOrNull_(mat3.colorChannelControls, i))
             .ToArray();

    this.TextureMatrices =
        entry.TexMatrices
             .Select(i => GetOrNull_(mat3.textureMatrices, i))
             .ToArray();

    this.TevOrderInfos =
        entry.TevOrderInfoIndexes
             .Select(i => {
               var tevOrder = GetOrNull_(mat3.tevOrders, i);
               if (tevOrder == null) {
                 return null;
               }

               return new TevOrderWrapper(tevOrder) {
                   KonstAlphaSel = entry.KonstAlphaSel[i],
                   KonstColorSel = entry.KonstColorSel[i],
               };
             })
             .ToArray();

    this.TevStageInfos =
        entry.TevStageInfoIndexes
             .Select(i => GetOrNull_(mat3.tevStages, i))
             .ToArray();

    this.TevSwapModes =
        entry.TevSwapModeInfo
             .Select(i => GetOrNull_(mat3.tevSwapModes, i))
             .ToArray();
    this.TevSwapModeTables =
        entry.TevSwapModeTable
             .Select(i => GetOrNull_(mat3.tevSwapModeTables, i))
             .ToArray();

    this.TextureIndices =
        entry.TextureIndexes
             .Select(t => (short) (t != -1 ? mat3.textureIndices[t] : -1))
             .ToArray();

    this.TexCoordGens =
        entry.TexGenInfo
             .Select(i => GetOrNull_(mat3.texCoordGens, i))
             .ToArray();

    this.AlphaCompare = mat3.alphaCompares[entry.alphaCompareIndex];
    this.BlendMode = mat3.blendFunctions[entry.blendModeIndex];

    if (index < mat3.indirectTextures.Count) {
      var indirectTexture = mat3.indirectTextures[index];
      this.IndirectTexture = indirectTexture.HasLookup ? indirectTexture : null;
    }
  }

  private class TevOrderWrapper(TevOrder impl) : ITevOrder {
    public GxTexCoord TexCoordId => impl.TexCoordId;
    public GxTexMap TexMap => impl.TexMap;
    public GxColorChannel ColorChannelId => impl.ColorChannelId;

    public GxKonstColorSel KonstColorSel { get; set; }
    public GxKonstAlphaSel KonstAlphaSel { get; set; }
  }

  private static T? GetOrNull_<T>(IList<T> array, int i)
      where T : notnull
    => i != -1 ? array[i] : default;

  private static T? GetOrNullStruct_<T>(IList<T> array, int i)
      where T : struct
    => i != -1 ? array[i] : null;
}