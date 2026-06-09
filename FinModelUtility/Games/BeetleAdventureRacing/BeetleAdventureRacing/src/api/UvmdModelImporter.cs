using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

namespace bar.api;

public sealed record UvmdModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public sealed class UvmdModelFileImporter
    : IModelImporter<UvmdModelFileBundle> {
  public IModel Import(UvmdModelFileBundle fileBundle) {
    var files = fileBundle.MainFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files
    };

    return finModel;
  }
}