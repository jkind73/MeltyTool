using System.Drawing;

using fin.image;
using fin.ui.rendering.gl.texture;

namespace fin.ui.rendering.gl.material;

public static class GlMaterialConstants {
  public static IGlTexture NullWhiteTexture { get; private set; }
  public static IGlTexture NullGrayTexture { get; private set;}
  public static IGlTexture NullBlackTexture { get; private set;}

  public static void Initialize() {
    if (NullWhiteTexture != null) {
      return;
    }

    NullWhiteTexture =
        new GlTexture(FinImage.Create1X1FromColor(Color.White));
    NullGrayTexture =
        new GlTexture(FinImage.Create1X1FromColor(Color.Gray));
    NullBlackTexture =
        new GlTexture(FinImage.Create1X1FromColor(Color.Black));
  }

  public static bool IsCommonTexture(IGlTexture texture)
    => texture == NullWhiteTexture ||
       texture == NullGrayTexture ||
       texture == NullBlackTexture;
}