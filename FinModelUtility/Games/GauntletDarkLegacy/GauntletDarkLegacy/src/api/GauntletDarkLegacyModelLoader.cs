using System.Drawing;
using System.Numerics;

using fin.color;
using fin.data.dictionaries;
using fin.data.queues;
using fin.image;
using fin.image.formats;
using fin.image.io;
using fin.image.io.pixel;
using fin.io;
using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.hex;

using gdl.schema.anim;
using gdl.schema.objects;

using schema.binary;

using SixLabors.ImageSharp.PixelFormats;

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
        Files = new HashSet<IReadOnlyGenericFile>([fileBundle.MainFile])
    };

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
      }
    }

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

        finBoneByGdlBone[gdlBone] = finBone;

        if (gdlBonesByParent.TryGetList(gdlBone, out var childGdlBones)) {
          boneQueue.Enqueue(
              childGdlBones.Select(childGdlBone => (
                                       childGdlBone,
                                       finBone,
                                       worldMatrix.AssertInvert())));
        }
      }

      foreach (var (gdlAnimationHeader, gdlAnimationData) in gdlSkeleton.Data
                   .AnimationHeaders.Zip(gdlSkeleton.Data.AnimationDatas)) {
        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = gdlAnimationHeader.Name;
        finAnimation.FrameCount = gdlAnimationHeader.FrameCount;
        finAnimation.FrameRate = gdlAnimationHeader.FrameRate;

        foreach (var gdlBone in gdlAnimationData.SequencesByBone.Keys) {
          var gdlSequences = gdlAnimationData.SequencesByBone[gdlBone];

          var finBoneTracks
              = finAnimation.GetOrCreateBoneTracks(finBoneByGdlBone[gdlBone]);

          // TODO: Add tracks
        }
      }
    }

    return finModel;
  }
}