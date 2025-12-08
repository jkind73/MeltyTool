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
  [TestCase(0 * 8 + 0, 8, 3, ExpectedResult = 0f)]
  [TestCase(0 * 8 + 1, 8, 3, ExpectedResult = 0f)]
  [TestCase(0 * 8 + 2, 8, 3, ExpectedResult = 0f)]
  [TestCase(1 * 8 + 0, 8, 3, ExpectedResult = 1 / 7f)]
  [TestCase(1 * 8 + 1, 8, 3, ExpectedResult = 1 / 7f)]
  [TestCase(1 * 8 + 2, 8, 3, ExpectedResult = 1 / 7f)]
  [TestCase(2 * 8 + 0, 8, 3, ExpectedResult = 2 / 7f)]
  public float GetsExpectedAdjustedSteppedProgress(
      int elapsedTicks,
      int tickDuration,
      int ballCount)
    => BallTimeUtil.GetAdjustedSteppedProgress(
        (ulong) elapsedTicks,
        (ulong) tickDuration,
        (uint) ballCount);
}