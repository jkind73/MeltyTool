using System;

using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;

using fin.ui.rendering.gl;
using fin.util.time;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.avalonia.gl;

public class OpenTkControl(Action initGl, Action renderGl, Action teardownGl)
    : OpenGlControlBase {
  private TopLevel topLevel_;
  private AvaloniaOpenTkContext? avaloniaTkContext_;
  private TimedCallback renderCallback_;
  private FpsTimer fpsTimer_;

  private static bool isLoaded_ = false;

  protected sealed override void OnOpenGlInit(GlInterface gl) {
    this.topLevel_ = TopLevel.GetTopLevel(this);

    if (!isLoaded_) {
      //Initialize the OpenTK<->Avalonia Bridge
      this.avaloniaTkContext_ = new AvaloniaOpenTkContext(gl);

      GL.LoadBindings(this.avaloniaTkContext_);
      isLoaded_ = true;
    }

    GlUtil.SwitchContext(this);
    initGl();

    this.fpsTimer_ = new FpsTimer(this) {
        Pause = false,
    };
  }

  protected override void OnOpenGlRender(GlInterface gl, int fb) {
    GlUtil.SwitchContext(this);
    renderGl();
  }

  protected sealed override void OnOpenGlDeinit(GlInterface gl)
    => teardownGl();
}