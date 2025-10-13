using System;
using System.Numerics;

using fin.config.avalonia.services;
using fin.math;
using fin.math.rotations;
using fin.model;
using fin.model.skeleton;
using fin.scene;
using fin.util.time;

namespace MarioArtistTool.view;

/// <summary>
///   Needs to be a render component to be able to project the point relative
///   to the current view.
/// </summary>
public class LookAtMouseTickComponent(
    SimpleBoneTransformView boneTransformView,
    IReadOnlyBone neckBone)
    : ISceneNodeTickComponent {
  private static readonly Quaternion DEFAULT
      = CalculateEulerRadiansForMousePosition_(Vector2.Zero).CreateZyxRadians();

  private Quaternion currentRotation_ = DEFAULT;

  public void Dispose() { }

  public void Tick(ISceneNodeInstance self) {
    var lookAtMouse = !MainViewInputService.MouseDown &&
                      MainViewInputService.MouseInView;

    var fromRotation = this.currentRotation_;

    var bodyNode = self.ChildNodes[0];
    var bodyYRadians = bodyNode.Rotation.YRadians + MathF.PI / 2;

    var slerpTheLongWay = false;
    Quaternion toRotation;
    if (!lookAtMouse) {
      toRotation = new Vector3(bodyYRadians, -MathF.PI / 2, MathF.PI / 2)
          .CreateZyxRadians();
    } else {
      var toRadians = CalculateEulerRadiansForMousePosition_(
          MainViewInputService.NormalizedMousePosition -
          new Vector2(.5f, .25f));

      var maxDeltaYRadians = 110 * FinTrig.DEG_2_RAD;

      var deltaYRadians
          = RadiansUtil.CalculateRadiansTowards(bodyYRadians, toRadians.X)
                       .Clamp(-maxDeltaYRadians, maxDeltaYRadians);
      toRadians.X = bodyYRadians + deltaYRadians;
      toRotation = toRadians.CreateZyxRadians();

      var bodyBackRotation
          = new Vector3(bodyYRadians + MathF.PI, 0, 0).CreateZyxRadians();

      var distanceToFacingBackwards
          = Quaternion.Dot(fromRotation, bodyBackRotation);
      var distanceToFacingNewDirection
          = Quaternion.Dot(fromRotation, toRotation);

      slerpTheLongWay
          = distanceToFacingBackwards > distanceToFacingNewDirection;
    }

    var newRotation
        = fromRotation.SlerpTowards(toRotation,
                                20 * FrameTime.DeltaTime,
                                !slerpTheLongWay);

    this.currentRotation_ = newRotation;

    boneTransformView.OverrideWorldRotation(
        neckBone,
        newRotation);
  }

  private static Vector3 CalculateEulerRadiansForMousePosition_(
      Vector2 position)
    => new(position.X * 2 + MathF.PI / 2,
           -position.Y - MathF.PI / 2,
           MathF.PI / 2);
}