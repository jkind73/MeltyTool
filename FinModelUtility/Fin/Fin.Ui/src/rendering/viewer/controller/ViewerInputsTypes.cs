namespace fin.ui.rendering.viewer.controller;

public interface IViewerInputs {
  float MovementForwardVector { get; }
  float MovementRightwardVector { get; }
  float MovementUpwardVector { get; }
  float MovementSpeed { get; }

  float YawDegreesDelta { get; }
  float PitchDegreesDelta { get; }
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