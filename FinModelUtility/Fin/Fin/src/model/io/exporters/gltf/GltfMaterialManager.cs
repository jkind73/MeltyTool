using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CommunityToolkit.HighPerformance.Helpers;

using fin.data.lazy;
using fin.image;
using fin.model.util;
using fin.image.util;

using SharpGLTF.Materials;
using SharpGLTF.Memory;
using SharpGLTF.Schema2;

using AlphaMode = SharpGLTF.Materials.AlphaMode;

namespace fin.model.io.exporters.gltf;

public static class GltfMaterialBuilder {
  private readonly struct Fin2GltfImageConverter(
      IReadOnlyImage[] finImages,
      IDictionary<IReadOnlyImage, MemoryImage> gltfImageByFinImage)
      : IAction {
    public void Invoke(int i) {
      var finImage = finImages[i];

      using var imageStream = new MemoryStream();
      finImage.ExportToStream(imageStream, LocalImageFormat.PNG);

      gltfImageByFinImage[finImage] =
          new MemoryImage(imageStream.ToArray());
    }
  }

  public static IDictionary<IReadOnlyMaterial, Material> GetMaterials(
      ModelRoot gltfModelRoot,
      IReadOnlyMaterialManager finMaterialManager)
    => ConvertMaterials_(finMaterialManager)
           .ToDictionary(tuple => tuple.Item1,
                         tuple => gltfModelRoot.CreateMaterial(tuple.Item2));

  public static IDictionary<IReadOnlyMaterial, MaterialBuilder> GetMaterialBuilders(
      IReadOnlyMaterialManager finMaterialManager)
    => ConvertMaterials_(finMaterialManager)
           .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

  private static IEnumerable<(IReadOnlyMaterial, MaterialBuilder)> ConvertMaterials_(
      IReadOnlyMaterialManager finMaterialManager) {
    var finImages = finMaterialManager.Textures
                                      .Select(texture => texture.Image)
                                      .Distinct()
                                      .ToArray();
    var gltfImageByFinImage
        = new ConcurrentDictionary<IReadOnlyImage, MemoryImage>();
    ParallelHelper.For(0,
                       finImages.Length,
                       new Fin2GltfImageConverter(
                           finImages,
                           gltfImageByFinImage));

    var gltfImageBuilderByFinTexture
        = new LazyDictionary<IReadOnlyTexture, ImageBuilder>(finTexture => {
          ImageBuilder gltfImageBuilder = gltfImageByFinImage[finTexture.Image];
          gltfImageBuilder.Name = finTexture.Name;
          gltfImageBuilder.AlternateWriteFileName = finTexture.ValidFileName;
          return gltfImageBuilder;
        });

    // TODO: Update this if GLTF is ever extended...
    return finMaterialManager.All.Select(
        finMaterial => {
          var gltfMaterialBuilder = new MaterialBuilder(finMaterial.Name)
                  .WithDoubleSide(
                      finMaterial.CullingMode
                          switch {
                              CullingMode.SHOW_FRONT_ONLY => false,
                              // Darn, guess we can't support this.
                              CullingMode.SHOW_BACK_ONLY => true,
                              CullingMode.SHOW_BOTH      => true,
                              // Darn, guess we can't support this either.
                              CullingMode.SHOW_NEITHER => false,
                              _ => throw new ArgumentOutOfRangeException()
                          });

          gltfMaterialBuilder.WithAlpha(finMaterial.GetTransparencyType() switch {
              TransparencyType.OPAQUE => AlphaMode.OPAQUE,
              TransparencyType.MASK => AlphaMode.MASK,
              TransparencyType.TRANSPARENT => AlphaMode.BLEND,
              _ => throw new ArgumentOutOfRangeException()
          });

          switch (finMaterial) {
            case IStandardMaterial standardMaterial: {
              gltfMaterialBuilder.WithMetallicRoughnessIfLit(finMaterial);

              var diffuseTexture = standardMaterial.DiffuseTexture;
              if (diffuseTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.BaseColor)
                    .UseTexture(diffuseTexture,
                                gltfImageBuilderByFinTexture[diffuseTexture]);
              }

              var diffuseColor = standardMaterial.DiffuseColor;
              if (diffuseColor != null) {
                gltfMaterialBuilder.WithBaseColor(diffuseColor.Value);
              }

              var normalTexture = standardMaterial.NormalTexture;
              if (normalTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Normal)
                    .UseTexture(normalTexture,
                                gltfImageBuilderByFinTexture[normalTexture]);
              }

              var emissiveTexture = standardMaterial.EmissiveTexture;
              if (emissiveTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Emissive)
                    .UseTexture(emissiveTexture,
                                gltfImageBuilderByFinTexture[emissiveTexture]);
              }

              var specularTexture = standardMaterial.SpecularTexture;
              if (specularTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.SpecularFactor)
                    .UseTexture(specularTexture,
                                gltfImageBuilderByFinTexture[specularTexture]);
              }

              var ambientOcclusionTexture
                  = standardMaterial.AmbientOcclusionTexture;
              if (ambientOcclusionTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Occlusion)
                    .UseTexture(ambientOcclusionTexture,
                                gltfImageBuilderByFinTexture[ambientOcclusionTexture]);
              }

              break;
            }
            case IFixedFunctionMaterial fixedFunctionMaterial: {
              var equations = fixedFunctionMaterial.Equations;
              var usesDiffuseLighting
                  = equations
                      .DoOutputsDependOn(
                          Enumerable
                              .Range(0, MaterialConstants.MAX_LIGHTS)
                              .SelectMany<int, FixedFunctionSource>(i => [
                                  FixedFunctionSource.LIGHT_DIFFUSE_COLOR_0 +
                                  i,
                                  FixedFunctionSource.LIGHT_DIFFUSE_ALPHA_0 +
                                  i
                              ])
                              .Concat([
                                  FixedFunctionSource
                                      .LIGHT_DIFFUSE_COLOR_MERGED,
                                  FixedFunctionSource
                                      .LIGHT_DIFFUSE_ALPHA_MERGED
                              ])
                              .ToArray());
              var usesSpecularLighting
                  = equations
                      .DoOutputsDependOn(
                          Enumerable
                              .Range(0, MaterialConstants.MAX_LIGHTS)
                              .SelectMany<int
                                  , FixedFunctionSource>(
                                  i => [
                                      FixedFunctionSource.LIGHT_SPECULAR_COLOR_0 + i,
                                      FixedFunctionSource.LIGHT_SPECULAR_ALPHA_0 + i
                                  ])
                              .Concat([
                                  FixedFunctionSource.LIGHT_SPECULAR_COLOR_MERGED,
                                  FixedFunctionSource.LIGHT_SPECULAR_ALPHA_MERGED
                              ])
                              .ToArray());

              gltfMaterialBuilder.WithMetallicRoughnessIfLit(
                  finMaterial,
                  usesDiffuseLighting,
                  usesSpecularLighting);

              var texture = PrimaryTextureFinder.GetFor(finMaterial);
              if (texture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.BaseColor)
                    .UseTexture(texture, gltfImageBuilderByFinTexture[texture]);
              }

              var normalTexture = fixedFunctionMaterial.NormalTexture;
              if (normalTexture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.Normal)
                    .UseTexture(normalTexture, gltfImageBuilderByFinTexture[normalTexture]);
              }

              break;
            }
            case IColorMaterial finColorMaterial: {
              gltfMaterialBuilder.WithMetallicRoughnessIfLit(finMaterial)
                                 .WithBaseColor(finColorMaterial.Color);
              break;
            }
            default: {
              gltfMaterialBuilder.WithMetallicRoughnessIfLit(finMaterial);

              var texture = PrimaryTextureFinder.GetFor(finMaterial);
              if (texture != null) {
                gltfMaterialBuilder
                    .UseChannel(KnownChannel.BaseColor)
                    .UseTexture(texture, gltfImageBuilderByFinTexture[texture]);
              }

              break;
            }
          }

          return (finMaterial, gltfMaterial: gltfMaterialBuilder);
        });
  }
}