namespace fin.io;

public sealed record FinRootDirectory(string Name, ISystemDirectory Impl) {
  public string GetDisplayName(ISystemFile file)
    => $"//{this.Name}/{file.FullPath[this.Impl.FullPath.Length..]}";

  public string GetDisplayName(ISystemDirectory dir)
    => $"//{this.Name}/{dir.FullPath[this.Impl.FullPath.Length..]}";
}