using System.Numerics;

using bar.schema;

using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.data.lazy;
using fin.io;
using fin.io.bundles;
using fin.math;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.util.enums;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public sealed record UvmdModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeDirectory RootDirectory)
    : IModelFileBundle;

public sealed class UvmdModelFileImporter
    : IModelImporter<UvmdModelFileBundle> {
  public IModel Import(UvmdModelFileBundle fileBundle)
    => Import(fileBundle, true);

  public static IModel Import(UvmdModelFileBundle fileBundle, bool fixRotation) {
    var fileChunks
        = fileBundle.MainFile.ReadNew<FileChunks>(Endianness.BigEndian);
    if (fileChunks.Chunks.Count == 0) {
      return new ModelImpl {
          FileBundle = fileBundle,
          Files = fileBundle.MainFile.AsFileSet(),
      };
    }

    var uvmd
        = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                 Endianness.BigEndian).ReadNew<Uvmd>();
    return FromMaterialMeshes(
        fileBundle,
        fileBundle.RootDirectory,
        uvmd.Lods[0].ModelParts.Select(p => p.MaterialMeshes),
        true,
        uvmd.Transforms);
  }

  public static IModel FromMaterialMeshes(
      IFileBundle fileBundle,
      IReadOnlyTreeDirectory rootDirectory,
      IEnumerable<UvmdMaterialMesh> materialMeshes,
      bool fixRotation)
    => FromMaterialMeshes(fileBundle,
                          rootDirectory,
                          [materialMeshes],
                          fixRotation,
                          null);

  public static IModel FromMaterialMeshes(
      IFileBundle fileBundle,
      IReadOnlyTreeDirectory rootDirectory,
      IEnumerable<IEnumerable<UvmdMaterialMesh>> materialMeshesByBone,
      bool fixRotation,
      Matrix4x4[]? boneMatrices) {
    var files = fileBundle.MainFile.AsFileSet();
    var n64Hardware = new N64Hardware<SeparateN64Memory>();
    var n64Memory = n64Hardware.Memory = new SeparateN64Memory();
    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    var rsp = n64Hardware.Rsp = new Rsp {
        GeometryMode = GeometryMode.G_LIGHTING
    };
    var dlModelBuilder = new DlModelBuilder(n64Hardware, fileBundle, files);
    var finModel = dlModelBuilder.Model;

    var finSkeletonRoot = finModel.Skeleton.Root;
    if (fixRotation) {
      finSkeletonRoot.Transform.LocalRotation
          = Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0);
    }

    var finSkin = finModel.Skin;
    var allFinBoneWeights
        = (boneMatrices ?? [])
          .Select(transform => finSkeletonRoot.AddChild(transform))
          .Select(bone => finSkin.GetOrCreateBoneWeights(
                      VertexSpace.RELATIVE_TO_BONE,
                      bone))
          .ToArray();

    var textureSegmentsAndDisplayListByUvtxIndex
        = new LazyDictionary<uint, ((byte, ISegment)[] segments, IDisplayList displayList)>(uvtxIndex => {
          var uvtxFile
              = rootDirectory.AssertGetExistingFile($"uvtx/{uvtxIndex}.uvtx");

          var fileChunks
              = uvtxFile.ReadNew<FileChunks>(Endianness.BigEndian);
          var uvtx
              = new SchemaBinaryReader(fileChunks.Chunks[0].Buffer,
                                       Endianness.BigEndian).ReadNew<Uvtx>();

          var displayList = new DisplayListReader().ReadDisplayList(
              n64Memory,
              new F3dzex2OpcodeParser(),
              new SchemaBinaryReader(uvtx.DlCommandsData, Endianness.BigEndian));
          return ([
              (0, new BytesSegment {
                  Offset = 0,
                  Bytes = uvtx.TexelData,
              })
          ], displayList);
        });

    var i = 0;
    foreach (var materialMeshesForBone in materialMeshesByBone) {
      if (boneMatrices != null) {
        rsp.ActiveBoneWeights = allFinBoneWeights[i];
      } else if (fixRotation) {
        rsp.ActiveBoneWeights
            = finSkin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                             finSkeletonRoot);
      }

      foreach (var uvmdMaterialMesh in materialMeshesForBone) {
        // TODO: Handle billboards

        SetUpMaterial_(dlModelBuilder,
                       uvmdMaterialMesh,
                       textureSegmentsAndDisplayListByUvtxIndex,
                       n64Hardware.Memory,
                       rsp,
                       rdp);
        dlModelBuilder.AddDl(uvmdMaterialMesh.DisplayList);
      }
    }

    return finModel;
  }

  /// <summary>
  ///   Shamelessly stolen from:
  ///   https://github.com/magcius/noclip.website/blob/main/src/BeetleAdventureRacing/MaterialRenderer.ts#L163
  /// </summary>
  private static void SetUpMaterial_(
      DlModelBuilder dlModelBuilder,
      UvmdMaterialMesh materialMesh,
      ILazyDictionary<uint, ((byte, ISegment)[] segments, IDisplayList displayList)> textureSegmentsAndDisplayListByUvtxIndex,
      ISeparateN64Memory memory,
      IRsp rsp,
      IRdp rdp) {
    var renderOpts = materialMesh.RenderOptions;
    var isTextured = materialMesh.UvtxIndex != 0xFFF;

    var otherModeLRenderMode = (uint) 0;
    if (renderOpts.CheckFlag(RenderOptions.UNK_18)) {
      var m = (uint) (renderOpts &
                      (RenderOptions.UNK_17 | RenderOptions.UNK_16));
      if (m == 0)
        otherModeLRenderMode = 0x00112e10;
      if (m == 0x400000)
        otherModeLRenderMode = 0x00112d58;
      if (m == 0x800000)
        otherModeLRenderMode = 0x00104e50;
      if (m == 0xc00000)
        otherModeLRenderMode = 0x00104dd8;
    } else if (renderOpts.CheckFlag(RenderOptions.UNK_17)) {
      var m = (uint) (renderOpts &
                      (RenderOptions.UNK_16 |
                       RenderOptions.ENABLE_DEPTH_CALCULATIONS));

      Uvtx? uvtx = null;

      if (m == 0) {
        if (!isTextured)
          otherModeLRenderMode = 0x00104340;
        else
          otherModeLRenderMode = 0x00104240;
      }

      if (m == 0x200000) {
        if (!isTextured)
          otherModeLRenderMode = 0x00104b50;
        //else if (uvtx.usesAlphaBlending)
        //  otherModeLRenderMode = 0x00105278;
        else
          otherModeLRenderMode = 0x00104a50;
      }

      if (m == 0x400000) {
        if (!isTextured)
          otherModeLRenderMode = 0x001041c8;
        else if ( /* TODO: complicated flag checks */ false)
          otherModeLRenderMode = 0x00103048;
        else
          otherModeLRenderMode = 0x001041c8;
      }

      if (m == 0x600000) {
        if (!isTextured)
          otherModeLRenderMode = 0x001045d8;
        //else if (uvtx.usesAlphaBlending)
        //  otherModeLRenderMode = 0x00105278;
        else if ( /* TODO: complicated flag checks */ false)
          otherModeLRenderMode = 0x00103078;
        else
          otherModeLRenderMode = 0x001049d8;
      }
    } else {
      var m = (uint) (renderOpts &
                      (RenderOptions.UNK_16 |
                       RenderOptions.ENABLE_DEPTH_CALCULATIONS));
      if (m == 0)
        otherModeLRenderMode = 0x03024000;
      if (m == 0x200000)
        otherModeLRenderMode = 0x00112230;
      if (m == 0x400000)
        otherModeLRenderMode = 0x00102048;
      if (m == 0x600000)
        otherModeLRenderMode = 0x00102078;
    }

    // This sets A to 0 and B to 1 in the first cycle,
    // so the first cycle equation is always just ((P * 0 + M * 1) / (0 + 1)) = (0 + M) / 1 = M
    otherModeLRenderMode |= 0x0c080000;

    rdp.OtherModeL = otherModeLRenderMode;

    var cullBack = renderOpts.CheckFlag(RenderOptions.ENABLE_BACKFACE_CULLING);
    var cullFront
        = renderOpts.CheckFlag(RenderOptions.ENABLE_FRONTFACE_CULLING);
    rsp.CullingMode = (cullFront, cullBack) switch {
        (false, false) => CullingMode.SHOW_BOTH,
        (false, true)  => CullingMode.SHOW_FRONT_ONLY,
        (true, false)  => CullingMode.SHOW_BACK_ONLY,
        (true, true)   => CullingMode.SHOW_NEITHER,
    };

    rsp.Lighting = renderOpts.CheckFlag(RenderOptions.USES_LIGHTING);
    rdp.SetSimpleCombinerCycleParams(isTextured, true, false);

    if (isTextured) {
      var (segments, displayList)
          = textureSegmentsAndDisplayListByUvtxIndex[materialMesh.UvtxIndex];

      foreach (var (segmentIndex, segment) in segments) {
        memory.SetSegment(segmentIndex, segment);
      }
      dlModelBuilder.AddDl(displayList);
    }
  }
}