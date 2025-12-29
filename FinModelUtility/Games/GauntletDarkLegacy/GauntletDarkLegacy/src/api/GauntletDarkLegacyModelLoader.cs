using System.Numerics;

using fin.animation.keyframes;
using fin.data.dictionaries;
using fin.data.lazy;
using fin.data.queues;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.language.equations.fixedFunction;
using fin.math;
using fin.math.floats;
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

    return ImportImpl(
        fileBundle,
        new HashSet<IReadOnlyGenericFile>([
            fileBundle.ObjectsFile,
            fileBundle.AnimFile,
            fileBundle.TexturesFile,
        ]),
        objects,
        anim,
        fileBundle.TexturesFile,
        new Dictionary<int, IReadOnlyImage>(),
        null,
        default);
  }

  public static IModel ImportImpl(
      IFileBundle fileBundle,
      IReadOnlySet<IReadOnlyGenericFile> files,
      Objects objects,
      Anim anim,
      IReadOnlyGenericFile texturesFile,
      Dictionary<int, IReadOnlyImage> textureImageCache,
      string? rootObjectName,
      MbFlags worldObjectMbFlags) {
    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = files,
    };

    using var textureBr = texturesFile.OpenReadAsBinary(Endianness.BigEndian);

    var lazyFinImages = new LazyDictionary<int, IReadOnlyImage>(textureIndex
          => {
        if (textureImageCache.TryGetValue(textureIndex,
                                          out var existingTextureImage)) {
          return existingTextureImage;
        }

        var gdlTexture = objects.Textures[textureIndex];
        textureBr.Position = gdlTexture.TextureDataPointer;
        return textureImageCache[textureIndex]
            = new GdlImageReader(gdlTexture).ReadImage(textureBr);
      });
    var lazyFinTextures = new LazyDictionary<(ushort textureIndex, bool isLightmap), IReadOnlyTexture>(tuple
          => {
        var (textureIndex, isLightmap) = tuple;

        var gdlTexture = objects.Textures[textureIndex];
        var finImage = lazyFinImages[textureIndex];

        var finTexture = finModel.MaterialManager.CreateTexture(finImage);
        var prefix = !isLightmap ? "texture" : "lightmap";
        finTexture.Name
            = $"{prefix}{textureIndex}_{gdlTexture.Format}_{gdlTexture.TextureDataPointer.ToHex()}";

        finTexture.WrapModeU = gdlTexture.Flags.CheckFlag(TextureFlags.CLAMP_U)
            ? WrapMode.CLAMP
            : WrapMode.REPEAT;
        finTexture.WrapModeV = gdlTexture.Flags.CheckFlag(TextureFlags.CLAMP_V)
            ? WrapMode.CLAMP
            : WrapMode.REPEAT;

        finTexture.UvIndex = !isLightmap ? 0 : 1;

        return finTexture;
      });

    var lazyFinMaterials
        = new LazyDictionary<(ushort textureIndex, short lmIndex, bool hasVertexColors, MbFlags), IReadOnlyMaterial>(tuple => {
          var (textureIndex, lmIndex, hasVertexColors, boneMbFlags) = tuple;

          var finMaterial = finModel.MaterialManager.AddFixedFunctionMaterial();
          var equations = finMaterial.Equations;

          var outputColorAlpha = finMaterial.GenerateDiffuse(
              (equations.ColorOps.One, equations.ScalarOps.One),
              lazyFinTextures[(textureIndex, false)],
              (hasVertexColors, hasVertexColors));

          var noShading = boneMbFlags.CheckFlag(MbFlags.NO_SHADING) ||
                          worldObjectMbFlags.CheckFlag(MbFlags.NO_SHADING);
          if (!noShading) {
            if (objects.LmTexNum > 0) {
              if (lmIndex > 0) {
                var lightmapColor = finMaterial.AddTextureSourceColor(
                    lazyFinTextures[((ushort) lmIndex, true)]);

                outputColorAlpha = (outputColorAlpha.Item1.Multiply(lightmapColor),
                                    outputColorAlpha.Item2);
              } else {
                outputColorAlpha
                    = (equations.ColorOps.MultiplyWithConstant(
                           outputColorAlpha.Item1,
                           .25f), outputColorAlpha.Item2);
              }
            } else {
              outputColorAlpha
                  = equations.GenerateLighting(outputColorAlpha,
                                               equations.ColorOps.One);
            }
          }

          equations.SetOutputColorAlpha(outputColorAlpha);

          finMaterial.Shininess = MaterialConstants.DISABLED_SHININESS;

          finMaterial.DepthMode = (
              boneMbFlags.CheckFlag(MbFlags.NO_Z_TEST),
              boneMbFlags.CheckFlag(MbFlags.NO_Z_WRITE)) switch {
              (false, false) => DepthMode.READ_AND_WRITE,
              (false, true)  => DepthMode.READ_ONLY,
              (true, false)  => DepthMode.WRITE_ONLY,
              (true, true) => DepthMode.NONE,
          };

          var blendSrcAndDst = GetBlendMode_(boneMbFlags, worldObjectMbFlags);
          if (blendSrcAndDst != null) {
            finMaterial.SetBlending(BlendEquation.ADD,
                                    blendSrcAndDst.Value.src,
                                    blendSrcAndDst.Value.dst,
                                    LogicOp.UNDEFINED);
          }

          return finMaterial;
        });

    var gdlNodeAndFinBonesByMeshNamePart
        = new Dictionary<string, (ANodeInfo, IReadOnlyBone)>();

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

        // Fuck, man. There are way easier ways to do this... why the fuck did
        // Midway not use integer IDs to link bones/meshes??????
        if (gdlANodeInfo.Type is AnimType.OBJ_ANIM) {
          if (gdlSkeleton.Data.ObjectAnimationHeader
                         .ObjectAnimationSequencesByBone.TryGetList(
                             gdlANodeInfo,
                             out var objectAnimations) &&
              objectAnimations.Count > 0) {
            foreach (var objectAnimation in objectAnimations) {
              SplitObjAnimMeshName_(
                  objectAnimation.Name,
                  out var meshNamePrefix,
                  out _,
                  out _);
              gdlNodeAndFinBonesByMeshNamePart[meshNamePrefix]
                  = (gdlANodeInfo, finBone);
            }
          }
        } else {
          gdlNodeAndFinBonesByMeshNamePart[gdlANodeInfo.MbDesc]
              = (gdlANodeInfo, finBone);
        }

        SetFaceTowardsCamera(finBone, gdlANodeInfo.MbFlags);

        // HACK: Need to manually attach the bone to the right wrist.
        // This is fucking stupid. Why is the weapon's parent bone not set?
        if (finBone.Name is "R_WRIST") {
          weaponBone = finBone.AddChild(Vector3.Zero);
          weaponBone.Name = "WEAPON";
        }

        if (gdlBonesByParent.TryGetList(gdlANodeInfo, out var childGdlBones)) {
          boneQueue.Enqueue(
              childGdlBones.Select(childGdlBone => (childGdlBone, finBone)));
        }
      }

      var gdlAnimationData = gdlSkeleton.Data.SkeletalAnimationHeader;
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

        foreach (var gdlBone in gdlAnimationData.SkeletalSequencesByBone.Keys) {
          var allGdlSequencesForBone
              = gdlAnimationData.SkeletalSequencesByBone[gdlBone];
          var gdlBoneSequencesForAnimation
              = headerIs.Select(i => allGdlSequencesForBone[i]).ToArray();

          if (gdlBoneSequencesForAnimation.All(s => s.Size == 0)) {
            continue;
          }

          var totalSequenceFrameCount
              = gdlBoneSequencesForAnimation.Sum(s => s.FrameCount);

          var finBone = finBoneByGdlBone[gdlBone];
          var finBoneTracks = finAnimation.GetOrCreateBoneTracks(finBone);

          finAnimation.FrameCount
              = Math.Max(finAnimation.FrameCount, totalSequenceFrameCount);

          var localPosition = finBone.LocalTransform.Translation;

          var rotationKeyframes
              = finBoneTracks.UseSeparateEulerRadiansKeyframes(
                  totalSequenceFrameCount);
          rotationKeyframes.ConvertRadiansToQuaternionImpl
              = ConvertGdlRadiansToQuaternion_;
          var positionKeyframes = finBoneTracks.UseSeparateTranslationKeyframes(
              totalSequenceFrameCount);
          var scaleKeyframes
              = finBoneTracks.UseSeparateScaleKeyframes(
                  totalSequenceFrameCount);

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
                positionKeyframes.SetKeyframe(
                    0,
                    frame,
                    localPosition.X - positionX.Value);
              }

              var positionY = gdlSequence.PositionYs[f];
              if (positionY != null) {
                positionKeyframes.SetKeyframe(
                    1,
                    frame,
                    localPosition.Y +
                    positionY.Value);
              }

              var positionZ = gdlSequence.PositionZs[f];
              if (positionZ != null) {
                positionKeyframes.SetKeyframe(
                    2,
                    frame,
                    localPosition.Z + positionZ.Value);
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
    var rootFinMeshByName = new Dictionary<string, IMesh>();
    foreach (var (definition, obj) in objects.ObjectDefinitions.Zip(
                 objects.RootObjects)) {
      if (rootObjectName != null && definition.Name != rootObjectName) {
        continue;
      }

      var finRootMesh = finSkin.AddMesh();
      finRootMesh.Name = definition.Name;

      rootFinMeshByName[finRootMesh.Name] = finRootMesh;

      var (gdlNode, finBone)
          = gdlNodeAndFinBonesByMeshNamePart
            .FirstOrDefault(kvp => finRootMesh.Name.Contains(kvp.Key))
            .Value;

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
        var textureIndex = gdlMesh.SubObject.TextureIndex;

        foreach (var gdlPrimitive in gdlMesh.Primitives) {
          if (gdlPrimitive.Positions.Count < 3) {
            continue;
          }

          var finVertices = new IReadOnlyVertex[gdlPrimitive.Positions.Count];
          for (var i = 0; i < gdlPrimitive.Positions.Count; ++i) {
            var p = gdlPrimitive.Positions[i];
            p /= 128f;

            // For some inexplicable reason, the meshes are mirrored.
            p.X *= -1;

            var finVertex = finSkin.AddVertex(p);

            var normal = gdlPrimitive.Normals[i];
            normal.X *= -1;

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
                                gdlNode?.MbFlags ?? default)]);
          finPrimitive.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
        }
      }
    }

    foreach (var gdlSkeleton in anim.Atrees) {
      foreach (var gdlObjectAnimation in gdlSkeleton.Data.ObjectAnimationHeader
                                                    .ObjectAnimations) {
        SplitObjAnimMeshName_(
            gdlObjectAnimation.Name,
            out var meshNamePrefix,
            out var lengthOfIndex,
            out var currentMeshIndex);
        var finMeshAnimation = finModel.AnimationManager.AddAnimation();
        finMeshAnimation.Name = meshNamePrefix;

        finMeshAnimation.FrameCount = gdlObjectAnimation.FrameCount;
        finMeshAnimation.FrameRate = 30;

        var meshesWithPrefix
            = rootFinMeshByName
              .Where(kvp => kvp.Key.StartsWith(meshNamePrefix))
              .OrderBy(kvp => kvp.Key)
              .Select(kvp => kvp.Value)
              .GetEnumerator();

        IMeshTracks? previousMeshTracks = null;
        for (var f = gdlObjectAnimation.StartFrame;
             f < gdlObjectAnimation.FrameCount;
             ++f) {
          meshesWithPrefix.MoveNext();
          var finMesh = meshesWithPrefix.Current;

          finMesh.DefaultDisplayState = MeshDisplayState.HIDDEN;

          var finMeshTracks = finMeshAnimation.AddMeshTracks(finMesh);
          finMeshTracks.DisplayStates.Add(
              new Keyframe<MeshDisplayState>(f, MeshDisplayState.VISIBLE));

          previousMeshTracks?.DisplayStates.Add(
              new Keyframe<MeshDisplayState>(f, MeshDisplayState.HIDDEN));
          previousMeshTracks = finMeshTracks;
        }

        previousMeshTracks.DisplayStates.Add(
            new Keyframe<MeshDisplayState>(gdlObjectAnimation.FrameCount,
                                           MeshDisplayState.HIDDEN));
      }
    }

    return finModel;
  }

  public static void SetFaceTowardsCamera(IBone bone, MbFlags mbFlags) {
    if (mbFlags.CheckFlag(MbFlags.YAW_ONLY_BILLBOARD)) {
      bone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_ONLY);
    } else if (mbFlags.CheckFlag(MbFlags.YAW_AND_PITCH_BILLBOARD)) {
      bone.AlwaysFaceTowardsCamera(FaceTowardsCameraType.YAW_AND_PITCH);
    }
  }

  private static (BlendFactor src, BlendFactor dst)? GetBlendMode_(
      MbFlags boneFlags,
      MbFlags worldObjFlags) {
    if (boneFlags.CheckFlag(MbFlags.BLEND_ADD) ||
        worldObjFlags.CheckFlag(MbFlags.BLEND_ADD)) {
      return (BlendFactor.SRC_ALPHA, BlendFactor.ONE);
    }

    if (boneFlags.CheckFlag(MbFlags.BLEND_MULTIPLY) ||
        worldObjFlags.CheckFlag(MbFlags.BLEND_MULTIPLY)) {
      return (BlendFactor.DST_COLOR, BlendFactor.ZERO);
    }

    return null;
  }

  private static void SplitObjAnimMeshName_(
      string objAnimMeshName,
      out string prefix,
      out int lengthOfIndex,
      out int firstIndex) {
    var indexOfFMarker = objAnimMeshName.LastIndexOf('F');

    prefix = objAnimMeshName[..indexOfFMarker];
    lengthOfIndex = objAnimMeshName.Length - 1 - indexOfFMarker;
    firstIndex = int.Parse(objAnimMeshName[(indexOfFMarker + 1)..]);
  }

  private static Quaternion ConvertGdlRadiansToQuaternion_(
      float x,
      float y,
      float z) {
    // y is definitely yaw, needs to be negative (e.g. turning side-to-side in idle)
    // x is definitely pitch, needs to be negative (e.g. legs running)
    // z must be roll, needs to be negative (e.g. archer bobbing back and forth in idle)

    var yawQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -y);
    var pitchQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -x);
    var rollQuaternion = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, z);

    // TODO: Still not quite right, e.g. the warrior model's SHOVE animation
    // has his arms broken

    return yawQuaternion * pitchQuaternion * rollQuaternion;
  }
}