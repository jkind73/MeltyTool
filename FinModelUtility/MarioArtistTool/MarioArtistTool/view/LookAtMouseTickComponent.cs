using System;

using fin.config.avalonia.services;
using fin.math.rotations;
using fin.model;
using fin.model.skeleton;
using fin.scene;

namespace MarioArtistTool.view;

public class LookAtMouseTickComponent(
    SimpleBoneTransformView boneTransformView,
    IReadOnlyBone headBone)
    : ISceneNodeTickComponent {
  public void Dispose() { }

  public void Tick(ISceneNodeInstance self) {
    if (MainViewInputService.MouseDown) {
      return;
    }

    var mouseX = MainViewInputService.NormalizedMousePosition.X - .5f;
    var mouseY = MainViewInputService.NormalizedMousePosition.Y - .5f;

    boneTransformView.OverrideWorldRotation(
        headBone,
        QuaternionUtil.CreateZyxRadians(
            mouseX * 3 + MathF.PI / 2,
            -mouseY * 3,
    MathF.PI / 2));
  }
}