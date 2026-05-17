using System.Numerics;

using fin.ui;
using fin.ui.rendering.gl;
using fin.ui.rendering.viewer.controller;
using fin.util.time;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace UniversalAssetTool.Ui.Gl;

public class SceneViewerWindow : GameWindow {
  private readonly SceneViewerGl viewerImpl_ = new();
  private readonly MouseAndKeyboardViewerInputs inputsImpl_ = new();

  private readonly TimedCallback fpsTimer_;

  public SceneViewerWindow(GameWindowSettings gameWindowSettings,
                           NativeWindowSettings nativeWindowSettings) : base(
      gameWindowSettings,
      nativeWindowSettings) {
    this.viewerImpl_.ShowGrid = true;
    this.fpsTimer_ = new(() => this.Title = FrameTime.FpsString, 1);

    this.KeyDown += args
        => this.inputsImpl_.PressKey(GetViewerKeyFromTk_(args.Key));
    this.KeyUp += args
        => this.inputsImpl_.ReleaseKey(GetViewerKeyFromTk_(args.Key));
    this.MouseDown += args => this.inputsImpl_.PressMouse();
    this.MouseUp += args => this.inputsImpl_.ReleaseMouse();
    this.MouseMove += args => this.inputsImpl_.MoveMouse(
        new Vector2(args.Position.X, args.Position.Y));
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

    {
      this.inputsImpl_.ViewerSize = new Vector2(this.Size.X, this.Size.Y);
      this.inputsImpl_.FovY = this.viewerImpl_.FovY;
      this.inputsImpl_.Tick();

      var camera = this.viewerImpl_.Camera;
      camera.Move(this.inputsImpl_.MovementForwardVector,
                  this.inputsImpl_.MovementRightwardVector,
                  this.inputsImpl_.MovementUpwardVector,
                  this.inputsImpl_.MovementSpeed);
      camera.YawDegrees += this.inputsImpl_.YawDegreesDelta;
      camera.PitchDegrees = float.Clamp(
          camera.PitchDegrees + this.inputsImpl_.PitchDegreesDelta,
          -90,
          90);
    }

    this.viewerImpl_.Render();

    this.SwapBuffers();
  }

  private static ViewerInputKey GetViewerKeyFromTk_(Keys key)
    => key switch {
        Keys.W                                => ViewerInputKey.MOVE_FORWARD,
        Keys.S                                => ViewerInputKey.MOVE_BACKWARD,
        Keys.D                                => ViewerInputKey.MOVE_RIGHT,
        Keys.A                                => ViewerInputKey.MOVE_LEFT,
        Keys.Q                                => ViewerInputKey.MOVE_DOWN,
        Keys.E                                => ViewerInputKey.MOVE_UP,
        Keys.LeftShift or Keys.RightShift     => ViewerInputKey.SPEED_UP,
        Keys.LeftControl or Keys.RightControl => ViewerInputKey.SLOW_DOWN,
        _                                     => ViewerInputKey.UNDEFINED,
    };
}