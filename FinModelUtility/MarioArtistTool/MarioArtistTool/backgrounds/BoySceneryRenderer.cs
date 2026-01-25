using System;
using System.Drawing;
using System.Numerics;

using fin.math.matrix.four;
using fin.ui.rendering;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;
using fin.util.time;

namespace marioartisttool.backgrounds;

public sealed class BoySceneryRenderer : IRenderable {
  private readonly IModelRenderer twoThousandRenderer_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/boy/2000.png", false);
  private readonly IModelRenderer deedeeRenderer_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/boy/deedee.png", false);
  private readonly IModelRenderer elPeeRenderer_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/boy/el-pee.png", false);
  private readonly IModelRenderer ntdRenderer_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/boy/ntd.png", false);
  private readonly IModelRenderer phaceRenderer_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/boy/phace.png", false);
  private readonly IModelRenderer traxRenderer_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/boy/trax.png", false);

  private readonly Matrix4x4 topLeftMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(0, 0), new Vector2(269, 112)));

  private readonly Matrix4x4 middleLeftMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(85, 154), new Vector2(256, 231)));

  private readonly Matrix4x4 bottomLeftMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(112, 301), new Vector2(341, 368)));

  private readonly Matrix4x4 topRightMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(387, 13), new Vector2(630, 119)));

  private readonly Matrix4x4 middleRightMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(385, 143), new Vector2(635, 260)));

  private readonly Matrix4x4 bottomRightMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(379, 285), new Vector2(640, 427)));

  public void Dispose() {
    this.twoThousandRenderer_.Dispose();
    this.deedeeRenderer_.Dispose();
    this.elPeeRenderer_.Dispose();
    this.ntdRenderer_.Dispose();
    this.phaceRenderer_.Dispose();
    this.traxRenderer_.Dispose();
  }

  public void Render() {
    var timeSeconds
        = (float) (6 + FrameTime.ElapsedTimeSinceApplicationOpened
                                .TotalSeconds);

    Span<(float cycleSeconds, IModelRenderer modelRenderer, Matrix4x4 matrix, Color color)>
        cycleSecondsAndMatrices = [
            ((timeSeconds - 0) % 6, this.phaceRenderer_, this.topLeftMatrix_, Color.Yellow),
            ((timeSeconds - 1) % 6, this.ntdRenderer_, this.middleLeftMatrix_, Color.Green),
            ((timeSeconds - 2) % 6, this.traxRenderer_, this.bottomLeftMatrix_, Color.Cyan),

            ((timeSeconds - 3) % 6, this.twoThousandRenderer_, this.topRightMatrix_, Color.DarkBlue),
            ((timeSeconds - 4) % 6, this.elPeeRenderer_, this.middleRightMatrix_, Color.Blue),
            ((timeSeconds - 5) % 6, this.deedeeRenderer_, this.bottomRightMatrix_, Color.HotPink),
        ];

    foreach (var (cycleSeconds, renderer, matrix, color) in cycleSecondsAndMatrices) {
      var alpha = cycleSeconds switch {
          < 1 => cycleSeconds,
          > 3 => 0,
          > 2 => 3 - cycleSeconds,
          _   => 1
      };

      GlUtil.SetBlendColor(
          Color.FromArgb((byte) (SceneryRendererUtils.MAX_ALPHA * alpha),
                         color.R,
                         color.G,
                         color.B));

      GlTransform.PushMatrix();
      GlTransform.MultMatrix(matrix);
      renderer.Render();
      GlTransform.PopMatrix();
    }
  }

  private static (Vector2 center, Vector2 size) GetCenterAndSize_(
      Vector2 pt1,
      Vector2 pt2) {
    var center = (pt1 + pt2) / 2;
    var size = (pt2 - pt1) / 2;
    return (center, size);
  }

  private static Matrix4x4 GetMergedMatrix_(
      (Vector2 center, Vector2 size) tuple) {
    var (center, size) = tuple;

    return SystemMatrix4X4Util.FromTrs(
        new Vector3(center.X, center.Y, 0),
        (Quaternion?) null,
        new Vector3(size, 1));
  }
}

