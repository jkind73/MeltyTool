using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;

using fin.config.avalonia.services;
using fin.io;
using fin.model.io;
using fin.ui.avalonia.observables;

using MarioArtistTool.config;

using marioartisttool.services;

using MarioArtistTool.services;

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

  private IModelFileBundle[] allModelFileBundles_ = [];

  public FileSelectTopBar() {
    InitializeComponent();

    {
      AnimateButton_(this.DiskSwapButton,
                     this.DiskSwapIcon,
                     AssetLoaderUtil.LoadBitmap(
                         "file_select/top_bar/disk_swap/idle.png"),
                     AssetLoaderUtil.LoadBitmaps(
                         i => $"file_select/top_bar/disk_swap/anim_{i}.png",
                         6),
                     null);

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
    }

    {
      AnimateButton_(this.ExportAllButton,
                     this.ExportAllIcon,
                     null,
                     [AssetLoaderUtil.LoadBitmap("file_select/top_bar/export_all/hover.png")],
                     AssetLoaderUtil.LoadBitmap("file_select/top_bar/export_all/disabled.png"));

      this.ExportAllButton.Click += async (_, _) => {
        var storageProvider = TopLevelService.Instance.StorageProvider;
        var config = Config.INSTANCE;

        var mostRecentDirectory = config.MostRecentOutputDirectory ?? "./";

        var startLocation
            = await storageProvider
                .TryGetFolderFromPathAsync(mostRecentDirectory);

        var selectedStorageFolders
            = await storageProvider
                .OpenFolderPickerAsync(new FolderPickerOpenOptions {
                    SuggestedStartLocation = startLocation,
                    Title = "Select output directory",
                });
        if (selectedStorageFolders is not { Count: 1 }) {
          return;
        }

        var selectedStorageFolder = selectedStorageFolders[0];
        var outputDirectory = new FinDirectory(selectedStorageFolder.Path.LocalPath);

        config.MostRecentOutputDirectory = outputDirectory.FullPath;
        config.Save();

        ExportService.ExportBundles(this.allModelFileBundles_, outputDirectory);
      };
    }

    {
      AnimateButton_(this.ExportCurrentButton,
                     this.ExportCurrentIcon,
                     null,
                     [AssetLoaderUtil.LoadBitmap("file_select/top_bar/export_current/hover.png")],
                     AssetLoaderUtil.LoadBitmap("file_select/top_bar/export_current/disabled.png"));

      this.ExportCurrentButton.Click += async (_, _) => {

      };
    }
  }

  private static void AnimateButton_(
      Button button,
      Image icon,
      Bitmap? idleBitmap,
      Bitmap[] hoverBitmaps,
      Bitmap? disabledBitmap) {
    button.Bind(Button.CursorProperty, GRAB_CURSOR_);

    var isEnabledObservable = button.GetObservable(Button.IsEnabledProperty);
    var isFocusedObservable = button.GetObservable(Button.IsFocusedProperty);
    var isPointerOverObservable = button.GetObservable(Button.IsPointerOverProperty);

    var currentBitmap =
        Observable.CombineLatest(isEnabledObservable,
                                 isFocusedObservable,
                                 isPointerOverObservable)
                  .Select(states => {
                    var isEnabled = states[0];
                    var isFocused = states[1];
                    var isPointerOver = states[2];

                    if (!isEnabled) {
                      return Observable.Return(disabledBitmap);
                    }

                    if (!isPointerOver && !isFocused) {
                      return Observable.Return(idleBitmap);
                    }

                    if (hoverBitmaps.Length == 1) {
                      return Observable.Return(hoverBitmaps[0]);
                    }

                    return new LoopingObservable<Bitmap>(.1f, hoverBitmaps);
                  })
                  .Switch();
    icon.Bind(Image.SourceProperty, currentBitmap);
  }
}