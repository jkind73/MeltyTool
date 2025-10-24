using fin.animation;
using fin.model;
using fin.scene;
using fin.scene.components;
using fin.ui.rendering.gl.model;
using fin.ui.rendering.viewer;

namespace fin.ui.rendering;

public interface ISceneViewer {
  ISceneInstance? Scene { get; set; }

  IAnimatableModel? FirstSceneModel { get; }
  IAnimationPlaybackManager? AnimationPlaybackManager { get; }
  IReadOnlyModelAnimation? Animation { get; set; }
  ISkeletonRenderer? SkeletonRenderer { get; }
  ISkyboxRenderer? SkyboxRenderer { get; set; }
}