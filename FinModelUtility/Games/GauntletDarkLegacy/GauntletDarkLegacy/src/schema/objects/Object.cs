using fin.util.enumerables;

using gdl.schema.objects.mesh;

using schema.binary;
using schema.binary.attributes;

namespace gdl.schema.objects;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/haekb/io_scene_gdl/blob/master/src/model.py#L327
/// </summary>
[BinarySchema]
public sealed partial class Object : IBinaryDeserializable {
  public float InvRad { get; set; }
  public float BoundingRadius { get; set; }
  public uint Flags { get; set; }
  public uint SubObjectCount { get; set; }
  // For fuck's sake, why is the list split like this???
  public SubObject SubObject0 { get; } = new();
  public uint SubObjectPointer { get; set; }
  public uint SubObjectModelsPointer { get; set; }
  public uint VertexCount { get; set; }
  public uint TriangleCount { get; set; }
  public uint Index { get; set; }
  public uint ObjectDefPointer { get; set; }

  [SequenceLengthSource(4)]
  public uint[] Unk0 { get; set; }

  [Skip]
  public SubObjectModels? SubObjectModels { get; set; }

  [Skip]
  private int TrueSubObjCount_ => ((int) this.SubObjectCount) - 1;

  [RAtPosition(nameof(SubObjectPointer))]
  [RSequenceLengthSource(nameof(TrueSubObjCount_))]
  public SubObject[] SubObjectsPast0 { get; set; }

  [Skip]
  public IEnumerable<SubObject> SubObjects
    => this.SubObject0.Yield().Concat(this.SubObjectsPast0);
  
  [ReadLogic]
  private void ReadMesh_(IBinaryReader br) {
    if (this.SubObjectModelsPointer == 0) {
      this.SubObjectModels = null;
      return;
    }

    br.SubreadAt(this.SubObjectModelsPointer,
                 () => {
                   this.SubObjectModels ??= new SubObjectModels {
                       Parent = this,
                   };
                   this.SubObjectModels.Read(br);
                 });
  }
}