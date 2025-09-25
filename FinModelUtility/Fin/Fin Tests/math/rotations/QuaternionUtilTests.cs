using System;
using System.Numerics;

using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;


namespace fin.math.rotations;

public sealed class QuaternionUtilTests {
  [Test]
  [TestCase(0, 0, 0, 0, 0, 0, 1)]
  [TestCase(2 * MathF.PI, 2 * MathF.PI, 2 * MathF.PI, 0, 0, 0, -1)]
  [TestCase(1, 2, 3, -.71828705f, .31062245f, .44443506f, .43595284f)]
  [TestCase(2, 3, 4, -.5148352f, -.17015748f, .38405117f, .7473258f)]
  [TestCase(-1.6628352f, 0.42798066f, -0.3666214f, -0.6838529f, 0.27231407f, 0.034253977f, 0.67603034f)]
  public void TestCreateZyx(float xRadians,
                            float yRadians,
                            float zRadians,
                            float expectedQX,
                            float expectedQY,
                            float expectedQZ,
                            float expectedQW) {
    var tolerance = .0000001f;

    var actualQuaternion =
        QuaternionUtil.CreateZyxRadians(xRadians, yRadians, zRadians);

    Assert.AreEqual(expectedQX, actualQuaternion.X, tolerance);
    Assert.AreEqual(expectedQY, actualQuaternion.Y, tolerance);
    Assert.AreEqual(expectedQZ, actualQuaternion.Z, tolerance);
    Assert.AreEqual(expectedQW, actualQuaternion.W, tolerance);
  }

  [Test]
  [TestCase(0, 0, 0, 1, 1, 1, 1, 1, 0.00001f, 0.00001f, 0.00001f, 1)]
  [TestCase(1, 2, 3, 4, 5, 6, 7, 8, 1.0000399f, 2.00004f, 3.00004f, 4.00004f)]
  public void TestSlowButConsistentSlerp(float fromQx,
                                         float fromQy,
                                         float fromQz,
                                         float fromQw,
                                         float toQx,
                                         float toQy,
                                         float toQz,
                                         float toQw,
                                         float expectedQx,
                                         float expectedQy,
                                         float expectedQz,
                                         float expectedQw) {
    var tolerance = .0000001f;

    var actualQuaternion =
        QuaternionUtil.SlowButConsistentSlerp(
            new Quaternion(fromQx, fromQy, fromQz, fromQw),
            new Quaternion(toQx, toQy, toQz, toQw),
            0.00001f);

    Assert.AreEqual(expectedQx, actualQuaternion.X, tolerance);
    Assert.AreEqual(expectedQy, actualQuaternion.Y, tolerance);
    Assert.AreEqual(expectedQz, actualQuaternion.Z, tolerance);
    Assert.AreEqual(expectedQw, actualQuaternion.W, tolerance);
  }

  [Test]
  public void ToEulerRadians() {
    var degToRad = MathF.PI / 180;

    var x = 1 * 20 * degToRad;
    var y = 2 * 20 * degToRad;
    var z = 3 * 20 * degToRad;

    var q = QuaternionUtil.CreateZyxRadians(x, y, z);

    var v = QuaternionUtil.ToEulerRadians(q);

    Asserts.IsRoughly(x, v.X);
    Asserts.IsRoughly(y, v.Y);
    Asserts.IsRoughly(z, v.Z);
  }

  [Test]
  public void ToEulerRadiansIdentity() {
    var v = QuaternionUtil.ToEulerRadians(Quaternion.Identity);

    Asserts.IsRoughly(0, v.X);
    Asserts.IsRoughly(0, v.Y);
    Asserts.IsRoughly(0, v.Z);
  }
}