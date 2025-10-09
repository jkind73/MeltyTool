using System.Numerics;

using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering.gl.model;

using marioartisttool.util;

namespace MarioArtistTool.backgrounds;

public static class SceneryRendererUtils {
  public const byte MAX_ALPHA = 128;

  public static IModelRenderer CreateModelRendererForImage(string imageAsset) {
    var backgroundFlowerModel = ModelImpl.CreateForViewer(4);

    var backgroundFlowerImage = AssetLoaderUtil.LoadImage(imageAsset);
    var backgroundFlowerTexture
        = backgroundFlowerModel.MaterialManager.CreateTexture(
            backgroundFlowerImage);

    var backgroundFlowerModelMaterial
        = backgroundFlowerModel.MaterialManager.AddFixedFunctionMaterial();
    var equations = backgroundFlowerModelMaterial.Equations;
    equations.SetOutputColorAlpha(
        backgroundFlowerModelMaterial.GenerateDiffuseMixed(
            (equations.CreateOrGetColorInput(FixedFunctionSource.BLEND_COLOR),
             equations.CreateOrGetScalarInput(FixedFunctionSource.BLEND_ALPHA)),
            backgroundFlowerTexture,
            0xc8 / 255f,
            (false, false)));
    backgroundFlowerModelMaterial.CullingMode = CullingMode.SHOW_BOTH;

    var backgroundFlowerModelSkin = backgroundFlowerModel.Skin;
    backgroundFlowerModelSkin
        .AddMesh()
        .AddSimpleFloor(backgroundFlowerModelSkin,
                        new Vector3(-1, 1, 0),
                        new Vector3(1, -1, 0),
                        backgroundFlowerModelMaterial);

    return new ModelRenderer(backgroundFlowerModel);
  }
}

