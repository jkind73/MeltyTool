using System.Drawing;
using System.Numerics;

using fin.animation.keyframes;
using fin.color;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enums;
using fin.util.hex;

using gdl.schema.anim;
using gdl.schema.objects;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

using SequenceType = gdl.schema.anim.SequenceType;

namespace gdl.api;

public sealed class GauntletDarkLegacyModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile ObjectsFile { get; init; }
  public required IReadOnlyTreeFile AnimFile { get; init; }
  public required IReadOnlyTreeFile TexturesFile { get; init; }

  public IReadOnlyTreeFile MainFile => this.ObjectsFile;
}

public sealed class GauntletDarkLegacyModelImporter
    : IModelImporter<GauntletDarkLegacyModelFileBundle> {
  public IModel Import(GauntletDarkLegacyModelFileBundle fileBundle) {
    var objects = fileBundle.ObjectsFile.ReadNew<Objects>();
    var anim = fileBundle.AnimFile.ReadNew<Anim>();

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyGenericFile>([
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ])
    };

    var finTextures = new List<IReadOnlyTexture>();
    {
      using var textureBr
          = fileBundle.TexturesFile.OpenReadAsBinary(Endianness.BigEndian);

      foreach (var gdlTexture in objects.Textures) {
        textureBr.Position = gdlTexture.TextureDataPointer;

        IImage? finImage = null;
        if (!gdlTexture.Format.IsIndexed(out var paletteCount,
                                         out var pixelFormat)) {
          switch (gdlTexture.Format) {
            case ImageFormat.RGBA_5551: {
              finImage = TiledImageReader
                         .New(gdlTexture.Width,
                              gdlTexture.Height,
                              4,
                              4,
                              new Rgba5551PixelReader())
                         .ReadImage(textureBr);
              break;
            }
          }
        } else {
          IColor[]? palette = null;
          IImage<L8>? indexedImage = null;
          switch (pixelFormat) {
            case PixelFormat.RGBA5551: {
              palette = textureBr.ReadUInt16s(paletteCount)
                                 .Select(v => {
                                   ColorUtil.SplitRgb5A1(v,
                                     out var r,
                                     out var g,
                                     out var b,
                                     out var a);
                                   return FinColor.FromRgbaBytes(
                                       r,
                                       g,
                                       b,
                                       a);
                                 })
                                 .ToArray();
              break;
            }
            case PixelFormat.RGB555: {
              palette = textureBr.ReadUInt16s(paletteCount)
                                 .Select(v => {
                                   ColorUtil.SplitRgb5A1(v,
                                     out var r,
                                     out var g,
                                     out var b,
                                     out _);
                                   return FinColor.FromRgbBytes(
                                       r,
                                       g,
                                       b);
                                 })
                                 .ToArray();
              break;
            }
          }

          switch (paletteCount) {
            case 16: {
              indexedImage = TiledImageReader.New(
                                                 gdlTexture.Width,
                                                 gdlTexture.Height,
                                                 8,
                                                 8,
                                                 new P4PixelReader())
                                             .ReadImage(textureBr);
              break;
            }
            case 256: {
              indexedImage = TiledImageReader.New(
                                                 gdlTexture.Width,
                                                 gdlTexture.Height,
                                                 8,
                                                 4,
                                                 new L8PixelReader())
                                             .ReadImage(textureBr);
              break;
            }
          }

          if (palette != null && indexedImage != null) {
            finImage = new IndexedImage8(pixelFormat, indexedImage, palette);
          }
        }

        finImage ??= FinImage.CreateFromColor(Color.Magenta,
                                              gdlTexture.Width,
                                              gdlTexture.Height);

        var finTexture = finModel.MaterialManager.CreateTexture(finImage);
        finTexture.Name
            = $"texture{finTexture.Index}_{gdlTexture.Format}_{gdlTexture.TextureDataPointer.ToHex()}";

        finTextures.Add(finTexture);
      }
    }
    var lazyFinMaterials
        = new LazyDictionary<int, IReadOnlyMaterial>(index => {
          var finTexture = finTextures[index];
          return finModel.MaterialManager.AddTextureMaterial(finTexture);
        });

    var finBones = new List<IReadOnlyBone>();
    foreach (var gdlSkeleton in anim.Skeletons) {
      var gdlBones = gdlSkeleton.Data.Bones;

      var gdlBonesByParent = new ListDictionary<Bone?, Bone>();
      foreach (var gdlBone in gdlBones) {
        gdlBonesByParent.Add(
            gdlBone.ParentId == -1 ? null : gdlBones[gdlBone.ParentId],
            gdlBone);
      }

      var boneQueue = new FinTuple3Queue<Bone, IBone, Matrix4x4>(
          gdlBonesByParent[null]
              .Select(rootGdlBone => (
                          rootGdlBone,
                          finModel.Skeleton.Root,
                          Matrix4x4.Identity)));
      var finBoneByGdlBone = new Dictionary<Bone, IReadOnlyBone>();

      while (boneQueue.TryDequeue(out var gdlBone,
                                  out var finParentBone,
                                  out var invertedParentWorldMatrix)) {
        var worldMatrix = Matrix4x4.CreateTranslation(gdlBone.Position);
        var localMatrix = worldMatrix * invertedParentWorldMatrix;

        var finBone = finParentBone.AddChild(localMatrix);
        finBone.Name = gdlBone.Name;

        finBones.Add(finBone);
        finBoneByGdlBone[gdlBone] = finBone;

        if (gdlBonesByParent.TryGetList(gdlBone, out var childGdlBones)) {
          boneQueue.Enqueue(
              childGdlBones.Select(childGdlBone => (
                                       childGdlBone,
                                       finBone,
                                       worldMatrix.AssertInvert())));
        }
      }

      var gdlAnimationData = gdlSkeleton.Data.AnimationData;
      var gdlAnimationHeaders = gdlSkeleton.Data.AnimationHeaders;
      for (var i = 0; i < gdlAnimationHeaders.Length; ++i) {
        var gdlAnimationHeader = gdlAnimationHeaders[i];

        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = gdlAnimationHeader.Name;
        finAnimation.FrameCount = gdlAnimationHeader.FrameCount;
        finAnimation.FrameRate = gdlAnimationHeader.FrameRate;
        finAnimation.UseLoopingInterpolation = gdlAnimationHeader.Loop;

        foreach (var gdlBone in gdlAnimationData.SequencesByBone.Keys) {
          var gdlSequence = gdlAnimationData.SequencesByBone[gdlBone][i];
          if (gdlSequence.Size == 0) {
            continue;
          }

          var finBoneTracks
              = finAnimation.GetOrCreateBoneTracks(finBoneByGdlBone[gdlBone]);

          var gdlSequenceFrameCount = gdlSequence.FrameCount;
          finAnimation.FrameCount
              = Math.Max(finAnimation.FrameCount, gdlSequenceFrameCount);

          var rotationKeyframes
              = finBoneTracks.UseSeparateEulerRadiansKeyframes(
                  gdlSequenceFrameCount);

          var positionKeyframes = finBoneTracks.UseSeparateTranslationKeyframes(
              gdlSequenceFrameCount);
          var scaleKeyframes
              = finBoneTracks.UseSeparateScaleKeyframes(gdlSequenceFrameCount);

          for (var f = 0; f < gdlSequenceFrameCount; ++f) {
            var rotationX = gdlSequence.RotationXs[f];
            if (rotationX != null) {
              rotationKeyframes.SetKeyframe(0, f, rotationX.Value);
            }

            var rotationY = gdlSequence.RotationYs[f];
            if (rotationY != null) {
              rotationKeyframes.SetKeyframe(1, f, rotationY.Value);
            }

            var rotationZ = gdlSequence.RotationZs[f];
            if (rotationZ != null) {
              rotationKeyframes.SetKeyframe(2, f, rotationZ.Value);
            }

            var positionX = gdlSequence.PositionXs[f];
            if (positionX != null) {
              positionKeyframes.SetKeyframe(0, f, positionX.Value);
            }

            var positionY = gdlSequence.PositionYs[f];
            if (positionY != null) {
              positionKeyframes.SetKeyframe(1, f, positionY.Value);
            }

            var positionZ = gdlSequence.PositionZs[f];
            if (positionZ != null) {
              positionKeyframes.SetKeyframe(2, f, positionZ.Value);
            }

            var scaleX = gdlSequence.ScaleXs[f];
            if (scaleX != null) {
              scaleKeyframes.SetKeyframe(0, f, scaleX.Value);
            }

            var scaleY = gdlSequence.ScaleYs[f];
            if (scaleY != null) {
              scaleKeyframes.SetKeyframe(1, f, scaleY.Value);
            }

            var scaleZ = gdlSequence.ScaleZs[f];
            if (scaleZ != null) {
              scaleKeyframes.SetKeyframe(2, f, scaleZ.Value);
            }
          }
        }
      }
    }

    var finSkin = finModel.Skin;
    foreach (var (definition, obj) in objects.ObjectDefinitions.Zip(
                 objects.RootObjects)) {
      var finMesh = finSkin.AddMesh();
      finMesh.Name = definition.Name;

      var finBone
          = finBones.FirstOrDefault(b => finMesh.Name.Contains(b.Name!));
      var finBoneWeights
          = finBone != null
              ? finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                               finBone)
              : null;

      for (var m = 0; m < (obj.Mesh?.All.Count ?? 0); ++m) {
        var gdlMesh = obj.Mesh.All[m];

        var textureIndex = m == 0
            ? obj.SubObject0TextureIndex
            : obj.SubObjects[m - 1].TextureIndex;

        foreach (var gdlPrimitive in gdlMesh.Primitives) {
          if (gdlPrimitive.Positions.Count < 3) {
            continue;
          }

          var finVertices
              = gdlPrimitive
                .Positions
                .Select((p, i) => {
                  var finVertex = finSkin.AddVertex(p / 128f);

                  if (gdlPrimitive.Uvs.Count >=
                      gdlPrimitive.Positions.Count) {
                    finVertex.SetUv(gdlPrimitive.Uvs[i].Value);
                  } else {
                    finVertex.SetUv(0, 0);
                  }

                  if (finBoneWeights != null) {
                    finVertex.SetBoneWeights(finBoneWeights);
                  }

                  return finVertex;
                })
                .ToArray();

          var finPrimitive = finMesh.AddTriangleStrip(finVertices);
          finPrimitive.SetMaterial(lazyFinMaterials[textureIndex]);
        }
      }
    }

    return finModel;
  }
}