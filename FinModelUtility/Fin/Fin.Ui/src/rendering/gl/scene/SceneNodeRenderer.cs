using fin.model;
using fin.scene;

namespace fin.ui.rendering.gl.scene;

public sealed class SceneNodeRenderer : IRenderable, IDisposable {
  private readonly ISceneNodeInstance sceneNode_;
  private readonly SceneModelRenderer[] modelRenderers_;
  private readonly SceneNodeRenderer[] childNodeRenderers_;

  private IReadOnlySceneNode? selectedNode_;

  public SceneNodeRenderer(ISceneNodeInstance sceneNode,
                           IReadOnlyLighting? lighting) {
    this.sceneNode_ = sceneNode;
    this.modelRenderers_
        = sceneNode
          .Models
          .Select(model => new SceneModelRenderer(model, lighting))
          .ToArray();
    this.childNodeRenderers_
        = sceneNode
          .ChildNodes
          .Select(n => new SceneNodeRenderer(n, lighting))
          .ToArray();

    SelectedNodeService.OnNodeSelected += selectedNode
        => this.selectedNode_ = selectedNode;
  }

  ~SceneNodeRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    foreach (var modelRenderer in this.ModelRenderers) {
      modelRenderer.Dispose();
    }

    foreach (var child in this.ChildNodeRenderers) {
      child.Dispose();
    }
  }

  public IReadOnlyList<SceneNodeRenderer> ChildNodeRenderers
    => this.childNodeRenderers_;

  public IReadOnlyList<SceneModelRenderer> ModelRenderers
    => this.modelRenderers_;

  public void Render() {
    var isSelected = this.selectedNode_ == this.sceneNode_.Definition;
    if (isSelected) {
      GlUtil.RenderOutline(this.RenderImpl_);
    }

    this.RenderImpl_();

    if (isSelected) {
      GlUtil.RenderHighlight(this.RenderImpl_);
    }
  }

  private void RenderImpl_() {
    GlTransform.PushMatrix();
    GlTransform.MultMatrix(this.sceneNode_.Transform.LocalMatrix);

    foreach (var model in this.modelRenderers_) {
      model.Render();
    }

    foreach (var component in this.sceneNode_.Definition.Components) {
      if (component is ISceneNodeRenderComponent renderComponent) {
        renderComponent.Render(this.sceneNode_);
      }
    }

    foreach (var child in this.ChildNodeRenderers) {
      child.Render();
    }

    GlTransform.PopMatrix();
  }
}