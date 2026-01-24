using fin.scene;

using HaloWarsTools;

namespace hw.api;

public sealed class VisSceneImporter : ISceneImporter<VisSceneFileBundle> {
  public IScene Import(VisSceneFileBundle sceneFileBundle) {
    var visResource =
        HwVisResource.FromFile(sceneFileBundle.Context,
                               sceneFileBundle.VisFile.FullPath);
    return visResource.Scene;
  }
}