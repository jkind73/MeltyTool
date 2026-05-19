using System.Numerics;

using fin.scene;

namespace fin.ui.rendering.viewer.controller;

public class MouseAndKeyboardViewerInputs : IViewerInputs, ITickable {
  private readonly Dictionary<ViewerInputKey, bool> isKeyPressed_ = new();

  private bool isMousePressed_;
  private bool hasPreviousMousePosition_;
  private Vector2 previousMousePosition_;
  private bool hasNewMousePosition_;
  private Vector2 newMousePosition_;

  public float MovementForwardVector { get; private set; }
  public float MovementRightwardVector { get; private set; }
  public float MovementUpwardVector { get; private set; }
  public float MovementSpeed { get; private set; }

  public Vector2 ViewerSize { get; set; }
  public float FovY { get; set; }
  public float YawDegreesDelta { get; private set; }
  public float PitchDegreesDelta { get; private set; }

  public void PressKey(ViewerInputKey key) {
    if (key == ViewerInputKey.UNDEFINED) {
      return;
    }

    this.isKeyPressed_[key] = true;
  }

  public void ReleaseKey(ViewerInputKey key) {
    if (key == ViewerInputKey.UNDEFINED) {
      return;
    }

    this.isKeyPressed_[key] = false;
  }

  public void PressMouse() {
    this.isMousePressed_ = true;
    this.hasPreviousMousePosition_ = false;
  }

  public void ReleaseMouse() {
    this.isMousePressed_ = false;
  }

  public void MoveMouse(Vector2 position) {
    this.hasNewMousePosition_ = true;
    this.newMousePosition_ = position;
  }

  private bool IsKeyPressed_(ViewerInputKey key)
    => this.isKeyPressed_.GetValueOrDefault(key);

  public void Tick() {
    var isForwardPressed = this.IsKeyPressed_(ViewerInputKey.MOVE_FORWARD);
    var isBackwardPressed = this.IsKeyPressed_(ViewerInputKey.MOVE_BACKWARD);
    var isRightPressed = this.IsKeyPressed_(ViewerInputKey.MOVE_RIGHT);
    var isLeftPressed = this.IsKeyPressed_(ViewerInputKey.MOVE_LEFT);
    var isUpPressed = this.IsKeyPressed_(ViewerInputKey.MOVE_UP);
    var isDownPressed = this.IsKeyPressed_(ViewerInputKey.MOVE_DOWN);

    this.MovementForwardVector
        = (isForwardPressed ? 1 : 0) - (isBackwardPressed ? 1 : 0);
    this.MovementRightwardVector
        = (isRightPressed ? 1 : 0) - (isLeftPressed ? 1 : 0);
    this.MovementUpwardVector = (isUpPressed ? 1 : 0) - (isDownPressed ? 1 : 0);

    this.MovementSpeed = UiConstants.GLOBAL_SCALE * 15;
    if (this.IsKeyPressed_(ViewerInputKey.SPEED_UP)) {
      this.MovementSpeed *= 2;
    }

    if (this.IsKeyPressed_(ViewerInputKey.SLOW_DOWN)) {
      this.MovementSpeed /= 2;
    }

    this.YawDegreesDelta = this.PitchDegreesDelta = 0;

    if (this.isMousePressed_ &&
        this.hasNewMousePosition_ &&
        this.hasPreviousMousePosition_) {
      var deltaMousePosition
          = this.newMousePosition_ - this.previousMousePosition_;

      var deltaFrac = deltaMousePosition / this.ViewerSize;

      var mouseSpeed = 3;

      var fovX = this.FovY / this.ViewerSize.Y * this.ViewerSize.X;

      this.YawDegreesDelta = -deltaFrac.X * fovX * mouseSpeed;
      this.PitchDegreesDelta = -deltaFrac.Y * this.FovY * mouseSpeed;
    }

    this.previousMousePosition_ = this.newMousePosition_;
    this.hasPreviousMousePosition_ = this.hasNewMousePosition_;
    this.hasNewMousePosition_ = false;
  }
}