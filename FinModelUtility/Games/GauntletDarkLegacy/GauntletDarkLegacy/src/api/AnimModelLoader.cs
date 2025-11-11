using System.Numerics;

using fin.data.dictionaries;
using fin.data.queues;
using fin.io;
using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.model.io;
using fin.model.io.importers;
using fin.model.util;

using gdl.schema;

namespace gdl.api;

public sealed record AnimModelFileBundle(IReadOnlyTreeFile MainFile)
    : IModelFileBundle;

public sealed class AnimModelImporter : IModelImporter<AnimModelFileBundle> {
  public IModel Import(AnimModelFileBundle fileBundle) {
    var anim = fileBundle.MainFile.ReadNew<Anim>();

    var finModel = new ModelImpl {
        FileBundle = fileBundle,
        Files = new HashSet<IReadOnlyGenericFile>([fileBundle.MainFile])
    };

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
      while (boneQueue.TryDequeue(out var gdlBone,
                                  out var finParentBone,
                                  out var invertedParentWorldMatrix)) {
        var worldMatrix = Matrix4x4.CreateTranslation(gdlBone.Position);
        var localMatrix = worldMatrix * invertedParentWorldMatrix;

        var finBone = finParentBone.AddChild(localMatrix);
        finBone.Name = gdlBone.Name;

        if (gdlBonesByParent.TryGetList(gdlBone, out var childGdlBones)) {
          boneQueue.Enqueue(
              childGdlBones.Select(childGdlBone => (
                                       childGdlBone,
                                       finBone,
                                       worldMatrix.AssertInvert())));
        }
      }
    }

    return finModel;
  }
}