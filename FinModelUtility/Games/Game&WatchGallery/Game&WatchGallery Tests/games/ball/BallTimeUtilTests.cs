namespace gawg.games.ball;

public class BallTimeUtilTests {
  [Test]
  [TestCase(0, 3, ExpectedResult = 0)]
  [TestCase(1, 3, ExpectedResult = 0)]
  [TestCase(2, 3, ExpectedResult = 0)]
  [TestCase(3, 3, ExpectedResult = 1)]
  [TestCase(4, 3, ExpectedResult = 1)]
  public int GetsExpectedAdjustedStep(
      int elapsedTicks,
      int ballCount)
    => (int) BallTimeUtil.GetAdjustedStep(
        (ulong) elapsedTicks,
        (uint) ballCount);

  [Test]
  // Remains constant for a single tick
  [TestCase(0, 9, 3, ExpectedResult = 0f)]
  [TestCase(1, 9, 3, ExpectedResult = 0f)]
  [TestCase(2, 9, 3, ExpectedResult = 0f)]
  // Increases as expected
  [TestCase(3, 9, 3, ExpectedResult = 1 / 8f)]
  [TestCase(6, 9, 3, ExpectedResult = 2 / 8f)]
  [TestCase(9, 9, 3, ExpectedResult = 3 / 8f)]
  [TestCase(12, 9, 3, ExpectedResult = 4 / 8f)]
  [TestCase(15, 9, 3, ExpectedResult = 5 / 8f)]
  [TestCase(18, 9, 3, ExpectedResult = 6 / 8f)]
  [TestCase(21, 9, 3, ExpectedResult = 7 / 8f)]
  [TestCase(24, 9, 3, ExpectedResult = 1f)]
  // Clamped on end
  [TestCase(50, 9, 3, ExpectedResult = 1f)]
  public float GetsExpectedAdjustedSteppedProgress(
      int elapsedTicks,
      int tickDuration,
      int ballCount)
    => BallTimeUtil.GetAdjustedSteppedProgress(
        (ulong) elapsedTicks,
        (ulong) tickDuration,
        (uint) ballCount);
}