using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/model.py#L16
/// </summary>
[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public sealed partial class Objects : IBinaryDeserializable {
  [StringLengthSource(32)]
  public string DirectoryName { get; set; }

  [StringLengthSource(32)]
  public string Name { get; set; }

  public uint Version { get; set; }

  private uint objectCount_;
  private uint textureCount_;
  private uint objectDefinitionCount_;
  private uint textureDefinitionCount_;

  private uint objectPointer_;
  private uint texturePointer_;
  private uint objectDefinitionPointer_;
  private uint textureDefinitionPointer_;

  public uint SubObjectPointer { get; set; }
  public uint GeometryPointer { get; set; }
  public uint ObjectEndPointer { get; set; }

  public uint TextureStart { get; set; }
  public uint TextureEnd { get; set; }
  public uint TextureBits { get; set; }
  public ushort LmtexFirst { get; set; }
  public ushort LmtexEnd { get; set; }
  public uint TextureInfo { get; set; }

  [RAtPosition(nameof(objectPointer_))]
  [RSequenceLengthSource(nameof(objectCount_))]
  public Object[] RootObjects { get; set; }

  [RAtPosition(nameof(texturePointer_))]
  [RSequenceLengthSource(nameof(textureCount_))]
  public Texture[] Textures { get; set; }

  [RAtPosition(nameof(objectDefinitionPointer_))]
  [RSequenceLengthSource(nameof(objectDefinitionCount_))]
  public ObjectDefinition[] ObjectDefinitions { get; set; }

  [RAtPosition(nameof(textureDefinitionPointer_))]
  [RSequenceLengthSource(nameof(textureDefinitionCount_))]
  public TextureDefinition[] TextureDefinitions { get; set; }
}