using System.Numerics;

using fin.data.dictionaries;
using fin.data.lazy;
using fin.image;
using fin.io;
using fin.language.equations.fixedFunction;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.util.sets;

using rollingMadness.schema;

namespace rollingMadness.api;

public record AseMeshModelFileBundle(
    IReadOnlyTreeFile AseMeshFile,
    IReadOnlyTreeDirectory TextureDirectory) : IModelFileBundle {
  public IReadOnlyTreeFile MainFile => this.AseMeshFile;
}

public sealed class AseMeshModelImporter
    : IModelImporter<AseMeshModelFileBundle> {
  public IModel Import(AseMeshModelFileBundle fileBundle) {
    var aseMesh = fileBundle.AseMeshFile.ReadNew<AseMesh>();

    var frameCount = checked((int) aseMesh.AdditionalFrameCount + 1);
    if (aseMesh.Vertices.Length % frameCount != 0) {
      throw new InvalidDataException(
          $"Vertex count {aseMesh.Vertices.Length} is not divisible by " +
          $"the ASE mesh frame count {frameCount}.");
    }

    var verticesPerFrame = aseMesh.Vertices.Length / frameCount;
    if (verticesPerFrame == 0 && aseMesh.Triangles.Length > 0) {
      throw new InvalidDataException(
          "ASE mesh contains triangles but has no vertices.");
    }
    if (aseMesh.UvDatas.Length != verticesPerFrame) {
      throw new InvalidDataException(
          $"UV count {aseMesh.UvDatas.Length} does not match the " +
          $"per-frame vertex count {verticesPerFrame}.");
    }

    var fileSet = fileBundle.AseMeshFile.AsFileSet();
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = fileSet,
    };

    var finMaterialManager = finModel.MaterialManager;
    var lazyTextureMap
        = new LazyDictionary<(string aseImageName, int uvIndex, bool isLightmap),
            ITexture>(tuple => {
          var (aseImageName, uvIndex, isLightmap) = tuple;

          var imageFile = fileBundle.TextureDirectory.AssertGetExistingFile(
              aseImageName);
          fileSet.Add(imageFile);
          var finImage = FinImage.FromFile(imageFile);

          var finTexture = finMaterialManager.CreateTexture(finImage);
          // NameWithoutExtension truncates at the first dot, collapsing
          // lev1.ase.0.png through lev1.ase.5.png to the same name ("lev1").
          // Remove only the final extension so exporters retain six identities.
          finTexture.Name = System.IO.Path.GetFileNameWithoutExtension(
              imageFile.Name.ToString());
          finTexture.MinFilter = TextureMinFilter.LINEAR;

          finTexture.UvIndex = uvIndex;

          if (!isLightmap) {
            finTexture.WrapModeU = finTexture.WrapModeV = WrapMode.REPEAT;
          }

          return finTexture;
        });
    var lazyMaterialMap
        = new LazyDictionary<(int aseMainTextureIndex, int aseDecalTextureIndex,
            int aseLightmapIndex), IReadOnlyMaterial>(tuple => {
          var (aseMainTextureIndex, aseDecalTextureIndex, aseLightmapIndex) = tuple;

          var finMaterial = finMaterialManager.AddFixedFunctionMaterial();

          var finMainTexture = lazyTextureMap[
              (aseMesh.ImageNames[aseMainTextureIndex].Value, 0, false)];
          finMaterial.Name = finMainTexture.Name;

          var (diffuseColor, diffuseAlpha)
              = finMaterial.AddTextureSourceColorAlpha(finMainTexture);

          var equations = finMaterial.Equations;
          if (aseDecalTextureIndex >= 0) {
            var finDecalTexture = lazyTextureMap[
                (aseMesh.ImageNames[aseDecalTextureIndex].Value,
                 1, false)];

            finMaterial.Name += $"/{finDecalTexture.Name}";

            var (decalColor, decalAlpha)
                = finMaterial.AddTextureSourceColorAlpha(finDecalTexture);

            diffuseColor = equations.ColorOps.MixWithScalar(
                    diffuseColor,
                    decalColor,
                    decalAlpha);
          }

          IColorValue ambientColor = equations.CreateOrGetColorInput(
                  FixedFunctionSource.LIGHT_AMBIENT_COLOR);
          if (aseLightmapIndex >= 0) {
            var aseLightmapName
                = aseMesh.LightmapNames[aseLightmapIndex].Value;
            var finLightmapTexture = lazyTextureMap[(aseLightmapName, 2, true)];
            finMaterial.Name += $"/{finLightmapTexture.Name}";
            ambientColor = ambientColor.Multiply(
                finMaterial.AddTextureSourceColor(finLightmapTexture));
          }

          var outputColorAlpha
              = equations.GenerateLighting((diffuseColor, diffuseAlpha),
                                           ambientColor);

          equations.SetOutputColorAlpha(outputColorAlpha);

          return finMaterial;
        });

    var finSkin = finModel.Skin;
    var finMesh = finSkin.AddMesh();

    // UVs and triangle indices address one frame. Additional vertex frames are
    // stored consecutively and are represented as morph targets below.
    var baseFrameVertices = aseMesh.Vertices.AsSpan(0, verticesPerFrame)
                                           .ToArray();
    var finVertices
        = Enumerable.Zip(baseFrameVertices, aseMesh.UvDatas)
                    .Select(tuple => {
                      var (aseVertex, aseUvData) = tuple;
                      var finVertex = finSkin.AddVertex(aseVertex.Position);
                      if (aseVertex.Normal.LengthSquared() > 0) {
                        finVertex.SetLocalNormal(
                            Vector3.Normalize(aseVertex.Normal));
                      }

                      finVertex.SetUv(0, aseUvData.Uv0);
                      finVertex.SetUv(1, aseUvData.Uv1);
                      finVertex.SetUv(2, aseUvData.LightmapUv);

                      return (IReadOnlyVertex) finVertex;
                    })
                    .ToArray();

    var morphTargetFrames = new IMorphTarget?[frameCount];
    for (var frameIndex = 1; frameIndex < frameCount; ++frameIndex) {
      var morphTarget = finModel.AnimationManager.AddMorphTarget();
      morphTarget.Name = $"frame_{frameIndex}";
      morphTargetFrames[frameIndex] = morphTarget;

      var frameOffset = frameIndex * verticesPerFrame;
      for (var vertexIndex = 0; vertexIndex < verticesPerFrame; ++vertexIndex) {
        var aseVertex = aseMesh.Vertices[frameOffset + vertexIndex];
        var finVertex = finVertices[vertexIndex];
        morphTarget.SetNewLocalPosition(finVertex, aseVertex.Position);

        if (aseVertex.Normal.LengthSquared() > 0) {
          morphTarget.SetNewLocalNormal(finVertex,
                                        Vector3.Normalize(aseVertex.Normal));
        }
      }
    }

    if (frameCount > 1) {
      var animation = finModel.AnimationManager.AddAnimation();
      animation.Name = "vertex_animation";
      animation.FrameCount = frameCount;
      animation.FrameRate = aseMesh.AnimationDuration > 0
                                ? (frameCount - 1) /
                                  aseMesh.AnimationDuration
                                : 30;
      animation.UseLoopingInterpolation = true;
      animation.SetMorphTargetFrames(morphTargetFrames);
    }

    var trianglesByMaterialIndex
        = aseMesh.Triangles.ToListDictionary(t => (t.MainTextureIndex,
                                                   t.DecalTextureIndex,
                                                   t.LightmapIndex));

    var mainAndDecalAndLightmapIndexes = trianglesByMaterialIndex.Keys.OrderBy(k => k.MainTextureIndex)
                                       .ThenBy(k => k.DecalTextureIndex)
                                       .ThenBy(k => k.LightmapIndex);

    foreach (var mainAndDecalAndLightmapIndex in mainAndDecalAndLightmapIndexes) {
      var finMaterial = lazyMaterialMap[mainAndDecalAndLightmapIndex];

      var aseTriangles = trianglesByMaterialIndex[mainAndDecalAndLightmapIndex];
      foreach (var triangle in aseTriangles) {
        if (triangle.Vertex1 >= verticesPerFrame ||
            triangle.Vertex2 >= verticesPerFrame ||
            triangle.Vertex3 >= verticesPerFrame) {
          throw new InvalidDataException(
              $"Triangle references a vertex outside the base frame " +
              $"(vertex count: {verticesPerFrame}).");
        }
      }
      var triangleVertices = aseTriangles.Select(t => (
                                                     finVertices[t.Vertex1],
                                                     finVertices[t.Vertex2],
                                                     finVertices[t.Vertex3]))
                                         .ToArray();

      finMesh.AddTriangles(triangleVertices)
             .SetMaterial(finMaterial);
    }


    return finModel;
  }
}
