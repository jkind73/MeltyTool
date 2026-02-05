using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.model;
using fin.ui.avalonia;
using fin.ui.rendering;
using fin.util.asserts;

using ReactiveUI;

namespace uni.ui.avalonia.resources.model.skeleton;

public sealed class SkeletonTreeViewModelForDesigner
    : SkeletonTreeViewModel {
  public SkeletonTreeViewModelForDesigner() {
    this.Model = ModelDesignerUtil.CreateStubModel();
  }
}

public class SkeletonTreeViewModel : BViewModel {
  public required IReadOnlyModel? Model {
    set {
      this.Impl = value != null
          ? FullHierarchyTreeViewModel.FromModel(
              value,
              FullHierarchyTreeType.BONES)
          : null;

      this.Impl?.ExpandCollapse(FullHierarchyTreeType.BONES);
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

public partial class SkeletonTree : UserControl {
  public SkeletonTree() {
    this.InitializeComponent();
  }
}