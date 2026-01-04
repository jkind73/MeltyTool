using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Controls;
using Avalonia.Interactivity;

using fin.scene;
using fin.ui.avalonia;
using fin.ui.rendering;
using fin.util.asserts;

using ReactiveUI;

namespace uni.ui.avalonia.resources.scene.areas;

public sealed class AreasTreeViewModelForDesigner
    : AreasTreeViewModel {
  public AreasTreeViewModelForDesigner() {
    this.Areas = SceneDesignerUtil.CreateStubScene().Areas;
  }
}

public class AreasTreeViewModel : BViewModel {
  public required IReadOnlyList<IReadOnlySceneArea>? Areas {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.RootNode = value != null
          ? new AreaOrNode(value.Select(a => new AreaOrNode(a)).ToArray())
          : null;
    }
  }

  public AreaOrNode? RootNode {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public AreaOrNode? SelectedNode {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public sealed class AreaOrNode : BViewModel {
  public AreaOrNode(IReadOnlyList<AreaOrNode> children)
    => this.Children = children;

  public AreaOrNode(IReadOnlySceneArea area) {
    this.Area = area;
    this.Children = area.RootNodes.Select(b => new AreaOrNode(b)).ToArray();
  }

  public AreaOrNode(IReadOnlySceneNode node) {
    this.Node = node;
    this.Children = node.ChildNodes.Select(b => new AreaOrNode(b)).ToArray();
  }

  public IReadOnlySceneArea? Area { get; }
  public IReadOnlySceneNode? Node { get; }

  public IReadOnlyList<AreaOrNode> Children { get; }

  public bool IsExpanded {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  } = true;
}

public partial class AreasTree : UserControl {
  public AreasTree() {
    this.InitializeComponent();
  }

  public static readonly RoutedEvent<NodeSelectedEventArgs>
      NodeSelectedEvent =
          RoutedEvent.Register<AreasTree, NodeSelectedEventArgs>(
              nameof(NodeSelected),
              RoutingStrategies.Direct);

  public event EventHandler<NodeSelectedEventArgs> NodeSelected {
    add => this.AddHandler(NodeSelectedEvent, value);
    remove => this.RemoveHandler(NodeSelectedEvent, value);
  }

  protected void SelectingItemsControl_OnSelectionChanged(
      object? sender,
      SelectionChangedEventArgs e) {
    if (e.AddedItems.Count == 0) {
      SelectedBoneService.SelectBone(null);
      return;
    }

    var selectedNode = Asserts.AsA<AreaOrNode>(e.AddedItems[0]);
    this.RaiseEvent(new NodeSelectedEventArgs {
        RoutedEvent = NodeSelectedEvent,
        Node = selectedNode.Node
    });
    SelectedNodeService.SelectNode(selectedNode.Node);
  }
}

public sealed class NodeSelectedEventArgs : RoutedEventArgs {
  public required IReadOnlySceneNode Node { get; init; }
}