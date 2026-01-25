using System.Numerics;

using fin.schema.vector;

using schema.binary;

namespace visceral.schema.mtlb;

public sealed class Mtlb : IBinaryDeserializable {
  public string Name { get; private set; }

  public IReadOnlyList<MtlbChannel> HighLodMaterialChannels {
    get;
    private set;
  }

  public IReadOnlyList<MtlbChannel> LowLodMaterialChannels {
    get;
    private set;
  }

  public void Read(IBinaryReader br) {
      br.Position = 0x10;
      var stringsLength = br.ReadUInt32();

      br.Position = 0x40;

      // TODO: These might be wrong
      var highLodChannelCount = br.ReadUInt16();
      var lowLodChannelCount = br.ReadUInt16();

      var valuesOffset = br.ReadUInt32();
      var stringsOffset = br.ReadUInt32();

      this.Name = br.SubreadAt(stringsOffset + stringsLength,
                               () => br.ReadStringNT());

      br.Position = 0x50;

      var readNewChannel =
          () => {
            var id = br.ReadUInt32();
            var category = (MtlbChannelCategory) br.ReadUInt32();

            var typeString = br.SubreadAt(stringsOffset + br.ReadUInt32(),
                                          () => br.ReadStringNT());
            var type = typeString.ToMtlbChannelType();

            var unk0 = br.ReadUInt32();
            var unk1 = br.ReadUInt32();

            var valueOffset = valuesOffset + br.ReadUInt32();
            Vector4? colorValues = null;
            Vector2I? idValues = null;
            if (type.IsSampler()) {
              idValues
                  = br.SubreadAt(valueOffset, () => br.ReadNew<Vector2I>());
            } else {
              colorValues = br.SubreadAt(valueOffset, br.ReadVector4);
            }

            var path = br.SubreadAt(stringsOffset + br.ReadUInt32(),
                                    br.ReadStringNT);

            return new MtlbChannel {
                MtlbChannelCategory = category,
                Type = type,
                ColorValues = colorValues,
                IdValues = idValues,
                Path = path,
            };
          };

      var highLodMaterialChannels
          = new MtlbChannel[highLodChannelCount];
      this.HighLodMaterialChannels = highLodMaterialChannels;
      for (var i = 0; i < highLodChannelCount; i++) {
        highLodMaterialChannels[i] = readNewChannel();
      }

      var lowLodMaterialChannels = new MtlbChannel[lowLodChannelCount];
      this.LowLodMaterialChannels = lowLodMaterialChannels;
      for (var i = 0; i < lowLodChannelCount; i++) {
        lowLodMaterialChannels[i] = readNewChannel();
      }
    }
}

public enum MtlbChannelCategory {
  SAMPLER = 5,
}

public enum MtlbChannelType {
  NOT_SUPPORTED,
  OCCLUSION_SAMPLER,
  DIFFUSE_SAMPLER,
  NORMAL_SAMPLER,
  EMISSIVE_SAMPLER,
  SPEC_ENV_MAP_SAMPLER,
  SPECULAR_TEX_SAMPLER,
  AMB_LIGHT_AMB_OCCL_INTENSITY_FACING_RATIO,
  BUMP_DIFF_LGT_SPEC_MOD_SHINNYNESS,
  G_BLINK_PARAMS,
  G_MATERIAL_NORMAL_MAP_SCALE,
  G_SKIN_PS_PARAMS,
  SHININESS,
}

internal static class MtlbChannelTypeExtensions {
  public static MtlbChannelType ToMtlbChannelType(this string typeText)
    => typeText switch {
        "AmbLightAmbOcclIntensityFacingRatio" => MtlbChannelType
            .AMB_LIGHT_AMB_OCCL_INTENSITY_FACING_RATIO,
        "bumpDiffLgtSpecModShinnyness" => MtlbChannelType
            .BUMP_DIFF_LGT_SPEC_MOD_SHINNYNESS,
        "colorTexSampler"
            or "g_Sampler" => MtlbChannelType.DIFFUSE_SAMPLER,
        "g_blinkParams" => MtlbChannelType.G_BLINK_PARAMS,
        "g_materialNormalMapScale" => MtlbChannelType
            .G_MATERIAL_NORMAL_MAP_SCALE,
        "g_skinPSParams" => MtlbChannelType.G_SKIN_PS_PARAMS,
        "normalSampler"
            or "g_materialNormalMap" => MtlbChannelType.NORMAL_SAMPLER,
        "OcclusionTexSampler"
            or "AoMapSampler" => MtlbChannelType.OCCLUSION_SAMPLER,
        "SelfIllumTexSampler"
            or "LightMapSampler" => MtlbChannelType.EMISSIVE_SAMPLER,
        "Shinnyness"
            or "g_SpecularExponent" => MtlbChannelType.SHININESS,
        "SpecEnvMapSampler"
            or "g_materialSpecMap" => MtlbChannelType.SPEC_ENV_MAP_SAMPLER,
        "SpecularTexSampler"
            or "g_GlossMapSampler" => MtlbChannelType.SPECULAR_TEX_SAMPLER,
        _ => MtlbChannelType.NOT_SUPPORTED
    };

  public static bool IsSampler(this MtlbChannelType type)
    => type is MtlbChannelType.DIFFUSE_SAMPLER
               or MtlbChannelType.NORMAL_SAMPLER
               or MtlbChannelType.OCCLUSION_SAMPLER
               or MtlbChannelType.EMISSIVE_SAMPLER
               or MtlbChannelType.SPECULAR_TEX_SAMPLER;
}

public sealed class MtlbChannel {
  public MtlbChannelCategory MtlbChannelCategory { get; set; }
  public MtlbChannelType Type { get; set; }
  public Vector4? ColorValues { get; set; }
  public Vector2I? IdValues { get; set; }
  public string Path { get; set; }
}