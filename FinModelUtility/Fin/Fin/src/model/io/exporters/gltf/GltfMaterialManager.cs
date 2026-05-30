using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

using CommunityToolkit.HighPerformance.Helpers;

using fin.data.lazy;
using fin.image;
using fin.image.util;
using fin.language.equations.fixedFunction;
using fin.language.equations.fixedFunction.util;
using fin.math;
using fin.math.floats;
using fin.math.matrix.four;
using fin.math.matrix.three;
using fin.model.util;

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

          gltfMaterialBuilder.WithAlpha(finMaterial.GetTransparencyType());

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

              new FixedFunctionsEquationsLightingExtractor()
                  .ExtractLightingChannels(
                      equations,
                      out var diffuseColorAlpha,
                      out var specularColorAlpha,
                      out var ambientColorAlpha,
                      out var otherColorAlpha);

              var (diffuseColor, diffuseAlpha) = diffuseColorAlpha;
              var (specularColor, specularAlpha) = specularColorAlpha;
              var (ambientColor, ambientAlpha) = ambientColorAlpha;
              var (otherColor, otherAlpha) = otherColorAlpha;

              gltfMaterialBuilder.WithMetallicRoughnessIfLit(
                  finMaterial,
                  out var usesLighting,
                  !diffuseColor.IsZero(),
                  !specularColor.IsZero(),
                  !ambientColor.IsZero());

              var compiledTexture = fixedFunctionMaterial.CompiledTexture;

              if (!usesLighting) {
                if (compiledTexture != null) {
                  gltfMaterialBuilder
                      .WithAlpha(compiledTexture.TransparencyType)
                      .UseChannel(KnownChannel.BaseColor)
                      .UseTexture(compiledTexture,
                                  gltfImageBuilderByFinTexture[compiledTexture]);
                } else {
                  var (primaryTexture, primaryColor)
                      = FixedFunctionsEquationsUtil
                          .ExtractPrimaryTextureAndColor(
                              fixedFunctionMaterial,
                              (diffuseColor.Add(otherColor),
                               otherAlpha));

                  gltfMaterialBuilder.WithAlpha(
                      primaryTexture?.TransparencyType ??
                      (primaryColor.W.IsRoughly1()
                          ? TransparencyType.OPAQUE
                          : TransparencyType.TRANSPARENT));

                  if (primaryTexture != null) {
                    gltfMaterialBuilder
                        .UseChannel(KnownChannel.BaseColor)
                        .UseTexture(primaryTexture,
                                    gltfImageBuilderByFinTexture[primaryTexture]);
                  }

                  if (!primaryColor.IsRoughly(Vector4.One)) {
                    gltfMaterialBuilder.WithBaseColor(primaryColor);
                  }
                }
              } else {
                if (compiledTexture != null) {
                  gltfMaterialBuilder
                      .WithAlpha(compiledTexture.TransparencyType)
                      .UseChannel(KnownChannel.BaseColor)
                      .UseTexture(compiledTexture,
                                  gltfImageBuilderByFinTexture[compiledTexture]);
                } else {
                  var (diffusePrimaryTexture, diffusePrimaryColor)
                      = FixedFunctionsEquationsUtil
                          .ExtractPrimaryTextureAndColor(
                              fixedFunctionMaterial,
                              (diffuseColor, otherAlpha));

                  gltfMaterialBuilder.WithAlpha(
                      diffusePrimaryTexture?.TransparencyType ??
                      (diffusePrimaryColor.W.IsRoughly1()
                          ? TransparencyType.OPAQUE
                          : TransparencyType.TRANSPARENT));

                  if (diffusePrimaryTexture != null) {
                    gltfMaterialBuilder
                        .UseChannel(KnownChannel.BaseColor)
                        .UseTexture(diffusePrimaryTexture,
                                    gltfImageBuilderByFinTexture[
                                        diffusePrimaryTexture]);
                  }

                  if (!diffusePrimaryColor.IsRoughly(Vector4.One)) {
                    gltfMaterialBuilder.WithBaseColor(diffusePrimaryColor);
                  }
                }

                var (specularPrimaryTexture, specularPrimaryColor)
                    = FixedFunctionsEquationsUtil
                        .ExtractPrimaryTextureAndColor(
                            fixedFunctionMaterial,
                            specularColorAlpha);
                if (specularPrimaryTexture != null) {
                  gltfMaterialBuilder
                      .UseChannel(KnownChannel.SpecularColor)
                      .UseTexture(specularPrimaryTexture,
                                  gltfImageBuilderByFinTexture[
                                      specularPrimaryTexture]);
                }

                var specularRgb = specularPrimaryColor.Xyz();
                if (!specularRgb.IsRoughly(Vector3.One)) {
                  gltfMaterialBuilder.WithChannelParam(
                      KnownChannel.SpecularColor,
                      KnownProperty.RGB,
                      specularRgb);
                }

                // TODO: Handle ambient/emissive

                var normalTexture = fixedFunctionMaterial.NormalTexture;
                if (normalTexture != null) {
                  gltfMaterialBuilder
                      .UseChannel(KnownChannel.Normal)
                      .UseTexture(normalTexture, gltfImageBuilderByFinTexture[normalTexture]);
                }
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