using System.Collections.Generic;
using System.Linq;

using fin.model;

using gx;
using gx.impl;

using jsystem.GCN;
using jsystem.schema.jutility.bti;


namespace jsystem.exporter;

public sealed class BmdMaterialManager {
  private readonly Bmd bmd_;
  private readonly IList<IGxTexture> textures_;
  private readonly IList<GxFixedFunctionMaterial> materials_;

  public BmdMaterialManager(
      IModel model,
      Bmd bmd,
      IList<(string, Bti)>? pathsAndBtis = null) {
    this.bmd_ = bmd;

    var tex1 = bmd.Tex1.Data;
    this.textures_ = tex1.textureHeaders.Select((textureHeader, i) => {
                           var textureName = tex1.stringTable[i];

                           return (IGxTexture) new BmdGxTexture(
                               textureName,
                               textureHeader,
                               pathsAndBtis);
                         })
                         .ToList();

    this.materials_ = this.GetMaterials_(model, bmd);
  }

  public GxFixedFunctionMaterial Get(int entryIndex)
    => this.materials_[this.bmd_.mat3.materialEntryIndieces[entryIndex]];

  private IList<GxFixedFunctionMaterial>
      GetMaterials_(IModel model, Bmd bmd) {
    var lazyTextureMap = new GxLazyTextureDictionary(model);
    return bmd.mat3.materialEntries.Select(
                  (_, i) => new GxFixedFunctionMaterial(
                      model,
                      model.MaterialManager,
                      bmd.mat3.PopulatedMaterials[i],
                      this.textures_,
                      lazyTextureMap))
              .ToList();
  }
}