using System.Numerics;

using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using tlpe.scb;

namespace tlpe.api;

public sealed record ScbModelFileBundle(IReadOnlyTreeFile ScbFile)
    : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.ScbFile;
}

public sealed class ScbModelImporter : IModelImporter<ScbModelFileBundle> {
  public IModel Import(ScbModelFileBundle fileBundle) {
    var scb = fileBundle.ScbFile.ReadNew<Scb>();

    var files = fileBundle.ScbFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files
    };

    var finSkin = finModel.Skin;

    foreach (var scbSection in scb.Sections) {
      switch (scbSection) {
        case MeshSection meshSection: {
          var finMesh = finSkin.AddMesh();

          var finVertices
              = meshSection
                .Vertices.Select(v => {
                  var finVertex = finSkin.AddVertex(v.Position);
                  finVertex.SetLocalNormal(v.Normal);
                  finVertex.SetUv(0, v.Uv0);
                  finVertex.SetUv(1, v.Uv1);
                  return finVertex;
                })
                .ToArray();

          var triangles
              = meshSection
                .Faces.Select(f => {
                  var v0 = (IReadOnlyVertex) finVertices[f.Vertex0];
                  var v1 = (IReadOnlyVertex) finVertices[f.Vertex1];
                  var v2 = (IReadOnlyVertex) finVertices[f.Vertex2];

                  return (v0, v1, v2);
                })
                .ToArray();

          finMesh.AddTriangles(triangles);

          break;
        }
      }
    }

    return finModel;
  }
}