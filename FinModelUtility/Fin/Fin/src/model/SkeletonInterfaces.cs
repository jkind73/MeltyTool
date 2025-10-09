using System.Collections.Generic;
using System.Numerics;

using fin.data.indexable;
using fin.math.transform;

using readOnly;

namespace fin.model;

[GenerateReadOnly]
public partial interface ISkeleton : IEnumerable<IReadOnlyBone> {
  new IBone Root { get; }
  new IReadOnlyList<IBone> Bones { get; }
}

public enum FaceTowardsCameraType {
  NONE,
  YAW_ONLY,
  YAW_AND_PITCH
}

[GenerateReadOnly]
public partial interface ILeafBone : IIndexable, INamed {
  new IBone Root { get; }
  new IBone? Parent { get; }

  new ITransform3d LocalTransform { get; }

  new bool IgnoreParentScale { get; set; }

  IBone AlwaysFaceTowardsCamera(
      FaceTowardsCameraType faceTowardsCameraType,
      in Quaternion adjustment);

  IBone AlwaysFaceTowardsCamera(FaceTowardsCameraType faceTowardsCameraType);

  new FaceTowardsCameraType FaceTowardsCameraType { get; }
  new Quaternion FaceTowardsCameraAdjustment { get; }
}

[GenerateReadOnly]
public partial interface IBone : ILeafBone {
  new IReadOnlyList<IBone> Children { get; }
  IBone AddRoot(float x, float y, float z);
  IBone AddChild(float x, float y, float z);
}