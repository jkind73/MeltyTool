using NUnit.Framework;

using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace fin.color;

public sealed class ColorUtilTests {
  [Test]
  [TestCase(ushort.MinValue, 0, 0, 0)]
  [TestCase((ushort) 128, 0, 16, 0)]
  [TestCase((ushort) 255, 0, 28, 255)]
  [TestCase((ushort) 1000, 0, 124, 66)]
  [TestCase((ushort) 16383, 57, 252, 255)]
  [TestCase((ushort) 32767, 123, 252, 255)]
  [TestCase((ushort) 49151, 189, 252, 255)]
  [TestCase(ushort.MaxValue, 255, 252, 255)]
  public void TestRgb565(ushort value,
                         byte expectedR,
                         byte expectedG,
                         byte expectedB) {
    ColorUtil.SplitRgb565(value,
                          out var actualR,
                          out var actualG,
                          out var actualB);
    Assert.AreEqual((expectedR, expectedG, expectedB),
                    (actualR, actualG, actualB));

    var color = ColorUtil.ParseRgb565(value);
    Assert.AreEqual((color.Rb, color.Gb, color.Bb),
                    (actualR, actualG, actualB));
  }

  [Test]
  [TestCase(ushort.MinValue, 0, 0, 0, 0)]
  [TestCase((ushort) 128, 0, 136, 0, 0)]
  [TestCase((ushort) 255, 0, 255, 255, 0)]
  [TestCase((ushort) 1000, 51, 238, 136, 0)]
  [TestCase((ushort) 16383, 255, 255, 255, 109)]
  [TestCase((ushort) 32767, 255, 255, 255, 255)]
  [TestCase((ushort) 49151, 123, 255, 255, 255)]
  [TestCase(ushort.MaxValue, 255, 255, 255, 255)]
  public void TestRgb5A3(ushort value,
                         byte expectedR,
                         byte expectedG,
                         byte expectedB,
                         byte expectedA) {
    ColorUtil.SplitRgb5A3(value,
                          out var actualR,
                          out var actualG,
                          out var actualB,
                          out var actualA);
    Assert.AreEqual((expectedR, expectedG, expectedB, expectedA),
                    (actualR, actualG, actualB, actualA));

    var color = ColorUtil.ParseRgb5A3(value);
    Assert.AreEqual((color.Rb, color.Gb, color.Bb, color.Ab),
                    (actualR, actualG, actualB, actualA));
  }

  [Test]
  [TestCase(ushort.MinValue, 0, 0, 0, 0)]
  [TestCase((ushort) 128, 0, 33, 0, 0)]
  [TestCase((ushort) 255, 0, 57, 255, 0)]
  [TestCase((ushort) 1000, 0, 255, 66, 0)]
  [TestCase((ushort) 16383, 123, 255, 255, 0)]
  [TestCase((ushort) 32767, 255, 255, 255, 0)]
  [TestCase((ushort) 49151, 123, 255, 255, 255)]
  [TestCase(ushort.MaxValue, 255, 255, 255, 255)]
  public void TestRgb5A1(ushort value,
                         byte expectedR,
                         byte expectedG,
                         byte expectedB,
                         byte expectedA) {
    ColorUtil.SplitRgb5A1(value,
                          out var actualR,
                          out var actualG,
                          out var actualB,
                          out var actualA);
    Assert.AreEqual((expectedR, expectedG, expectedB, expectedA),
                    (actualR, actualG, actualB, actualA));

    var color = ColorUtil.ParseRgb5A1(value);
    Assert.AreEqual((color.Rb, color.Gb, color.Bb, color.Ab),
                    (actualR, actualG, actualB, actualA));
  }
}