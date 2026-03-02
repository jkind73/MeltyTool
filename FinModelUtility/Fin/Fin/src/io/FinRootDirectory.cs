namespace fin.io;

public sealed record FinRootDirectory(string Name, FinDirectory Impl) {
  public string GetDisplayName(FinFile file)
    => $"//{this.Name}/{file.FullPath[this.Impl.FullPath.Length..]}";

  public string GetDisplayName(FinDirectory dir)
    => $"//{this.Name}/{dir.FullPath[this.Impl.FullPath.Length..]}";
}