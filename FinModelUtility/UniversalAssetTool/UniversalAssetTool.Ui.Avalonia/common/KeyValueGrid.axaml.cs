using System.Collections.ObjectModel;

using Avalonia.Controls;

using fin.ui.avalonia;

using ReactiveUI;

namespace uni.ui.avalonia.common {
  public sealed class KeyValueGridViewModelForDesigner
      : KeyValueGridViewModel {
    public KeyValueGridViewModelForDesigner() {
      this.KeyValuePairs = [
          ("Foo", "Bar"),
          ("Number", 123),
          ("Null", null),
          ("Long wrappable text",
           "This is very long text that could theoretically wrap at any of the spaces."),
          ("Long unwrappable text",
           "Thisisverylongtextthatcannotwrapbecausetherearenospaces."),
          ("Newline text",
           "This text\nhas some newlines\nthat should be shown."),
      ];
    }
  }

  public class KeyValueGridViewModel : BViewModel {
    public ObservableCollection<KeyValuePairViewModel> KeyValuePairs {
      get;
      set => this.RaiseAndSetIfChanged(ref field, value);
    } = [];
  }

  public sealed class KeyValuePairViewModel(string key, string? value)
      : BViewModel {
    public string Key => key;
    public string? Value => value;

    public static implicit operator KeyValuePairViewModel(
        (string key, object? value) tuple)
      => new(tuple.key, tuple.value?.ToString());
  }

  public partial class KeyValueGrid : UserControl {
    public KeyValueGrid() {
      this.InitializeComponent();
    }
  }
}