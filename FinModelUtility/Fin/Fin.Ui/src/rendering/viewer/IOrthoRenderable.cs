
namespace fin.ui.rendering.viewer;

public interface IOrthoRenderable : IRenderable {
  float ViewportWidth { get; set; }
  float ViewportHeight { get; set; }
}