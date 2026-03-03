using fin.io.bundles;


namespace uni.ui.winforms.common.fileTreeView;

public abstract partial class FileTreeView<TFiles> {
  protected class LeafFileNode : BFileNode, IFileTreeLeafNode {
    public LeafFileNode(ParentFileNode parent,
                        IFileBundle file,
                        string? text = null) :
        base(parent, text ?? file.DisplayName.ToString()) {
        this.File = file;
        this.InitializeFilterNode(parent);

        this.treeNode.ClosedImage =
            this.treeNode.OpenImage = this.treeView.GetImageForFile(this.File);
      }

    public IFileBundle File { get; }
    public override string FullName => this.File.TrueFullPath;
  }
}