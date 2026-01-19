using System.Numerics;

using fin.math.floats;
using fin.model;
using fin.model.impl;

using gdl.schema.anim;
using gdl.schema.objects;

using Object = gdl.schema.objects.Object;


namespace gdl.api;

public static class MeshUtil {
  public static IModel AddObjectMesh(
      ModelImpl finModel,
      IReadOnlyBoneWeights? finBoneWeights,
      in Matrix4x4 matrix,
      ObjectDefinition definition,
      Object obj,
      LazyGdlMaterials lazyFinMaterials,
      MbFlags mbFlags,
      out IMesh outRootMesh) {
    var finSkin = finModel.Skin;
    var finRootMesh = finSkin.AddMesh();
    finRootMesh.Name = definition.Name;

    outRootMesh = finRootMesh;

    for (var m = 0; m < (obj.SubObjectModels?.All.Count ?? 0); ++m) {
      var finSubMesh = finRootMesh.AddSubMesh();

      var gdlMesh = obj.SubObjectModels.All[m];
      var textureIndex = gdlMesh.SubObject.TextureIndex;

      foreach (var gdlPrimitive in gdlMesh.Primitives) {
        if (gdlPrimitive.Positions.Count < 3) {
          continue;
        }

        var finVertices = new IReadOnlyVertex[gdlPrimitive.Positions.Count];
        for (var i = 0; i < gdlPrimitive.Positions.Count; ++i) {
          var p = gdlPrimitive.Positions[i];
          var normal = gdlPrimitive.Normals[i];

          p /= 128f;

          // For some inexplicable reason, the meshes are mirrored.
          p.X *= -1;
          normal.X *= -1;

          p = Vector3.Transform(p, matrix);
          normal = Vector3.TransformNormal(normal, matrix);

          var finVertex = finSkin.AddVertex(p);

          finVertex.SetLocalNormal(normal);

          var uv = gdlPrimitive.Uvs[i];
          finVertex.SetUv(0, uv.Value);
          finVertex.SetUv(1, uv.LightmapUv ?? Vector2.Zero);

          if (gdlPrimitive.VertexColors.Count > 0) {
            finVertex.SetColor(gdlPrimitive.VertexColors[i]);
          }

          if (finBoneWeights != null) {
            finVertex.SetBoneWeights(finBoneWeights);
          }

          finVertices[i] = finVertex;
        }

        var facesDrawn = gdlPrimitive.FacesDrawn;
        var triangleVertices
            = new List<(IReadOnlyVertex, IReadOnlyVertex, IReadOnlyVertex)>(facesDrawn.Count(b => b));

        var faceDir = gdlPrimitive.FaceDir.IsRoughly(-1f) ? 1 : 0;
        for (var f = 0; f < facesDrawn.Count; ++f) {
          if (!facesDrawn[f]) {
            continue;
          }

          if (((f + faceDir) & 1) == 1) {
            triangleVertices.Add((finVertices[f + 0],
                                  finVertices[f + 1],
                                  finVertices[f + 2]));
          } else {
            triangleVertices.Add((finVertices[f + 1],
                                  finVertices[f + 0],
                                  finVertices[f + 2]));
          }
        }

        var finPrimitive = finSubMesh.AddTriangles(triangleVertices);
        finPrimitive.SetMaterial(
            lazyFinMaterials[(textureIndex, 
                              gdlMesh.SubObject.LmIndex,
                              gdlPrimitive.VertexColors.Count > 0,
                              mbFlags)]);
        finPrimitive.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
      }
    }

    return finModel;
  }
}