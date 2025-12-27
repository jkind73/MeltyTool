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

  private uint itemInfoCount_;
  private uint itemInfoPointer_;

  private uint itemInstanceCount_;
  private uint itemInstancePointer_;
}