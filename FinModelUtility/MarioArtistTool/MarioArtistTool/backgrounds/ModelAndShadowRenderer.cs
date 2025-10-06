using fin.model;
using fin.scene;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.model;


namespace MarioArtistTool.backgrounds;

public sealed class ShadowRenderer(IReadOnlyModel model)
    : ISceneNodeRenderComponent {
  private readonly IModelRenderer impl_ = new ModelRenderer(model);
 
  public void Dispose() => this.impl_.Dispose();

  public void Render(ISceneNodeInstance self)
    => GlUtil.RenderAsShadow(() => this.impl_.Render());
}