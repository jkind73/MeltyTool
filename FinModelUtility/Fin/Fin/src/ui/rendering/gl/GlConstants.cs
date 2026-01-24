namespace fin.ui.rendering.gl;

public static class GlConstants {
  public const bool COMPATIBILITY = false;
  public const bool DEBUG
#if DEBUG
      = true;
#else
      = false;
#endif
}