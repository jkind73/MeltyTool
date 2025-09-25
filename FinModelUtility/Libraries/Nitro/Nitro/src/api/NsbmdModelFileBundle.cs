using fin.io;
using fin.model.io;

namespace nitro.api;

public sealed class NsbmdModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile NsbmdFile { get; init; }

  public required string? GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.NsbmdFile;
}