using System.Numerics;

using fin.data.queues;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using marioartist.schema.polygon_studio;

public sealed record Ma3d1ModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public sealed class Ma3d1ModelLoader : IModelImporter<Ma3d1ModelFileBundle> {
  public IModel Import(Ma3d1ModelFileBundle fileBundle) {
    var ma3d1 = fileBundle.MainFile.ReadNew<Ma3d1>();

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileBundle.MainFile.AsFileSet(),
    };

    var finSkin = finModel.Skin;
    foreach (var ma3d1Mesh in ma3d1.Data.Meshes) {
      var finMesh = finSkin.AddMesh();

      var finVertices
          = (ma3d1Mesh.Vertices ?? [])
            .Select(ma3d1Vertex => {
              var finVertex = finSkin.AddVertex(ma3d1Vertex.X,
                                                ma3d1Vertex.Y,
                                                ma3d1Vertex.Z);

              var normal = Vector3.Normalize(
                  new Vector3(ma3d1Vertex.NormalX,
                              ma3d1Vertex.NormalY,
                              ma3d1Vertex.NormalZ));
              finVertex.SetLocalNormal(normal);

              return (IReadOnlyVertex) finVertex;
            })
            .ToArray();

      var finTriangles
          = (ma3d1Mesh.Triangles ?? [])
            .Select(ma3d1Triangle => (
                        finVertices[ma3d1Triangle.VertexIndex0],
                        finVertices[ma3d1Triangle.VertexIndex1],
                        finVertices[ma3d1Triangle.VertexIndex2]))
            .ToArray();

      finMesh.AddTriangles(finTriangles);
    }

    // TODO: Add textures

    return finModel;
  }
}