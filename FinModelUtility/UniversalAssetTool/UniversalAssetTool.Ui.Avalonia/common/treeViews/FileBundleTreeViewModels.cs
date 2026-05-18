using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

using fin.audio.io.importers.ogg;
using fin.data.queues;
using fin.io;
using fin.io.bundles;
using fin.ui;
using fin.ui.avalonia.trees;
using fin.util.asserts;
using fin.util.io;

using grezzo.api;

using Material.Icons;
using Material.Icons.Avalonia;

using ObservableCollections;

using uni.ui.winforms.common.fileTreeView;


namespace uni.ui.avalonia.common.treeViews;

using IFileBundleNode = INode<IFileBundle>;

// Top-level view model types
public class FileBundleTreeViewModel
    : BViewModel, IFilterTreeViewViewModel<IFileBundle> {
  private readonly IReadOnlyList<IFileBundleNode> nodes_;

  public ISynchronizedView<IFileBundleNode, IFileBundleNode> FilteredNodes { get;
  }

  public FileBundleTreeViewModel(IReadOnlyList<IFileBundleNode> nodes) {
    this.nodes_ = nodes;

    var autoCompleteItems = new HashSet<string>();
    var nodeQueue = new FinQueue<IFileBundleNode>(nodes);
    while (nodeQueue.TryDequeue(out IFileBundleNode node)) {
      var file = node.Value;
      if (file != null) {
        autoCompleteItems.Add(file.DisplayFullPath.ToString());
      } else {
        autoCompleteItems.Add(node.Label);
      }

      if (node.FilteredSubNodes != null) {
        nodeQueue.Enqueue(node.FilteredSubNodes);
      }
    }

    this.AutoCompleteItems = autoCompleteItems.ToArray();

    var obsList = new ObservableList<IFileBundleNode>(nodes);
    this.FilteredNodes = obsList.CreateView(t => t);

    this.Source = TreeDataGridUtil.CreateSource(
        new TreeDataGridSourceParams<IFileBundleNode> {
            RootNodes = this.FilteredNodes,
            ChildSelector = x => x.FilteredSubNodes,
            HasChildren = x => x.HasChildren,
            OnSelectionChanged = this.ChangeSelection,
            Template = x => {
              if (x == null) {
                return null;
              }

              var textBlock = new TextBlock {
                  Text = x.Label,
                  Classes = { "regular" }
              };

              if (x.Icon == null) {
                return textBlock;
              }

              var icon = new MaterialIcon {
                  Kind = x.Icon.Value,
                  Height = 16,
                  Width = 32,
              };

              var stackPanel = new StackPanel {
                  Orientation = Orientation.Horizontal,
              };
              stackPanel.Children.AddRange([icon, textBlock]);

              var annotatedFileBundle = x.Value;
              var contextMenu = new ContextMenu();
              {
                var openInExplorerItem = new MenuItem {
                    Header = "Show in Explorer",
                    IsEnabled = annotatedFileBundle.MainFile != null
                };
                openInExplorerItem.Click += (_, _) => {
                  if (annotatedFileBundle.MainFile != null) {
                    ExplorerUtil.OpenInExplorer(
                        annotatedFileBundle.MainFile);
                  }
                };
                contextMenu.Items.Add(openInExplorerItem);
              }
              stackPanel.ContextMenu = contextMenu;

              return stackPanel;
            }
        });

    this.Source.ShowColumnHeaders = false;
    this.Source.CanUserResizeColumns = false;
  }

  public HierarchicalTreeDataGridSource<IFileBundleNode> Source { get;
    private set;
  }

  public string[] AutoCompleteItems { get; }

  public event EventHandler<IFileBundleNode>? NodeSelected;

  public void ChangeSelection(INode node)
    => this.NodeSelected?.Invoke(this, Asserts.AsA<IFileBundleNode>(node));

  public void UpdateFilter(string? text) {
    var filter = FileBundleFilter.FromText(text);

    foreach (var node in this.nodes_) {
      node.Filter = filter;
    }

    if (filter == null) {
      this.FilteredNodes.ResetFilter();
    } else {
      this.FilteredNodes.AttachFilter(n => n.InFilter);
    }

    this.Source.Items = this.FilteredNodes.Where(i => i.InFilter);
  }
}

public sealed class FileBundleTreeViewModelForDesigner()
    : FileBundleTreeViewModel([
        new FileBundleDirectoryNode("Animals",
        [
            new FileBundleDirectoryNode("Mammals",
            [
                new FileBundleLeafNode("Lion",
                                       new CmbModelFileBundle(new FinFile(""))),
                new FileBundleLeafNode("Cat",
                                       new OggAudioFileBundle(new FinFile("")))
            ])
        ])
    ]);

// Node types
public abstract class BFileBundleNode(string text)
    : BViewModel, IFileTreeNode {
  public string Text => text;
  public IFileTreeParentNode? Parent { get; set; }
}

public sealed class FileBundleDirectoryNode
    : BFileBundleNode, IFileBundleNode, IFileTreeParentNode {
  private readonly IReadOnlyList<IFileBundleNode>? subNodes_;

  private readonly ISynchronizedView<IFileBundleNode,
      IFileBundleNode>? filteredSubNodes_;

  public FileBundleDirectoryNode(
      string label,
      IReadOnlyList<IFileBundleNode>? subNodes) : this(label,
    subNodes,
    new HashSet<string>([label])) { }

  public FileBundleDirectoryNode(
      string label,
      IReadOnlyList<IFileBundleNode>? subNodes,
      IReadOnlySet<string> filterTerms) : base(label) {
    this.subNodes_ = subNodes;
    foreach (var subNode in this.subNodes_) {
      if (subNode is BFileBundleNode bNode) {
        bNode.Parent = this;
      }
    }

    this.Label = label;
    this.FilterTerms = filterTerms;
    var obsList = subNodes != null
        ? new ObservableList<IFileBundleNode>(subNodes)
        : null;
    this.filteredSubNodes_ = obsList?.CreateView(t => t);
    this.FilteredSubNodes = this.filteredSubNodes_?.ToNotifyCollectionChanged();
  }

  public IFileBundle? Value => null;

  public bool HasChildren => true;

  public INotifyCollectionChangedSynchronizedViewList<
      IFileBundleNode>? FilteredSubNodes { get; }

  public MaterialIconKind? Icon => null;
  public string Label { get; }
  public IReadOnlySet<string> FilterTerms { get; }

  public IFilter<IFileBundle>? Filter {
    set {
      if (this.subNodes_ == null || this.FilteredSubNodes == null) {
        return;
      }

      foreach (var node in this.subNodes_) {
        node.Filter = value;
      }

      if (value == null) {
        this.filteredSubNodes_?.ResetFilter();
        this.InFilter = true;
        return;
      }

      this.filteredSubNodes_?.AttachFilter(n => n.InFilter);
      this.InFilter = this.subNodes_.Any(n => n.InFilter);
    }
  }

  public bool InFilter { get; private set; } = true;

  public IEnumerable<IFileTreeNode> ChildNodes
    => this.subNodes_?.Cast<IFileTreeNode>() ?? [];
}

public sealed class FileBundleLeafNode(
    string label,
    IFileBundle data)
    : BFileBundleNode(label), IFileBundleNode, IFileTreeLeafNode {
  public INotifyCollectionChangedSynchronizedViewList<
      IFileBundleNode>? FilteredSubNodes => null;
  public bool HasChildren => false;

  public MaterialIconKind? Icon => data.Type switch {
      FileBundleType.AUDIO => MaterialIconKind.VolumeHigh,
      FileBundleType.IMAGE => MaterialIconKind.ImageOutline,
      FileBundleType.MODEL => MaterialIconKind.CubeOutline,
      FileBundleType.SCENE => MaterialIconKind.Web,
  };

  public IFileBundle Value => data;
  public IFileBundle File => data;
  public string Label => label;

  public IFilter<IFileBundle>? Filter {
    set => this.InFilter = value?.MatchesNode(this) ?? true;
  }

  public bool InFilter { get; private set; } = true;
}

public sealed class FileBundleFilter(IReadOnlySet<string> tokens)
    : IFilter<IFileBundle> {
  public static FileBundleFilter? FromText(string? text) {
    var tokens = text?.Split([' ', '\t', '\n'],
                             StringSplitOptions.RemoveEmptyEntries |
                             StringSplitOptions.TrimEntries);
    return tokens?.Length > 0
        ? new FileBundleFilter(new HashSet<string>(tokens))
        : null;
  }

  public bool MatchesNode(IFileBundleNode node) {
    foreach (var token in tokens) {
      var annotatedFileBundle = node.Value;
      var fileBundle = annotatedFileBundle;

      if (this.ContainsToken_(node.Label, token)) {
        goto FoundMatch;
      }

      if (this.ContainsToken_(annotatedFileBundle.DisplayFullPath, token)) {
        goto FoundMatch;
      }

      foreach (var file in fileBundle.Files) {
        if (this.ContainsToken_(file.DisplayFullPath, token)) {
          goto FoundMatch;
        }
      }

      return false;

      FoundMatch: ;
    }

    return true;
  }

  private bool ContainsToken_(ReadOnlySpan<char> text, ReadOnlySpan<char> token)
    => text.Contains(token, StringComparison.OrdinalIgnoreCase);
}