using System.Numerics;

using fin.scene;

namespace fin.ui.rendering.viewer.controller;

public interface IViewerInputs {
  float MovementForwardVector { get; set; }
  float MovementRightwardVector { get; set; }
  float MovementUpwardVector { get; set; }
  float MovementSpeed { get; set; }

  Vector2 LookAxes { get; set; }
}

public enum ViewerInputKey {
  UNDEFINED,
  MOVE_FORWARD,
  MOVE_BACKWARD,
  MOVE_RIGHT,
  MOVE_LEFT,
  MOVE_UP,
  MOVE_DOWN,
  SPEED_UP,
  SLOW_DOWN,
}

public class MouseAndKeyboardViewerInputs : IViewerInputs, ITickable {
  private readonly Dictionary<ViewerInputKey, bool> isKeyPressed_ = new();

  private Vector2 mousePosition_;
  private bool isMousePressed_;

  public float MovementForwardVector { get; set; }
  public float MovementRightwardVector { get; set; }
  public float MovementUpwardVector { get; set; }
  public float MovementSpeed { get; set; }

  public Vector2 LookAxes { get; set; }

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
  }
}