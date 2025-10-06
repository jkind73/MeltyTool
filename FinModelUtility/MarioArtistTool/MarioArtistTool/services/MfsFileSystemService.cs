using System;
using System.Threading.Tasks;

using Avalonia.Platform.Storage;

using fin.config.avalonia.services;
using fin.io;
using fin.io.web;
using fin.services;

using marioartist.api;
using marioartist.schema.mfs;

using MarioArtistTool.config;

using schema.binary;

namespace marioartisttool.services;

public static class MfsFileSystemService {
  public static async Task LoadFromConfigOrPromptUserForDiskFile() {
    var config = Config.INSTANCE;

    if (config.MostRecentDiskFile != null) {
      var diskFile = new FinFile(config.MostRecentDiskFile);
      if (diskFile.Exists) {
        MfsFileSystemService.LoadDiskFile(diskFile, config.MostRecentFileName);
        return;
      }
    }

    await PromptUserForDiskFileAndLoadIfValid();
  }

  public static async Task PromptUserForDiskFileAndLoadIfValid() {
    var storageProvider = TopLevelService.Instance.StorageProvider;
    var config = Config.INSTANCE;

    string mostRecentDirectory = "./";
    if (config.MostRecentDiskFile != null) {
      var mostRecentDiskFile = new FinFile(config.MostRecentDiskFile);
      mostRecentDirectory = mostRecentDiskFile.AssertGetParent().FullPath;
    }

    var startLocation
        = await storageProvider
            .TryGetFolderFromPathAsync(mostRecentDirectory);

    var selectedStorageFiles
        = await storageProvider
            .OpenFilePickerAsync(new FilePickerOpenOptions {
                SuggestedStartLocation = startLocation,
                Title = "Select 64DD disk file",
                FileTypeFilter = [
                    new FilePickerFileType("All supported files") {
                        Patterns = [
                            "*.ndd", "*.ndr", "*.ram", "*.n64", "*.z64",
                            "*.disk"
                        ],
                    }
                ]
            });
    if (selectedStorageFiles is not { Count: 1 }) {
      return;
    }

    var selectedStorageFile = selectedStorageFiles[0];
    var diskFile = new FinFile(selectedStorageFile.Path.LocalPath);

    config.MostRecentDiskFile = diskFile.FullPath;
    config.Save();

    MfsFileSystemService.LoadDiskFile(diskFile, config.MostRecentFileName);
  }

  public static void LoadDiskFile(IReadOnlyTreeFile diskFile,
                                  string? defaultShownFile = null) {
    try {
      using var br = diskFile.OpenReadAsBinary(Endianness.BigEndian);
      var mfsDisk = br.ReadNew<MfsDisk>();
      var mfsRootDirectory = MfsTreeDirectory.CreateTreeFromMfsDisk(mfsDisk);
      LoadFileSystem(mfsRootDirectory);

      if (defaultShownFile != null &&
          mfsRootDirectory.TryToGetExistingFile(defaultShownFile,
                                                out var mostRecentFile) &&
          mostRecentFile is MfsTreeFile) {
        MfsFileSystemService.SelectFile(mostRecentFile as MfsTreeFile);
      } else {
        MfsFileSystemService.SelectFile(null);
      }
    } catch (Exception e) {
      ExceptionService.HandleException(e, new LoadFileException(diskFile));
    }
  }

  public static event Action<MfsTreeDirectory?>? OnFileSystemLoaded;

  public static void LoadFileSystem(MfsTreeDirectory? root)
    => OnFileSystemLoaded?.Invoke(root);

  public static event Action<MfsTreeFile?>? OnFileSelected;

  public static void SelectFile(MfsTreeFile? file)
    => OnFileSelected?.Invoke(file);
}