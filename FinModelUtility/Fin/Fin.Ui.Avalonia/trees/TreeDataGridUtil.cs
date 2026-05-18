using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace fin.ui.avalonia.trees;

public record TreeDataGridSourceParams<T> {
  public required IEnumerable<T> RootNodes { get; init; }
  public required Expression<Func<T, IEnumerable<T>?>> ChildSelector { get; init; }
  public required Expression<Func<T, bool>> HasChildren { get; init; }
  public Action<T> OnSelectionChanged { get; init; }
  public required Func<T?, Control?> Template { get; init; }
}

public static class TreeDataGridUtil {
  public static HierarchicalTreeDataGridSource<T> CreateSource<T>(
      TreeDataGridSourceParams<T> prms) where T : class {
    var source = new HierarchicalTreeDataGridSource<T>(prms.RootNodes)
        .WithHierarchicalExpanderColumn(
            "Name",
            new TreeDataGridTemplateColumn {
                CellTemplate
                    = new FuncDataTemplate<T>((x, _) => prms.Template(x)),
                Width = GridLength.Star
            },
            children: prms.ChildSelector,
            hasChildren: prms.HasChildren,
            options: o => {
              o.Width = GridLength.Star;
            });

    source.ShowColumnHeaders = false;
    source.CanUserResizeColumns = false;

    var rowSelection = source.RowSelection!;
    rowSelection.SelectionChanged += (_, e) => {
      var selectedItems = e.SelectedItems;
      if (selectedItems.Count == 0) {
        return;
      }

      prms.OnSelectionChanged(selectedItems[0]!);
    };

    return source;
  }
}
