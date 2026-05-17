using System.Numerics;

using fin.ui;
using fin.ui.rendering.gl;
using fin.util.time;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace UniversalAssetTool.Ui.Gl;

public class SceneViewerWindow : GameWindow {
  private readonly SceneViewerGl viewerImpl_ = new();
  private readonly TimedCallback fpsTimer_;

  public SceneViewerWindow(GameWindowSettings gameWindowSettings,
                           NativeWindowSettings nativeWindowSettings) : base(
      gameWindowSettings,
      nativeWindowSettings) {
    this.viewerImpl_.ShowGrid = true;
    this.fpsTimer_ = new(() => this.Title = FrameTime.FpsString, 1);
  }

  protected override void OnLoad() {
    base.OnLoad();

    GlUtil.SwitchContext(this.Context);
    GlUtil.InitGl();
  }

  protected override void OnUpdateFrame(FrameEventArgs e) {
    base.OnUpdateFrame(e);
  }

  protected override void OnRenderFrame(FrameEventArgs e) {
    base.OnRenderFrame(e);

    GlUtil.SwitchContext(this.Context);

    this.viewerImpl_.Width = this.Size.X;
    this.viewerImpl_.Height = this.Size.Y;

    this.viewerImpl_.GlobalScale = UiConstants.GLOBAL_SCALE;
    this.viewerImpl_.NearPlane = UiConstants.NEAR_PLANE;
    this.viewerImpl_.FarPlane = UiConstants.FAR_PLANE;

    this.viewerImpl_.Render();

    this.SwapBuffers();
  }
}