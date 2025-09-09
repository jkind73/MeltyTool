using System;
using System.Numerics;

using fin.math.floats;
using fin.util.asserts;

using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.math.rotations;

public class QuaternionUtilTests {
  [Test]
  [TestCase(0, 0, 0, 0, 0, 0, 1)]
  [TestCase(2 * MathF.PI, 2 * MathF.PI, 2 * MathF.PI, 0, 0, 0, -1)]
  [TestCase(1, 2, 3, -.71829f, .31061998f, .44443998f, .43594998f)]
  [TestCase(2, 3, 4, -.51484f, -.17016f, .38404998f, .74733f)]
  [TestCase(-1.6628352f, 0.42798066f, -0.3666214f, -0.68385f, 0.27231f, 0.03425f, 0.67603f)]
  public void TestCreateZyx(float xRadians,
                            float yRadians,
                            float zRadians,
                            float expectedQX,
                            float expectedQY,
                            float expectedQZ,
                            float expectedQW) {
    var tolerance = .0000001f;

    var actualQuaternion = QuaternionUtil.CreateZyxRadians(xRadians, yRadians, zRadians);

    Assert.AreEqual(expectedQX, actualQuaternion.X, tolerance);
    Assert.AreEqual(expectedQY, actualQuaternion.Y, tolerance);
    Assert.AreEqual(expectedQZ, actualQuaternion.Z, tolerance);
    Assert.AreEqual(expectedQW, actualQuaternion.W, tolerance);
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