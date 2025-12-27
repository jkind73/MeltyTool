using System.Numerics;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.worlds;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/gdl-tools/blob/master/Addons/GDLFormat/Models/Worlds.gd
/// </summary>
[BinarySchema]
[Endianness(Endianness.LittleEndian)]
public sealed partial class Worlds : IBinaryDeserializable {
  private uint worldObjectCount_;
  private uint worldObjectPointer_;

  [RAtPosition(nameof(worldObjectPointer_))]
  [RSequenceLengthSource(nameof(worldObjectCount_))]
  public WorldObject[] WorldObjects { get; private set; }

  private uint collisionTriangleCount_;
  private uint collisionTrianglePointer_;
 
  private uint gridEntryCount_;
  private uint gridEntryPointer_;
 
  private uint gridListValueCount_;
  private uint gridListPointer_;

  private uint gridRowPointer_;

  public Vector3 WorldMinBounds { get; set; }
  public Vector3 WorldMaxBounds { get; set; }

  public float GridSize { get; set; }
  public uint GridNumX { get; set; }
  public uint GridNumY { get; set; }

  private uint itemInstanceInfoCount_;
  private uint itemInstanceInfoPointer_;

  [RAtPosition(nameof(itemInstanceInfoPointer_))]
  [RSequenceLengthSource(nameof(itemInstanceInfoCount_))]
  public ItemInstanceInfo[] ItemInstanceInfos { get; private set; }

  private uint itemInstanceCount_;
  private uint itemInstancePointer_;

  [RAtPosition(nameof(itemInstancePointer_))]
  [RSequenceLengthSource(nameof(itemInstanceCount_))]
  public ItemInstance[] ItemInstances { get; private set; }
}