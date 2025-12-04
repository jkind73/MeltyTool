using System;
using System.Collections.Generic;

using fin.config.avalonia.services;
using fin.math;
using fin.math.rotations;
using fin.model;
using fin.model.skeleton;
using fin.scene;

using marioartist.api;

namespace MarioArtistTool.view;

public class BallTickComponent : ISceneNodeTickComponent {
  private readonly SimpleBoneTransformView boneTransformView_;

  private readonly IReadOnlyBone leftUpperArmBone_;
  private readonly IReadOnlyBone leftForearmBone_;
  private readonly IReadOnlyBone leftHandBone_;
  private readonly IReadOnlyBone rightUpperArmBone_;
  private readonly IReadOnlyBone rightForearmBone_;
  private readonly IReadOnlyBone rightHandBone_;

  private readonly float[] ballDistances_ = new float[3];

  private readonly (float upperArmRadians, float forearmRadians)[]
      armAngles_ = new (float, float)[3];

  private uint leftArmDistance_;

  private bool previousLeftDown_;
  private bool previousRightDown_;

  public void Dispose() { }

  public BallTickComponent(SimpleBoneTransformView boneTransformView,
                           IReadOnlySkeleton skeleton) {
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

      this.ballDistances_[0] = totalArmLength * .32f;
      this.ballDistances_[1] = totalArmLength * .54f;
      this.ballDistances_[2] = totalArmLength * .76f;

      this.armAngles_[0] = GetAnglesFromLegsOfTriangle_(this.ballDistances_[0],
        upperArmLength,
        forearmLength);
      this.armAngles_[1] = GetAnglesFromLegsOfTriangle_(this.ballDistances_[1],
        upperArmLength,
        forearmLength);
      this.armAngles_[2] = GetAnglesFromLegsOfTriangle_(this.ballDistances_[2],
        upperArmLength,
        forearmLength);
    }
  }

  public void Tick(ISceneNodeInstance self) {
    {
      var currentLeftDown = MainViewInputService.AKeyDown;
      var currentRightDown = MainViewInputService.DKeyDown;

      var leftPressed = !this.previousLeftDown_ && currentLeftDown;
      var rightPressed = !this.previousRightDown_ && currentRightDown;

      var leftwardChange = (leftPressed ? 1 : 0) + (rightPressed ? -1 : 0);
      this.leftArmDistance_
          = (uint) (this.leftArmDistance_ + leftwardChange).Clamp(0, 2);

      this.previousLeftDown_ = currentLeftDown;
      this.previousRightDown_ = currentRightDown;
    }

    var leftArmAngles = this.armAngles_[this.leftArmDistance_];
    this.boneTransformView_.OverrideWorldRotation(
        this.leftUpperArmBone_,
        QuaternionUtil.CreateZyxRadians(0, leftArmAngles.upperArmRadians, MathF.PI));
    this.boneTransformView_.OverrideWorldRotation(
        this.leftForearmBone_,
        QuaternionUtil.CreateZyxRadians(0, -leftArmAngles.forearmRadians, MathF.PI));
    this.boneTransformView_.OverrideWorldRotation(
        this.leftHandBone_,
        QuaternionUtil.CreateZyxRadians(-MathF.PI / 2, 0, MathF.PI));

    var rightArmDistance = 2 - this.leftArmDistance_;
    var rightArmAngles = this.armAngles_[rightArmDistance];
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
}