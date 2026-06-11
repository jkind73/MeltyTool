using bar.schema;

using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;

using schema.binary;

namespace bar.api;

public sealed record UvctModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public sealed class UvctModelFileImporter
    : IModelImporter<UvctModelFileBundle> {
  public IModel Import(UvctModelFileBundle fileBundle) {
    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return new ModelImpl {
          FileBundle = fileBundle,
          Files = new HashSet<IReadOnlyGenericFile>(),
      };
    }

    var uvct = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                      Endianness.BigEndian)
            .ReadNew<Uvct>();
    return UvmdModelFileImporter.FromMaterialMeshes(
        fileBundle,
        uvct.MaterialMeshes.Select(m => m.Impl));
  }}