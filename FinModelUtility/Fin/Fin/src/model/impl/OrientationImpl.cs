using System;
using System.Numerics;

using fin.math.rotations;

namespace fin.model.impl;
public sealed class RotationImpl : IRotation {
  private const float DEG_2_RAD_ = MathF.PI / 180;
  private const float RAD_2_DEG_ = 1 / DEG_2_RAD_;

  public float XDegrees => this.XRadians * RAD_2_DEG_;
  public float YDegrees => this.YRadians * RAD_2_DEG_;
  public float ZDegrees => this.ZRadians * RAD_2_DEG_;

  public IRotation SetDegrees(float x, float y, float z)
    => this.SetRadiansImpl_(x * DEG_2_RAD_,
                            y * DEG_2_RAD_,
                            z * DEG_2_RAD_);

  public float XRadians { get; private set; }
  public float YRadians { get; private set; }
  public float ZRadians { get; private set; }

  public IRotation SetRadians(float x, float y, float z)
    => this.SetRadiansImpl_(x, y, z);

  private RotationImpl SetRadiansImpl_(float x, float y, float z) {
    this.XRadians = x;
    this.YRadians = y;
    this.ZRadians = z;
    return this;
  }

  public IRotation SetQuaternion(in Quaternion q) {
    var eulerRadians = QuaternionUtil.ToEulerRadians(q);
    return this.SetRadians(eulerRadians.X, eulerRadians.Y, eulerRadians.Z);
  }

  public override string ToString() =>
      $"{{{this.XDegrees}°, {this.YDegrees}°, {this.ZDegrees}°}}";
}