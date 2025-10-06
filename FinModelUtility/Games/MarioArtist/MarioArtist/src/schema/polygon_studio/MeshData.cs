using schema.binary;
using schema.binary.attributes;

namespace marioartist.schema.polygon_studio;

public sealed class MeshData : IBinaryDeserializable, IChildOf<Ma3d1> {
  public Ma3d1 Parent { get; set; }
  public IReadOnlyList<Mesh> Meshes { get; private set; }

  public void Read(IBinaryReader br) {
    var header = this.Parent.Header;

    br.Position = header.MeshDataOffset;
    {
      br.PushLocalSpace();

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

    br.Position = header.TextureOffset;
    {
      br.PushLocalSpace();

      foreach (var mesh in this.Meshes) {
        br.Position = mesh.TextureOffset;

        var textureWidth = 32;
        var textureHeight = 32 * (int) (mesh.TextureSize / 2048);

        mesh.Texture = new Argb1555Image(textureWidth, textureHeight);
        mesh.Texture.Read(br);
      }

      br.PopLocalSpace();
    }
  }
}