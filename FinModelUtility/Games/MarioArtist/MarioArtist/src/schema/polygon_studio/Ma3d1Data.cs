using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.polygon_studio;

public sealed class Ma3d1Data : IBinaryDeserializable, IChildOf<Ma3d1> {
  public Ma3d1 Parent { get; set; }
  public IReadOnlyList<Mesh> Meshes { get; private set; }

  public void Read(IBinaryReader br) {
    br.PushLocalSpace();

    var header = this.Parent.Header;

    var meshes = new Mesh[header.MeshCount];

    uint meshOffset = 0;
    for (var i = 0; i < header.MeshCount; ++i) {
      br.Position = meshOffset;
      var mesh = br.ReadNew<Mesh>();
      meshes[i] = mesh;
      meshOffset = mesh.NextMeshOffset;
    }

    this.Meshes = meshes;

    br.PopLocalSpace();
  }
}