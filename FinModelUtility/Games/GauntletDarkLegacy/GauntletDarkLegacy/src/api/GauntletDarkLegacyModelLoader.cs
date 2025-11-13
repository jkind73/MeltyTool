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

          var rotationKeyframes
              = finBoneTracks.UseSeparateEulerRadiansKeyframes();
          rotationKeyframes.ConvertRadiansToQuaternionImpl
              = QuaternionUtil.CreateZxy;

          var positionKeyframes
              = finBoneTracks.UseSeparateTranslationKeyframes();
          var scaleKeyframes = finBoneTracks.UseSeparateScaleKeyframes();

          for (var f = 0; f < finAnimation.FrameCount; ++f) {
            var rotation = gdlSequence.Rotations[f];
            if (gdlSequence.Type.CheckFlag(SequenceType.ROTATION_X)) {
              rotationKeyframes.SetKeyframe(0, f, rotation.X);
            }

            if (gdlSequence.Type.CheckFlag(SequenceType.ROTATION_Y)) {
              rotationKeyframes.SetKeyframe(1, f, rotation.Y);
            }

            if (gdlSequence.Type.CheckFlag(SequenceType.ROTATION_Z)) {
              rotationKeyframes.SetKeyframe(2, f, rotation.Z);
            }

            var position = gdlSequence.Positions[f];
            if (gdlSequence.Type.CheckFlag(SequenceType.POSITION_X)) {
              positionKeyframes.SetKeyframe(0, f, position.X);
            }

            if (gdlSequence.Type.CheckFlag(SequenceType.POSITION_Y)) {
              positionKeyframes.SetKeyframe(1, f, position.Y);
            }

            if (gdlSequence.Type.CheckFlag(SequenceType.POSITION_Z)) {
              positionKeyframes.SetKeyframe(2, f, position.Z);
            }

            var scale = gdlSequence.Scales[f];
            if (gdlSequence.Type.CheckFlag(SequenceType.SCALE_X)) {
              scaleKeyframes.SetKeyframe(0, f, scale.X);
            }

            if (gdlSequence.Type.CheckFlag(SequenceType.SCALE_Y)) {
              scaleKeyframes.SetKeyframe(1, f, scale.Y);
            }

            if (gdlSequence.Type.CheckFlag(SequenceType.SCALE_Z)) {
              scaleKeyframes.SetKeyframe(2, f, scale.Z);
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
          = finBones.SingleOrDefault(b => finMesh.Name.Contains(b.Name!));
      var finBoneWeights
          = finBone != null
              ? finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, finBone)
              : null;

      for (var m = 0; m < (obj.Mesh?.Primitives.Count ?? 0); ++m) {
        var gdlMesh = obj.Mesh.Primitives[m];
        if (gdlMesh.Positions.Count < 3) {
          continue;
        }

        var textureIndex = m == 0
            ? obj.SubObject0TextureIndex
            : obj.SubObjects[m - 1].TextureIndex;

        var finVertices = gdlMesh.Positions
                                 .Select((p, i) => {
                                   var finVertex = finSkin.AddVertex(p / 128f);

                                   /*if (gdlMesh.Normals.Count > 0) {
                                     finVertex.SetLocalNormal(gdlMesh.Normals[i]);
                                   }*/

                                   if (gdlMesh.Uvs.Count ==
                                       gdlMesh.Positions.Count) {
                                     finVertex.SetUv(gdlMesh.Uvs[i].Value);
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

    return finModel;
  }
}