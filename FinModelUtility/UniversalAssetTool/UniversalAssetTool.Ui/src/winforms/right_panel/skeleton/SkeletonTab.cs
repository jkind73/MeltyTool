using System.Windows.Forms;

using fin.model;
using fin.ui.rendering;

namespace uni.ui.winforms.right_panel.skeleton;

public partial class SkeletonTab : UserControl {
  public SkeletonTab() {
    this.InitializeComponent();

      this.skeletonTreeView_.BoneSelected += boneNode =>
          SelectedBoneService.SelectBone(boneNode.Bone);
    }

  public IReadOnlyModel? Model {
    set => this.skeletonTreeView_.Populate(value?.Skeleton);
  }
}