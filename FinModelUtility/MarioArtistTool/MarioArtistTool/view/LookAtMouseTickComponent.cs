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
    IReadOnlyBone neckBone,
    IReadOnlyBone torsoBone)
    : ISceneNodeTickComponent {
  private Quaternion currentNeckRotation_
      = new Vector3(MathF.PI / 2, -MathF.PI / 2, MathF.PI / 2)
          .CreateZyxRadians();

  private Quaternion currentTorsoRotation_
      = new Vector3(0, -MathF.PI / 2, MathF.PI / 2).CreateZyxRadians();

  public void Dispose() { }

  public void Tick(ISceneNodeInstance self) {
    var lookAtMouse = !MainViewInputService.MouseDown &&
                      MainViewInputService.MouseInView;

    var bodyNode = self.ChildNodes[0];

    var bodyYRadiansForNeck = bodyNode.Rotation.YRadians + MathF.PI / 2;
    var bodyYRadiansForTorso = bodyNode.Rotation.YRadians;

    Quaternion newNeckRotation;
    Quaternion newTorsoRotation;
    newTorsoRotation
        = new Vector3(bodyYRadiansForTorso, -MathF.PI / 2, MathF.PI / 2)
            .CreateZyxRadians();
    if (!lookAtMouse) {
      newNeckRotation
          = new Vector3(bodyYRadiansForNeck, -MathF.PI / 2, MathF.PI / 2)
              .CreateZyxRadians();
    } else {
      var toRadiansForNeck = CalculateEulerRadiansForMousePosition_(
          MainViewInputService.NormalizedMousePosition -
          new Vector2(.5f, .25f));

      var maxDeltaYRadians = 110 * FinTrig.DEG_2_RAD;

      var deltaYRadians
          = RadiansUtil
            .CalculateRadiansTowards(bodyYRadiansForNeck, toRadiansForNeck.X)
            .Clamp(-maxDeltaYRadians, maxDeltaYRadians);
      toRadiansForNeck.X = bodyYRadiansForNeck + deltaYRadians;

      var toRadiansForTorso = new Vector3(bodyYRadiansForTorso + deltaYRadians * .5f,
                                          -MathF.PI / 2,
                                          MathF.PI / 2);

      var bodyBackRotation
          = new Vector3(bodyYRadiansForNeck + MathF.PI, 0, 0)
              .CreateZyxRadians();

      {
        var fromRotationForNeck = this.currentNeckRotation_;
        var toRotationForNeck = toRadiansForNeck.CreateZyxRadians();

        var distanceToFacingBackwards
            = Quaternion.Dot(fromRotationForNeck, bodyBackRotation);
        var distanceToFacingNewDirection
            = Quaternion.Dot(fromRotationForNeck, toRotationForNeck);

        var slerpTheLongWay
            = distanceToFacingBackwards > distanceToFacingNewDirection;

        newNeckRotation = fromRotationForNeck.SlerpTowards(toRotationForNeck,
          5 * FrameTime.DeltaTime,
          !slerpTheLongWay);
      }

      {
        var fromRotationForTorso = this.currentTorsoRotation_;
        var toRotationForTorso = toRadiansForTorso.CreateZyxRadians();

        var distanceToFacingBackwards
            = Quaternion.Dot(fromRotationForTorso, bodyBackRotation);
        var distanceToFacingNewDirection
            = Quaternion.Dot(fromRotationForTorso, toRotationForTorso);

        var slerpTheLongWay
            = distanceToFacingBackwards > distanceToFacingNewDirection;

        newTorsoRotation = fromRotationForTorso
            .SlerpTowards(toRotationForTorso,
                          8 * FrameTime.DeltaTime,
                          !slerpTheLongWay);
      }
    }

    this.currentNeckRotation_ = newNeckRotation;
    this.currentTorsoRotation_ = newTorsoRotation;

    boneTransformView.OverrideWorldRotation(
        neckBone,
        newNeckRotation);
    boneTransformView.OverrideWorldRotation(
        torsoBone,
        newTorsoRotation);
  }

  private static Vector3 CalculateEulerRadiansForMousePosition_(
      Vector2 position)
    => new(position.X * 2 + MathF.PI / 2,
           -position.Y - MathF.PI / 2,
           MathF.PI / 2);
}