using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.math.interpolation;

public sealed class HermiteInterpolationUtilTests {
  [Test]
  public void TestGetTangent() {
    var fromTime = 0;
    var fromValue = 5;
    var fromTangent = 5;
    var toTime = 10;
    var toValue = 15;
    var toTangent = -10;

    Assert.AreEqual(2.75f,
                    HermiteInterpolationUtil.GetTangent(
                        fromValue,
                        fromTime,
                        fromTangent,
                        toValue,
                        toTime,
                        toTangent,
                        5));
  }


  [Test]
  public void TestInterpolationStartAndEnd() {
    var fromTime = 1;
    var fromValue = 2;
    var fromTangent = 3;
    var toTime = 4;
    var toValue = 5;
    var toTangent = 6;

    Assert.AreEqual(fromValue,
                    HermiteInterpolationUtil.InterpolateFloats(
                        fromTime,
                        fromValue,
                        fromTangent,
                        toTime,
                        toValue,
                        toTangent,
                        fromTime));

    Assert.AreEqual(toValue,
                    HermiteInterpolationUtil.InterpolateFloats(
                        fromTime,
                        fromValue,
                        fromTangent,
                        toTime,
                        toValue,
                        toTangent,
                        toTime));
  }
}