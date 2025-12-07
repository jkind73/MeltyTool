using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.config.avalonia.services;
using fin.math;
using fin.math.rotations;
using fin.model;
using fin.model.skeleton;
using fin.model.util;
using fin.scene;
using fin.ui.rendering.gl;

using gawg.games.ball;

using marioartist.api;

namespace MarioArtistTool.view;

public class BallGameComponent
    : ISceneNodeTickComponent, ISceneNodeRenderComponent {
  private readonly BallGameManager gameManager_;
  private readonly BallRenderer ballRenderer_ = new(12);

  private readonly SimpleBoneTransformView boneTransformView_;

  private readonly IReadOnlyBone leftUpperArmBone_;
  private readonly IReadOnlyBone leftForearmBone_;
  private readonly IReadOnlyBone leftHandBone_;
  private readonly IReadOnlyBone rightUpperArmBone_;
  private readonly IReadOnlyBone rightForearmBone_;
  private readonly IReadOnlyBone rightHandBone_;

  private readonly float baseBallDistance_;
  private readonly float[] ballDistances_;

  private readonly float ballBaseY_;
  private readonly float minBallArcY_;
  private readonly float maxBallArcY_;

  private readonly (float upperArmRadians, float forearmRadians)[] armAngles_;

  private bool previousLeftDown_;
  private bool previousRightDown_;

  public BallGameComponent(SimpleBoneTransformView boneTransformView,
                           IReadOnlyModel model,
                           uint ballCount) {
    this.boneTransformView_ = boneTransformView;

    var skeleton = model.Skeleton;

    var bonesByJointIndex = new Dictionary<JointIndex, IReadOnlyBone>();
    foreach (var bone in skeleton.Bones) {
      var boneName = bone.Name;
      var indexOfColon = boneName?.IndexOf(':') ?? -1;
      if (indexOfColon == -1) {
        continue;
      }

      if (Enum.TryParse(boneName![..indexOfColon], out JointIndex jointIndex)) {
        bonesByJointIndex[jointIndex] = bone;
      }
    }

    this.leftUpperArmBone_ = bonesByJointIndex[JointIndex.UPPER_ARM_1];
    this.leftForearmBone_ = bonesByJointIndex[JointIndex.FOREARM_1];
    this.leftHandBone_ = bonesByJointIndex[JointIndex.HAND_1];
    this.rightUpperArmBone_ = bonesByJointIndex[JointIndex.UPPER_ARM_0];
    this.rightForearmBone_ = bonesByJointIndex[JointIndex.FOREARM_0];
    this.rightHandBone_ = bonesByJointIndex[JointIndex.HAND_0];

    {
      var upperArmLength
          = this.leftForearmBone_.LocalTransform.Translation.Length();
      var forearmLength
          = this.leftHandBone_.LocalTransform.Translation.Length();

      var totalArmLength = upperArmLength + forearmLength;

      this.gameManager_ = new BallGameManager(ballCount, 1);

      this.baseBallDistance_
          = this.leftUpperArmBone_.LocalTransform.Translation.X;

      var minBallDistance = totalArmLength * .32f;
      var maxBallDistance = totalArmLength * .76f;

      this.ballDistances_
          = Enumerable.Range(0, (int) ballCount)
                      .Select(i => FinMath.MapRange(i,
                                                    0,
                                                    ballCount - 1,
                                                    minBallDistance,
                                                    maxBallDistance))
                      .ToArray();
      this.armAngles_
          = this.ballDistances_
                .Select(ballDistance => GetAnglesFromLegsOfTriangle_(
                            ballDistance,
                            upperArmLength,
                            forearmLength))
                .ToArray();
    }

    {
      var minMaxBoundsCalculator = new ModelMinMaxBoundsScaleCalculator();
      var bounds = minMaxBoundsCalculator.CalculateBounds(model);

      this.minBallArcY_ = bounds.MaxY;
      this.maxBallArcY_ = this.minBallArcY_ +
                          this.ballDistances_.Last() -
                          this.ballDistances_.First();

      this.ballBaseY_ = this.minBallArcY_ / 2;
    }
  }

  private static (float upperArmRadians, float forearmRadians)
      GetAnglesFromLegsOfTriangle_(float ballDistance,
                                   float upperArmLength,
                                   float forearmLength)
    => (CalculateLawOfSignsRadians_(forearmLength,
                                    ballDistance,
                                    upperArmLength),
        CalculateLawOfSignsRadians_(upperArmLength,
                                    ballDistance,
                                    forearmLength));

  private static float CalculateLawOfSignsRadians_(
      float oppositeSideLength,
      float otherSideLength0,
      float otherSideLength1) {
    var numerator = otherSideLength0 * otherSideLength0 +
                    otherSideLength1 * otherSideLength1 -
                    oppositeSideLength * oppositeSideLength;
    var denominator = 2 * otherSideLength0 * otherSideLength1;

    var angle = MathF.Acos(numerator / denominator);

    return angle;
  }

  ~BallGameComponent() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.ballRenderer_?.Dispose();

  public void Tick(ISceneNodeInstance self) {
    this.gameManager_.Tick();
    var gameState = this.gameManager_.GameState;

    {
      var currentLeftDown = MainViewInputService.AKeyDown;
      var currentRightDown = MainViewInputService.DKeyDown;

      var leftPressed = !this.previousLeftDown_ && currentLeftDown;
      if (leftPressed) {
        gameState.MoveLeft();
      }

      var rightPressed = !this.previousRightDown_ && currentRightDown;
      if (rightPressed) {
        gameState.MoveRight();
      }

      this.previousLeftDown_ = currentLeftDown;
      this.previousRightDown_ = currentRightDown;
    }

    var leftArmAngles = this.armAngles_[gameState.LeftHandPosition];
    this.boneTransformView_.OverrideWorldRotation(
        this.leftUpperArmBone_,
        QuaternionUtil.CreateZyxRadians(0,
                                        leftArmAngles.upperArmRadians,
                                        MathF.PI));
    this.boneTransformView_.OverrideWorldRotation(
        this.leftForearmBone_,
        QuaternionUtil.CreateZyxRadians(0,
                                        -leftArmAngles.forearmRadians,
                                        MathF.PI));
    this.boneTransformView_.OverrideWorldRotation(
        this.leftHandBone_,
        QuaternionUtil.CreateZyxRadians(-MathF.PI / 2, 0, MathF.PI));

    var rightArmAngles = this.armAngles_[gameState.RightHandPosition];
    this.boneTransformView_.OverrideWorldRotation(
        this.rightUpperArmBone_,
        QuaternionUtil.CreateZyxRadians(0, rightArmAngles.upperArmRadians, 0));
    this.boneTransformView_.OverrideWorldRotation(
        this.rightForearmBone_,
        QuaternionUtil.CreateZyxRadians(0, -rightArmAngles.forearmRadians, 0));
    this.boneTransformView_.OverrideWorldRotation(
        this.rightHandBone_,
        QuaternionUtil.CreateZyxRadians(-MathF.PI / 2, 0, 0));
  }

  public void Render(ISceneNodeInstance self) {
    var ballStates = this.gameManager_.BallStates;
    foreach (var ballState in ballStates) {
      GlTransform.PushMatrix();

      var ballDistanceX = this.baseBallDistance_ +
                          this.ballDistances_[ballState.Index];
      var ballDistanceY = float.Lerp(this.minBallArcY_,
                                     this.maxBallArcY_,
                                     1f * ballState.Index / ballStates.Count) -
                          this.ballBaseY_;

      var progress0To1 = ballState.AirSteppedProgress;

      var radians = MathF.PI * progress0To1;
      var x = ballDistanceX *
                    MathF.Cos(radians) *
                    ballState.Direction switch {
                        BallDirection.LEFT  => -1,
                        BallDirection.RIGHT => 1,
                    };
      var y = this.ballBaseY_ + ballDistanceY * MathF.Sin(radians);

      GlTransform.Translate(x, y, 0);
      this.ballRenderer_.Render();

      GlTransform.PopMatrix();
    }
  }
}