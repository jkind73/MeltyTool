using System;
using System.Drawing;
using System.Numerics;

using fin.math.matrix.four;
using fin.ui.rendering;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;
using fin.util.time;

namespace MarioArtistTool.backgrounds;

public sealed class OtherSceneryRenderer : IRenderable, IDisposable {
  private readonly IModelRenderer fossilRenderer0_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/other/fossil_0.png");
  private readonly IModelRenderer fossilRenderer1_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/other/fossil_1.png");
  private readonly IModelRenderer fossilRenderer2_
      = SceneryRendererUtils.CreateModelRendererForImage(
          "backgrounds/other/fossil_2.png");

  private readonly Matrix4x4 topLeftMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(109, -2), new Vector2(221, 96)));

  private readonly Matrix4x4 middleLeftMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(171, 103), new Vector2(302, 234)));

  private readonly Matrix4x4 bottomLeftMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(69, 280), new Vector2(240, 445)));

  private readonly Matrix4x4 topRightMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(393, -4), new Vector2(563, 153)));

  private readonly Matrix4x4 middleRightMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(545, 147), new Vector2(646, 253)));

  private readonly Matrix4x4 bottomRightMatrix_
      = GetMergedMatrix_(
          GetCenterAndSize_(new Vector2(365, 236), new Vector2(519, 387)));

  public void Dispose() {
    this.fossilRenderer0_.Dispose();
    this.fossilRenderer1_.Dispose();
    this.fossilRenderer2_.Dispose();
  }

  public void Render() {
    var timeSeconds
        = (float) (6 + FrameTime.ElapsedTimeSinceApplicationOpened
                                .TotalSeconds);

    Span<(float cycleSeconds, IModelRenderer modelRenderer, Matrix4x4 matrix, Color color)>
        cycleSecondsAndMatrices = [
            ((timeSeconds - 0) % 6, this.fossilRenderer2_, this.topLeftMatrix_, Color.Yellow),
            ((timeSeconds - 1) % 6, this.fossilRenderer1_, this.middleLeftMatrix_, Color.Green),
            ((timeSeconds - 2) % 6, this.fossilRenderer2_, this.bottomLeftMatrix_, Color.Cyan),

            ((timeSeconds - 3) % 6, this.fossilRenderer1_, this.topRightMatrix_, Color.DarkBlue),
            ((timeSeconds - 4) % 6, this.fossilRenderer0_, this.middleRightMatrix_, Color.Blue),
            ((timeSeconds - 5) % 6, this.fossilRenderer0_, this.bottomRightMatrix_, Color.HotPink),
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

    var scale = 1.4f;

    return SystemMatrix4x4Util.FromTrs(
        scale * new Vector3(center.X - 320, 480 - center.Y, -100),
        (Quaternion?) null,
        scale * new Vector3(size, 1));
  }
}

