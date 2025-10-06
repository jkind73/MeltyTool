using Avalonia;
using Avalonia.Controls;

using fin.util.asserts;
using fin.util.types;

namespace fin.config.avalonia.services;

[IocCandiate]
public static class TopLevelService {
  public static TopLevel Instance { get; private set; }

  public static void Init(Visual visual)
    => Instance = TopLevel.GetTopLevel(visual).AssertNonnull();
}