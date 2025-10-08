using System;
using System.Numerics;

using fin.config.avalonia.services;
using fin.math.rotations;
using fin.model;
using fin.model.skeleton;
using fin.scene;

namespace MarioArtistTool.view;

/// <summary>
///   Needs to be a render component to be able to project the point relative
///   to the current view.
/// </summary>
public class LookAtMouseTickComponent(
    IReadOnlyBoneTransformManager2 boneTransformManager,
    SimpleBoneTransformView boneTransformView,
    IReadOnlyBone neckBone)
    : ISceneNodeTickComponent {
  public void Dispose() { }

  public void Tick(ISceneNodeInstance self) {
    if (MainViewInputService.MouseDown || !MainViewInputService.MouseInView) {
      return;
    }

    var mouseX = MainViewInputService.NormalizedMousePosition.X - .5f;
    var mouseY = MainViewInputService.NormalizedMousePosition.Y - .25f;

    boneTransformView.OverrideWorldRotation(
        neckBone,
        QuaternionUtil.CreateZyxRadians(
            mouseX * 2 + MathF.PI / 2,
            -mouseY,
    MathF.PI / 2));
  }
}