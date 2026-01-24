using sm64.LevelInfo;
using sm64.memory;

namespace sm64.Scripts {
  public interface IGeoScripts {
    void Parse(
        IReadOnlySm64Memory n64Memory,
        Model3DLods mdlLods,
        ref Level lvl,
        byte seg,
        uint off);
  }

  public sealed class GeoScriptsWrapper : IGeoScripts {
    private readonly IGeoScripts geoScriptsImplementation_ = new GeoScriptsV2();

    public void Parse(
        IReadOnlySm64Memory n64Memory,
        Model3DLods mdlLods,
        ref Level lvl,
        byte seg,
        uint off)
      => this.geoScriptsImplementation_.Parse(n64Memory,
                                              mdlLods,
                                              ref lvl,
                                              seg,
                                              off);
  }
}