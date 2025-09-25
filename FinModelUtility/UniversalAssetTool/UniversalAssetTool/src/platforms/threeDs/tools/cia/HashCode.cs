namespace uni.platforms.threeDs.tools.cia;

public sealed class HashCode {
  private readonly byte[] hash_ = new byte[32];

  public HashCode() { }

  public IReadOnlyList<byte> Bytes => this.hash_;
}