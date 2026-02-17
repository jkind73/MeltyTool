using System.Numerics;

using fin.animation.keyframes;
using fin.image;
using fin.io;
using fin.io.bundles;
using fin.math.transform;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.sets;

using gm.schema.d3d;


namespace gm.api;

public sealed class D3dModelFileBundle : IModelFileBundle {
  public required IReadOnlyTreeFile ModFile { get; init; }

  public IReadOnlyTreeFile? TextureFile { get; init; }

  public (IReadOnlyTreeFile gifFile, float frameRate)? AnimatedTextureFile {
    get;
    init;
  }

  public IReadOnlyTreeFile MainFile => this.ModFile;
  public bool FlipNormals { get; init; }
  public WrapMode TextureWrapMode { get; init; }
}

public sealed class D3dModelImporter : IModelImporter<D3dModelFileBundle> {
  public IModel Import(D3dModelFileBundle modelFileBundle) {
    var modFile = modelFileBundle.ModFile;
    var mod = modFile.ReadNewFromText<D3d>();

    var (finModel, finRootBone)
        = CreateModel((modelFileBundle, modFile.AsFileSet()));

    ITextureMaterial? material = null;
    if (modelFileBundle.TextureFile is { } textureFile) {
      (material, var texture) =
          finModel.MaterialManager.AddSimpleTextureMaterialFromFile(
              textureFile);
      texture.WrapModeU = texture.WrapModeV = modelFileBundle.TextureWrapMode;
    } else if (modelFileBundle.AnimatedTextureFile is { } gifFileAndFrameRate) {
      var (gifFile, frameRate) = gifFileAndFrameRate;

      var frameImages = FinImage.FromGifFile(gifFile);
      var name = gifFile.NameWithoutExtension.ToString();

      var finMaterialManager = finModel.MaterialManager;
      var frameTextures
          = frameImages
            .Select((frameImage, i) => {
              var frameTexture = finMaterialManager.CreateTexture(
                  frameImage.RemoveTopLeftBackgroundColor());
              frameTexture.Name =
                  frameImages.Length > 0 ? $"{name}_{i}" : name;
              frameTexture.WrapModeU = frameTexture.WrapModeV =
                  modelFileBundle.TextureWrapMode;
              return frameTexture;
            })
            .ToArray();

      var baseTexture = frameTextures[0];

      material = finMaterialManager.AddTextureMaterial(baseTexture);
      material.Name = "diffuse";

      var animation = finModel.AnimationManager.AddAnimation();
      animation.Name = name;
      animation.FrameCount = frameImages.Length;
      animation.FrameRate = frameRate;

      var textureTracks = animation.AddTextureTracks(baseTexture);
      var flipbookSwapKeyframes
          = textureTracks.UseFlipbookSwapKeyframes(frameTextures.Length);
      for (var f = 0; f < frameTextures.Length; ++f) {
        flipbookSwapKeyframes.Add(
            new Keyframe<IReadOnlyTexture?>(f, frameTextures[f]));
      }
    }

    AddToModel(mod, finModel, finRootBone, out _, material);

    if (modelFileBundle.FlipNormals) {
      finModel.FlipAllNormals();
    }

    return finModel;
  }

  public static (IModel<ISkin<NormalUvVertexImpl>>, IBone) CreateModel(
      (IFileBundle fileBundle, IReadOnlySet<IReadOnlyGenericFile> files)?
          modelMetadata = null) {
    var finModel =
        new ModelImpl<NormalUvVertexImpl>((index, position)
                                              => new NormalUvVertexImpl(
                                                  index,
                                                  position)) {
            FileBundle = modelMetadata?.fileBundle!,
            Files = modelMetadata?.files!
        };
    var finRootBone = CreateAdjustedRootBone(finModel);
    return (finModel, finRootBone);
  }

  public static IBone CreateAdjustedRootBone(IModel finModel) {
    var finSkeleton = finModel.Skeleton;
    var bone = finSkeleton.Root.AddRoot(0, 0, 0);
    bone.Transform.SetRotationDegrees(-90, 180, 0);
    bone.Transform.SetScale(-1, 1, 1);
    return bone;
  }

  public static void AddToModel(D3d d3d,
                                IModel<ISkin<NormalUvVertexImpl>> finModel,
                                IReadOnlyBone bone,
                                out IMesh finMesh,
                                IMaterial? material = null) {
    var finSkin = finModel.Skin;
    finMesh = finSkin.AddMesh();

    var boneWeights = finSkin.GetOrCreateBoneWeights(
        VertexSpace.RELATIVE_TO_BONE,
        bone);

    D3dPrimitiveType d3dPrimitiveType = default;
    var finVertices = new LinkedList<IVertex>();
    foreach (var d3dCommand in d3d.Commands) {
      switch (d3dCommand.CommandType) {
        case D3dCommandType.BEGIN: {
          d3dPrimitiveType = (D3dPrimitiveType) d3dCommand.Parameters[0];
          break;
        }
        case D3dCommandType.END: {
          switch (d3dPrimitiveType) {
            case D3dPrimitiveType.TRIANGLE_LIST: {
              finMesh.AddTriangles(finVertices.ToArray()).SetMaterial(material);
              break;
            }
            default: throw new NotImplementedException();
          }

          finVertices.Clear();
          break;
        }
        case D3dCommandType.VERTEX_NORMAL_TEXTURE: {
          var d3dParams = d3dCommand.Parameters.AsSpan();

          var finVertex = finSkin.AddVertex(new Vector3(d3dParams[..3]));
          finVertex.SetLocalNormal(-new Vector3(d3dParams.Slice(3, 3)));
          finVertex.SetUv(new Vector2(d3dParams.Slice(6, 2)));
          finVertex.SetBoneWeights(boneWeights);

          finVertices.AddLast(finVertex);
          break;
        }
        case D3dCommandType.VERTEX_TEXTURE: {
          var d3dParams = d3dCommand.Parameters.AsSpan();

          var finVertex = finSkin.AddVertex(new Vector3(d3dParams[..3]));
          finVertex.SetUv(new Vector2(d3dParams.Slice(3, 2)));
          finVertex.SetBoneWeights(boneWeights);

          finVertices.AddLast(finVertex);
          break;
        }
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}