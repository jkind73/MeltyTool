using System;

using NUnit.Framework;

namespace fin.math.rotations;

[DefaultFloatingPointTolerance(.0001)]
public sealed class RadiansUtilTests {
  private const float PI_ = MathF.PI;

  [Test]
  [TestCase(0, 0, ExpectedResult = 0f)]
  [TestCase(PI_, PI_, ExpectedResult = 0f)]
  [TestCase(2 * PI_, 0, ExpectedResult = 0f)]
  [TestCase(-2 * PI_, 0, ExpectedResult = 0f)]
  [TestCase(4 * PI_, 0, ExpectedResult = 0f)]
  [TestCase(PI_, -PI_, ExpectedResult = 0f)]
  // Clockwise (positive)
  [TestCase(-PI_ / 4, PI_ / 4, ExpectedResult = PI_ / 2)]
  [TestCase(PI_ / 4, PI_ * 3 / 4, ExpectedResult = PI_ / 2)]
  [TestCase(PI_ * 3 / 4, -PI_ * 3 / 4, ExpectedResult = PI_ / 2)]
  [TestCase(PI_ * 7 / 4, PI_ / 4, ExpectedResult = PI_ / 2)]
  // Counterclockwise (Negative)
  [TestCase(PI_ / 4, -PI_ / 4, ExpectedResult = -PI_ / 2)]
  [TestCase(PI_ * 3 / 4, PI_ / 4, ExpectedResult = -PI_ / 2)]
  [TestCase(-PI_ * 3 / 4, PI_ * 3 / 4, ExpectedResult = -PI_ / 2)]
  [TestCase(PI_ / 4, PI_ * 7 / 4, ExpectedResult = -PI_ / 2)]
  public float TestCalculateRadiansTowardsExact(float from, float to)
    => RadiansUtil.CalculateRadiansTowards(from, to);

  [Test]
  // Clockwise (positive)
  [TestCase(0, PI_, ExpectedResult = PI_)]
  [TestCase(PI_, 2 * PI_, ExpectedResult = PI_)]
  // Counterclockwise (Negative)
  [TestCase(PI_, 0, ExpectedResult = -PI_)]
  [TestCase(2 * PI_, PI_, ExpectedResult = -PI_)]
  public float TestCalculateRadiansTowardsHalfRotation(float from, float to)
    => RadiansUtil.CalculateRadiansTowards(from, to);
}