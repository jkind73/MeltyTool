using System;
using System.Collections.Generic;
using System.Numerics;

using fin.animation;
using fin.data.dictionaries;
using fin.math;
using fin.model;
using fin.model.skeleton;
using fin.model.util;
using fin.scene.components;

using readOnly;

namespace fin.scene;

public interface ITickable {
  void Tick();
}

[GenerateReadOnly]
public partial interface ISceneInstance : ITickable, IDisposable {
  new IReadOnlyScene Definition { get; }

  new IReadOnlyList<ISceneAreaInstance> Areas { get; }

  new IReadOnlyLighting? Lighting { get; }

  new float ViewerScale { get; set; }
}

/// <summary>
///   A single area in a scene. This is used to split out the different
///   regions into separate pieces that can be loaded separately; for
///   example, in Ocarina of Time, this is used to represent a single room in
///   a dungeon.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneAreaInstance : ITickable, IDisposable {
  new IReadOnlySceneArea Definition { get; }

  new IReadOnlyList<ISceneNodeInstance> RootNodes { get; }

  new float ViewerScale { get; set; }

  new ISceneNodeInstance? CustomSkyboxObject { get; }
}

/// <summary>
///   An instance of an object in a scene. This can be used for anything that
///   appears in the scene, such as the level geometry, scenery, or
///   characters.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneNodeInstance : ITickable, IDisposable {
  new IReadOnlySceneNode Definition { get; }

  new IReadOnlyList<ISceneNodeInstance> ChildNodes { get; }

  new Vector3 Position { get; }
  new IRotation Rotation { get; }
  new Vector3 Scale { get; }

  ISceneNodeInstance SetPosition(float x, float y, float z);

  ISceneNodeInstance SetPosition(Vector3 position)
    => this.SetPosition(position.X, position.Y, position.Z);

  ISceneNodeInstance SetRotationRadians(float xRadians,
                                        float yRadians,
                                        float zRadians);

  ISceneNodeInstance SetRotationDegrees(float xDegrees,
                                        float yDegrees,
                                        float zDegrees);

  new IReadOnlyList<ISceneModelInstance> Models { get; }

  new float ViewerScale { get; set; }
}

/// <summary>
///   An instance of a model rendered in a scene. This will automatically
///   take care of rendering animations, and also supports adding sub-models
///   onto bones.
/// </summary>
[GenerateReadOnly]
public partial interface ISceneModelInstance
    : IAnimatableModel, ITickable, IDisposable {
  new IReadOnlySceneModel Definition { get; }

  new IReadOnlyListDictionary<IReadOnlyBone, ISceneModelInstance> Children {
    get;
  }

  new SimpleBoneTransformView SimpleBoneTransformView { get; }
  new ITextureTransformManager TextureTransformManager { get; }

  new float ViewerScale { get; set; }
}