using fin.model;
using fin.scene;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;

namespace MarioArtistTool.scenery;

public sealed class ShadowRenderer(IReadOnlyModel model) : ISceneNodeRenderComponent {
  private readonly IModelRenderer impl_ = new ModelRenderer(model);

  public void Render(ISceneObjectInstance self)
    => GlUtil.RenderAsShadow(() => { this.impl_.Render(); });
}