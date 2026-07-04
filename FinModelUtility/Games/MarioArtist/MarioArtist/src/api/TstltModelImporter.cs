using System.Drawing;
using System.Numerics;

using CommunityToolkit.Diagnostics;

using f3dzex2.combiner;
using f3dzex2.displaylist;
using f3dzex2.displaylist.opcodes;
using f3dzex2.displaylist.opcodes.f3dzex2;
using f3dzex2.image;
using f3dzex2.io;
using f3dzex2.model;

using fin.animation.keyframes;
using fin.color;
using fin.data.dictionaries;
using fin.data.queues;
using fin.io;
using fin.math.matrix.four;
using fin.math.rotations;
using fin.model;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;
using fin.schema.color;
using fin.schema.vector;
using fin.util.linq;
using fin.util.sets;

using marioartist.schema;
using marioartist.schema.talent_studio;

using OneOf;

using schema.binary;
using schema.binary.attributes;


namespace marioartist.api;

using BoneTuple = (IReadOnlyBone bone, Joint joint, int jointIndex);
using ChosenPart0Tuple =
    (ISegment segment, MeshDefinition meshDefinition, SubUnkSection5 unkSection5
    ,
    ChosenPart0 chosenPart, int unkSection5I, int subUnkSection5I);
using ChosenPart1Tuple
    = (ISegment segment, ChosenPart1 chosenPart, IReadOnlyBone bone, bool isHead
    );

public record TstltModelFileBundle(
    IReadOnlyTreeFile MainFile,
    IReadOnlyTreeFile? RomFile = null)
    : IModelFileBundle;

public enum JointIndex {
  // Head bones
  HEAD_ROOT = 0,
  HAIR = 2,
  HAT = 3,
  HELMET = 4,
  NOSE = 5,

  EAR_0 = 6,
  EAR_1 = 7,

  EARRING_0 = 10,
  EARRING_1 = 11,

  // Body bones
  BODY_ROOT = 13,

  // Connects head bones to body
  BODY_HEAD_ADAPTER = 14,

  NECK = 15,

  TORSO = 16,
  HIP = 17,

  UPPER_ARM_0 = 18,
  UPPER_ARM_1 = 19,

  FOREARM_0 = 20,
  FOREARM_1 = 21,

  HAND_0 = 22,
  HAND_1 = 23,

  UPPER_LEG_0 = 24,
  UPPER_LEG_1 = 25,

  LOWER_LEG_0 = 26,
  LOWER_LEG_1 = 27,

  FOOT_0 = 28,
  FOOT_1 = 29,
}

public sealed class TstltModelImporter : IModelImporter<TstltModelFileBundle> {
  public const int HARDCODED_MESH_SET_ID = -1;
  public const bool INCLUDE_FACE = true;

  public const bool USE_JANK = true;

  public const bool USE_GRANULAR_MESHES = false;

  public IModel Import(TstltModelFileBundle fileBundle)
    => Import(fileBundle, out _);

  public static IModel Import(TstltModelFileBundle fileBundle,
                              out Gender gender) {
    using var br = fileBundle.MainFile.OpenReadAsBinary(Endianness.BigEndian);

    var n64Hardware = new N64Hardware<SlicedN64Memory>();
    var rdp = n64Hardware.Rdp = new Rdp {
        Tmem = new NoclipTmem(n64Hardware),
    };
    var rsp = n64Hardware.Rsp = new Rsp {
        GeometryMode = GeometryMode.G_LIGHTING,
    };
    var n64Memory
        = n64Hardware.Memory = new SlicedN64Memory(fileBundle.MainFile);
    n64Memory.SetSegment(0, 0, (uint) br.Length);

    var dlModelBuilder =
        new DlModelBuilder(n64Hardware,
                           fileBundle,
                           fileBundle.MainFile.AsFileSet());

    var model = dlModelBuilder.Model;
    var headSectionOffset = 0x16770;
    br.Position = 0x49C;
    var headSectionLength = br.ReadUInt32();
    var bodySectionOffset = headSectionOffset + headSectionLength;
    var bodySectionLength = br.Length - bodySectionOffset;

    var headSegment = new SliceSegment {
        Offset = (uint) headSectionOffset, Length = headSectionLength
    };
    var bodySegment = new SliceSegment {
        Offset = (uint) bodySectionOffset, Length = (uint) bodySectionLength
    };

    br.Position = 0x166ab;
    var selectedGenderIndex = br.ReadByte();
    gender = selectedGenderIndex < 8
        ? (selectedGenderIndex % 2 == 1 ? Gender.GIRL : Gender.BOY)
        : Gender.OTHER;

    br.Position = 0x91bc;
    var skinColor = br.ReadNew<Rgba32>();

    var skinChosenPart = new ChosenPart0();
    skinChosenPart.ChosenColor0.Color = skinColor;

    br.Position = 0xb840;
    var headChosenPart0s = br.ReadNews<ChosenPart0>(5);
    var headChosenPart0sById = headChosenPart0s.ToDictionary(p => p.Id);
    var headUnkSection5s = br.ReadNews<UnkSection5>(8);

    br.Position = 0xbd08;
    var bodyChosenPart0s = br.ReadNews<ChosenPart0>(8);
    var bodyChosenPart0sById = bodyChosenPart0s.ToDictionary(p => p.Id);
    bodyChosenPart0sById[0] = skinChosenPart;
    var bodyUnkSection5s = br.ReadNews<UnkSection5>(19);

    br.Position = 0xc6c8;
    var headChosenPart1Count = br.ReadInt32();
    var headChosenPart1s = br.ReadNews<ChosenPart1>(headChosenPart1Count);

    br.Position = 0xeb00;
    var bodyChosenPart1Count = br.ReadInt32();
    var bodyChosenPart1s = br.ReadNews<ChosenPart1>(bodyChosenPart1Count);

    br.Position = 0xd530;
    var headMeshDefinitions = br.ReadNews<MeshDefinition>(0x20);

    br.Position = 0xf968;
    var bodyMeshDefinitions = br.ReadNews<MeshDefinition>(0x4C);

    br.Position = 0xa934;
    var joints = br.ReadNews<Joint>(0x1F);
    var originalFlipByJoint = new Dictionary<Joint, Vector3>();

    foreach (var joint in joints) {
      var matrix = Matrix4x4.Transpose(joint.matrix);
      matrix.AssertDecompose(out var translation,
                             out var rotation,
                             out var scale);

      originalFlipByJoint[joint] = new Vector3(
          Math.Sign(scale.X),
          Math.Sign(scale.Y),
          Math.Sign(scale.Z));

      joint.matrix = SystemMatrix4x4Util.FromTrs(
          translation,
          rotation,
          new Vector3(Math.Abs(scale.X), Math.Abs(scale.Y), Math.Abs(scale.Z)));
    }

    var neckJoint = joints[(int) JointIndex.BODY_HEAD_ADAPTER];
    neckJoint.matrix.AssertDecompose(out var neckTranslation,
                                     out _,
                                     out var neckScale);
    var forwardRotation =
        QuaternionUtil.CreateZyxRadians(MathF.PI / 2, 0, MathF.PI / 2);
    var neckTMatrix = Matrix4x4.CreateTranslation(neckTranslation);

    var jointsByParent =
        new BidirectionalDictionary<Joint?, List<(Joint joint, int index)>>();
    for (var i = 0; i < joints.Length; ++i) {
      var joint = joints[i];

      if (i < 13) {
        joint.matrix.AssertDecompose(out var jointTranslation,
                                     out _,
                                     out var jointScale);

        // What is going on???
        // HACK: Fixes position of head meshes
        switch ((JointIndex) i) {
          case JointIndex.NOSE: {
            // TODO: Still not quite right for some reason.
            jointTranslation = joint.matrix.Translation with {
                Z = 0
            };
            break;
          }
          case JointIndex.HAIR:
          case JointIndex.HAT:
          case JointIndex.HELMET: {
            jointTranslation = Vector3.Zero;
            break;
          }
        }

        var scaledJointMatrix =
            SystemMatrix4x4Util.FromTrs(jointTranslation * neckScale,
                                        forwardRotation,
                                        neckScale);

        joint.matrix = scaledJointMatrix * neckTMatrix;
      }

      var jointIndex = (JointIndex) i;
      var parentIndex = (int) (jointIndex switch {
          JointIndex.HEAD_ROOT   => JointIndex.BODY_HEAD_ADAPTER,
          < JointIndex.BODY_ROOT => JointIndex.HEAD_ROOT,

          JointIndex.HIP   => JointIndex.BODY_ROOT,
          JointIndex.TORSO => JointIndex.HIP,
          JointIndex.NECK  => JointIndex.TORSO,

          JointIndex.BODY_HEAD_ADAPTER => JointIndex.NECK,

          JointIndex.UPPER_LEG_0 => JointIndex.HIP,
          JointIndex.UPPER_LEG_1 => JointIndex.HIP,

          JointIndex.LOWER_LEG_0 => JointIndex.UPPER_LEG_0,
          JointIndex.LOWER_LEG_1 => JointIndex.UPPER_LEG_1,

          JointIndex.FOOT_0 => JointIndex.LOWER_LEG_0,
          JointIndex.FOOT_1 => JointIndex.LOWER_LEG_1,

          JointIndex.UPPER_ARM_0 => JointIndex.TORSO,
          JointIndex.UPPER_ARM_1 => JointIndex.TORSO,

          JointIndex.FOREARM_0 => JointIndex.UPPER_ARM_0,
          JointIndex.FOREARM_1 => JointIndex.UPPER_ARM_1,

          JointIndex.HAND_0 => JointIndex.FOREARM_0,
          JointIndex.HAND_1 => JointIndex.FOREARM_1,

          _ => (JointIndex) (-1),
      });

      var parentJoint = parentIndex < 0 || parentIndex >= joints.Length
          ? null
          : joints[parentIndex];

      List<(Joint joint, int index)> parentChildren;
      if (jointsByParent.ContainsKey(parentJoint)) {
        parentChildren = jointsByParent[parentJoint];
      } else {
        parentChildren = jointsByParent[parentJoint] =
            [];
      }

      parentChildren.Add((joint, i));
    }

    var finBonesAndJoints = new BoneTuple[joints.Length];
    var originalFlipByBone = new Dictionary<IReadOnlyBone, Vector3>();
    var jointQueue =
        new FinTuple3Queue<(Joint joint, int index), Matrix4x4, IBone>(
            jointsByParent[(Joint?) null]
                .Select(rootJoint => (rootJoint, Matrix4x4.Identity,
                                      model.Skeleton.Root)));
    while (jointQueue.TryDequeue(out var jointAndIndex,
                                 out var parentMatrix,
                                 out var parentFinBone)) {
      var invertedParentMatrix = parentMatrix.AssertInvert();

      var (joint, index) = jointAndIndex;
      var worldMatrix = joint.matrix;
      var localMatrix = worldMatrix * invertedParentMatrix;

      var finBone = parentFinBone.AddChild(localMatrix);
      finBone.Name = $"{(JointIndex) index}: {index}";
      finBonesAndJoints[index] = (finBone, joint, index);

      originalFlipByBone[finBone] = originalFlipByJoint[joint];

      if (jointsByParent.ContainsKey(joint)) {
        jointQueue.Enqueue(jointsByParent[joint]
                               .Select(childJoint => (
                                           childJoint, worldMatrix,
                                           bone: finBone)));
      }
    }

    TryToAddAnimations_(model, finBonesAndJoints, fileBundle.RomFile);

    // Adds face
    if (INCLUDE_FACE) {
      var faceMeshes = new List<IMesh>();
      if (!USE_GRANULAR_MESHES) {
        faceMeshes.Add(dlModelBuilder.StartNewMesh("face"));
      }

      n64Memory.SetSegment(0xF, headSegment);

      JankTstltUtil.SetCombiner(n64Hardware, true, false);

      br.Position = 0xeae0;
      var nosePosition = br.ReadUInt32s(2);
      var faceDlSegmentedAddresses = br.ReadUInt32s(3);

      var headRootBone = finBonesAndJoints[(int) JointIndex.HEAD_ROOT].Item1;
      var noseBone = finBonesAndJoints[(int) JointIndex.NOSE].Item1;

      rsp.ActiveBoneWeights =
          model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                            headRootBone);

      for (var i = 0; i < 3; ++i) {
        var offset = 0x4b0 + (uint) (i * 2 * 64 * 32);
        rdp.Tmem.SetImageSimple(offset,
                                N64ColorFormat.RGBA,
                                BitsPerTexel._16BPT,
                                64,
                                32,
                                F3dWrapMode.CLAMP,
                                F3dWrapMode.CLAMP);

        var faceDlSegmentedAddress = faceDlSegmentedAddresses[i];
        if (USE_GRANULAR_MESHES) {
          faceMeshes.Add(dlModelBuilder.StartNewMesh(
                             $"face {i}/3: {faceDlSegmentedAddress.ToHexString()}"));
        }

        dlModelBuilder.AddDl(new DisplayListReader().ReadDisplayList(
                                 n64Hardware.Memory,
                                 new F3dzex2OpcodeParser(),
                                 faceDlSegmentedAddress));
      }

      // Top of head (for bald characters)
      {
        var headDlSegmentedAddresses = br.ReadUInt32s(2);
        rdp.Tmem.SetImageSimple(0x4b0,
                                N64ColorFormat.RGBA,
                                BitsPerTexel._16BPT,
                                64,
                                1,
                                F3dWrapMode.CLAMP,
                                F3dWrapMode.CLAMP);
        for (var i = 1; i >= 0; --i) {
          var headDlSegmentedAddress = headDlSegmentedAddresses[i];
          if (headDlSegmentedAddress == 0) {
            continue;
          }

          if (USE_GRANULAR_MESHES) {
            faceMeshes.Add(dlModelBuilder.StartNewMesh(
                               $"head {i}/2: {headDlSegmentedAddress.ToHexString()}"));
          }

          dlModelBuilder.AddDl(new DisplayListReader().ReadDisplayList(
                                   n64Hardware.Memory,
                                   new F3dzex2OpcodeParser(),
                                   headDlSegmentedAddress));
        }
      }

      var noseDlSegmentedAddress = br.ReadUInt32();
      rsp.ActiveBoneWeights =
          model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE,
                                            noseBone);

      var noseX = nosePosition[0];
      var noseY = nosePosition[1];
      // These values look super random, but they come from digging into what
      // the game does--it generates a display list like this at runtime.
      var noseUls = (ushort) (noseX - 15);
      var noseUlt = (ushort) (noseY - 22);
      var noseLrs = (ushort) (noseX + 16);
      var noseLrt = (ushort) (noseY + 9);

      rdp.Tmem.SetImage(0x4b0,
                        N64ColorFormat.RGBA,
                        BitsPerTexel._16BPT,
                        63,
                        noseUls,
                        noseUlt,
                        noseLrs,
                        noseLrt,
                        F3dWrapMode.CLAMP,
                        F3dWrapMode.CLAMP);

      if (USE_GRANULAR_MESHES) {
        faceMeshes.Add(dlModelBuilder.StartNewMesh(
                           $"nose: {noseDlSegmentedAddress.ToHexString()}"));
      }

      dlModelBuilder.AddDl(new DisplayListReader().ReadDisplayList(
                               n64Hardware.Memory,
                               new F3dzex2OpcodeParser(),
                               noseDlSegmentedAddress));

      foreach (var faceMesh in faceMeshes) {
        foreach (var p in faceMesh.Primitives) {
          p.SetVertexOrder(VertexOrder.COUNTER_CLOCKWISE);
        }
      }
    }

    var headChosenPart0Tuples = headUnkSection5s
        .Select((unkSection5, unkSection5I) => {
          var segment = headSegment;
          return (segment, unkSection5, headMeshDefinitions,
                  headChosenPart0sById, unkSection5I);
        });
    var bodyChosenPart0Tuples = bodyUnkSection5s
        .Select((unkSection5, unkSection5I) => {
          var segment = bodySegment;
          return (segment, unkSection5, bodyMeshDefinitions,
                  bodyChosenPart0sById, unkSection5I);
        });

    var headAndBodyChosenPart0Tuples =
        headChosenPart0Tuples
            .Concat(bodyChosenPart0Tuples)
            .SelectMany(t => {
              var (segment, unkSection5, meshDefinitions, chosenPart0sById,
                  unkSection5I) = t;

              return unkSection5
                     .Subs
                     .Select((subUnkSection5, subUnkSection5I) => {
                       var meshDefinitionI = 4 * unkSection5I + subUnkSection5I;
                       var meshDefinition = meshDefinitions[meshDefinitionI];

                       var chosenPart = chosenPart0sById.GetValueOrDefault(
                           subUnkSection5.ChosenPartId,
                           skinChosenPart);

                       return (segment, meshDefinition, subUnkSection5,
                               chosenPart, unkSection5I, subUnkSection5I);
                     })
                     .Where(t => t.subUnkSection5 is {
                         IsEnabled: true, MeshDefinitionRamAddress: not 0
                     })
                     .Reverse();
            });

    var chosenPart0TuplesByMeshSetId =
        new ListDictionary<uint, ChosenPart0Tuple>();
    foreach (var chosenPart0Tuple in headAndBodyChosenPart0Tuples) {
      chosenPart0TuplesByMeshSetId.Add(
          chosenPart0Tuple.meshDefinition.MeshSetId,
          chosenPart0Tuple);
    }

    var headChosenPart1Tuples = headChosenPart1s.Select(chosenPart => {
      var segment = headSegment;
      return (segment, chosenPart,
              finBonesAndJoints[(uint) JointIndex.BODY_HEAD_ADAPTER].Item1,
              true);
    });
    var bodyChosenPart1Tuples = bodyChosenPart1s.Select(chosenPart => {
      var segment = bodySegment;
      return (segment, chosenPart,
              finBonesAndJoints[(uint) JointIndex.BODY_ROOT].Item1, false);
    });

    var chosenPart1TuplesByMeshSetId =
        new ListDictionary<uint, ChosenPart1Tuple>();
    foreach (var chosenPart1Tuple in headChosenPart1Tuples.Concat(
                 bodyChosenPart1Tuples)) {
      chosenPart1TuplesByMeshSetId.Add(
          chosenPart1Tuple.chosenPart.MeshSetId,
          chosenPart1Tuple);
    }

    // TODO: Add opaque ChosenPart1s here
    AddAllChosenPart0sInPass(
        false,
        model,
        n64Hardware,
        dlModelBuilder,
        finBonesAndJoints,
        skinChosenPart,
        chosenPart0TuplesByMeshSetId,
        originalFlipByBone);
    // TODO: Add transparent ChosenPart1s here
    AddAllChosenPart0sInPass(
        true,
        model,
        n64Hardware,
        dlModelBuilder,
        finBonesAndJoints,
        skinChosenPart,
        chosenPart0TuplesByMeshSetId,
        originalFlipByBone);

    foreach (var (bone, joint, jointIndex) in finBonesAndJoints) {
      var meshSetId = joint.MeshSetId;
      if (chosenPart1TuplesByMeshSetId.TryGetList(
              meshSetId,
              out var chosenPart1Tuples)) {
        foreach (var chosenPart1Tuple in chosenPart1Tuples) {
          TryToAddChosenPart1Tuple_(
              model,
              chosenPart1Tuple,
              n64Hardware,
              dlModelBuilder,
              joint,
              chosenPart1Tuple.bone,
              skinColor
          );
        }
      }
    }

    foreach (var material in model.MaterialManager.All) {
      if (material is IFixedFunctionMaterial fixedFunctionMaterial) {
        if (fixedFunctionMaterial.TryToGetCompiledDiffuseImage(
                out var compiledDiffuseImage)) {
          // HACK: Generates compiled textures for each material.
          // TODO: Move this upstream so it happens automatically
          var compiledDiffuseTexture
              = model.MaterialManager.CreateTexture(compiledDiffuseImage);
          compiledDiffuseTexture.Name = fixedFunctionMaterial.Name;

          var patternTexture = material.Textures.First();
          compiledDiffuseTexture.WrapModeU = patternTexture.WrapModeU;
          compiledDiffuseTexture.WrapModeV = patternTexture.WrapModeV;

          fixedFunctionMaterial.CompiledTexture = compiledDiffuseTexture;

          // HACK: Fixes transparency
          // TODO: Move this to within F3dzex2 logic
          fixedFunctionMaterial.SetDefaultAlphaCompare();
        }
      }
    }

    return model;
  }

  public static void AddAllChosenPart0sInPass(
      bool isAlphaPass,
      IModel model,
      IN64Hardware<SlicedN64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      BoneTuple[] finBonesAndJoints,
      ChosenPart0 skinChosenPart,
      IReadOnlyListDictionary<uint, ChosenPart0Tuple>
          chosenPart0TuplesByMeshSetId,
      IReadOnlyDictionary<IReadOnlyBone, Vector3> originalFlipByBone) {
    var rdp = n64Hardware.Rdp;

    foreach (var boneTuple in finBonesAndJoints) {
      // TODO: Clears geometry mode
      if (isAlphaPass) {
        rdp.ZCompare = true;
        rdp.ZUpdate = false;
        rdp.ZMode = ZMode.ZMODE_XLU;
        rdp.MultiplyCoverageWithAlpha = false;
        rdp.UseCoverageForAlpha = false;
        rdp.ForceBlending = true;
        rdp.P0 = BlenderPm.G_BL_CLR_IN;
        rdp.A0 = BlenderA.G_BL_0;
        rdp.M0 = BlenderPm.G_BL_CLR_IN;
        rdp.B0 = BlenderB.G_BL_1;
        rdp.P1 = BlenderPm.G_BL_CLR_IN;
        rdp.A1 = BlenderA.G_BL_A_IN;
        rdp.M1 = BlenderPm.G_BL_CLR_MEM;
        rdp.B1 = BlenderB.G_BL_1MA;
      } else {
        rdp.ZCompare = true;
        rdp.ZUpdate = true;
        rdp.ZMode = ZMode.ZMODE_OPA;
        rdp.MultiplyCoverageWithAlpha = false;
        rdp.UseCoverageForAlpha = true;
        rdp.ForceBlending = false;
        rdp.P0 = BlenderPm.G_BL_CLR_IN;
        rdp.A0 = BlenderA.G_BL_0;
        rdp.M0 = BlenderPm.G_BL_CLR_IN;
        rdp.B0 = BlenderB.G_BL_1;
        rdp.P1 = BlenderPm.G_BL_CLR_IN;
        rdp.A1 = BlenderA.G_BL_A_IN;
        rdp.M1 = BlenderPm.G_BL_CLR_MEM;
        rdp.B1 = BlenderB.G_BL_A_MEM;
      }

      FindCorrectJointAndClearStateAndAddChosenPart0Meshes(
          isAlphaPass,
          model,
          n64Hardware,
          dlModelBuilder,
          boneTuple,
          skinChosenPart,
          chosenPart0TuplesByMeshSetId,
          originalFlipByBone);
    }
  }

  public static void FindCorrectJointAndClearStateAndAddChosenPart0Meshes(
      bool isAlphaPass,
      IModel model,
      IN64Hardware<SlicedN64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      BoneTuple boneTuple,
      ChosenPart0 skinChosenPart,
      IReadOnlyListDictionary<uint, ChosenPart0Tuple>
          chosenPart0TuplesByMeshSetId,
      IReadOnlyDictionary<IReadOnlyBone, Vector3> originalFlipByBone) {
    var (_, joint, jointIndex) = boneTuple;

    var meshSetId = joint.MeshSetId;
    if (chosenPart0TuplesByMeshSetId.TryGetList(
            meshSetId,
            out var chosenPart0Tuples)) {
      // TODO: This only sometimes works
      var chosenPart0TuplesInOrder =
          jointIndex is not (int) JointIndex.TORSO
              ? chosenPart0Tuples.AsEnumerable()
              : chosenPart0Tuples.Reverse();

      var firstChosenPart0Tuple = chosenPart0TuplesInOrder.First();
      foreach (var chosenPart0Tuple in chosenPart0TuplesInOrder) {
        var isFirst = chosenPart0Tuple.unkSection5I ==
                      firstChosenPart0Tuple.unkSection5I;

        AddChosenPart0MeshesForJoint(
            isAlphaPass,
            model,
            n64Hardware,
            dlModelBuilder,
            chosenPart0Tuple,
            boneTuple,
            isFirst,
            skinChosenPart,
            originalFlipByBone);
      }
    }
  }

  public static void AddChosenPart0MeshesForJoint(
      bool isAlphaPass,
      IModel model,
      IN64Hardware<SlicedN64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      ChosenPart0Tuple chosenPart0Tuple,
      BoneTuple boneTuple,
      bool useParentBone,
      ChosenPart0 skinChosenPart,
      IReadOnlyDictionary<IReadOnlyBone, Vector3> originalFlipByBone) {
    // Loosely based on logic from decomp at 0x803186d8

    var (segment, meshDefinition, unkSection5, chosenPart0, unkSection5I,
        subUnkSection5I) = chosenPart0Tuple;

    if (HARDCODED_MESH_SET_ID != -1 &&
        meshDefinition.MeshSetId != HARDCODED_MESH_SET_ID) {
      return;
    }

    // HACK: Not really sure how this works in-game, but this is necessary to
    // fix hair.
    n64Hardware.Memory.SetSegment(0xF, segment);
    if (meshDefinition.MeshSegmentedAddresses.TryGetFirst(
            i => i != 0,
            out var firstMeshSegmentedAddress) &&
        n64Hardware.Memory
                   .TryToOpenPossibilitiesAtSegmentedAddress(
                       firstMeshSegmentedAddress,
                       out var possibilities) &&
        possibilities.TryGetFirst(out var sbr)) {
      var relativeMeshBaseOffset = sbr.Position;
      var absoluteMeshOffset = segment.Offset + relativeMeshBaseOffset;

      sbr.Position = relativeMeshBaseOffset + 4 * 3;
      var vertexSectionSize = sbr.ReadUInt32();

      sbr.Position = relativeMeshBaseOffset + 4 * 8;
      var relativeVertexSectionOffset = sbr.ReadUInt32();
      var absoluteVertexSectionOffset
          = absoluteMeshOffset + relativeVertexSectionOffset;

      n64Hardware.Memory.SetSegment(0xE,
                                    (uint) absoluteVertexSectionOffset,
                                    vertexSectionSize);
    }

    // From decomp, at 0x80116b6c
    var rsp = n64Hardware.Rsp;
    rsp.PrimColor = Color.FromArgb(0xd2, 0xff, 0xff, 0xff);
    rsp.PrimLodFraction = 1f * 0x7f / 0x100;

    int[] patternIndices;
    if (isAlphaPass) {
      patternIndices = [0xa, 0xb, 0xc];
    } else {
      patternIndices = [0x0, 0x1, 0x3, 0x4, 0x5, 0x2, 0x6, 0x7, 0x8, 0x9];
    }

    foreach (var patternIndex in patternIndices) {
      var primitiveDlSegmentedAddress
          = meshDefinition.PrimitiveDisplayListSegmentedAddresses[patternIndex];
      var vertexDlSegmentedAddress
          = meshDefinition.VertexDisplayListSegmentedAddresses[patternIndex];

      if (primitiveDlSegmentedAddress == 0) {
        continue;
      }

      // TODO: Does an extra loop internally, why??
      {
        // TODO: Does some kind of check for these to make sure it doesn't
        // add redundant DLs. Are these needed??

        ChosenPart0Util.SetUpOtherModeLAndCombiner(
            n64Hardware,
            patternIndex,
            0);

        ChosenPart0Util.SetUp(n64Hardware, chosenPart0, patternIndex);
        if (chosenPart0.Id == 0) {
          ChosenPart0Util.SetUpInvertedEnvironmentColorOrSomethingElse(
              n64Hardware,
              skinChosenPart.ChosenColor0);
        }

        // TODO: Does actually set culling here internally, but we should use accurate logic
        if (USE_JANK) {
          n64Hardware.Rsp.CullingMode
              = JankTstltUtil.GetCullingModeForChosenPartId(chosenPart0.Id);
        }

        var (childBone, joint, jointIndex) = boneTuple;
        var parentBone = childBone.Parent!;

        var primitiveDlVertexMatrix
            = Matrix4x4.CreateScale(originalFlipByBone[childBone]);
        var primitiveDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
            VertexSpace.RELATIVE_TO_BONE,
            childBone);

        // HACK: What a fucking nightmare. Why does being first impact this????
        Matrix4x4 vertexMatrix;
        IBoneWeights vertexDlBoneWeights;
        if (!useParentBone) {
          vertexMatrix = primitiveDlVertexMatrix;
          vertexDlBoneWeights = primitiveDlBoneWeights;
        } else if (!joint.isLeft ||
                   jointIndex is not ((int) JointIndex.UPPER_ARM_1
                                      or (int) JointIndex.UPPER_LEG_1)) {
          vertexMatrix
              = Matrix4x4.CreateScale(originalFlipByBone[parentBone]);
          vertexDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
              VertexSpace.RELATIVE_TO_BONE,
              parentBone);
        } else {
          // HACK: Flips shoulder/hip across the axis.
          vertexMatrix = Matrix4x4.CreateScale(1, -1, 1);
          vertexDlBoneWeights = model.Skin.GetOrCreateBoneWeights(
              VertexSpace.RELATIVE_TO_BONE,
              parentBone);
        }

        (uint, Matrix4x4?, IBoneWeights)[] displayLists
            = vertexDlSegmentedAddress == 0
                ? [
                    (primitiveDlSegmentedAddress,
                     primitiveDlVertexMatrix,
                     primitiveDlBoneWeights)
                ]
                : [
                    (vertexDlSegmentedAddress,
                     vertexMatrix,
                     vertexDlBoneWeights),
                    (primitiveDlSegmentedAddress,
                     primitiveDlVertexMatrix,
                     primitiveDlBoneWeights)
                ];

        var mesh = TstltUtil.AddDisplayLists(
            model,
            segment,
            n64Hardware,
            dlModelBuilder,
            $"joint({(JointIndex) jointIndex}): chosenPart0(id: {chosenPart0.Id}, meshSetId: {meshDefinition.MeshSetId}, unkSection5: {unkSection5I}, subUnkSection5: {subUnkSection5I}, patternIndex: {patternIndex})",
            joint.isLeft,
            displayLists);

        dlModelBuilder.TransparentCutoff = .5f;
      }
    }
  }

  private static void TryToAddChosenPart1Tuple_(
      IModel model,
      ChosenPart1Tuple chosenPart1Tuple,
      N64Hardware<SlicedN64Memory> n64Hardware,
      DlModelBuilder dlModelBuilder,
      Joint joint,
      IReadOnlyBone bone,
      Rgba32 skinColor) {
    var (segment, chosenPart1, _, isHead) = chosenPart1Tuple;

    if (HARDCODED_MESH_SET_ID != -1 &&
        chosenPart1.MeshSetId != HARDCODED_MESH_SET_ID) {
      return;
    }

    n64Hardware.Memory.SetSegment(0xF, segment);

    // TODO: Is this correct??
    var dlIndex = 3;
    var dlCount = 2;

    JankTstltUtil.ResetRspAndRdp(n64Hardware);

    n64Hardware.Rsp.CullingMode
        = JankTstltUtil.GetCullingModeForMeshSetId(joint.MeshSetId);

    switch (isHead, chosenPart1Tuple.chosenPart.MeshSetId) {
      // Ear
      case (true, 6): {
        // HACK: Hardcodes ear color to skin color.
        JankTstltUtil.SetCombiner(
            n64Hardware,
            true,
            false,
            OneOf<uint, Color>.FromT1(skinColor.ToSystemColor()));
        n64Hardware.Rsp.EnvironmentColor = Color.FromArgb(
            0xFF,
            (byte) (0xFF - skinColor.Rb),
            (byte) (0xFF - skinColor.Gb),
            (byte) (0xFF - skinColor.Bb));
        n64Hardware.Rsp.PrimColor = Color.FromArgb(0xD2, 0, 0, 0);
        break;
      }
      // Beard
      case (true, 7): {
        // HACK: Hardcodes pattern 7.
        JankTstltUtil.SetCombiner(
            n64Hardware,
            true,
            false,
            OneOf<uint, Color>.FromT0(0x0F003800));
        break;
      }
      // Glasses
      case (true, 8): {
        // HACK: Hardcodes cycle count to 2.
        n64Hardware.Rdp.CycleType = CycleType.TWO_CYCLE;
        // TODO: This still isn't quite right because in-game, glasses catch
        // the light and reflect white. Currently, this just renders as the
        // frame with no reflection, though. Reflections are probably done with
        // blend mode nonsense?
        break;
      }
    }

    var boneWeights
        = model.Skin.GetOrCreateBoneWeights(VertexSpace.RELATIVE_TO_BONE, bone);
    TstltUtil.AddDisplayLists(
        model,
        segment,
        n64Hardware,
        dlModelBuilder,
        $"chosenPart1({chosenPart1.MeshSetId})",
        joint.isLeft,
        chosenPart1.DisplayListSegmentedAddresses
                   .Skip(dlIndex)
                   .Take(dlCount)
                   .Where(dlSegmentedAddress => dlSegmentedAddress != 0)
                   .Select(dlSegmentedAddress
                               => (dlSegmentedAddress, (Matrix4x4?) null,
                                   boneWeights)));

    JankTstltUtil.ResetRspAndRdp(n64Hardware);

    dlModelBuilder.TransparentCutoff = .5f;
  }

  private static void TryToAddAnimations_(
      IModel finModel,
      BoneTuple[] finBonesAndJoints,
      IReadOnlyTreeFile? romFile) {
    if (romFile == null) {
      return;
    }

    var br = romFile.OpenReadAsBinary(Endianness.BigEndian);
    br.Position = 0x1DEAC4;

    var finAnimationManager = finModel.AnimationManager;

    const uint SEGMENT_4_OFFSET = 0x1C0CD0;

    var headBone = finBonesAndJoints[(int) JointIndex.NECK].bone;
    var leftUpperArmBone = finBonesAndJoints[(int) JointIndex.UPPER_ARM_0].bone;
    var leftForearmBone = finBonesAndJoints[(int) JointIndex.FOREARM_0].bone;
    var leftHandBone = finBonesAndJoints[(int) JointIndex.HAND_0].bone;
    var rightUpperArmBone
        = finBonesAndJoints[(int) JointIndex.UPPER_ARM_1].bone;
    var rightForearmBone = finBonesAndJoints[(int) JointIndex.FOREARM_1].bone;
    var rightHandBone = finBonesAndJoints[(int) JointIndex.HAND_1].bone;
    var torsoBone = finBonesAndJoints[(int) JointIndex.TORSO].bone;
    var hipBone = finBonesAndJoints[(int) JointIndex.HIP].bone;
    var leftThighBone = finBonesAndJoints[(int) JointIndex.UPPER_LEG_0].bone;
    var rightThighBone = finBonesAndJoints[(int) JointIndex.UPPER_LEG_1].bone;

    for (var i = 0; i < 96; ++i) {
      var finAnimation = finAnimationManager.AddAnimation();

      finAnimation.FrameCount = 1;
      finAnimation.FrameRate = 30;

      var unk0 = br.ReadUInt16();
      var maybeLength = br.ReadUInt16();

      var unk1 = br.ReadUInt32();
      var segmentedAddress0 = br.ReadUInt32();
      var segmentedAddress1 = br.ReadUInt32();
      var segmentedAddress2 = br.ReadUInt32();

      var headBoneTracks = finAnimation.GetOrCreateBoneTracks(headBone);
      var leftUpperArmBoneTracks
          = finAnimation.GetOrCreateBoneTracks(leftUpperArmBone);
      var leftForearmBoneTracks
          = finAnimation.GetOrCreateBoneTracks(leftForearmBone);
      var leftHandBoneTracks = finAnimation.GetOrCreateBoneTracks(leftHandBone);
      var rightUpperArmBoneTracks
          = finAnimation.GetOrCreateBoneTracks(rightUpperArmBone);
      var rightForearmBoneTracks
          = finAnimation.GetOrCreateBoneTracks(rightForearmBone);
      var rightHandBoneTracks
          = finAnimation.GetOrCreateBoneTracks(rightHandBone);
      var torsoBoneTracks = finAnimation.GetOrCreateBoneTracks(torsoBone);
      var hipBoneTracks = finAnimation.GetOrCreateBoneTracks(hipBone);
      var leftThighBoneTracks
          = finAnimation.GetOrCreateBoneTracks(leftThighBone);
      var rightThighBoneTracks
          = finAnimation.GetOrCreateBoneTracks(rightThighBone);

      IoUtils.SplitSegmentedAddress(segmentedAddress0, out _, out var offset0);
      IoUtils.SplitSegmentedAddress(segmentedAddress1, out _, out var offset1);
      IoUtils.SplitSegmentedAddress(segmentedAddress2, out _, out var offset2);

      br.SubreadAt(
          SEGMENT_4_OFFSET + offset0,
          () => {
            var basePose = br.ReadNew<BasePose>();


            SetFirstFrame_(headBoneTracks, false, i, basePose.HeadRotation);

            SetFirstFrame_(leftUpperArmBoneTracks,
                           false,
                           i,
                           basePose.LeftUpperArmRotation);
            SetFirstFrame_(leftForearmBoneTracks,
                           false,
                           i,
                           basePose.LeftForearmRotation);
            SetFirstFrame_(leftHandBoneTracks,
                           true,
                           i,
                           basePose.LeftHandRotation);

            SetFirstFrame_(rightUpperArmBoneTracks,
                           false,
                           i,
                           basePose.RightUpperArmRotation);
            SetFirstFrame_(rightForearmBoneTracks,
                           false,
                           i,
                           basePose.RightForearmRotation);
            SetFirstFrame_(rightHandBoneTracks,
                           true,
                           i,
                           basePose.RightHandRotation);

            SetFirstFrame_(torsoBoneTracks, false, i, basePose.TorsoRotation);
            SetFirstFrame_(hipBoneTracks, false, i, basePose.HipRotation);
            SetFirstFrame_(leftThighBoneTracks,
                           false,
                           i,
                           basePose.LeftThighRotation);
            SetFirstFrame_(rightThighBoneTracks,
                           false,
                           i,
                           basePose.RightThighRotation);
          });
    }
  }

  private static void SetFirstFrame_(IBoneTracks boneTracks, bool isHand, int i, short degreesTimesTen) {
    var defaultLocalEulerRadians = boneTracks.Bone.Transform.LocalEulerRadians ?? Vector3.Zero;

    //degreesTimesTen = (short) (i * 10);
    var deltaRadians = (degreesTimesTen / 10f) / 180 * MathF.PI;

    var newLocalEulerRadians = defaultLocalEulerRadians + (isHand
        ? new Vector3(0, -deltaRadians, 0)
        : new Vector3(deltaRadians, 0, 0));

    boneTracks
        .UseCombinedQuaternionKeyframes(1)
        .Add(new Keyframe<Quaternion>(
                 0,
                 newLocalEulerRadians.CreateZyxRadians()));
  }

  // TODO: I have no idea what these are meant to be
  private static Quaternion
      GetQuaternionFromRotation_(short degreesTimesTen) {
    var radians = (degreesTimesTen / 10f) / 180 * MathF.PI;
    return Quaternion.CreateFromAxisAngle(Vector3.UnitY, radians);
  }
}

// https://wiki.cloudmodding.com/oot/F3DZEX2#Vertex_Structure
[BinarySchema]
public sealed partial class Vertex : IBinaryDeserializable {
  public Vector3s Position { get; } = new();

  private ushort padding_ = 0;

  public short U { get; set; }
  public short V { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalX { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalY { get; set; }

  [NumberFormat(SchemaNumberType.SN8)]
  public float NormalZ { get; set; }

  public byte Alpha { get; set; }
}

[BinarySchema]
public sealed partial class BasePose : IBinaryDeserializable {
  public short HeadRotation { get; set; }
  public short LeftForearmRotation { get; set; }
  public short LeftHandRotation { get; set; }
  public short RightForearmRotation { get; set; }
  public short RightHandRotation { get; set; }
  public short LeftUpperArmRotation { get; set; }
  public short RightUpperArmRotation { get; set; }
  public short LeftThighRotation { get; set; }
  public short RightThighRotation { get; set; }
  public short TorsoRotation { get; set; }
  public short HipRotation { get; set; }
}