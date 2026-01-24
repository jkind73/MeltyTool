using schema.binary;
using schema.binary.attributes;

namespace uni.platforms.threeDs.tools.cia;

[BinarySchema]
public sealed partial class Cia : IBinaryDeserializable {
  private const int SECTION_ALIGNMENT_ = 64;

  public CiaHeader Header { get; } = new();

  [AlignStart(SECTION_ALIGNMENT_)]
  public CiaCertificates Certificates { get; } = new();

  [AlignStart(SECTION_ALIGNMENT_)]
  public CiaTickets Tickets { get; } = new();

  [AlignStart(SECTION_ALIGNMENT_)]
  public CiaTmd Tmd { get; } = new();

  [AlignStart(SECTION_ALIGNMENT_)]
  public CiaContent Content { get; } = new();
}