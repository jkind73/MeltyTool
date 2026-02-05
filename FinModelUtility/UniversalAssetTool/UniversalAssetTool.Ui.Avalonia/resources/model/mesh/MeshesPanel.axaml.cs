using Avalonia.Controls;

using fin.model;
using fin.ui.avalonia;

using ReactiveUI;


namespace uni.ui.avalonia.resources.model.mesh;

public sealed class MeshesPanelViewModelForDesigner : MeshesPanelViewModel {
  public MeshesPanelViewModelForDesigner() {
    this.Model = ModelDesignerUtil.CreateStubModel();
  }
}

public class MeshesPanelViewModel : BViewModel {
  public required IReadOnlyModel? Model {
    set {
      this.Impl = value != null
          ? FullHierarchyTreeViewModel.FromModel(
              value,
              FullHierarchyTreeType.MESHES | FullHierarchyTreeType.PRIMITIVES)
          : null;

      this.Impl?.ExpandCollapse(FullHierarchyTreeType.MESHES |
                                FullHierarchyTreeType.PRIMITIVES);
    }
  }

  public FullHierarchyTreeViewModel? Impl {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.IsPopulated = (value?.Source.Rows.Count ?? 0) > 0;
    }
  }

  public bool IsPopulated {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class MeshesPanel : UserControl {
  public MeshesPanel() {
    this.InitializeComponent();
  }
}