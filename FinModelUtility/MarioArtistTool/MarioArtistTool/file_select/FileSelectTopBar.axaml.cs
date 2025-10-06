using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Media.Imaging;

using marioartisttool.services;
using marioartisttool.util;

namespace MarioArtistTool.file_select;

public partial class FileSelectTopBar : UserControl {
  private CancellationTokenSource? lastCancellationTokenSource_;

  private const float STATE_TIME = .1f;

  private static readonly Bitmap IDLE_IMAGE_
      = AssetLoaderUtil.LoadBitmap("file_select/top_bar/disk_swap/idle.png");

  private static readonly Bitmap[] ANIM_IMAGES_
      = AssetLoaderUtil.LoadBitmaps(
          i => $"file_select/top_bar/disk_swap/anim_{i}.png",
          6);

  public BehaviorSubject<Bitmap> DiskSwapImage { get; } = new(IDLE_IMAGE_);

  private bool DiskSwapMouseOver {
    get;
    set {
      if (value == field) {
        return;
      }

      field = value;
      this.UpdateDiskSwapAnimation_();
    }
  }

  public FileSelectTopBar() {
    InitializeComponent();

    this.DiskSwapButton.PointerEntered
        += (_, _) => this.DiskSwapMouseOver = true;
    this.DiskSwapButton.PointerExited
        += (_, _) => this.DiskSwapMouseOver = false;
    this.DiskSwapButton.Click += (_, _)
        => MfsFileSystemService.PromptUserForDiskFileAndLoadIfValid();

    this.DiskSwapIcon.Bind(Image.SourceProperty, this.DiskSwapImage);
  }

  private void UpdateDiskSwapAnimation_() {
    this.lastCancellationTokenSource_?.Cancel();
    this.lastCancellationTokenSource_?.Dispose();

    if (!this.DiskSwapMouseOver) {
      this.DiskSwapImage.OnNext(IDLE_IMAGE_);
      this.lastCancellationTokenSource_ = null;
      return;
    }

    var newCancellationTokenSource = new CancellationTokenSource();
    this.lastCancellationTokenSource_ = newCancellationTokenSource;

    var cancellationToken = newCancellationTokenSource.Token;

    Task.Run(
        async () => {
          var i = 0;
          while (true) {
            var image = ANIM_IMAGES_[i];
            i = (i + 1) % ANIM_IMAGES_.Length;

            this.DiskSwapImage.OnNext(image);

            await Task.Delay((int) (STATE_TIME * 1000), cancellationToken);
          }
        },
        cancellationToken);
  }
}