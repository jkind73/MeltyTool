using fin.io;
using fin.model.io;

using UoT.memory;

namespace UoT.api {
  public sealed class OotModelFileBundle(
      IReadOnlyTreeDirectory directory,
      IReadOnlyTreeFile ootRom,
      IZFile zFile) : IModelFileBundle {
    public IReadOnlyTreeFile MainFile => ootRom;
    public IReadOnlyTreeDirectory Directory => directory;

    public IReadOnlyTreeFile OotRom => ootRom;
    public IZFile ZFile => zFile;

    string IUiFile.HumanReadableName => this.ZFile.FileName;
    public string TrueFullPath => this.OotRom.FullPath;
  }
}