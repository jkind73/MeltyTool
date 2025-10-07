using fin.scene;
using fin.ui.rendering.gl;


namespace MarioArtistTool.backgrounds;

public sealed class ShadowRenderComponent(ISceneNodeRenderComponent impl)
    : ISceneNodeRenderComponent {
  public void Dispose() => impl.Dispose();

  public void Render(ISceneNodeInstance self)
    => GlUtil.RenderAsShadow(() => impl.Render(self));
}