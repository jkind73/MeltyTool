using System.Numerics;

using fin.util.types;

namespace fin.config.avalonia.services;

[IocCandiate]
public static class MainViewInputService {
  public static Vector2 Resolution { get; set; }
  public static bool MouseInView { get; set; }
  public static bool MouseDown { get; set; }
  public static Vector2 NormalizedMousePosition { get; set; }
}