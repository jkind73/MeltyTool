using fin.util.types;

using uni.ui.winforms.common.fileTreeView;

namespace uni.services;

[IocCandiate]
public static class FileTreeLeafNodeService {
  public static event Action<IFileTreeLeafNode>? OnFileTreeLeafNodeOpened;

  public static void OpenFileTreeLeafNode(IFileTreeLeafNode fileTreeLeafNode)
    => OnFileTreeLeafNodeOpened?.Invoke(fileTreeLeafNode);
}