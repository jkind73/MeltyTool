using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Media.Imaging;

using marioartisttool.util;

namespace MarioArtistTool.file_select;

public sealed class BucketBitmapObservableManager {
  private static readonly Bitmap IDLE_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/idle.png");

  private static readonly Bitmap WAVE_0_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_0.png");

  private static readonly Bitmap WAVE_1_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_1.png");

  private static readonly Bitmap WAVE_2_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_2.png");

  private static readonly Bitmap WAVE_3_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/wave_3.png");

  private static readonly Bitmap OPEN_0_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/open_0.png");

  private static readonly Bitmap OPEN_1_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/open_1.png");

  private static readonly Bitmap OPEN_2_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/open_2.png");

  private static readonly Bitmap OPEN_3_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/open_3.png");

  private static readonly Bitmap OPEN_4_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/open_4.png");

  private static readonly Bitmap OPEN_5_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/open_5.png");

  private static readonly Bitmap OPEN_6_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/open_6.png");

  private static readonly Bitmap HAT_IMAGE_
      = AssetLoaderUtil.LoadBitmap("bucket/hat.png");

  public BucketBitmapState CurrentState { get; private set; }
    = BucketBitmapState.IDLE;

  private CancellationTokenSource? lastCancellationTokenSource_;

  public ReplaySubject<Bitmap> BucketImage { get; } = new(1);
  public ReplaySubject<Bitmap?> HatImage { get; } = new(1);

  public BucketBitmapObservableManager() {
    this.BucketImage.OnNext(IDLE_IMAGE_);
    this.HatImage.OnNext(HAT_IMAGE_);
  }

  public bool IsMouseOver {
    get;
    set {
      field = value;
      this.UpdateState_();
    }
  }

  public bool IsOpen {
    get;
    set {
      field = value;
      this.UpdateState_();
    }
  }

  private void UpdateState_() {
    this.lastCancellationTokenSource_?.Cancel();
    this.lastCancellationTokenSource_?.Dispose();

    var newCancellationTokenSource = new CancellationTokenSource();
    this.lastCancellationTokenSource_ = newCancellationTokenSource;

    var from = this.CurrentState;

    BucketBitmapState to = BucketBitmapState.IDLE;

    if (this.IsMouseOver) {
      to = BucketBitmapState.WAVING;
    }

    if (this.IsOpen) {
      to = BucketBitmapState.OPEN;
    }

    Task.Run(async () => {
      await foreach (var next in BucketBitmapStateUtils.GetPath(
                         from,
                         to,
                         newCancellationTokenSource.Token)) {
        this.CurrentState = next;
        var nextBucketImage = next switch {
            BucketBitmapState.IDLE => IDLE_IMAGE_,
            BucketBitmapState.WAVE_0_IN or BucketBitmapState.WAVE_0_OUT
                => WAVE_0_IMAGE_,
            BucketBitmapState.WAVE_1_IN or BucketBitmapState.WAVE_1_OUT
                => WAVE_1_IMAGE_,
            BucketBitmapState.WAVE_2_IN or BucketBitmapState.WAVE_2_OUT
                => WAVE_2_IMAGE_,
            BucketBitmapState.WAVE_3_IN or BucketBitmapState.WAVE_3_OUT
                => WAVE_3_IMAGE_,
            BucketBitmapState.OPEN_0 => OPEN_0_IMAGE_,
            BucketBitmapState.OPEN_1 => OPEN_1_IMAGE_,
            BucketBitmapState.OPEN_2 => OPEN_2_IMAGE_,
            BucketBitmapState.OPEN_3 => OPEN_3_IMAGE_,
            BucketBitmapState.OPEN_4 => OPEN_4_IMAGE_,
            BucketBitmapState.OPEN_5 => OPEN_5_IMAGE_,
            BucketBitmapState.OPEN_6 => OPEN_6_IMAGE_,
        };

        this.BucketImage.OnNext(nextBucketImage);

        var hasHat = !next.IsOpen();
        this.HatImage.OnNext(hasHat ? HAT_IMAGE_ : null);
      }
    },
    newCancellationTokenSource.Token);
  }
}