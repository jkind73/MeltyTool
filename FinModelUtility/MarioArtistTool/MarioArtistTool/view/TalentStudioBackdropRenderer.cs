using System;

using fin.ui.rendering;
using fin.ui.rendering.gl;
using fin.ui.rendering.viewer;

using marioartist.schema.talent_studio;

using marioartisttool.util;

using MarioArtistTool.backgrounds;

namespace MarioArtistTool.view;

public sealed class TalentStudioBackdropRenderer : IOrthoRenderable {
  private readonly BackgroundRenderer backgroundRenderer_;
  private readonly FloorShadowRenderer floorShadowRenderer_ = new();
  private readonly IRenderable? sceneryRenderer_;
  public float ViewportWidth { get; set; }
  public float ViewportHeight { get; set; }

  public float NearPlane { get; set; }
  public float FarPlane { get; set; }
  
  public TalentStudioBackdropRenderer(Gender gender) {
    this.backgroundRenderer_ = new BackgroundRenderer {
        BackgroundImage = gender switch {
            Gender.BOY =>
                AssetLoaderUtil.LoadImage("backgrounds/boy/background.png"),
            Gender.GIRL =>
                AssetLoaderUtil.LoadImage("backgrounds/girl/background.png"),
            Gender.OTHER =>
                AssetLoaderUtil.LoadImage("backgrounds/other/background.png"),
        },
        BackgroundImageScale = .3f,
    };

    this.sceneryRenderer_ = gender switch {
        Gender.BOY => null,
        Gender.GIRL => new GirlSceneryRenderer(),
        Gender.OTHER => new OtherSceneryRenderer(),
    };
  }

  ~TalentStudioBackdropRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.backgroundRenderer_.Dispose();
    this.floorShadowRenderer_.Dispose();
    this.sceneryRenderer_?.Dispose();
  }

  public void Render() {
    GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();

    GlTransform.MatrixMode(TransformMatrixMode.VIEW);
    GlTransform.PushMatrix();
    GlTransform.LoadIdentity();

    {
      GlTransform.MatrixMode(TransformMatrixMode.MODEL);
      GlTransform.LoadIdentity();

      var width = this.ViewportWidth;
      var height = this.ViewportHeight;

      var hWidth = width / 2f;
      var hHeight = height / 2f;

      GlTransform.Ortho2d(0, (int) width, (int) height, 0);

      GlTransform.PushMatrix();
      GlTransform.Translate(hWidth, hHeight, 0);
      GlTransform.Scale(hWidth, hHeight, 1);

      this.backgroundRenderer_.AspectRatio = hWidth / hHeight;
      this.backgroundRenderer_.Render();

      this.floorShadowRenderer_.ViewportWidth = width;
      this.floorShadowRenderer_.ViewportHeight = height;

      GlTransform.PushMatrix();
      GlTransform.LoadIdentity();
      this.floorShadowRenderer_.Render();
      GlTransform.PopMatrix();

      GlTransform.PopMatrix();
      GlTransform.Translate(hWidth - 320, hHeight - 240, 0);
      
      this.sceneryRenderer_?.Render();
    }

    GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
    GlTransform.PopMatrix();

    GlTransform.MatrixMode(TransformMatrixMode.VIEW);
    GlTransform.PopMatrix();
  }
}