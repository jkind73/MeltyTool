using sm64ds.api;

namespace uni.games.super_mario_64_ds;

public sealed class SuperMario64DsMassExporter : IMassExporter {
  public void ExportAll() => ExporterUtil.ExportAllOfTypeForCli(
      new SuperMario64DsFileBundleGatherer(),
      new Sm64dsModelImporter());
}