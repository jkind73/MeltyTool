using Avalonia.Controls;

using fin.scene;
using fin.ui.avalonia;
using fin.ui.rendering;

using ReactiveUI;

using uni.ui.avalonia.resources.scene.areas;

namespace uni.ui.avalonia.resources.scene;

public sealed class ScenePanelViewModelForDesigner : ScenePanelViewModel {
  public ScenePanelViewModelForDesigner() {
    this.Scene = SceneDesignerUtil.CreateStubScene();
  }
}

public class ScenePanelViewModel : BViewModel {
  public IReadOnlyScene Scene {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);
      this.AreasTree = new AreasTreeViewModel { Areas = value.Areas, };
      this.FilesPanel = new FilesPanelViewModel(value);
    }
  }

  public AreasTreeViewModel AreasTree {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }


  public FilesPanelViewModel FilesPanel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class ScenePanel : UserControl {
  public ScenePanel() {
    this.InitializeComponent();
  }

  private void ClearSelectedItemsWhenTabChanged_(
      object? sender,
      SelectionChangedEventArgs e) {
    if (e.Source != this.ModelTabs) {
      return;
    }

    var shouldDeselectArea = true;
    if (e.AddedItems.Count > 0) {
      if (e.AddedItems[0] is TabItem item) {
        var header = item.Header;

        if (header == this.AreasTabHeader) {
          shouldDeselectArea = true;
        }
      }
    }

    if (shouldDeselectArea) {
      SelectedNodeService.SelectNode(null);
    }
  }
}