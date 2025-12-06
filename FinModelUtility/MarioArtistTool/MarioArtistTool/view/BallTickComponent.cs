using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.config.avalonia.services;
using fin.math;
using fin.math.rotations;
using fin.model;
using fin.model.skeleton;
using fin.scene;
using fin.ui.rendering.gl;

using gawg.games.ball;

using marioartist.api;

namespace MarioArtistTool.view;

public class BallTickComponent
    : ISceneNodeTickComponent, ISceneNodeRenderComponent {
  private readonly BallGameManager gameManager_;
  private readonly BallRenderer ballRenderer_ = new(32);

  private readonly SimpleBoneTransformView boneTransformView_;

  private readonly IReadOnlyBone leftUpperArmBone_;
  private readonly IReadOnlyBone leftForearmBone_;
  private readonly IReadOnlyBone leftHandBone_;
  private readonly IReadOnlyBone rightUpperArmBone_;
  private readonly IReadOnlyBone rightForearmBone_;
  private readonly IReadOnlyBone rightHandBone_;

  private readonly float[] ballDistances_;
  private readonly float[] ballHeights_;
  private readonly (float upperArmRadians, float forearmRadians)[] armAngles_;

  private bool previousLeftDown_;
  private bool previousRightDown_;

  public BallTickComponent(SimpleBoneTransformView boneTransformView,
                           IReadOnlySkeleton skeleton,
                           uint ballCount) {
    this.boneTransformView_ = boneTransformView;

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
      this.ballHeights_ = Enumerable.Range(0, (int) ballCount)
                                    .Select(i => 50f + 32f * i)
                                    .ToArray();
      this.armAngles_
          = this.ballDistances_
                .Select(ballDistance => GetAnglesFromLegsOfTriangle_(
                            ballDistance,
                            upperArmLength,
                            forearmLength))
                .ToArray();
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

  ~BallTickComponent() => this.ReleaseUnmanagedResources_();

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
    foreach (var ballState in this.gameManager_.BallStates) {
      GlTransform.PushMatrix();

      var ballDistance = this.ballDistances_[ballState.Distance];

      var progress0To1 = ballState.InAirEvent.Progress;
      var progressMinus1To1 = 2 * (progress0To1 - .5f);

      var xPosition = ballDistance *
                      progressMinus1To1 *
                      ballState.Direction switch {
                          BallDirection.LEFT  => -1,
                          BallDirection.RIGHT => 1,
                      };

      var yPosition = (1 - progressMinus1To1 * progressMinus1To1) *
                      this.ballHeights_[ballState.Distance];
      
      var position = new Vector3(xPosition, yPosition, 0);

      GlTransform.Translate(position);
      this.ballRenderer_.Render();

      GlTransform.PopMatrix();
    }
  }
}