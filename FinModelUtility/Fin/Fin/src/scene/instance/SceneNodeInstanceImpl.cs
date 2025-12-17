using System;
using System.Collections.Generic;
using System.Linq;

using fin.math.rotations;
using fin.math.transform;

namespace fin.scene.instance;

public partial class SceneInstanceImpl {
  private class SceneNodeInstanceImpl
      : ISceneNodeInstance {
    private float viewerScale_ = 1;
    private readonly IReadOnlySceneNode sceneObject_;
    public SceneNodeInstanceImpl(SceneNodeInstanceImpl? parent,
                                 IReadOnlySceneNode sceneObject) {
      this.sceneObject_ = sceneObject;
      
      this.Transform = new Transform3d(parent?.Transform);
      this.Transform.Translation = sceneObject.Position;
      this.Transform.Rotation = QuaternionUtil.Create(sceneObject.Rotation);
      this.Transform.Scale = sceneObject.Scale;

      this.ChildNodes = sceneObject.ChildNodes
                                   .Select(n => new SceneNodeInstanceImpl(
                                               this,
                                               n))
                                   .ToArray();
      this.Models = sceneObject.Models
                               .Select(m => new SceneModelInstanceImpl(m))
                               .ToArray();
    }

    ~SceneNodeInstanceImpl() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      foreach (var model in this.Models) {
        model.Dispose();
      }

      foreach (var child in this.ChildNodes) {
        child.Dispose();
      }
    }

    public IReadOnlySceneNode Definition => this.sceneObject_;

    public IReadOnlyList<ISceneNodeInstance> ChildNodes { get; }

    public Transform3d Transform { get; }

    public IReadOnlyList<ISceneModelInstance> Models { get; }

    public void Tick() {
      foreach (var component in this.sceneObject_.Components) {
        if (component is ISceneNodeTickComponent tickComponent) {
          tickComponent.Tick(this);
        }
      }

      foreach (var child in this.ChildNodes) {
        child.Tick();
      }
    }

    public float ViewerScale {
      get => this.viewerScale_;
      set {
        this.viewerScale_ = value;
        foreach (var model in this.Models) {
          model.ViewerScale = this.viewerScale_;
        }
      }
    }
  }
}