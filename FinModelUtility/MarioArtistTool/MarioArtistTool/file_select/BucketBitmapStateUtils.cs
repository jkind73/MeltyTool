using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MarioArtistTool.file_select;

public enum BucketBitmapState {
  IDLE,
  WAVE_0_IN,
  WAVE_1_IN,
  WAVE_2_IN,
  WAVE_3_IN,
  WAVE_3_OUT,
  WAVE_2_OUT,
  WAVE_1_OUT,
  WAVE_0_OUT,
  WAVING,
  OPEN_0,
  OPEN_1,
  OPEN_2,
  OPEN_3,
  OPEN_4,
  OPEN_5,
  OPEN_6,
  OPEN,
}

public static class BucketBitmapStateExtensions {
  public static bool IsWaving(this BucketBitmapState state)
    => state is BucketBitmapState.WAVE_0_IN
                or BucketBitmapState.WAVE_1_IN
                or BucketBitmapState.WAVE_2_IN
                or BucketBitmapState.WAVE_3_IN
                or BucketBitmapState.WAVE_3_OUT
                or BucketBitmapState.WAVE_2_OUT
                or BucketBitmapState.WAVE_1_OUT
                or BucketBitmapState.WAVE_0_OUT
                or BucketBitmapState.WAVING;

  public static bool IsOpen(this BucketBitmapState state)
    => state is BucketBitmapState.OPEN_0
                or BucketBitmapState.OPEN_1
                or BucketBitmapState.OPEN_2
                or BucketBitmapState.OPEN_3
                or BucketBitmapState.OPEN_4
                or BucketBitmapState.OPEN_5
                or BucketBitmapState.OPEN_6
                or BucketBitmapState.OPEN;
}

public static class BucketBitmapStateUtils {
  private const float STATE_TIME = .05f;

  public static async IAsyncEnumerable<BucketBitmapState> GetPath(
      BucketBitmapState from,
      BucketBitmapState to,
      CancellationToken cancellationToken) {
    while (!cancellationToken.IsCancellationRequested &&
           TryGetNextState_(from, to, out var next)) {
      yield return next;
      await Task.Delay((int) (STATE_TIME * 1000), cancellationToken);
      from = next;
    }
  }

  private static bool TryGetNextState_(BucketBitmapState from,
                                       BucketBitmapState to,
                                       out BucketBitmapState next) {
    if (from == to) {
      next = to;
      return false;
    }

    // Opening
    if (to == BucketBitmapState.OPEN) {
      to = BucketBitmapState.OPEN_6;
    }

    if (from.IsOpen() && to.IsOpen()) {
      next = from + Math.Sign(to - from);
      return true;
    }

    if (from.IsOpen()) {
      if (from == BucketBitmapState.OPEN_1) {
        next = BucketBitmapState.IDLE;
      } else {
        next = from - 1;
      }

      return true;
    }

    if (from == BucketBitmapState.IDLE && to.IsOpen()) {
      next = BucketBitmapState.OPEN_1;
      return true;
    }

    // Continuously waving
    if (from.IsWaving() && to.IsWaving()) {
      if (from == BucketBitmapState.WAVE_0_OUT) {
        next = BucketBitmapState.WAVE_0_IN;
      } else if (from == BucketBitmapState.WAVE_1_OUT) {
        next = BucketBitmapState.WAVE_1_IN;
      } else {
        next = from + 1;
      }

      return true;
    }

    if (from.IsWaving()) {
      if (from is BucketBitmapState.WAVE_0_OUT) {
        next = BucketBitmapState.IDLE;
      } else {
        next = from + 1;
      }

      return true;
    }

    if (from == BucketBitmapState.IDLE && to.IsWaving()) {
      next = BucketBitmapState.WAVE_0_IN;
      return true;
    }

    throw new NotImplementedException();
  }
}