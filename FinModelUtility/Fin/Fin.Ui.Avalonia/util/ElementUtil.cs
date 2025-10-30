using Avalonia;
using Avalonia.Data;

namespace fin.ui.avalonia.util;

public static class ElementUtil {
  public static void AddClass(this StyledElement element, string className)
    => element.BindClass(className, new Binding {
        FallbackValue = true,
    }, null);
}