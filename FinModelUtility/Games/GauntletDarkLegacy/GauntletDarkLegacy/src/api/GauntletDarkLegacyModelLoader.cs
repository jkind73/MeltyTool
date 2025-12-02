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
using fin.math;
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
            case PixelFormat.RGBA5553: {
              palette = textureBr.ReadUInt16s(paletteCount)
                                 .Select(v => {
                                   ColorUtil.SplitRgb5A3(v,
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

        finTexture.WrapModeU = gdlTexture.Flags.CheckFlag(TextureFlags.CLAMP_U)
            ? WrapMode.CLAMP
            : WrapMode.REPEAT;
        finTexture.WrapModeV = gdlTexture.Flags.CheckFlag(TextureFlags.CLAMP_V)
            ? WrapMode.CLAMP
            : WrapMode.REPEAT;

        finTextures.Add(finTexture);
      }
    }
    var lazyFinMaterials
        = new LazyDictionary<int, IReadOnlyMaterial>(index => {
          var finTexture = finTextures[index];
          return finModel.MaterialManager.AddTextureMaterial(finTexture);
        });

    IBone? weaponBone = null;
    var finBones = new List<IReadOnlyBone>();
    foreach (var gdlSkeleton in anim.Atrees) {
      var gdlBones = gdlSkeleton.Data.ANodeInfos;

      var gdlBonesByParent = new ListDictionary<ANodeInfo?, ANodeInfo>();
      foreach (var gdlBone in gdlBones) {
        gdlBonesByParent.Add(
            gdlBone.ParentId == -1 ? null : gdlBones[gdlBone.ParentId],
            gdlBone);
      }

      var boneQueue = new FinTuple2Queue<ANodeInfo, IBone>(
          gdlBonesByParent[null]
              .Select(rootGdlBone => (rootGdlBone, finModel.Skeleton.Root)));
      var finBoneByGdlBone = new Dictionary<ANodeInfo, IReadOnlyBone>();

      while (boneQueue.TryDequeue(out var gdlANodeInfo,
                                  out var finParentBone)) {
        var initialPosition = gdlANodeInfo.InitialPosition;

        // For some inexplicable reason, the meshes are mirrored.
        initialPosition.X *= -1;
        
        var worldMatrix = Matrix4x4.CreateTranslation(initialPosition);

        var finBone = finParentBone.AddChild(worldMatrix);
        finBone.Name = gdlANodeInfo.MbDesc;

        finBones.Add(finBone);
        finBoneByGdlBone[gdlANodeInfo] = finBone;

        if (gdlANodeInfo.MbFlags.CheckFlag(MbFlags.YAW_ONLY_BILLBOARD)) {
          finBone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_ONLY);
        } else if (gdlANodeInfo.MbFlags.CheckFlag(
                       MbFlags.YAW_AND_PITCH_BILLBOARD)) {
          finBone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_AND_PITCH);
        }

        if (finBone.Name is "R_WRIST") {
          weaponBone = finBone.AddChild(Vector3.Zero);
          weaponBone.Name = "WEAPON";
        }

        if (gdlBonesByParent.TryGetList(gdlANodeInfo, out var childGdlBones)) {
          boneQueue.Enqueue(
              childGdlBones.Select(childGdlBone => (childGdlBone, finBone)));
        }
      }

      var gdlAnimationData = gdlSkeleton.Data.AnimHeader;
      var gdlAnimationHeaders = gdlSkeleton.Data.ATreeSequences;

      var logicalAnimations = new List<(string name, IEnumerable<int>)>();
      var gdlAnimationHeaderIndicesByName = new Dictionary<string, int>();
      for (var i = 0; i < gdlAnimationHeaders.Length; ++i) {
        gdlAnimationHeaderIndicesByName[gdlAnimationHeaders[i].Name] = i;

        logicalAnimations.Add((gdlAnimationHeaders[i].Name, [i]));
      }

      // HACK: Combine animations that swap back and forth
      {
        if (gdlAnimationHeaderIndicesByName.TryGetValue(
                "WALK1",
                out var walk1I) &&
            gdlAnimationHeaderIndicesByName.TryGetValue(
                "WALK2",
                out var walk2I)) {
          logicalAnimations.Add(("WALK", [walk1I, walk2I]));
        }

        if (gdlAnimationHeaderIndicesByName
                .TryGetValue("RUN1", out var run1I) &&
            gdlAnimationHeaderIndicesByName
                .TryGetValue("RUN2", out var run2I)) {
          logicalAnimations.Add(("RUN", [run1I, run2I]));
        }
      }

      foreach (var (animationName, headerIs) in logicalAnimations) {
        var finAnimation = finModel.AnimationManager.AddAnimation();
        finAnimation.Name = animationName;

        var gdlHeaders = headerIs.Select(i => gdlAnimationHeaders[i]).ToArray();
        finAnimation.FrameCount = gdlHeaders.Sum(h => h.FrameCount);

        var header0 = gdlAnimationHeaders[headerIs.First()];
        finAnimation.FrameRate = header0.FrameRate;
        finAnimation.UseLoopingInterpolation
            = header0.Loop || gdlHeaders.Length > 1;

        foreach (var gdlBone in gdlAnimationData.SequencesByBone.Keys) {
          var allGdlSequencesForBone
              = gdlAnimationData.SequencesByBone[gdlBone];
          var gdlBoneSequencesForAnimation
              = headerIs.Select(i => allGdlSequencesForBone[i]).ToArray();

          if (gdlBoneSequencesForAnimation.All(s => s.Size == 0)) {
            continue;
          }

          var totalSequnceFrameCount
              = gdlBoneSequencesForAnimation.Sum(s => s.FrameCount);

          var finBoneTracks
              = finAnimation.GetOrCreateBoneTracks(finBoneByGdlBone[gdlBone]);

          finAnimation.FrameCount
              = Math.Max(finAnimation.FrameCount, totalSequnceFrameCount);

          var rotationKeyframes
              = finBoneTracks.UseSeparateEulerRadiansKeyframes(
                  totalSequnceFrameCount);
          rotationKeyframes.ConvertRadiansToQuaternionImpl
              = this.ConvertGdlRadiansToQuaternion_;
          var positionKeyframes = finBoneTracks.UseSeparateTranslationKeyframes(
              totalSequnceFrameCount);
          var scaleKeyframes
              = finBoneTracks.UseSeparateScaleKeyframes(
                  totalSequnceFrameCount);

          var startFrame = 0;

          foreach (var gdlSequence in gdlBoneSequencesForAnimation) {
            var header = gdlSequence.Header;

            var isReversed = header.Flags.GetBit(0);

            for (var f = 0; f < gdlSequence.FrameCount; ++f) {
              var frame = startFrame +
                          (!isReversed ? f : (gdlSequence.FrameCount - 1 - f));

              var rotationX = gdlSequence.RotationXs[f];
              if (rotationX != null) {
                rotationKeyframes.SetKeyframe(0,
                                              frame,
                                              rotationX.Value);
              }

              var rotationY = gdlSequence.RotationYs[f];
              if (rotationY != null) {
                rotationKeyframes.SetKeyframe(1,
                                              frame,
                                              rotationY.Value);
              }

              var rotationZ = gdlSequence.RotationZs[f];
              if (rotationZ != null) {
                rotationKeyframes.SetKeyframe(2,
                                              frame,
                                              rotationZ.Value);
              }

              var positionX = gdlSequence.PositionXs[f];
              if (positionX != null) {
                // For some inexplicable reason, the meshes are mirrored.
                positionKeyframes.SetKeyframe(0,
                                              frame,
                                              -positionX.Value);
              }

              var positionY = gdlSequence.PositionYs[f];
              if (positionY != null) {
                positionKeyframes.SetKeyframe(1,
                                              frame,
                                              positionY.Value);
              }

              var positionZ = gdlSequence.PositionZs[f];
              if (positionZ != null) {
                positionKeyframes.SetKeyframe(2,
                                              frame,
                                              positionZ.Value);
              }

              var scaleX = gdlSequence.ScaleXs[f];
              if (scaleX != null) {
                scaleKeyframes.SetKeyframe(0, frame, scaleX.Value);
              }

              var scaleY = gdlSequence.ScaleYs[f];
              if (scaleY != null) {
                scaleKeyframes.SetKeyframe(1, frame, scaleY.Value);
              }

              var scaleZ = gdlSequence.ScaleZs[f];
              if (scaleZ != null) {
                scaleKeyframes.SetKeyframe(2, frame, scaleZ.Value);
              }
            }

            startFrame += gdlSequence.FrameCount;
          }
        }
      }
    }

    var finSkin = finModel.Skin;
    foreach (var (definition, obj) in objects.ObjectDefinitions.Zip(
                 objects.RootObjects)) {
      var finRootMesh = finSkin.AddMesh();
      finRootMesh.Name = definition.Name;

      var finBone
          = finBones.FirstOrDefault(b => finRootMesh.Name.Contains(b.Name!));

      // HACK: Weapon isn't attached to the hand otherwise
      if (finBone == null && definition.Name.StartsWith("WEAP_")) {
        finBone = weaponBone;
      }

      var finBoneWeights
          = finBone != null
              ? finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                               finBone)
              : null;

      for (var m = 0; m < (obj.SubObjectModels?.All.Count ?? 0); ++m) {
        var finSubMesh = finRootMesh.AddSubMesh();

        var gdlMesh = obj.SubObjectModels.All[m];

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
                  p /= 128f;

                  // For some inexplicable reason, the meshes are mirrored.
                  p.X *= -1;

                  var finVertex = finSkin.AddVertex(p);

                  /*if (gdlPrimitive.Normals.Count >=
                      gdlPrimitive.Positions.Count) {
                    finVertex.SetLocalNormal(gdlPrimitive.Normals[i]);
                  }*/

                  if (gdlPrimitive.Uvs.Count >=
                      gdlPrimitive.Positions.Count) {
                    finVertex.SetUv(gdlPrimitive.Uvs[i].Value);
                  } else {
                    finVertex.SetUv(0, 0);
                  }

                  if (gdlPrimitive.VertexColors.Count >=
                      gdlPrimitive.Positions.Count) {
                    finVertex.SetColor(gdlPrimitive.VertexColors[i]);
                  }

                  if (finBoneWeights != null) {
                    finVertex.SetBoneWeights(finBoneWeights);
                  }

                  return finVertex;
                })
                .ToArray();

          var finPrimitive = finSubMesh.AddTriangleStrip(finVertices);
          finPrimitive.SetMaterial(lazyFinMaterials[textureIndex]);
          finPrimitive.SetVertexOrder(gdlPrimitive.VertexOrder switch {
              VertexOrder.CLOCKWISE => VertexOrder.COUNTER_CLOCKWISE,
              VertexOrder.COUNTER_CLOCKWISE => VertexOrder.CLOCKWISE,
          });
        }
      }
    }

    return finModel;
  }

  private Quaternion ConvertGdlRadiansToQuaternion_(float x, float y, float z) {
    // y is definitely yaw, needs to be negative (e.g. turning side-to-side in idle)
    // x is definitely pitch, needs to be negative (e.g. legs running)
    // z must be roll, needs to be negative (e.g. archer bobbing back and forth in idle)

    var yawQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -y);
    var pitchQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -x);
    var rollQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, z);

    return yawQuaternion * pitchQuaternion * rollQuaternion;
  }
}