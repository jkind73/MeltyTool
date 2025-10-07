using fin.config.avalonia.services;
using fin.scene;

namespace MarioArtistTool.view;

public class RotateTalentTickComponent : ISceneNodeTickComponent {
  private float? prevMouseX_;

  public void Dispose() { }

  public void Tick(ISceneNodeInstance self) {
    if (!MainViewInputService.MouseDown) {
      this.prevMouseX_ = null;
      return;
    }

    var mouseX = MainViewInputService.NormalizedMousePosition.X;
    var mouseDeltaX = 0f;
    if (this.prevMouseX_ != null) {
      mouseDeltaX = mouseX - this.prevMouseX_.Value;
    }

    self.SetRotationRadians(0, self.Rotation.YRadians + 10 * mouseDeltaX, 0);

    this.prevMouseX_ = mouseX;
  }
}