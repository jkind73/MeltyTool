using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using fin.model;

namespace fin.scene.instance;

public partial class SceneInstanceImpl {
  private class SceneNodeInstanceImpl(IReadOnlySceneNode sceneObject)
      : ISceneNodeInstance {
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

    public IReadOnlySceneNode Definition => sceneObject;

    public IReadOnlyList<ISceneNodeInstance> ChildNodes { get; }
      = sceneObject.ChildNodes
                   .Select(n => new SceneNodeInstanceImpl(n))
                   .ToArray();

    public Vector3 Position { get; private set; } = sceneObject.Position;
    public IRotation Rotation { get; } = sceneObject.Rotation;
    public Vector3 Scale { get; private set; } = sceneObject.Scale;

    public ISceneNodeInstance SetPosition(float x, float y, float z) {
      this.Position = new Vector3(x, y, z);
      return this;
    }

    public ISceneNodeInstance SetRotationRadians(float xRadians,
                                                 float yRadians,
                                                 float zRadians) {
      this.Rotation.SetRadians(
          xRadians,
          yRadians,
          zRadians
      );
      return this;
    }

    public ISceneNodeInstance SetRotationDegrees(float xDegrees,
                                                 float yDegrees,
                                                 float zDegrees) {
      this.Rotation.SetDegrees(
          xDegrees,
          yDegrees,
          zDegrees
      );
      return this;
    }

    public ISceneNodeInstance SetScale(float x, float y, float z) {
      this.Scale = new Vector3(x, y, z);
      return this;
    }

    public IReadOnlyList<ISceneModelInstance> Models { get; }
      = sceneObject.Models
                   .Select(m => new SceneModelInstanceImpl(m))
                   .ToArray();

    public void Tick() {
      foreach (var component in sceneObject.Components) {
        if (component is ISceneNodeTickComponent tickComponent) {
          tickComponent.Tick(this);
        }
      }

      foreach (var child in this.ChildNodes) {
        child.Tick();
      }
    }


    private float viewerScale_ = 1;

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