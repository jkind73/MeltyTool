using System;
using System.Numerics;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.math.rotations;

public sealed class FinTrigTests {
  [Test]
  [TestCase(-.5f * MathF.PI)]
  [TestCase(0)]
  [TestCase(.5f * MathF.PI)]
  [TestCase(MathF.PI)]
  [TestCase(1.5f * MathF.PI)]
  [TestCase(2 * MathF.PI)]
  [TestCase(2.5f * MathF.PI)]
  public void TestSin(float radians)
    => Asserts.IsRoughly(MathF.Sin(radians), FinTrig.Sin(radians));

  [Test]
  [TestCase(-.5f * MathF.PI)]
  [TestCase(0)]
  [TestCase(.5f * MathF.PI)]
  [TestCase(MathF.PI)]
  [TestCase(1.5f * MathF.PI)]
  [TestCase(2 * MathF.PI)]
  [TestCase(2.5f * MathF.PI)]
  public void TestCos(float radians)
    => Asserts.IsRoughly(MathF.Cos(radians), FinTrig.Cos(radians));

  [Test]
  [TestCase(-1)]
  [TestCase(-.5f)]
  [TestCase(0)]
  [TestCase(.5f)]
  [TestCase(1)]
  public void TestAsin(float radians)
    => Asserts.IsRoughly(MathF.Asin(radians), FinTrig.Asin(radians));

  [Test]
  [TestCase(-1)]
  [TestCase(-.5f)]
  [TestCase(0)]
  [TestCase(.5f)]
  [TestCase(1)]
  public void TestAcos(float radians)
    => Asserts.IsRoughly(MathF.Acos(radians), FinTrig.Acos(radians));

  [Test]
  [TestCase(-1, -1)]
  [TestCase(-1, 1)]
  [TestCase(1, -1)]
  [TestCase(1, 1)]
  public void TestAtan2(float x, float y)
    => Asserts.IsRoughly(MathF.Atan2(y, x), FinTrig.Atan2(y, x));

  [Test]
  [TestCase(0.5f, 0.5f, .707106781f, 45, 45)]
  [TestCase(-0.5f, 0.5f, .707106781f, 45, 135)]
  [TestCase(0.5f, -0.5f, .707106781f, 45, -45)]
  [TestCase(-0.5f, -0.5f, .707106781f, 45, -135)]
  [TestCase(0.5f, 0.5f, -.707106781f, -45, 45)]
  public void TestPointTowardsNormal_Angles(
      float xNormal,
      float yNormal,
      float zNormal,
      float expectedPitchDegrees,
      float expectedYawDegrees) {
    var normal = new Vector3(xNormal, yNormal, zNormal);
    var (actualPitchDegrees, actualYawDegrees)
        = FinTrig.GetPitchYawDegreesFromTowards(Vector3.Zero, normal, 0);

    Assert.AreEqual(expectedPitchDegrees, actualPitchDegrees);
    Assert.AreEqual(expectedYawDegrees, actualYawDegrees);
  }

  [Test]
  [TestCase(.5f, .5f, 0.707106781f)]
  public void TestPointTowardsNormal_CanGetSameNormal(
      float xNormal,
      float yNormal,
      float zNormal) {
    var expectedNormal = new Vector3(xNormal, yNormal, zNormal);
    var (pitchDegrees, yawDegrees)
        = FinTrig.GetPitchYawDegreesFromTowards(
            Vector3.Zero,
            expectedNormal,
            0);

    FinTrig.FromPitchYawDegrees(
        pitchDegrees,
        yawDegrees,
        out var actualXNormal,
        out var actualYNormal,
        out var actualZNormal);
    var actualNormal = new Vector3(actualXNormal, actualYNormal, actualZNormal);

    Asserts.IsRoughly(expectedNormal, actualNormal);
  }

  [Test]
  [TestCase(.5f, .5f, 0.707106781f, 3.14f)]
  [TestCase(-.5f, .5f, 0.707106781f, 3.14f)]
  [TestCase(.5f, -.5f, 0.707106781f, 3.14f)]
  [TestCase(-.5f, -.5f, 0.707106781f, 3.14f)]
  [TestCase(.5f, .5f, -0.707106781f, 3.14f)]
  public void TestPointTowardsNormal_CanGetSameNormalViaQuaternion(
      float xNormal,
      float yNormal,
      float zNormal,
      float rollRadians) {
    var expectedNormal = new Vector3(xNormal, yNormal, zNormal);

    var quaternion = FinTrig.GetQuaternionTowards(expectedNormal, rollRadians);
    var matrix = Matrix4x4.CreateFromQuaternion(quaternion);

    var actualNormal = Vector3.TransformNormal(new Vector3(1, 0, 0), matrix);

    Asserts.IsRoughly(expectedNormal, actualNormal);
  }

  [Test]
  [TestCase(.5f, .5f, 0.707106781f, 3.14f)]
  [TestCase(-.5f, .5f, 0.707106781f, 3.14f)]
  [TestCase(.5f, -.5f, 0.707106781f, 3.14f)]
  [TestCase(-.5f, -.5f, 0.707106781f, 3.14f)]
  [TestCase(.5f, .5f, -0.707106781f, 3.14f)]
  public void TestPointTowardsNormal_CanConvertToUnitYSpace(
      float xNormal,
      float yNormal,
      float zNormal,
      float rollRadians) {
    var inNormal = new Vector3(xNormal, yNormal, zNormal);

    var quaternion = FinTrig.GetQuaternionTowards(inNormal, rollRadians);
    quaternion = FinTrig.ConvertFromZUpToYUp(quaternion);
    var matrix = Matrix4x4.CreateFromQuaternion(quaternion);

    var expectedNormal = new Vector3(inNormal.X, inNormal.Z, -inNormal.Y);
    var actualNormal = Vector3.TransformNormal(new Vector3(1, 0, 0), matrix);

    Asserts.IsRoughly(expectedNormal, actualNormal);
  }
}