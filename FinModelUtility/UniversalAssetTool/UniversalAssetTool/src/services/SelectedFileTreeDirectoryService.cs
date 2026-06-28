using fin.util.types;

using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

[IocCandiate]
public static class SelectedFileTreeDirectoryService {
  public static event Action<IFileTreeParentNode>? OnFileTreeDirectorySelected;

  public static void SelectFileTreeDirectory(
      IFileTreeParentNode fileTreeParentNode)
    => OnFileTreeDirectorySelected?.Invoke(fileTreeParentNode);
}