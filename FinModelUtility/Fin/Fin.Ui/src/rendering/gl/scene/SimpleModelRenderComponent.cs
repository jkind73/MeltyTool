using fin.animation;
using fin.config;
using fin.math;
using fin.model;
using fin.model.skeleton;
using fin.model.util;
using fin.scene;
using fin.scene.components;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering.gl.scene;

public sealed class SimpleModelRenderComponent : IModelRenderComponent {
  private readonly IReadOnlyMesh[] meshes_;
  private readonly IModelRenderer modelRenderer_;
  private readonly HashSet<IReadOnlyMesh> hiddenMeshes_ = [];
  private bool isBoneSelected_;
  private bool hadOverrides_;

  private bool needsToAlwaysUpdateMatrices_;

  public SimpleModelRenderComponent(IReadOnlyModel model,
                                    IReadOnlyLighting? lighting) {
    this.Model = model;
    this.meshes_ = model.Skin.Meshes.ToArray();

    this.SimpleBoneTransformView = new();

    this.BoneTransformManager = new BoneTransformManager2();
    this.BoneTransformManager.CalculateStaticMatricesForManualProjection(
        this.Model,
        true);

    this.AnimationPlaybackManager = new FrameAdvancer {
        LoopPlayback = true,
    };

    this.Animation =
        this.Model.AnimationManager.Animations.FirstOrDefault();
    this.AnimationPlaybackManager.IsPlaying = true;

    this.TextureTransformManager = new TextureTransformManager();

    this.modelRenderer_ =
        new ModelRenderer(model,
                          lighting,
                          this.BoneTransformManager,
                          this.TextureTransformManager) {
            HiddenMeshes = this.hiddenMeshes_,
            UseLighting = new UseLightingDetector().ShouldUseLightingFor(model)
        };

    this.SkeletonRenderer
        = new SkeletonRenderer(model, this.BoneTransformManager);

    SelectedBoneService.OnBoneSelected += selectedBone => {
      var isBoneInModel = false;
      if (selectedBone != null) {
        isBoneInModel = model.Skeleton.Bones.Contains(selectedBone);
      }

      this.isBoneSelected_ = isBoneInModel;
      this.SkeletonRenderer.SelectedBone
          = this.isBoneSelected_ ? selectedBone : null;
    };

    this.needsToAlwaysUpdateMatrices_
        = model.Skeleton.Bones.Any(b => b.FaceTowardsCameraType !=
                                        FaceTowardsCameraType.NONE);
  }

  ~SimpleModelRenderComponent() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.modelRenderer_.Dispose();

  public ISkeletonRenderer SkeletonRenderer { get; }

  public void Render(ISceneNodeInstance _) => this.Render();

  public void Render(bool allowUpdatingState = true) {
    GlTransform.PushMatrix();

    var model = this.Model;
    var skeleton = model.Skeleton;

    var animation = this.Animation;
    var animationPlaybackManager = this.AnimationPlaybackManager;

    if (allowUpdatingState) {
      this.hiddenMeshes_.Clear();
      foreach (var mesh in this.meshes_) {
        if (mesh.DefaultDisplayState == MeshDisplayState.HIDDEN) {
          this.hiddenMeshes_.Add(mesh);
        }
      }

      var hasAnyOverrides = this.SimpleBoneTransformView.HasAnyOverrides;
      if (animation != null ||
          this.needsToAlwaysUpdateMatrices_ ||
          // Need to update for one extra frame, if overrides were just cleared.
          this.hadOverrides_ ||
          hasAnyOverrides) {
        animationPlaybackManager.Tick();
        this.BoneTransformManager.CalculateMatrices(
            skeleton.Root,
            model.Skin.BoneWeights,
            this.SimpleBoneTransformView,
            BoneWeightTransformType.FOR_RENDERING,
            GlTransform.ModelMatrix);
      }
      
      this.hadOverrides_ = hasAnyOverrides;

      if (animation != null) {
        var frame = (float) animationPlaybackManager.Frame;
        this.TextureTransformManager.CalculateMatrices(
            model.MaterialManager.Textures,
            (animation, frame));

        if (animation.HasAnyMeshTracks) {
          foreach (var meshTracks in animation.MeshTracks) {
            if (!meshTracks.DisplayStates.TryGetAtFrame(
                    frame,
                    out var displayState)) {
              continue;
            }

            if (displayState == MeshDisplayState.HIDDEN) {
              this.hiddenMeshes_.Add(meshTracks.Mesh);
            } else {
              this.hiddenMeshes_.Remove(meshTracks.Mesh);
            }
          }
        }
      } else {
        this.TextureTransformManager.CalculateMatrices(
            model.MaterialManager.Textures,
            null);
      }
    }

    this.modelRenderer_.Render();

    if (FinConfig.ShowSkeleton || this.isBoneSelected_) {
      this.SkeletonRenderer.Render();
    }

    GlTransform.PopMatrix();
  }

  public IReadOnlyModel Model { get; }

  public IBoneTransformManager2 BoneTransformManager { get; }
  public SimpleBoneTransformView SimpleBoneTransformView { get; }
  public ITextureTransformManager TextureTransformManager { get; }

  public IReadOnlyModelAnimation? Animation {
    get;
    set {
      if (field == value) {
        return;
      }

      field = value;
      this.SimpleBoneTransformView.AnimatedBoneTransformView.Animation = value;

      var apm = this.AnimationPlaybackManager;

      apm.Frame = 0;
      apm.FrameRate = (int) (value?.FrameRate ?? 20);
      apm.TotalFrames = value?.FrameCount ?? 0;
    }
  }

  public IAnimationPlaybackManager AnimationPlaybackManager {
    get;
    set {
      field = value;
      this.SimpleBoneTransformView.AnimatedBoneTransformView.PlaybackManager
          = value;
    }
  }
}