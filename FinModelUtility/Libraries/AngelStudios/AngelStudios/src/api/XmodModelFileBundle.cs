using fin.io;
using fin.model.io;


namespace xmod.api;

public sealed class XmodModelFileBundle : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.XmodFile;
  public required IReadOnlyTreeFile XmodFile { get; init; }
  public required IReadOnlyTreeDirectory TextureDirectory { get; init; }
}