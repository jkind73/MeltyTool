using System.Numerics;

using fin.data.dictionaries;
using fin.io;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;
using fin.util.strings;

using schema.text.reader;

using tlpe.scb;

namespace tlpe.api;

public sealed record ScbModelFileBundle(
    IReadOnlyTreeFile ScbFile,
    IReadOnlyTreeFile BallsFile,
    IReadOnlyTreeDirectory TexturesDir)
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

    var allBallAttributes = ReadAllBallAttributes_(fileBundle.BallsFile);
    var ballAttributesByGeometry
        = allBallAttributes
          .Where(a => a.Geometry != null)
          .ToListDictionary(a => a.Geometry!.ToLower());

    var textureFiles = new List<IReadOnlyTreeFile>();
    if (ballAttributesByGeometry.TryGetList(
            fileBundle.ScbFile.Name.ToString().ToLower(),
            out var matchingBallAttributes)) {
      foreach (var ballAttributes in matchingBallAttributes) {
        if (ballAttributes.Texture != null) {
          var textureFile = fileBundle.TexturesDir.AssertGetExistingFile(
              ballAttributes.Texture);
          textureFiles.Add(textureFile);
        }
      }
    }

    IMaterial[] textureMaterials
        = textureFiles
          .Select(textureFile => {
            (var finMaterial, _)
                = finModel.MaterialManager.AddSimpleTextureMaterialFromFile(
                    textureFile);
            return finMaterial;
          })
          .ToArray();

    var finMaterial = textureMaterials.Length > 0
        ? textureMaterials[0]
        : finModel.MaterialManager.AddNullMaterial();

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

          var finPrimitive = finMesh.AddTriangles(triangles);
          finPrimitive.SetMaterial(finMaterial);

          break;
        }
      }
    }

    return finModel;
  }

  private class BallAttributes {
    public string Id { get; set; }
    public string? Geometry { get; set; }
    public string? Texture { get; set; }
    public bool MapFromFile { get; set; }
    public bool NoEnvMap { get; set; }
  }

  private static IList<BallAttributes> ReadAllBallAttributes_(
      IReadOnlyTreeFile file) {
    using var tr = new SchemaTextReader(file.OpenRead());

    var allAttributes = new List<BallAttributes>();

    BallAttributes currentAttributes = default!;
    while (!tr.Eof) {
      var line = tr.ReadLine().Trim();

      if (line.StartsWith('[')) {
        var id = line[1..].SubstringUpTo(']');
        currentAttributes = new BallAttributes { Id = id };
        allAttributes.Add(currentAttributes);
      } else {
        var equalsParts = line.Split('=');
        if (equalsParts.Length > 1) {
          var before = equalsParts[0].Trim();
          var after = equalsParts[1].Trim();

          switch (before) {
            case "Geometry": {
              currentAttributes.Geometry = after;
              break;
            }
            case "MapFromFile": {
              currentAttributes.MapFromFile = after == "1";
              break;
            }
            case "NoEnvMap": {
              currentAttributes.NoEnvMap = after == "1";
              break;
            }
            case "Texture": {
              currentAttributes.Texture = after;
              break;
            }
          }
        }
      }
    }

    return allAttributes;
  }
}