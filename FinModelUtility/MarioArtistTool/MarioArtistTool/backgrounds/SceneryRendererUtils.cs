using System.Numerics;

using fin.image.util;
using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering.gl.model;

using marioartisttool.util;

namespace MarioArtistTool.backgrounds;

public static class SceneryRendererUtils {
  public const byte MAX_ALPHA = 170;

  public static IModelRenderer CreateModelRendererForImage(string imageAsset) {
    var model = ModelImpl.CreateForViewer(4);

    var image = AssetLoaderUtil.LoadImage(imageAsset);
    var texture = model.MaterialManager.CreateTexture(image);

    var material = model.MaterialManager.AddFixedFunctionMaterial();
    var equations = material.Equations;
    equations.SetOutputColorAlpha(
        material.GenerateDiffuseMixed(
            (equations.CreateOrGetColorInput(FixedFunctionSource.BLEND_COLOR),
             equations.CreateOrGetScalarInput(FixedFunctionSource.BLEND_ALPHA)),
            texture,
            0xc8 / 255f,
            (false, false)));
    material.CullingMode = CullingMode.SHOW_BOTH;
    material.DepthCompareType = DepthCompareType.Always;
    material.DepthMode = DepthMode.NONE;

    var scale = 1f;

    var backgroundFlowerModelSkin = model.Skin;
    backgroundFlowerModelSkin
        .AddMesh()
        .AddSimpleFloor(backgroundFlowerModelSkin,
                        new Vector3(-scale, scale, 0),
                        new Vector3(scale, -scale, 0),
                        material);

    return new ModelRenderer(model);
  }
}

