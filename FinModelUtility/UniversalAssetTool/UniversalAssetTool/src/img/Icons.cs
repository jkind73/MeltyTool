using System.Drawing;
using System.Reflection;

using uni.util.image;

namespace uni.img;

public static class Icons {
  private static readonly Assembly ASSEMBLY_ =
      Assembly.GetExecutingAssembly();

  public static readonly Image FOLDER_CLOSED_IMAGE =
      LoadIcon_("uni.src.img.folder_closed.png");

  public static readonly Image FOLDER_OPEN_IMAGE =
      LoadIcon_("uni.src.img.folder_open.png");

  public static readonly Image MODEL_IMAGE =
      LoadIcon_("uni.src.img.model.png");

  public static readonly Image SCENE_IMAGE =
      LoadIcon_("uni.src.img.scene.png");

  public static readonly Image MUSIC_IMAGE =
      LoadIcon_("uni.src.img.music.png");

  private static Image LoadIcon_(string embeddedResourceName)
    => EmbeddedResourceImageUtil.Load(ASSEMBLY_, embeddedResourceName);
}