using sysdolphin.api;

namespace uni.games.chibi_robo;

public sealed class ChibiRoboMassExporter : IMassExporter {
  public void ExportAll()
    => ExporterUtil.ExportAllForCli(new ChibiRoboFileBundleGatherer(),
                                    new DatModelImporter());
}