using f3dzex2.io;

namespace UoT.memory {
  public enum ZFileType {
    OBJECT,
    CODE,
    SCENE,
    MAP,

    /// <summary>
    ///   A set of objects in a given map. These seem to be used to switch
    ///   between different versions of rooms.
    /// </summary>
    OBJECT_SET,
    OTHER,
  }

  public interface IZFile {
    ZFileType Type { get; }

    string FileName { get; }
    ISegment Segment { get; }
  }


  public abstract class BZFile(ISegment segment) : IZFile {
    public abstract ZFileType Type { get; }
    public string FileName { get; set; }
    public ISegment Segment { get; } = segment;
    public override string ToString() => this.FileName;
  }


  public sealed class ZObject(ISegment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.OBJECT;
  }


  public sealed class ZCodeFiles(ISegment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.CODE;
  }


  public sealed class ZScene(ISegment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.SCENE;

    // TODO: Make nonnull via init, C#9.
    public ZMap[]? Maps;
  }

  public sealed class ZMap(ISegment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.MAP;

    // TODO: Make nonnull via init, C#9.
    public ZScene? Scene { get; set; }
  }

  public sealed class ZObjectSet(ISegment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.OBJECT_SET;
  }

  public sealed class ZOtherData(ISegment segment) : BZFile(segment) {
    public override ZFileType Type => ZFileType.OTHER;
  }
}