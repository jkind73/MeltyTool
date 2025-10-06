using Avalonia;
using Avalonia.Controls;

using fin.util.asserts;

namespace marioartisttool.services;

public static class TopLevelService {
  public static TopLevel Instance { get; private set; }

  public static void Init(Visual visual)
    => Instance = TopLevel.GetTopLevel(visual).AssertNonnull();
}