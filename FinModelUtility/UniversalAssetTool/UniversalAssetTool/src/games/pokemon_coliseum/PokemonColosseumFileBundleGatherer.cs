using fin.io;
using fin.io.bundles;
using fin.util.progress;

namespace uni.games.pokemon_colosseum;

public sealed class PokemonColosseumFileBundleGatherer
    : BGameCubeFileBundleGatherer {
  public override string Name => "pokemon_colosseum";

  protected override void GatherFileBundlesFromHierarchy(
      IFileBundleOrganizer organizer,
      IMutablePercentageProgress mutablePercentageProgress,
      IFileHierarchy fileHierarchy) {
  }
}