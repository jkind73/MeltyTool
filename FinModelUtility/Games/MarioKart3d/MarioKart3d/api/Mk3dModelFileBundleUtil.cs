using System.Linq;

using fin.io;

using gm.api;

namespace mk3d.api;

public static class Mk3dModelFileBundleUtil {
  public static D3dModelFileBundle FromSmkFile(IReadOnlyTreeFile smkFile) {
    var gifFile = smkFile.AssertGetParent()
                         .GetExistingFiles()
                         .WithFileType(".gif")
                         .SingleOrDefault();
    var pngFile = smkFile.AssertGetParent()
                         .GetExistingFiles()
                         .WithFileType(".png")
                         .SingleOrDefault();

    return new D3dModelFileBundle {
        D3dFile = smkFile,
        TextureFile = gifFile == null ? pngFile : null,
        AnimatedTextureFile = pngFile == null && gifFile != null
            ? (gifFile, 30)
            : null,
    };
  }
}