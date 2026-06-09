using System.Numerics;

using bar.schema;

using fin.io;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public sealed record UvmdModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public sealed class UvmdModelFileImporter
    : IModelImporter<UvmdModelFileBundle> {
  public IModel Import(UvmdModelFileBundle fileBundle) {
    var files = fileBundle.MainFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files
    };

    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return finModel;
    }

    var uvmd
        = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                 Endianness.BigEndian).ReadNew<Uvmd>();

    var finSkeletonRoot = finModel.Skeleton.Root;
    finSkeletonRoot.Transform.LocalRotation
        = Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0);

    var finSkin = finModel.Skin;
    var allFinBoneWeights
        = uvmd.Transforms
              .Select(transform => finSkeletonRoot.AddChild(transform))
              .Select(bone => finSkin.GetOrCreateBoneWeights(
                          VertexSpace.RELATIVE_TO_BONE,
                          bone))
              .ToArray();

    var finMesh = finSkin.AddMesh();

    var uvmdLod0ModelParts = uvmd.Lods[0].ModelParts;
    for (var i = 0; i < uvmdLod0ModelParts.Length; ++i) {
      var finBoneWeights = allFinBoneWeights[i];

      var uvmdModelPart = uvmdLod0ModelParts[i];
      foreach (var uvmdMaterialMesh in uvmdModelPart.MaterialMeshes) {
        // TODO: Handle billboards
        // TODO: Handle materials, https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/MaterialRenderer.ts#L163

        var finVertices = uvmdMaterialMesh.Vertices.Select(v => {
                                            var finVertex
                                                = finSkin.AddVertex(
                                                    v.Position.X,
                                                    v.Position.Y,
                                                    v.Position.Z);
                                            finVertex.SetUv(v.TexCoords.X / 32f,
                                              v.TexCoords.Y / 32f);
                                            finVertex.SetColor(v.Color);
                                            finVertex.SetBoneWeights(
                                                finBoneWeights);
                                            return (IReadOnlyVertex) finVertex;
                                          })
                                          .ToArray();

        finMesh.AddTriangles(
            uvmdMaterialMesh.Triangles.Select(t => (finVertices[t.Item1],
                                                    finVertices[t.Item2],
                                                    finVertices[t.Item3]))
                            .ToArray());
      }
    }

    return finModel;
  }
}