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

  public uint ObjectCount { get; set; }
  public uint TextureCount { get; set; }
  public uint ObjectDefinitionCount { get; set; }
  public uint TextureDefinitionCount { get; set; }

  public uint ObjectPointer { get; set; }
  public uint TexturePointer { get; set; }
  public uint ObjectDefinitionPointer { get; set; }
  public uint TextureDefinitionPointer { get; set; }
  public uint SubObjectPointer { get; set; }
  public uint GeometryPointer { get; set; }
  public uint ObjectEndPointer { get; set; }

  public uint TextureStart { get; set; }
  public uint TextureEnd { get; set; }
  public uint TextureBits { get; set; }
  public ushort LmtexFirst { get; set; }
  public ushort LmtexEnd { get; set; }
  public uint TextureInfo { get; set; }
  
  [RAtPosition(nameof(TexturePointer))]
  [RSequenceLengthSource(nameof(TextureCount))]
  public Texture[] Textures { get; set; }

  [RAtPosition(nameof(ObjectDefinitionPointer))]
  [RSequenceLengthSource(nameof(ObjectDefinitionCount))]
  public ObjectDefinition[] ObjectDefinitions { get; set; }

  [RAtPosition(nameof(TextureDefinitionPointer))]
  [RSequenceLengthSource(nameof(TextureDefinitionCount))]
  public TextureDefinition[] TextureDefinitions { get; set; }
}