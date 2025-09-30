namespace fin.ui.rendering.gl;

public static class GlConstants {
  public const bool Es = false;
  public const int MajorVersion = 4;
  public const int MinorVersion = 6;
  public const bool Compatibility = false;
  public const bool Debug
#if DEBUG
      = true;
#else
      = false;
#endif
}