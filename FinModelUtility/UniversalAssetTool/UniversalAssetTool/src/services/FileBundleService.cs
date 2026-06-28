using fin.io.bundles;
using fin.util.types;

using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

[IocCandiate]
public static class FileBundleService {
  static FileBundleService() {
    FileTreeLeafNodeService.OnFileTreeLeafNodeOpened += fileTreeLeafNode
        => OpenFileBundle(fileTreeLeafNode, fileTreeLeafNode.File);
  }

  public static event Action<IFileTreeLeafNode?, IFileBundle>
      OnFileBundleOpened;

  public static void OpenFileBundle(IFileTreeLeafNode? fileTreeLeafNode,
                                    IFileBundle fileBundle)
    => OnFileBundleOpened?.Invoke(fileTreeLeafNode, fileBundle);
}