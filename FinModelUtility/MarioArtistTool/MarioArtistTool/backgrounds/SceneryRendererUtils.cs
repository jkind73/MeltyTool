using System.Numerics;

using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.ui.rendering.gl.model;

using marioartisttool.util;

namespace marioartisttool.backgrounds;

public static class SceneryRendererUtils {
  public const byte GIRL_ALPHA = 70;
  public const byte MAX_ALPHA = 50;

  public static IModelRenderer CreateModelRendererForImage(
      string imageAsset,
      bool mixed = true) {
    var model = ModelImpl.CreateForViewer(4);

    var image = AssetLoaderUtil.LoadImage(imageAsset);
    var texture = model.MaterialManager.CreateTexture(image);

    var material = model.MaterialManager.AddFixedFunctionMaterial();
    var equations = material.Equations;

    if (mixed) {
      equations.SetOutputColorAlpha(
          material.GenerateDiffuseColorized(
              (equations.CreateOrGetColorInput(FixedFunctionSource.BLEND_COLOR),
               equations.CreateOrGetScalarInput(FixedFunctionSource.BLEND_ALPHA)),
              texture,
              (false, false)));
    } else {
      equations.SetOutputColorAlpha(
          material.GenerateDiffuse(
              (equations.CreateOrGetColorInput(FixedFunctionSource.BLEND_COLOR),
               equations.CreateOrGetScalarInput(FixedFunctionSource.BLEND_ALPHA)),
              texture,
              (false, false)));
    }

    material.CullingMode = CullingMode.SHOW_BOTH;
    material.DepthCompareType = DepthCompareType.Always;
    material.DepthMode = DepthMode.NONE;

    var scale = 1f;

    var backgroundFlowerModelSkin = model.Skin;
    backgroundFlowerModelSkin
        .AddMesh()
        .AddSimpleFloor(backgroundFlowerModelSkin,
                        new Vector3(-scale, -scale, 0),
                        new Vector3(scale, scale, 0),
                        material);

    return new ModelRenderer(model);
  }
}

