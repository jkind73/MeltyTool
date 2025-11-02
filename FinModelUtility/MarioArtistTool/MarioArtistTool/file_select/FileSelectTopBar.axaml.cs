using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;

using fin.ui.avalonia.observables;

using marioartisttool.services;
using marioartisttool.util;

namespace MarioArtistTool.file_select;

public partial class FileSelectTopBar : UserControl {
  private static readonly LoopingObservable<Cursor> GRAB_CURSOR_;

  static FileSelectTopBar() {
    var grabOrigin = new PixelPoint(2, 2);
    var grabCursor0 = AssetLoaderUtil.LoadCursor("grab_0.png", grabOrigin);
    var grabCursor1 = AssetLoaderUtil.LoadCursor("grab_1.png", grabOrigin);
    var grabCursor2 = AssetLoaderUtil.LoadCursor("grab_2.png", grabOrigin);

    GRAB_CURSOR_ = new LoopingObservable<Cursor>(.1f,
                                                 grabCursor0,
                                                 grabCursor1,
                                                 grabCursor2,
                                                 grabCursor2,
                                                 grabCursor1);
  }

  public FileSelectTopBar() {
    InitializeComponent();

    AnimateButton_(this.DiskSwapButton,
                   this.DiskSwapIcon,
                   AssetLoaderUtil.LoadBitmap(
                       "file_select/top_bar/disk_swap/idle.png"),
                   AssetLoaderUtil.LoadBitmaps(
                       i => $"file_select/top_bar/disk_swap/anim_{i}.png",
                       6));

    this.DiskSwapButton.Click += async (_, _) => {
      var originalBackground = this.DiskSwapButton.Background;
      var originalHeight = this.DiskSwapIcon.Height;

      this.DiskSwapButton.Background
          = new SolidColorBrush(Color.FromRgb(9, 56, 1));
      this.DiskSwapIcon.Height = 18;

      await MfsFileSystemService.PromptUserForDiskFileAndLoadIfValid();

      this.DiskSwapButton.Background = originalBackground;
      this.DiskSwapIcon.Height = originalHeight;
    };

    AnimateButton_(this.DiskSwapButton,
                   this.DiskSwapIcon,
                   AssetLoaderUtil.LoadBitmap(
                       "file_select/top_bar/disk_swap/idle.png"),
                   AssetLoaderUtil.LoadBitmaps(
                       i => $"file_select/top_bar/disk_swap/anim_{i}.png",
                       6));

    this.DiskSwapButton.Click += async (_, _) => {
      var originalBackground = this.DiskSwapButton.Background;

      this.DiskSwapButton.Background
          = new SolidColorBrush(Color.FromRgb(9, 56, 1));
      this.DiskSwapIcon.Width = this.DiskSwapIcon.Height = 18;

      await MfsFileSystemService.PromptUserForDiskFileAndLoadIfValid();

      this.DiskSwapButton.Background = originalBackground;
      this.DiskSwapIcon.Width = this.DiskSwapIcon.Height = 20;
    };
  }

  private static void AnimateButton_(
      Button button,
      Image icon,
      Bitmap idleBitmap,
      Bitmap[] hoverBitmaps) {
    button.Bind(Button.CursorProperty, GRAB_CURSOR_);

    var pointerOverObservable = button.GetObservable(Button.IsPointerOverProperty);
    var focusObservable = button.GetObservable(Button.IsFocusedProperty);

    var currentBitmap =
        Observable.CombineLatest(pointerOverObservable, focusObservable)
                  .Select(states => {
                    var pointerOver = states[0];
                    var focus = states[1];

                    if (!pointerOver && !focus) {
                      return Observable.Return(idleBitmap);
                    }


                    return new LoopingObservable<Bitmap>(.1f, hoverBitmaps);
                  })
                  .Switch();
    icon.Bind(Image.SourceProperty, currentBitmap);
  }
}